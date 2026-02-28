using Collector.Core.Drivers;
using Collector.Core.Models;

namespace Collector.Core.Engine;

/// <summary>
/// 采集引擎接口
/// </summary>
public interface ICollectionEngine
{
    /// <summary>
    /// 引擎状态
    /// </summary>
    EngineState State { get; }

    /// <summary>
    /// 当前配置
    /// </summary>
    NodeConfig? CurrentConfig { get; }

    /// <summary>
    /// 任务运行状态列表
    /// </summary>
    IReadOnlyList<TaskRuntimeStatus> TaskStatuses { get; }

    /// <summary>
    /// 设备运行状态列表
    /// </summary>
    IReadOnlyList<DeviceRuntimeStatus> DeviceStatuses { get; }

    /// <summary>
    /// 加载配置
    /// </summary>
    /// <param name="config">节点配置</param>
    Task LoadConfigAsync(NodeConfig config);

    /// <summary>
    /// 启动采集
    /// </summary>
    Task StartAsync();

    /// <summary>
    /// 停止采集
    /// </summary>
    Task StopAsync();

    /// <summary>
    /// 暂停采集
    /// </summary>
    Task PauseAsync();

    /// <summary>
    /// 恢复采集
    /// </summary>
    Task ResumeAsync();

    /// <summary>
    /// 启动指定任务
    /// </summary>
    /// <param name="taskId">任务ID</param>
    Task StartTaskAsync(Guid taskId);

    /// <summary>
    /// 停止指定任务
    /// </summary>
    /// <param name="taskId">任务ID</param>
    Task StopTaskAsync(Guid taskId);

    /// <summary>
    /// 手动触发一次采集
    /// </summary>
    /// <param name="taskId">任务ID</param>
    Task TriggerCollectionAsync(Guid taskId);

    /// <summary>
    /// 测试设备连接
    /// </summary>
    /// <param name="deviceId">设备ID（从已加载的配置中查找）</param>
    Task<ConnectionTestResult> TestDeviceConnectionAsync(Guid deviceId);

    /// <summary>
    /// 测试设备连接（直接使用设备配置，无需引擎已加载配置）
    /// </summary>
    /// <param name="device">设备配置</param>
    Task<ConnectionTestResult> TestDeviceConnectionAsync(DeviceConfig device);

    /// <summary>
    /// 手动读取设备数据点
    /// </summary>
    /// <param name="deviceId">设备ID（从已加载的配置中查找）</param>
    /// <param name="tagIds">标签ID列表（为空则读取全部）</param>
    Task<List<TagReadResult>> ReadDeviceTagsAsync(Guid deviceId, IEnumerable<Guid>? tagIds = null);

    /// <summary>
    /// 手动读取设备数据点（直接使用设备配置，无需引擎已加载配置）
    /// </summary>
    /// <param name="device">设备配置</param>
    /// <param name="tagIds">标签ID列表（为空则读取全部）</param>
    Task<List<TagReadResult>> ReadDeviceTagsAsync(DeviceConfig device, IEnumerable<Guid>? tagIds = null);

    /// <summary>
    /// 配置加载/重载完成事件（每次调用 LoadConfigAsync 后都会触发）
    /// </summary>
    event EventHandler? ConfigLoaded;

    /// <summary>
    /// 状态变更事件
    /// </summary>
    event EventHandler<EngineState>? StateChanged;

    /// <summary>
    /// 任务状态变更事件
    /// </summary>
    event EventHandler<TaskRuntimeStatus>? TaskStatusChanged;

    /// <summary>
    /// 设备状态变更事件
    /// </summary>
    event EventHandler<DeviceRuntimeStatus>? DeviceStatusChanged;

    /// <summary>
    /// 数据采集完成事件
    /// </summary>
    event EventHandler<CollectionData>? DataCollected;

    /// <summary>
    /// 错误发生事件
    /// </summary>
    event EventHandler<EngineErrorEventArgs>? ErrorOccurred;
}

/// <summary>
/// 引擎状态枚举
/// </summary>
public enum EngineState
{
    /// <summary>
    /// 未初始化
    /// </summary>
    Uninitialized,
    
    /// <summary>
    /// 已加载配置
    /// </summary>
    Configured,
    
    /// <summary>
    /// 运行中
    /// </summary>
    Running,
    
    /// <summary>
    /// 已暂停
    /// </summary>
    Paused,
    
    /// <summary>
    /// 已停止
    /// </summary>
    Stopped,
    
    /// <summary>
    /// 错误状态
    /// </summary>
    Error
}

/// <summary>
/// 引擎错误事件参数
/// </summary>
public class EngineErrorEventArgs : EventArgs
{
    public string Message { get; set; } = string.Empty;
    public Exception? Exception { get; set; }
    public Guid? TaskId { get; set; }
    public Guid? DeviceId { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}
