namespace Auth.Api.Services.Abstractions;

/// <summary>
/// 审计日志条目 - 用于异步队列传递的审计数据
/// </summary>
public class AuditLogEntry
{
    /// <summary>
    /// 租户ID
    /// </summary>
    public string? TenantId { get; set; }

    /// <summary>
    /// 用户ID
    /// </summary>
    public Guid? UserId { get; set; }

    /// <summary>
    /// 操作类型
    /// </summary>
    public required string Action { get; set; }

    /// <summary>
    /// 资源类型
    /// </summary>
    public string? ResourceType { get; set; }

    /// <summary>
    /// 资源ID
    /// </summary>
    public string? ResourceId { get; set; }

    /// <summary>
    /// IP地址
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// 用户代理
    /// </summary>
    public string? UserAgent { get; set; }

    /// <summary>
    /// 请求体
    /// </summary>
    public string? RequestBody { get; set; }

    /// <summary>
    /// 响应状态码
    /// </summary>
    public int? ResponseStatus { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// 审计服务接口 - 定义异步审计日志记录的契约
/// </summary>
public interface IAuditService
{
    /// <summary>
    /// 将审计日志排队（非阻塞）
    /// </summary>
    /// <param name="entry">审计日志条目</param>
    void EnqueueAuditLog(AuditLogEntry entry);

    /// <summary>
    /// 获取当前队列大小
    /// </summary>
    /// <returns>队列中待处理的日志数量</returns>
    int GetQueueSize();
}
