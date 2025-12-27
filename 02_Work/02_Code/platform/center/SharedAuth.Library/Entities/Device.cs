using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Domain.Entities;

/// <summary>
/// 设备实体 - 跨平台统一设备管理模型
/// </summary>
/// <remarks>
/// 对应LLD文档 11.1.1 核心数据模型中的 devices 表
/// 支持OPC UA、Modbus TCP/RTU、MQTT、西门子S7、三菱MC等协议
/// 继承自 BaseEntity，自动包含审计字段和多租户支持
/// </remarks>
[Table("devices")]
public class Device : BaseEntity
{
    /// <summary>
    /// 主键ID
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 设备标识符（唯一）
    /// </summary>
    [Required]
    [MaxLength(64)]
    [Column("device_id")]
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// 设备名称
    /// </summary>
    [Required]
    [MaxLength(128)]
    [Column("device_name")]
    public string DeviceName { get; set; } = string.Empty;

    /// <summary>
    /// 设备类型
    /// </summary>
    [Required]
    [MaxLength(32)]
    [Column("device_type")]
    public string DeviceType { get; set; } = string.Empty;

    /// <summary>
    /// 协议类型 - OPC_UA, MODBUS_TCP, MODBUS_RTU, MQTT, S7, MC等
    /// </summary>
    [Required]
    [MaxLength(32)]
    [Column("protocol_type")]
    public string ProtocolType { get; set; } = string.Empty;

    /// <summary>
    /// 所属边缘节点ID（外键，可选）
    /// </summary>
    [Column("edge_node_id")]
    public Guid? EdgeNodeId { get; set; }

    /// <summary>
    /// 连接配置（JSON格式）
    /// 存储IP、端口、超时等连接参数
    /// </summary>
    [Required]
    [Column("connection_config", TypeName = "jsonb")]
    public string ConnectionConfig { get; set; } = string.Empty;

    /// <summary>
    /// 协议特定配置（JSON格式）
    /// 存储协议相关的参数配置
    /// </summary>
    [Required]
    [Column("protocol_config", TypeName = "jsonb")]
    public string ProtocolConfig { get; set; } = string.Empty;

    /// <summary>
    /// 连接状态 - Connected, Disconnected, Connecting, Error
    /// </summary>
    [MaxLength(16)]
    [Column("connection_status")]
    public string ConnectionStatus { get; set; } = "Disconnected";

    /// <summary>
    /// 最后连接时间
    /// </summary>
    [Column("last_connect_time")]
    public DateTimeOffset? LastConnectTime { get; set; }

    /// <summary>
    /// 错误计数
    /// </summary>
    [Column("error_count")]
    public int ErrorCount { get; set; } = 0;

    /// <summary>
    /// 最后错误信息
    /// </summary>
    [Column("last_error")]
    public string? LastError { get; set; }

    /// <summary>
    /// 标签配置（JSON格式数组）
    /// 存储设备关联的标签列表
    /// </summary>
    [Column("tags_config", TypeName = "jsonb")]
    public string TagsConfig { get; set; } = "[]";

    /// <summary>
    /// 设备厂商
    /// </summary>
    [MaxLength(128)]
    [Column("vendor")]
    public string? Vendor { get; set; }

    /// <summary>
    /// 设备型号
    /// </summary>
    [MaxLength(128)]
    [Column("model")]
    public string? Model { get; set; }

    /// <summary>
    /// 固件版本
    /// </summary>
    [MaxLength(64)]
    [Column("firmware_version")]
    public string? FirmwareVersion { get; set; }

    /// <summary>
    /// 设备描述
    /// </summary>
    [Column("description")]
    public string? Description { get; set; }

    /// <summary>
    /// 设备物理位置
    /// </summary>
    [MaxLength(256)]
    [Column("location")]
    public string? Location { get; set; }

    /// <summary>
    /// 业务分组ID（可选）
    /// </summary>
    [Column("group_id")]
    public Guid? GroupId { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    [Column("enabled")]
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 所属边缘节点（导航属性）
    /// </summary>
    [ForeignKey("EdgeNodeId")]
    public virtual EdgeNode? EdgeNode { get; set; }

    /// <summary>
    /// 所属业务分组（导航属性）
    /// </summary>
    [ForeignKey("GroupId")]
    public virtual DeviceGroup? Group { get; set; }

    /// <summary>
    /// 关联的标签定义列表（导航属性）
    /// </summary>
    public virtual ICollection<TagDefinition> TagDefinitions { get; set; } = new List<TagDefinition>();
}
