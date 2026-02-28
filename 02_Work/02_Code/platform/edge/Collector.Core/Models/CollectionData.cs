namespace Collector.Core.Models;

/// <summary>
/// 采集数据 - 发送到 RabbitMQ 的消息格式
/// </summary>
public class CollectionData
{
    /// <summary>
    /// 消息ID
    /// </summary>
    public string MessageId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 节点ID
    /// </summary>
    public string NodeId { get; set; } = string.Empty;

    /// <summary>
    /// 任务ID
    /// </summary>
    public Guid TaskId { get; set; }

    /// <summary>
    /// 设备ID
    /// </summary>
    public Guid DeviceId { get; set; }

    /// <summary>
    /// 设备标识符
    /// </summary>
    public string DeviceCode { get; set; } = string.Empty;

    /// <summary>
    /// 采集时间戳
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// 数据点列表
    /// </summary>
    public List<DataPoint> DataPoints { get; set; } = new();

    /// <summary>
    /// 采集质量
    /// </summary>
    public CollectionQuality Quality { get; set; } = CollectionQuality.Good;

    /// <summary>
    /// 错误信息（如果有）
    /// </summary>
    public string? ErrorMessage { get; set; }
}

/// <summary>
/// 单个数据点
/// </summary>
public class DataPoint
{
    /// <summary>
    /// 标签ID
    /// </summary>
    public string TagId { get; set; } = string.Empty;

    /// <summary>
    /// 标签名称
    /// </summary>
    public string TagName { get; set; } = string.Empty;

    /// <summary>
    /// 原始值
    /// </summary>
    public object? RawValue { get; set; }

    /// <summary>
    /// 转换后的值（应用比例因子和偏移量后）
    /// </summary>
    public object? Value { get; set; }

    /// <summary>
    /// 数据类型
    /// </summary>
    public DataPointType DataType { get; set; }

    /// <summary>
    /// 单位
    /// </summary>
    public string? Unit { get; set; }

    /// <summary>
    /// 数据质量
    /// </summary>
    public DataQuality Quality { get; set; } = DataQuality.Good;

    /// <summary>
    /// 采集时间戳
    /// </summary>
    public DateTimeOffset Timestamp { get; set; }
}

/// <summary>
/// 采集质量枚举
/// </summary>
public enum CollectionQuality
{
    Good,
    Uncertain,
    Bad,
    ConfigError,
    CommunicationError,
    DeviceError
}

/// <summary>
/// 数据质量枚举
/// </summary>
public enum DataQuality
{
    Good,
    Uncertain,
    Bad,
    OutOfRange,
    Timeout,
    NotAvailable
}
