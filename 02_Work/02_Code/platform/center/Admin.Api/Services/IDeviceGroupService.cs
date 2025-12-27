using Admin.Api.Domain.DTOs;

namespace Admin.Api.Services;

/// <summary>
/// 设备分组服务接口
/// </summary>
public interface IDeviceGroupService
{
    /// <summary>
    /// 分页查询设备分组
    /// </summary>
    Task<DeviceGroupListResponse> GetGroupsAsync(DeviceGroupQueryRequest request);

    /// <summary>
    /// 获取设备分组树（完整树形结构）
    /// </summary>
    Task<List<DeviceGroupDto>> GetGroupTreeAsync();

    /// <summary>
    /// 获取设备分组树（简化版，用于下拉选择）
    /// </summary>
    Task<List<DeviceGroupTreeNode>> GetGroupTreeSimpleAsync();

    /// <summary>
    /// 根据ID获取分组详情
    /// </summary>
    Task<DeviceGroupDto?> GetGroupByIdAsync(Guid id);

    /// <summary>
    /// 创建分组
    /// </summary>
    Task<Guid> CreateGroupAsync(CreateDeviceGroupRequest request);

    /// <summary>
    /// 更新分组
    /// </summary>
    Task UpdateGroupAsync(Guid id, UpdateDeviceGroupRequest request);

    /// <summary>
    /// 删除分组（级联删除子分组）
    /// </summary>
    Task DeleteGroupAsync(Guid id);

    /// <summary>
    /// 移动分组（更改父级或排序）
    /// </summary>
    Task MoveGroupAsync(Guid id, MoveDeviceGroupRequest request);
}
