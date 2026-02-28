using Collector.Core.Models;

namespace Collector.Core.ApiClient;

/// <summary>
/// Admin.Api 客户端接口
/// </summary>
public interface IAdminApiClient
{
    /// <summary>
    /// 节点自动注册
    /// </summary>
    /// <param name="request">注册请求</param>
    /// <returns>注册结果</returns>
    Task<ApiResponse<NodeInfo>> RegisterNodeAsync(NodeRegisterRequest request);

    /// <summary>
    /// 发送心跳
    /// </summary>
    /// <param name="nodeId">节点ID</param>
    /// <param name="request">心跳请求</param>
    /// <returns>心跳响应</returns>
    Task<ApiResponse<object>> SendHeartbeatAsync(string nodeId, HeartbeatRequest request);

    /// <summary>
    /// 获取节点完整配置
    /// </summary>
    /// <param name="nodeId">节点ID</param>
    /// <returns>节点配置</returns>
    Task<ApiResponse<NodeConfig>> GetNodeConfigAsync(string nodeId);

    /// <summary>
    /// 设置认证令牌
    /// </summary>
    /// <param name="token">JWT Token</param>
    void SetToken(string token);

    /// <summary>
    /// 上报节点状态
    /// </summary>
    /// <param name="nodeId">节点ID</param>
    /// <param name="status">状态信息</param>
    /// <returns>操作结果</returns>
    Task<ApiResponse<object>> ReportStatusAsync(string nodeId, NodeStatusReport status);
}

/// <summary>
/// 节点状态报告
/// </summary>
public class NodeStatusReport
{
    public string Status { get; set; } = "Online";
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public double DiskUsage { get; set; }
    public List<TaskRuntimeStatus> TaskStatuses { get; set; } = new();
    public List<DeviceRuntimeStatus> DeviceStatuses { get; set; } = new();
}
