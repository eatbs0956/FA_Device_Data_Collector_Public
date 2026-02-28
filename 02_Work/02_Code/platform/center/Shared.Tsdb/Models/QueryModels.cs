namespace Shared.Tsdb.Models;

/// <summary>
/// 设备数据查询请求
/// </summary>
public class DeviceDataQuery
{
    /// <summary>
    /// 租户ID
    /// </summary>
    public string Tenant { get; set; } = string.Empty;

    /// <summary>
    /// 设备ID（可选，不指定则查询所有设备）
    /// </summary>
    public string? Device { get; set; }

    /// <summary>
    /// 字段列表（可选，不指定则查询所有字段）
    /// </summary>
    public List<string>? Fields { get; set; }

    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime EndTime { get; set; }

    /// <summary>
    /// 聚合窗口（如 1m, 5m, 1h）
    /// </summary>
    public string? AggregateWindow { get; set; }

    /// <summary>
    /// 聚合函数（mean, max, min, sum, count）
    /// </summary>
    public string AggregateFn { get; set; } = "mean";

    /// <summary>
    /// 最大返回记录数
    /// </summary>
    public int? Limit { get; set; }
}

/// <summary>
/// 设备数据查询结果
/// </summary>
public class DeviceDataResult
{
    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// 设备ID
    /// </summary>
    public string Device { get; set; } = string.Empty;

    /// <summary>
    /// 字段名
    /// </summary>
    public string Field { get; set; } = string.Empty;

    /// <summary>
    /// 值
    /// </summary>
    public object? Value { get; set; }
}

/// <summary>
/// 设备最新数据结果
/// </summary>
public class DeviceLatestData
{
    /// <summary>
    /// 设备ID
    /// </summary>
    public string Device { get; set; } = string.Empty;

    /// <summary>
    /// 最新数据时间
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// 字段值
    /// </summary>
    public Dictionary<string, object?> Fields { get; set; } = new();
}

/// <summary>
/// 设备状态查询结果
/// </summary>
public class DeviceStatusResult
{
    /// <summary>
    /// 设备ID
    /// </summary>
    public string Device { get; set; } = string.Empty;

    /// <summary>
    /// 状态
    /// </summary>
    public string Status { get; set; } = "offline";

    /// <summary>
    /// 状态消息
    /// </summary>
    public string? Message { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime UpdatedAt { get; set; }
}
