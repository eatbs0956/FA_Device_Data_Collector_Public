using System.ComponentModel.DataAnnotations;

namespace Shared.Domain.Entities;

/// <summary>
/// 审计日志实体 - 系统操作审计记录
/// </summary>
/// <remarks>
/// 用于记录系统中所有关键操作，满足安全审计和合规要求
/// 继承自 BaseEntity，但只使用 CreatedAt 和 TenantId 字段
/// </remarks>
public class AuditLog : BaseEntity
{
    /// <summary>
    /// 日志标识 - 全局唯一标识符
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 用户标识 - 执行操作的用户ID
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// 操作类型 - 操作名称（Login/Logout/CreateUser/UpdateRole等）
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Action { get; set; } = string.Empty;

    /// <summary>
    /// 资源类型 - 操作对象类型（User/Role/Menu等）
    /// </summary>
    [MaxLength(50)]
    public string? ResourceType { get; set; }

    /// <summary>
    /// 资源标识 - 操作对象的ID
    /// </summary>
    [MaxLength(100)]
    public string? ResourceId { get; set; }

    /// <summary>
    /// IP地址 - 客户端IP地址
    /// </summary>
    [MaxLength(45)]
    public string? IpAddress { get; set; }

    /// <summary>
    /// 用户代理 - 客户端User-Agent信息
    /// </summary>
    [MaxLength(500)]
    public string? UserAgent { get; set; }

    /// <summary>
    /// 请求体 - HTTP请求内容（敏感信息已脱敏）
    /// </summary>
    public string? RequestBody { get; set; }

    /// <summary>
    /// 响应状态码 - HTTP响应状态码
    /// </summary>
    public int? ResponseStatus { get; set; }

    /// <summary>
    /// 错误信息 - 操作失败时的错误详情
    /// </summary>
    public string? ErrorMessage { get; set; }
}
