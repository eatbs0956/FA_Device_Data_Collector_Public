namespace Collector.Core.Models;

/// <summary>
/// 标签（数据点）配置
/// </summary>
public class TagConfig
{
    /// <summary>
    /// 标签ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 标签标识符
    /// </summary>
    public string TagId { get; set; } = string.Empty;

    /// <summary>
    /// 标签名称
    /// </summary>
    public string TagName { get; set; } = string.Empty;

    /// <summary>
    /// 标签地址（协议相关）
    /// 例如 Modbus: "40001" 表示保持寄存器地址1
    /// </summary>
    public string TagAddress { get; set; } = string.Empty;

    /// <summary>
    /// 数据类型
    /// </summary>
    public DataPointType DataType { get; set; } = DataPointType.Float;

    /// <summary>
    /// 单位
    /// </summary>
    public string? Unit { get; set; }

    /// <summary>
    /// 描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 启用状态
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 最小值
    /// </summary>
    public decimal? MinValue { get; set; }

    /// <summary>
    /// 最大值
    /// </summary>
    public decimal? MaxValue { get; set; }

    /// <summary>
    /// 比例因子
    /// </summary>
    public decimal ScalingFactor { get; set; } = 1.0m;

    /// <summary>
    /// 偏移量
    /// </summary>
    public decimal Offset { get; set; } = 0.0m;

    /// <summary>
    /// 访问模式
    /// </summary>
    public AccessMode AccessMode { get; set; } = AccessMode.ReadOnly;

    /// <summary>
    /// 死区值（变化小于此值不上报）
    /// </summary>
    public decimal Deadband { get; set; } = 0.0m;
}

/// <summary>
/// 数据点类型枚举
/// </summary>
public enum DataPointType
{
    Boolean,
    Int16,
    UInt16,
    Int32,
    UInt32,
    Int64,
    UInt64,
    Float,
    Double,
    String,
    DateTime,
    ByteArray
}

/// <summary>
/// 访问模式枚举
/// </summary>
public enum AccessMode
{
    ReadOnly,
    WriteOnly,
    ReadWrite
}
