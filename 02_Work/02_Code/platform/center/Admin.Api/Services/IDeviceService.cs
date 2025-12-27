using Admin.Api.Domain.DTOs;

namespace Admin.Api.Services;

/// <summary>
/// 设备服务接口
/// </summary>
public interface IDeviceService
{
    /// <summary>
    /// 获取设备列表（分页）
    /// </summary>
    Task<DeviceListResponse> GetDevicesAsync(DeviceQueryRequest request);

    /// <summary>
    /// 根据ID获取设备详情
    /// </summary>
    Task<DeviceDto?> GetDeviceByIdAsync(Guid id);

    /// <summary>
    /// 创建设备
    /// </summary>
    Task<Guid> CreateDeviceAsync(CreateDeviceRequest request);

    /// <summary>
    /// 更新设备
    /// </summary>
    Task UpdateDeviceAsync(Guid id, UpdateDeviceRequest request);

    /// <summary>
    /// 删除设备（软删除）
    /// </summary>
    Task DeleteDeviceAsync(Guid id);

    /// <summary>
    /// 批量删除设备（软删除）
    /// </summary>
    Task BatchDeleteAsync(List<Guid> ids);

    /// <summary>
    /// 测试设备连接
    /// </summary>
    Task<DeviceConnectionTestResult> TestConnectionAsync(Guid id);

    /// <summary>
    /// 启用/禁用设备
    /// </summary>
    Task ToggleEnabledAsync(Guid id, bool enabled);
}
