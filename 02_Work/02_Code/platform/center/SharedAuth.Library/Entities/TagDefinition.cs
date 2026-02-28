using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Domain.Entities;

/// <summary>
/// 标签定义实体 - 统一标签模型
/// </summary>
/// <remarks>
/// 对应LLD文档 11.1.1 核心数据模型中的 tag_definitions 表
/// 继承自 BaseEntity，自动包含审计字段和多租户支持
/// </remarks>
[Table("tag_definitions")]
public class TagDefinition : BaseEntity
{
    /// <summary>
    /// 主键ID
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 标签标识符
    /// </summary>
    [Required]
    [MaxLength(128)]
    [Column("tag_id")]
    public string TagId { get; set; } = string.Empty;

    /// <summary>
    /// 所属设备ID（外键）
    /// </summary>
    [Required]
    [Column("device_id")]
    public Guid DeviceId { get; set; }

    /// <summary>
    /// 标签名称
    /// </summary>
    [Required]
    [MaxLength(128)]
    [Column("tag_name")]
    public string TagName { get; set; } = string.Empty;

    /// <summary>
    /// 标签地址 - 协议相关的点位地址
    /// </summary>
    [Required]
    [MaxLength(256)]
    [Column("tag_address")]
    public string TagAddress { get; set; } = string.Empty;

    /// <summary>
    /// 数据类型 - Boolean, Int16, UInt16, Int32, UInt32, Int64, UInt64, Float, Double, String, DateTime, ByteArray
    /// </summary>
    [Required]
    [MaxLength(32)]
    [Column("data_type")]
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// 单位
    /// </summary>
    [MaxLength(16)]
    [Column("unit")]
    public string? Unit { get; set; }

    /// <summary>
    /// 标签描述
    /// </summary>
    [Column("description")]
    public string? Description { get; set; }

    /// <summary>
    /// 启用状态
    /// </summary>
    [Column("enabled")]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 启用实时推送 - 是否将该标签的实时数据推送到前端
    /// </summary>
    [Column("enable_realtime")]
    public bool EnableRealtime { get; set; } = false;

    /// <summary>
    /// 最小值
    /// </summary>
    [Column("min_value", TypeName = "numeric")]
    public decimal? MinValue { get; set; }

    /// <summary>
    /// 最大值
    /// </summary>
    [Column("max_value", TypeName = "numeric")]
    public decimal? MaxValue { get; set; }

    /// <summary>
    /// 比例因子 - 数值转换缩放
    /// </summary>
    [Column("scaling_factor", TypeName = "numeric")]
    public decimal ScalingFactor { get; set; } = 1.0m;

    /// <summary>
    /// 偏移量
    /// </summary>
    [Column("offset", TypeName = "numeric")]
    public decimal Offset { get; set; } = 0.0m;

    /// <summary>
    /// 访问模式 - ReadOnly, WriteOnly, ReadWrite
    /// </summary>
    [MaxLength(16)]
    [Column("access_mode")]
    public string AccessMode { get; set; } = "ReadOnly";

    /// <summary>
    /// 死区值 - 变化小于此值不触发订阅通知
    /// </summary>
    [Column("deadband", TypeName = "numeric")]
    public decimal Deadband { get; set; } = 0.0m;

    /// <summary>
    /// 所属设备（导航属性）
    /// </summary>
    [ForeignKey("DeviceId")]
    public virtual Device? Device { get; set; }
}
