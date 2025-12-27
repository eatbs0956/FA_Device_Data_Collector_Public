using System.ComponentModel.DataAnnotations;

namespace Admin.Api.Domain.DTOs;

/// <summary>
/// 创建设备请求
/// </summary>
public class CreateDeviceRequest
{
    /// <summary>
    /// 设备编码（唯一标识，必填）
    /// </summary>
    [Required(ErrorMessage = "设备编码不能为空")]
    [MaxLength(64, ErrorMessage = "设备编码长度不能超过64")]
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// 设备名称（必填）
    /// </summary>
    [Required(ErrorMessage = "设备名称不能为空")]
    [MaxLength(128, ErrorMessage = "设备名称长度不能超过128")]
    public string DeviceName { get; set; } = string.Empty;

    /// <summary>
    /// 设备类型（必填）
    /// </summary>
    [Required(ErrorMessage = "设备类型不能为空")]
    [MaxLength(32, ErrorMessage = "设备类型长度不能超过32")]
    public string DeviceType { get; set; } = string.Empty;

    /// <summary>
    /// 协议类型（必填）- OPC_UA, MODBUS_TCP, MODBUS_RTU, MQTT, S7, MC�?
    /// </summary>
    [Required(ErrorMessage = "协议类型不能为空")]
    [MaxLength(32, ErrorMessage = "协议类型长度不能超过32")]
    public string ProtocolType { get; set; } = string.Empty;

    /// <summary>
    /// 所属边缘节点ID（可选）
    /// </summary>
    public Guid? EdgeNodeId { get; set; }

    /// <summary>
    /// 连接配置（JSON格式，必填）
    /// 示例: {"ip":"192.168.1.100","port":4840,"timeout":5000}
    /// </summary>
    [Required(ErrorMessage = "连接配置不能为空")]
    public string ConnectionConfig { get; set; } = string.Empty;

    /// <summary>
    /// 协议配置（JSON格式，必填）
    /// 示例: {"securityMode":"None","samplingInterval":1000}
    /// </summary>
    [Required(ErrorMessage = "协议配置不能为空")]
    public string ProtocolConfig { get; set; } = string.Empty;

    /// <summary>
    /// 标签配置（JSON格式数组，可选）
    /// 示例: [{"tagId":"tag-001","nodeId":"ns=2;s=Temperature","dataType":"Float"}]
    /// </summary>
    public string TagsConfig { get; set; } = "[]";

    /// <summary>
    /// 设备厂商（可选）
    /// </summary>
    [MaxLength(128, ErrorMessage = "厂商名称长度不能超过128")]
    public string? Vendor { get; set; }

    /// <summary>
    /// 设备型号（可选）
    /// </summary>
    [MaxLength(128, ErrorMessage = "型号长度不能超过128")]
    public string? Model { get; set; }

    /// <summary>
    /// 固件版本（可选）
    /// </summary>
    [MaxLength(64, ErrorMessage = "固件版本长度不能超过64")]
    public string? FirmwareVersion { get; set; }

    /// <summary>
    /// 设备描述（可选）
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 设备物理位置（可选）
    /// </summary>
    [MaxLength(256, ErrorMessage = "位置信息长度不能超过256")]
    public string? Location { get; set; }

    /// <summary>
    /// 业务分组ID（可选）
    /// </summary>
    public Guid? GroupId { get; set; }

    /// <summary>
    /// 是否启用（默认true�?
    /// </summary>
    public bool Enabled { get; set; } = true;
}
