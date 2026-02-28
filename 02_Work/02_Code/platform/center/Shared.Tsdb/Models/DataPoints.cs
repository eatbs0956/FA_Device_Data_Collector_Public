using InfluxDB.Client.Core;

namespace Shared.Tsdb.Models;

/// <summary>
/// 设备采集数据点
/// </summary>
[Measurement("device_data")]
public class DeviceDataPoint
{
    /// <summary>
    /// 时间戳
    /// </summary>
    [Column(IsTimestamp = true)]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// 租户ID
    /// </summary>
    [Column("tenant", IsTag = true)]
    public string Tenant { get; set; } = string.Empty;

    /// <summary>
    /// 设备ID
    /// </summary>
    [Column("device", IsTag = true)]
    public string Device { get; set; } = string.Empty;

    /// <summary>
    /// 数据来源 (collector/manual/import)
    /// </summary>
    [Column("source", IsTag = true)]
    public string Source { get; set; } = "collector";

    /// <summary>
    /// 标签值（动态字段，使用 Dictionary 存储）
    /// </summary>
    public Dictionary<string, object> Fields { get; set; } = new();
}

/// <summary>
/// 设备状态数据点
/// </summary>
[Measurement("device_status")]
public class DeviceStatusPoint
{
    /// <summary>
    /// 时间戳
    /// </summary>
    [Column(IsTimestamp = true)]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// 租户ID
    /// </summary>
    [Column("tenant", IsTag = true)]
    public string Tenant { get; set; } = string.Empty;

    /// <summary>
    /// 设备ID
    /// </summary>
    [Column("device", IsTag = true)]
    public string Device { get; set; } = string.Empty;

    /// <summary>
    /// 状态 (online/offline/error)
    /// </summary>
    [Column("status")]
    public string Status { get; set; } = "offline";

    /// <summary>
    /// 状态消息
    /// </summary>
    [Column("message")]
    public string? Message { get; set; }
}

/// <summary>
/// 系统指标数据点
/// </summary>
[Measurement("system_metrics")]
public class SystemMetricsPoint
{
    /// <summary>
    /// 时间戳
    /// </summary>
    [Column(IsTimestamp = true)]
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// 租户ID
    /// </summary>
    [Column("tenant", IsTag = true)]
    public string Tenant { get; set; } = string.Empty;

    /// <summary>
    /// 节点ID
    /// </summary>
    [Column("node", IsTag = true)]
    public string Node { get; set; } = string.Empty;

    /// <summary>
    /// CPU 使用率 (%)
    /// </summary>
    [Column("cpu_usage")]
    public double CpuUsage { get; set; }

    /// <summary>
    /// 内存使用率 (%)
    /// </summary>
    [Column("memory_usage")]
    public double MemoryUsage { get; set; }

    /// <summary>
    /// 磁盘使用率 (%)
    /// </summary>
    [Column("disk_usage")]
    public double DiskUsage { get; set; }

    /// <summary>
    /// 网络流量 (bytes/s)
    /// </summary>
    [Column("network_io")]
    public long NetworkIo { get; set; }
}
