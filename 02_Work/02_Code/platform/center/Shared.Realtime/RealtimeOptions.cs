namespace Shared.Realtime;

/// <summary>
/// Redis 配置选项
/// </summary>
public class RealtimeRedisOptions
{
    public const string SectionName = "Redis";

    /// <summary>
    /// 连接字符串
    /// </summary>
    public string ConnectionString { get; set; } = "localhost:6379";
}

/// <summary>
/// 实时推送 Channel 名称
/// </summary>
public static class RealtimeChannels
{
    /// <summary>
    /// 设备状态变更 Channel
    /// 格式: realtime:status:{tenant}:{device}
    /// </summary>
    public const string DeviceStatusPrefix = "realtime:status";

    /// <summary>
    /// 设备实时数据 Channel
    /// 格式: realtime:data:{tenant}:{device}
    /// </summary>
    public const string DeviceDataPrefix = "realtime:data";

    /// <summary>
    /// 告警通知 Channel
    /// 格式: realtime:alert:{tenant}
    /// </summary>
    public const string AlertPrefix = "realtime:alert";

    /// <summary>
    /// 采集任务状态 Channel
    /// 格式: realtime:task:{tenant}
    /// </summary>
    public const string TaskStatusPrefix = "realtime:task";

    /// <summary>
    /// 获取设备状态 Channel
    /// </summary>
    public static string GetDeviceStatusChannel(string tenant, string? device = null)
        => device == null 
            ? $"{DeviceStatusPrefix}:{tenant}:*" 
            : $"{DeviceStatusPrefix}:{tenant}:{device}";

    /// <summary>
    /// 获取设备实时数据 Channel
    /// </summary>
    public static string GetDeviceDataChannel(string tenant, string? device = null)
        => device == null 
            ? $"{DeviceDataPrefix}:{tenant}:*" 
            : $"{DeviceDataPrefix}:{tenant}:{device}";

    /// <summary>
    /// 获取告警 Channel
    /// </summary>
    public static string GetAlertChannel(string tenant)
        => $"{AlertPrefix}:{tenant}";

    /// <summary>
    /// 获取任务状态 Channel
    /// </summary>
    public static string GetTaskStatusChannel(string tenant)
        => $"{TaskStatusPrefix}:{tenant}";
}
