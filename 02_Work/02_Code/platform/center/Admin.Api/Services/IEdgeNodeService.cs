using Admin.Api.Domain.DTOs;

namespace Admin.Api.Services;

/// <summary>
/// 边缘节点服务接口
/// </summary>
public interface IEdgeNodeService
{
    /// <summary>
    /// 获取边缘节点列表（分页）
    /// </summary>
    Task<EdgeNodeListResponse> GetEdgeNodesAsync(EdgeNodeQueryRequest request);

    /// <summary>
    /// 根据ID获取边缘节点详情
    /// </summary>
    Task<EdgeNodeDto?> GetEdgeNodeByIdAsync(Guid id);

    /// <summary>
    /// 创建边缘节点（手动添加）
    /// </summary>
    Task<Guid> CreateEdgeNodeAsync(CreateEdgeNodeRequest request);

    /// <summary>
    /// 更新边缘节点
    /// </summary>
    Task UpdateEdgeNodeAsync(Guid id, UpdateEdgeNodeRequest request);

    /// <summary>
    /// 删除边缘节点（软删除）
    /// </summary>
    /// <returns>关联设备数量</returns>
    Task<int> DeleteEdgeNodeAsync(Guid id);

    /// <summary>
    /// 获取边缘节点关联的设备数量
    /// </summary>
    Task<int> GetDeviceCountAsync(Guid id);
}
