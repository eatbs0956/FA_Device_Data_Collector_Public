using System.ComponentModel.DataAnnotations;

namespace Admin.Api.Domain.DTOs;

/// <summary>
/// 更新设备请求
/// </summary>
public class UpdateDeviceRequest
{
    /// <summary>
    /// 设备名称（可选更新）
    /// </summary>
    [MaxLength(128, ErrorMessage = "设备名称长度不能超过128")]
    public string? DeviceName { get; set; }

    /// <summary>
    /// 设备类型（可选更新）
    /// </summary>
    [MaxLength(32, ErrorMessage = "设备类型长度不能超过32")]
    public string? DeviceType { get; set; }

    /// <summary>
    /// 协议类型（可选更新）
    /// </summary>
    [MaxLength(32, ErrorMessage = "协议类型长度不能超过32")]
    public string? ProtocolType { get; set; }

    /// <summary>
    /// 所属边缘节点ID（可选更新）
    /// </summary>
    public Guid? EdgeNodeId { get; set; }

    /// <summary>
    /// 连接配置（可选更新）
    /// </summary>
    public string? ConnectionConfig { get; set; }

    /// <summary>
    /// 协议配置（可选更新）
    /// </summary>
    public string? ProtocolConfig { get; set; }

    /// <summary>
    /// 标签配置（可选更新）
    /// </summary>
    public string? TagsConfig { get; set; }

    /// <summary>
    /// 设备厂商（可选更新）
    /// </summary>
    [MaxLength(128, ErrorMessage = "厂商名称长度不能超过128")]
    public string? Vendor { get; set; }

    /// <summary>
    /// 设备型号（可选更新）
    /// </summary>
    [MaxLength(128, ErrorMessage = "型号长度不能超过128")]
    public string? Model { get; set; }

    /// <summary>
    /// 固件版本（可选更新）
    /// </summary>
    [MaxLength(64, ErrorMessage = "固件版本长度不能超过64")]
    public string? FirmwareVersion { get; set; }

    /// <summary>
    /// 设备描述（可选更新）
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 设备物理位置（可选更新）
    /// </summary>
    [MaxLength(256, ErrorMessage = "位置信息长度不能超过256")]
    public string? Location { get; set; }

    /// <summary>
    /// 业务分组ID（可选更新）
    /// </summary>
    public Guid? GroupId { get; set; }

    /// <summary>
    /// 是否启用（可选更新）
    /// </summary>
    public bool? Enabled { get; set; }
}
