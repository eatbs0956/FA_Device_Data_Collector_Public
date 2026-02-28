using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Shared.Domain.Data;
using Shared.Domain.Entities;
using Auth.Api.Services.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace Auth.Api.Services;

/// <summary>
/// JWT 令牌服务 - 负责生成、验证和管理 JWT 访问令牌及刷新令牌
/// </summary>
public class TokenService(UnifiedDbContext db) : ITokenService
{
    // RSA 加密算法实例 - 用于 JWT 签名的 RSA 密钥对
    private static RSA? _rsa;
    // RSA 安全密钥 - 封装的 RSA 密钥，用于令牌签名验证
    private static RsaSecurityKey? _key;
    // 密钥标识符 - 用于 JWKS 中标识特定密钥
    private static string? _kid;

    /// <summary>
    /// 令牌对记录 - 包含访问令牌和刷新令牌的数据结构
    /// </summary>
    public record TokenPair(string Token, string RefreshToken);

    /// <summary>
    /// 确保密钥已初始化 - 如果 RSA 密钥尚未创建，则生成新的 2048 位密钥对
    /// </summary>
    public void EnsureKey()
    {
        if (_rsa != null) return;
        // RSA 密钥生成 - 创建 2048 位的 RSA 密钥对用于 JWT 签名
        _rsa = RSA.Create(2048);
        // 安全密钥封装 - 将 RSA 密钥封装为 SecurityKey 并分配唯一标识符
        _key = new RsaSecurityKey(_rsa) { KeyId = Guid.NewGuid().ToString("N") };
        // 密钥标识符赋值 - 保存密钥 ID 用于 JWKS 响应
        _kid = _key.KeyId;
    }

    /// <summary>
    /// 获取安全密钥 - 返回用于 JWT 验证的 SecurityKey 实例
    /// </summary>
    public SecurityKey GetSecurityKey()
    {
        EnsureKey();
        return _key!;
    }

    /// <summary>
    /// 获取 JWKS 公钥信息 - 返回符合 RFC 7517 标准的 JSON Web Key Set 格式数据
    /// </summary>
    public object GetJwks()
    {
        EnsureKey();
        // RSA 参数导出 - 获取 RSA 公钥参数（不包含私钥信息）
        var parameters = _rsa!.ExportParameters(false);
        return new
        {
            keys = new[]
            {
                new
                {
                    kty = "RSA",
                    use = "sig",
                    kid = _kid,
                    alg = "RS256",
                    // 模数编码 - 将 RSA 模数转换为 Base64Url 编码
                    n = Base64UrlEncoder.Encode(parameters.Modulus!),
                    // 指数编码 - 将 RSA 公钥指数转换为 Base64Url 编码
                    e = Base64UrlEncoder.Encode(parameters.Exponent!)
                }
            }
        };
    }

    /// <summary>
    /// 异步颁发令牌对 - 为指定用户生成新的访问令牌和刷新令牌
    /// </summary>
    /// <param name="user">用户实体 - 需要颁发令牌的用户</param>
    /// <param name="accessTtl">访问令牌生存时间 - JWT 访问令牌的有效期</param>
    /// <param name="refreshTtl">刷新令牌生存时间 - 刷新令牌的有效期</param>
    /// <returns>令牌对 - 包含访问令牌和刷新令牌的结构</returns>
    public async Task<TokenPair> IssueAsync(User user, TimeSpan accessTtl, TimeSpan refreshTtl)
    {
        EnsureKey();
        // 当前时间戳 - 用于设置令牌的颁发时间和过期时间
        var now = DateTimeOffset.UtcNow;
        // JWT 唯一标识符 - 每个令牌的唯一 ID，用于追踪和撤销
        var jti = Guid.NewGuid().ToString("N");
        // 声明列表 - 包含用户身份和权限信息的 JWT 声明集合
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            // 同时写入 NameIdentifier 以兼容不同读取方式
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.UserName),
            new(JwtRegisteredClaimNames.Jti, jti),
            new("tenant_id", user.TenantId), // 添加租户ID到JWT Token
            new("user_type", user.UserType ?? "user") // 添加用户类型：user=交互账号, service=服务账号
        };
		// 角色代码查询 - 获取用户拥有的所有角色代码用于授权
		var roleCodes = await db.UserRoles.Where(x => x.UserId == user.Id)
			.Join(db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Code)
			.ToListAsync();

        // 角色声明添加 - 将每个角色添加为 JWT 中的角色声明
		foreach (var r in roleCodes) claims.Add(new Claim("role", r));
        // 签名凭据 - 使用 RSA SHA-256 算法的签名凭据
        var credentials = new SigningCredentials(_key!, SecurityAlgorithms.RsaSha256);
        // JWT 安全令牌 - 创建包含所有声明和签名的 JWT 令牌
        var token = new JwtSecurityToken(
            issuer: "devdcp.auth",
            audience: "devdcp.portal",
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: now.Add(accessTtl).UtcDateTime,
            signingCredentials: credentials);
        // JWT 字符串 - 将 JWT 令牌序列化为字符串格式
        var jwt = new JwtSecurityTokenHandler().WriteToken(token);

        // 刷新令牌轮换策略：每个用户只保持一个活跃的刷新令牌
        // 随机刷新令牌 - 生成 48 字节的随机令牌并转换为 Base64 编码
        var rToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(48));
        // RefreshToken哈希 - 计算刷新令牌的SHA256哈希值用于Session存储
        var rTokenHash = Convert.ToBase64String(SHA256.HashData(System.Text.Encoding.UTF8.GetBytes(rToken)));
        
        // 现有令牌查询 - 获取用户当前所有未撤销的刷新令牌
        var existing = await db.RefreshTokens.Where(x => x.UserId == user.Id && !x.Revoked).ToListAsync();
        // 撤销现有令牌 - 将所有旧的刷新令牌标记为已撤销
        foreach (var item in existing) item.Revoked = true;
        
        // 撤销现有会话 - 将用户所有未撤销的会话标记为已撤销
        var existingSessions = await db.Sessions.Where(x => x.UserId == user.Id && !x.Revoked).ToListAsync();
        foreach (var session in existingSessions)
        {
            session.Revoked = true;
            session.RevokedAt = now;
        }
        
        db.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            Token = rToken,
            ExpiresAt = now.Add(refreshTtl)
        });
        
        // 创建新会话记录 - 记录JWT会话信息用于Token撤销检查
        db.Sessions.Add(new Session
        {
            UserId = user.Id,
            AccessTokenJti = jti,
            RefreshTokenHash = rTokenHash,
            IssuedAt = now,
            ExpiresAt = now.Add(refreshTtl)
        });
        
        await db.SaveChangesAsync();

        return new TokenPair(jwt, rToken);
    }

    /// <summary>
    /// 异步验证刷新令牌 - 验证刷新令牌的有效性并返回对应的用户实体
    /// </summary>
    /// <param name="refreshToken">刷新令牌字符串 - 需要验证的刷新令牌</param>
    /// <returns>用户实体或null - 如果令牌有效且用户启用则返回用户，否则返回null</returns>
    public async Task<User?> ValidateRefreshAsync(string refreshToken)
    {
        // 当前时间戳 - 用于检查令牌是否过期
        var now = DateTimeOffset.UtcNow;
        // 刷新令牌记录 - 从数据库中查找匹配且未撤销的刷新令牌
        var rec = await db.RefreshTokens.FirstOrDefaultAsync(x => x.Token == refreshToken && !x.Revoked);
        // 令牌验证 - 检查令牌是否存在且未过期
        if (rec is null || rec.ExpiresAt <= now) return null;
        // 用户查询 - 返回令牌对应的已启用用户
        return await db.Users.FirstOrDefaultAsync(x => x.Id == rec.UserId && x.Enabled);
    }
}
