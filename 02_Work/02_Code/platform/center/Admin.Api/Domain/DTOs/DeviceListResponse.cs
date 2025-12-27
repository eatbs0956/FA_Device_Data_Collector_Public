namespace Admin.Api.Domain.DTOs;

/// <summary>
/// 设备列表响应 - 适配前端 Common.PaginatingQueryRecord 结构
/// </summary>
public class DeviceListResponse
{
    /// <summary>
    /// 设备列表 - 前端字段�?records
    /// </summary>
    public List<DeviceDto> Records { get; set; } = new();

    /// <summary>
    /// 总记录数
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// 当前页码
    /// </summary>
    public int Current { get; set; }

    /// <summary>
    /// 每页条数
    /// </summary>
    public int Size { get; set; }
}
