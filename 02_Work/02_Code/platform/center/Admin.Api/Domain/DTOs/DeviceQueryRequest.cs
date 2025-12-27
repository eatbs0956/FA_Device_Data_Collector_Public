namespace Admin.Api.Domain.DTOs;

/// <summary>
/// 设备查询请求
/// </summary>
public class DeviceQueryRequest
{
    /// <summary>
    /// 设备名称（模糊搜索）
    /// </summary>
    public string? DeviceName { get; set; }

    /// <summary>
    /// 设备编码（精确搜索）
    /// </summary>
    public string? DeviceId { get; set; }

    /// <summary>
    /// 协议类型（精确搜索）
    /// </summary>
    public string? ProtocolType { get; set; }

    /// <summary>
    /// 设备类型（精确搜索）
    /// </summary>
    public string? DeviceType { get; set; }

    /// <summary>
    /// 连接状态（精确搜索�?
    /// </summary>
    public string? ConnectionStatus { get; set; }

    /// <summary>
    /// 边缘节点ID（精确搜索）
    /// </summary>
    public Guid? EdgeNodeId { get; set; }

    /// <summary>
    /// 业务分组ID（精确搜索）
    /// </summary>
    public Guid? GroupId { get; set; }

    /// <summary>
    /// 是否启用（精确搜索）
    /// </summary>
    public bool? Enabled { get; set; }

    /// <summary>
    /// 设备位置（模糊搜索）
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// 设备厂商（模糊搜索）
    /// </summary>
    public string? Vendor { get; set; }

    /// <summary>
    /// 页码（从1开始）- 前端参数�?current
    /// </summary>
    public int Current { get; set; } = 1;

    /// <summary>
    /// 每页条数 - 前端参数�?size
    /// </summary>
    public int Size { get; set; } = 20;

    /// <summary>
    /// 排序字段（默认按创建时间倒序�?
    /// 可选�? DeviceName, CreatedAt, LastConnectTime�?
    /// </summary>
    public string? SortBy { get; set; } = "CreatedAt";

    /// <summary>
    /// 排序方向（asc/desc�?
    /// </summary>
    public string? SortOrder { get; set; } = "desc";
}
