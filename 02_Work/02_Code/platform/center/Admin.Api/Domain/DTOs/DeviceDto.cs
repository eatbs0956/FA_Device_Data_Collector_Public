namespace Admin.Api.Domain.DTOs;

/// <summary>
/// 设备数据传输对象
/// </summary>
public class DeviceDto
{
    /// <summary>
    /// 设备ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 设备编码（唯一标识�?
    /// </summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// 设备名称
    /// </summary>
    public string DeviceName { get; set; } = string.Empty;

    /// <summary>
    /// 设备类型
    /// </summary>
    public string DeviceType { get; set; } = string.Empty;

    /// <summary>
    /// 协议类型
    /// </summary>
    public string ProtocolType { get; set; } = string.Empty;

    /// <summary>
    /// 所属边缘节点ID（可选）
    /// </summary>
    public Guid? EdgeNodeId { get; set; }

    /// <summary>
    /// 边缘节点名称
    /// </summary>
    public string? EdgeNodeName { get; set; }

    /// <summary>
    /// 连接配置（JSON字符串）
    /// </summary>
    public string ConnectionConfig { get; set; } = string.Empty;

    /// <summary>
    /// 协议配置（JSON字符串）
    /// </summary>
    public string ProtocolConfig { get; set; } = string.Empty;

    /// <summary>
    /// 连接状�?
    /// </summary>
    public string ConnectionStatus { get; set; } = "Disconnected";

    /// <summary>
    /// 最后连接时�?
    /// </summary>
    public DateTimeOffset? LastConnectTime { get; set; }

    /// <summary>
    /// 错误计数
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// 最后错误信�?
    /// </summary>
    public string? LastError { get; set; }

    /// <summary>
    /// 标签数量
    /// </summary>
    public int TagCount { get; set; }

    /// <summary>
    /// 设备厂商
    /// </summary>
    public string? Vendor { get; set; }

    /// <summary>
    /// 设备型号
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// 固件版本
    /// </summary>
    public string? FirmwareVersion { get; set; }

    /// <summary>
    /// 设备描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 设备物理位置
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// 业务分组ID
    /// </summary>
    public Guid? GroupId { get; set; }

    /// <summary>
    /// 业务分组名称
    /// </summary>
    public string? GroupName { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// 租户ID
    /// </summary>
    public string TenantId { get; set; } = "t1";

    /// <summary>
    /// 创建人ID
    /// </summary>
    public Guid? CreatedBy { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// 更新人ID
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; }
}
