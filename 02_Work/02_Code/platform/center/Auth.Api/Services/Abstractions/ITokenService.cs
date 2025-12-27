using Shared.Domain.Entities;
using Auth.Api.Services;
using Microsoft.IdentityModel.Tokens;

namespace Auth.Api.Services.Abstractions;

/// <summary>
/// 令牌管理服务接口 - 定义JWT令牌生成和验证的抽象契约
/// </summary>
public interface ITokenService
{
    /// <summary>
    /// 确保RSA密钥已初始化
    /// </summary>
    void EnsureKey();

    /// <summary>
    /// 获取RSA安全密钥
    /// </summary>
    SecurityKey GetSecurityKey();

    /// <summary>
    /// 获取JWKS公钥集合
    /// </summary>
    object GetJwks();

    /// <summary>
    /// 为用户签发令牌对(访问令牌+刷新令牌)
    /// </summary>
    Task<TokenService.TokenPair> IssueAsync(User user, TimeSpan accessTtl, TimeSpan refreshTtl);

    /// <summary>
    /// 验证刷新令牌并返回用户信息
    /// </summary>
    Task<User?> ValidateRefreshAsync(string refreshToken);
}
