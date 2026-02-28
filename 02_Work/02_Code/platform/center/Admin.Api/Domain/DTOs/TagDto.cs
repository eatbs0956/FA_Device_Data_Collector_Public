namespace Admin.Api.Domain.DTOs;

/// <summary>
/// 标签数据传输对象
/// </summary>
public class TagDto
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 标签标识符
    /// </summary>
    public string TagId { get; set; } = string.Empty;

    /// <summary>
    /// 所属设备ID
    /// </summary>
    public Guid DeviceId { get; set; }

    /// <summary>
    /// 所属设备名称
    /// </summary>
    public string DeviceName { get; set; } = string.Empty;

    /// <summary>
    /// 设备协议类型
    /// </summary>
    public string ProtocolType { get; set; } = string.Empty;

    /// <summary>
    /// 标签名称
    /// </summary>
    public string TagName { get; set; } = string.Empty;

    /// <summary>
    /// 标签地址（JSON格式，存储协议特定的地址配置）
    /// </summary>
    public string TagAddress { get; set; } = string.Empty;

    /// <summary>
    /// 数据类型
    /// </summary>
    public string DataType { get; set; } = string.Empty;

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
    /// 是否启用实时推送
    /// </summary>
    public bool EnableRealtime { get; set; }

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
    public string AccessMode { get; set; } = "ReadOnly";

    /// <summary>
    /// 死区值
    /// </summary>
    public decimal Deadband { get; set; } = 0.0m;

    /// <summary>
    /// 租户ID
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// 创建人
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// 更新人
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }
}
