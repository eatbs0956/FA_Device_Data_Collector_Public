namespace Shared.Tsdb;

/// <summary>
/// InfluxDB 连接配置
/// </summary>
public class InfluxDbOptions
{
    public const string SectionName = "InfluxDb";

    /// <summary>
    /// InfluxDB 服务地址
    /// </summary>
    public string Url { get; set; } = "http://localhost:8086";

    /// <summary>
    /// 访问令牌
    /// </summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>
    /// 组织名称
    /// </summary>
    public string Org { get; set; } = "devorg";

    /// <summary>
    /// 默认 Bucket（采集数据）
    /// </summary>
    public string DefaultBucket { get; set; } = "collected";
}

/// <summary>
/// Bucket 名称常量
/// </summary>
public static class TsdbBuckets
{
    /// <summary>
    /// 采集数据桶 (30天保留)
    /// </summary>
    public const string Collected = "collected";

    /// <summary>
    /// 聚合数据桶 (365天保留)
    /// </summary>
    public const string Aggregated = "aggregated";

    /// <summary>
    /// 系统指标桶 (90天保留)
    /// </summary>
    public const string Metrics = "metrics";
}

/// <summary>
/// Measurement 名称常量
/// </summary>
public static class TsdbMeasurements
{
    /// <summary>
    /// 设备数据
    /// </summary>
    public const string DeviceData = "device_data";

    /// <summary>
    /// 设备状态
    /// </summary>
    public const string DeviceStatus = "device_status";

    /// <summary>
    /// 1分钟聚合数据
    /// </summary>
    public const string DeviceData1m = "device_data_1m";

    /// <summary>
    /// 5分钟聚合数据
    /// </summary>
    public const string DeviceData5m = "device_data_5m";

    /// <summary>
    /// 1小时聚合数据
    /// </summary>
    public const string DeviceData1h = "device_data_1h";

    /// <summary>
    /// 系统指标
    /// </summary>
    public const string SystemMetrics = "system_metrics";
}
