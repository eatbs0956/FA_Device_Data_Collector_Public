namespace Admin.Api.Domain.DTOs;

/// <summary>
/// 设备连接测试结果
/// </summary>
public class DeviceConnectionTestResult
{
    /// <summary>
    /// 测试是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 响应时间（毫秒）
    /// </summary>
    public int ResponseTime { get; set; }

    /// <summary>
    /// 测试消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 协议类型
    /// </summary>
    public string Protocol { get; set; } = string.Empty;

    /// <summary>
    /// 服务器信息（可选）
    /// </summary>
    public Dictionary<string, string>? ServerInfo { get; set; }

    /// <summary>
    /// 错误详情（失败时�?
    /// </summary>
    public string? ErrorDetails { get; set; }

    /// <summary>
    /// 测试时间
    /// </summary>
    public DateTimeOffset TestedAt { get; set; } = DateTimeOffset.UtcNow;
}
