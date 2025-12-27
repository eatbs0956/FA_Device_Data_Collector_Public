using Admin.Api.Domain.DTOs;

namespace Admin.Api.Services;

/// <summary>
/// 采集任务服务接口
/// </summary>
public interface ICollectionTaskService
{
    /// <summary>
    /// 获取采集任务列表（分页）
    /// </summary>
    Task<CollectionTaskListResponse> GetTasksAsync(CollectionTaskQueryRequest request);

    /// <summary>
    /// 根据ID获取采集任务详情
    /// </summary>
    Task<CollectionTaskDto?> GetTaskByIdAsync(Guid id);

    /// <summary>
    /// 创建采集任务
    /// </summary>
    Task<Guid> CreateTaskAsync(CreateCollectionTaskRequest request);

    /// <summary>
    /// 更新采集任务
    /// </summary>
    Task UpdateTaskAsync(Guid id, UpdateCollectionTaskRequest request);

    /// <summary>
    /// 删除采集任务（软删除）
    /// </summary>
    Task DeleteTaskAsync(Guid id);

    /// <summary>
    /// 变更任务状态
    /// </summary>
    /// <remarks>
    /// 支持的状态变更：
    /// Draft -> Active (启动)
    /// Active -> Paused (暂停)
    /// Paused -> Active (恢复)
    /// Active/Paused -> Stopped (停止)
    /// Stopped -> Active (重新启动)
    /// </remarks>
    Task ChangeStatusAsync(Guid id, string targetStatus);

    /// <summary>
    /// 获取可用设备列表（用于任务关联）
    /// </summary>
    /// <remarks>
    /// 返回已启用的设备列表
    /// </remarks>
    Task<List<TaskDeviceOptionDto>> GetAvailableDevicesAsync();
}

/// <summary>
/// 任务可选设备DTO
/// </summary>
public class TaskDeviceOptionDto
{
    /// <summary>
    /// 设备ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 设备名称
    /// </summary>
    public string DeviceName { get; set; } = string.Empty;

    /// <summary>
    /// 设备标识符
    /// </summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// 协议类型
    /// </summary>
    public string ProtocolType { get; set; } = string.Empty;

    /// <summary>
    /// 连接状态
    /// </summary>
    public string ConnectionStatus { get; set; } = string.Empty;
}
