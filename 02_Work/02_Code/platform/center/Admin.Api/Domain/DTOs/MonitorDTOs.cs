namespace Admin.Api.Domain.DTOs;

#region 请求 DTOs

/// <summary>
/// 历史数据查询请求
/// </summary>
public class HistoryQueryRequest
{
    /// <summary>
    /// 开始时间（ISO 8601 格式）
    /// </summary>
    public DateTime? Start { get; set; }

    /// <summary>
    /// 结束时间（ISO 8601 格式）
    /// </summary>
    public DateTime? End { get; set; }

    /// <summary>
    /// 标签列表（逗号分隔）
    /// </summary>
    public string? Tags { get; set; }

    /// <summary>
    /// 采样间隔（如 1m, 5m, 1h，不指定则自动计算）
    /// </summary>
    public string? Interval { get; set; }

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
/// 统计查询请求
/// </summary>
public class StatisticsQueryRequest
{
    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime? Start { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime? End { get; set; }

    /// <summary>
    /// 聚合粒度（1h, 1d, 1w, 1mo）
    /// </summary>
    public string Granularity { get; set; } = "1h";

    /// <summary>
    /// 设备ID列表（逗号分隔）
    /// </summary>
    public string? DeviceIds { get; set; }

    /// <summary>
    /// 分组ID
    /// </summary>
    public Guid? GroupId { get; set; }

    /// <summary>
    /// 边缘节点ID
    /// </summary>
    public Guid? NodeId { get; set; }
}

#endregion

#region 响应 DTOs

/// <summary>
/// 设备监控数据（用于实时监控卡片）
/// </summary>
public class DeviceMonitorData
{
    /// <summary>
    /// 设备ID（数据库主键）
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 设备标识
    /// </summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// 设备名称
    /// </summary>
    public string DeviceName { get; set; } = string.Empty;

    /// <summary>
    /// 连接状态
    /// </summary>
    public string ConnectionStatus { get; set; } = "Disconnected";

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// 所属分组名称
    /// </summary>
    public string? GroupName { get; set; }

    /// <summary>
    /// 所属节点名称
    /// </summary>
    public string? NodeName { get; set; }

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTime? LastUpdateTime { get; set; }

    /// <summary>
    /// 关键标签数据
    /// </summary>
    public List<TagValueItem> KeyTags { get; set; } = [];
}

/// <summary>
/// 标签值项
/// </summary>
public class TagValueItem
{
    /// <summary>
    /// 标签名称
    /// </summary>
    public string TagName { get; set; } = string.Empty;

    /// <summary>
    /// 标签显示名
    /// </summary>
    public string? DisplayName { get; set; }

    /// <summary>
    /// 当前值
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    /// 单位
    /// </summary>
    public string? Unit { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// 设备详细数据（用于弹窗展示）
/// </summary>
public class DeviceDetailData
{
    /// <summary>
    /// 设备ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 设备标识
    /// </summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// 设备名称
    /// </summary>
    public string DeviceName { get; set; } = string.Empty;

    /// <summary>
    /// 连接状态
    /// </summary>
    public string ConnectionStatus { get; set; } = "Disconnected";

    /// <summary>
    /// 设备类型
    /// </summary>
    public string DeviceType { get; set; } = string.Empty;

    /// <summary>
    /// 协议类型
    /// </summary>
    public string ProtocolType { get; set; } = string.Empty;

    /// <summary>
    /// 位置
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// 最后连接时间
    /// </summary>
    public DateTime? LastConnectTime { get; set; }

    /// <summary>
    /// 所有标签的最新值
    /// </summary>
    public List<TagValueItem> AllTags { get; set; } = [];
}

/// <summary>
/// 历史数据结果
/// </summary>
public class HistoryDataResult
{
    /// <summary>
    /// 设备ID
    /// </summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// 时间序列数据
    /// </summary>
    public List<TimeSeriesPoint> Series { get; set; } = [];
}

/// <summary>
/// 时间序列数据点
/// </summary>
public class TimeSeriesPoint
{
    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// 标签值（key: 标签名, value: 数值）
    /// </summary>
    public Dictionary<string, object?> Values { get; set; } = [];
}

/// <summary>
/// 统计数据结果
/// </summary>
public class StatisticsResult
{
    /// <summary>
    /// 统计维度（device/group/node）
    /// </summary>
    public string Dimension { get; set; } = string.Empty;

    /// <summary>
    /// 维度ID
    /// </summary>
    public string DimensionId { get; set; } = string.Empty;

    /// <summary>
    /// 维度名称
    /// </summary>
    public string DimensionName { get; set; } = string.Empty;

    /// <summary>
    /// 统计项列表
    /// </summary>
    public List<StatisticsItem> Items { get; set; } = [];
}

/// <summary>
/// 统计项
/// </summary>
public class StatisticsItem
{
    /// <summary>
    /// 时间段开始
    /// </summary>
    public DateTime PeriodStart { get; set; }

    /// <summary>
    /// 时间段结束
    /// </summary>
    public DateTime PeriodEnd { get; set; }

    /// <summary>
    /// 数据点数量
    /// </summary>
    public long DataPointCount { get; set; }

    /// <summary>
    /// 在线设备数
    /// </summary>
    public int OnlineDeviceCount { get; set; }

    /// <summary>
    /// 告警次数
    /// </summary>
    public int AlertCount { get; set; }

    /// <summary>
    /// 各标签的聚合数据
    /// </summary>
    public Dictionary<string, AggregatedValue> TagAggregations { get; set; } = [];
}

/// <summary>
/// 聚合值
/// </summary>
public class AggregatedValue
{
    /// <summary>
    /// 最小值
    /// </summary>
    public double? Min { get; set; }

    /// <summary>
    /// 最大值
    /// </summary>
    public double? Max { get; set; }

    /// <summary>
    /// 平均值
    /// </summary>
    public double? Avg { get; set; }

    /// <summary>
    /// 总和
    /// </summary>
    public double? Sum { get; set; }

    /// <summary>
    /// 计数
    /// </summary>
    public long Count { get; set; }
}

/// <summary>
/// 仪表盘摘要数据
/// </summary>
public class DashboardSummary
{
    /// <summary>
    /// 设备总数
    /// </summary>
    public int TotalDevices { get; set; }

    /// <summary>
    /// 在线设备数
    /// </summary>
    public int OnlineDevices { get; set; }

    /// <summary>
    /// 离线设备数
    /// </summary>
    public int OfflineDevices { get; set; }

    /// <summary>
    /// 错误设备数
    /// </summary>
    public int ErrorDevices { get; set; }

    /// <summary>
    /// 设备在线率
    /// </summary>
    public double OnlineRate { get; set; }

    /// <summary>
    /// 今日采集数据点数
    /// </summary>
    public long TodayDataPoints { get; set; }

    /// <summary>
    /// 昨日采集数据点数（用于对比）
    /// </summary>
    public long YesterdayDataPoints { get; set; }

    /// <summary>
    /// 今日告警数
    /// </summary>
    public int TodayAlerts { get; set; }

    /// <summary>
    /// 未处理告警数
    /// </summary>
    public int UnhandledAlerts { get; set; }

    /// <summary>
    /// 边缘节点总数
    /// </summary>
    public int TotalNodes { get; set; }

    /// <summary>
    /// 在线节点数
    /// </summary>
    public int OnlineNodes { get; set; }

    /// <summary>
    /// 最近告警列表
    /// </summary>
    public List<RecentAlert> RecentAlerts { get; set; } = [];

    /// <summary>
    /// 设备分组统计
    /// </summary>
    public List<GroupDeviceCount> GroupStats { get; set; } = [];

    /// <summary>
    /// 采集趋势数据（最近24小时）
    /// </summary>
    public List<TrendPoint> CollectionTrend { get; set; } = [];
}

/// <summary>
/// 最近告警
/// </summary>
public class RecentAlert
{
    /// <summary>
    /// 告警ID
    /// </summary>
    public string AlertId { get; set; } = string.Empty;

    /// <summary>
    /// 设备ID
    /// </summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// 设备名称
    /// </summary>
    public string DeviceName { get; set; } = string.Empty;

    /// <summary>
    /// 告警级别
    /// </summary>
    public string Level { get; set; } = string.Empty;

    /// <summary>
    /// 告警消息
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 告警时间
    /// </summary>
    public DateTime AlertTime { get; set; }

    /// <summary>
    /// 是否已处理
    /// </summary>
    public bool IsHandled { get; set; }
}

/// <summary>
/// 分组设备统计
/// </summary>
public class GroupDeviceCount
{
    /// <summary>
    /// 分组ID
    /// </summary>
    public Guid GroupId { get; set; }

    /// <summary>
    /// 分组名称
    /// </summary>
    public string GroupName { get; set; } = string.Empty;

    /// <summary>
    /// 设备总数
    /// </summary>
    public int DeviceCount { get; set; }

    /// <summary>
    /// 在线数
    /// </summary>
    public int OnlineCount { get; set; }
}

/// <summary>
/// 趋势数据点
/// </summary>
public class TrendPoint
{
    /// <summary>
    /// 时间点
    /// </summary>
    public DateTime Time { get; set; }

    /// <summary>
    /// 值
    /// </summary>
    public long Value { get; set; }
}

/// <summary>
/// 设备树节点（用于历史数据查询的设备选择）
/// </summary>
public class DeviceTreeNode
{
    /// <summary>
    /// 节点ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 节点名称
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// 节点类型（group/device/tag）
    /// </summary>
    public string Type { get; set; } = string.Empty;

    /// <summary>
    /// 是否为叶子节点
    /// </summary>
    public bool IsLeaf { get; set; }

    /// <summary>
    /// 子节点
    /// </summary>
    public List<DeviceTreeNode>? Children { get; set; }

    /// <summary>
    /// 设备ID（仅 device 类型有值）
    /// </summary>
    public string? DeviceId { get; set; }

    /// <summary>
    /// 设备状态（仅 device 类型有值）
    /// </summary>
    public string? Status { get; set; }
}

#endregion
