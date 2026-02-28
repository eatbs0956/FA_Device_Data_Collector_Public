using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Domain.Entities;

/// <summary>
/// 边缘节点实体 - 支持双平台架构（.NET 8.0 + .NET Framework 4.5+）
/// </summary>
/// <remarks>
/// 对应LLD文档 11.1.1 核心数据模型中的 edge_nodes 表
/// 继承自 BaseEntity，自动包含审计字段和多租户支持
/// </remarks>
[Table("edge_nodes")]
public class EdgeNode : BaseEntity
{
    /// <summary>
    /// 主键ID
    /// </summary>
    [Key]
    [Column("id")]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 节点标识符（唯一）
    /// </summary>
    [Required]
    [MaxLength(64)]
    [Column("node_id")]
    public string NodeId { get; set; } = string.Empty;

    /// <summary>
    /// 节点名称
    /// </summary>
    [Required]
    [MaxLength(128)]
    [Column("node_name")]
    public string NodeName { get; set; } = string.Empty;

    /// <summary>
    /// 平台类型 - NET8.0（现代设备）或 NET45（老旧设备）
    /// </summary>
    [Required]
    [MaxLength(16)]
    [Column("platform")]
    public string Platform { get; set; } = string.Empty;

    /// <summary>
    /// 节点版本号
    /// </summary>
    [Required]
    [MaxLength(32)]
    [Column("version")]
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// 部署位置
    /// </summary>
    [MaxLength(256)]
    [Column("location")]
    public string? Location { get; set; }

    /// <summary>
    /// IP地址
    /// </summary>
    [Column("ip_address")]
    public string? IpAddress { get; set; }

    /// <summary>
    /// 通信端口
    /// </summary>
    [Column("port")]
    public int? Port { get; set; }

    /// <summary>
    /// 节点状态 - Online, Offline, Error
    /// </summary>
    [MaxLength(16)]
    [Column("status")]
    public string Status { get; set; } = "Offline";

    /// <summary>
    /// 平台特定配置（JSON格式）
    /// 存储平台差异化配置信息
    /// </summary>
    [Column("platform_config", TypeName = "jsonb")]
    public string PlatformConfig { get; set; } = "{}";

    /// <summary>
    /// 资源限制配置（JSON格式）
    /// 例如：{"maxMemoryMB": 512, "maxConcurrentTasks": 5}
    /// </summary>
    [Column("resource_limits", TypeName = "jsonb")]
    public string ResourceLimits { get; set; } = "{\"maxMemoryMB\": 512, \"maxConcurrentTasks\": 5}";

    /// <summary>
    /// 操作系统信息
    /// </summary>
    [MaxLength(128)]
    [Column("os_info")]
    public string? OsInfo { get; set; }

    /// <summary>
    /// 硬件信息（JSON格式）
    /// </summary>
    [Column("hardware_info", TypeName = "jsonb")]
    public string? HardwareInfo { get; set; }

    /// <summary>
    /// 安装路径
    /// </summary>
    [MaxLength(512)]
    [Column("install_path")]
    public string? InstallPath { get; set; }

    /// <summary>
    /// 最后心跳时间
    /// </summary>
    [Column("last_heartbeat")]
    public DateTimeOffset? LastHeartbeat { get; set; }

    /// <summary>
    /// 注册类型 - auto（自动注册）或 manual（手动添加）
    /// </summary>
    [Required]
    [MaxLength(16)]
    [Column("registration_type")]
    public string RegistrationType { get; set; } = "auto";

    /// <summary>
    /// 关联的服务账号用户ID
    /// 该节点归属于哪个服务账号管理，用于权限控制
    /// </summary>
    [Column("service_user_id")]
    public Guid? ServiceUserId { get; set; }

    /// <summary>
    /// 服务账号导航属性
    /// </summary>
    public virtual User? ServiceUser { get; set; }

    /// <summary>
    /// 关联的设备列表（导航属性）
    /// </summary>
    public virtual ICollection<Device> Devices { get; set; } = new List<Device>();
}
