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

    // ============ Collector API 新增方法 ============

    /// <summary>
    /// 边缘节点注册（采集器启动时调用）
    /// </summary>
    /// <remarks>
    /// 若NodeId不存在则新建（auto类型）；
    /// 若NodeId已存在则更新系统信息并记录心跳
    /// </remarks>
    /// <param name="nodeId">节点标识</param>
    /// <param name="request">注册请求</param>
    /// <returns>注册响应</returns>
    Task<EdgeNodeRegisterResponse> RegisterEdgeNodeAsync(string nodeId, EdgeNodeRegisterRequest request);

    /// <summary>
    /// 更新节点心跳
    /// </summary>
    /// <param name="nodeId">节点标识</param>
    /// <param name="request">心跳请求（可选）</param>
    /// <returns>心跳响应</returns>
    Task<EdgeNodeHeartbeatResponse> UpdateHeartbeatAsync(string nodeId, EdgeNodeHeartbeatRequest? request);

    /// <summary>
    /// 获取节点完整配置
    /// </summary>
    /// <param name="nodeId">节点标识</param>
    /// <returns>节点配置</returns>
    Task<EdgeNodeConfigResponse?> GetNodeConfigAsync(string nodeId);

    /// <summary>
    /// 根据NodeId获取节点（非UUID的业务标识）
    /// </summary>
    /// <param name="nodeId">节点标识</param>
    /// <returns>节点实体ID</returns>
    Task<Guid?> GetNodeIdByNodeIdAsync(string nodeId);

    // ============ 服务账号管理方法 ============

    /// <summary>
    /// 绑定服务账号到边缘节点
    /// </summary>
    /// <param name="edgeNodeId">边缘节点ID</param>
    /// <param name="serviceUserId">服务账号ID（为空则解绑）</param>
    Task BindServiceAccountAsync(Guid edgeNodeId, Guid? serviceUserId);

    /// <summary>
    /// 获取服务账号绑定的所有边缘节点
    /// </summary>
    /// <param name="serviceUserId">服务账号ID</param>
    /// <returns>边缘节点列表</returns>
    Task<List<EdgeNodeDto>> GetEdgeNodesByServiceUserAsync(Guid serviceUserId);
}
