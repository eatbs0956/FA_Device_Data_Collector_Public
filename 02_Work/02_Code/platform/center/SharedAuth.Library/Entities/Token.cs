using System.ComponentModel.DataAnnotations;

namespace Shared.Domain.Entities;

/// <summary>
/// 刷新令牌 - JWT刷新令牌存储实体
/// </summary>
/// <remarks>
/// 不继承 BaseEntity，因为这是临时数据，不需要完整的审计字段
/// </remarks>
public class RefreshToken
{
    /// <summary>
    /// 令牌标识 - 全局唯一标识符
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// 用户标识 - 令牌关联的用户ID
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// 令牌值 - 刷新令牌字符串，最大长度256字符
    /// </summary>
    [Required]
    [MaxLength(256)]
    public string Token { get; set; } = string.Empty;
    
    /// <summary>
    /// 过期时间 - 令牌失效的时间戳
    /// </summary>
    public DateTimeOffset ExpiresAt { get; set; }
    
    /// <summary>
    /// 创建时间 - 令牌创建的时间戳
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// 撤销状态 - 令牌是否已被撤销
    /// </summary>
    public bool Revoked { get; set; }
}

/// <summary>
/// 会话管理实体 - JWT会话跟踪与管理
/// </summary>
/// <remarks>
/// 用于跟踪用户登录会话，支持Token撤销和会话管理
/// 不继承 BaseEntity，因为这是会话数据，不需要完整的审计字段
/// </remarks>
public class Session
{
    /// <summary>
    /// 会话标识 - 全局唯一标识符
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 用户标识 - 会话关联的用户ID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 访问令牌JTI - JWT的jti声明值，用于唯一标识访问令牌
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string AccessTokenJti { get; set; } = string.Empty;

    /// <summary>
    /// 刷新令牌哈希 - 刷新令牌的SHA256哈希值（安全存储）
    /// </summary>
    [Required]
    [MaxLength(256)]
    public string RefreshTokenHash { get; set; } = string.Empty;

    /// <summary>
    /// 签发时间 - 令牌签发的时间戳
    /// </summary>
    public DateTimeOffset IssuedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// 过期时间 - 会话失效的时间戳
    /// </summary>
    public DateTimeOffset ExpiresAt { get; set; }

    /// <summary>
    /// 撤销状态 - 会话是否已被撤销
    /// </summary>
    public bool Revoked { get; set; } = false;

    /// <summary>
    /// 撤销时间 - 会话撤销的时间戳
    /// </summary>
    public DateTimeOffset? RevokedAt { get; set; }

    /// <summary>
    /// IP地址 - 会话创建时的客户端IP地址
    /// </summary>
    [MaxLength(45)]
    public string? IpAddress { get; set; }

    /// <summary>
    /// 用户代理 - 客户端浏览器User-Agent信息
    /// </summary>
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// 创建时间 - 记录创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
