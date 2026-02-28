using Shared.Realtime.Models;

namespace Shared.Realtime;

/// <summary>
/// 实时消息发布服务接口
/// </summary>
public interface IRealtimePublisher
{
    /// <summary>
    /// 发布设备状态变更
    /// </summary>
    Task PublishDeviceStatusAsync(DeviceStatusMessage message);

    /// <summary>
    /// 发布设备实时数据
    /// </summary>
    Task PublishDeviceDataAsync(DeviceDataMessage message);

    /// <summary>
    /// 发布告警通知
    /// </summary>
    Task PublishAlertAsync(AlertMessage message);

    /// <summary>
    /// 发布任务状态
    /// </summary>
    Task PublishTaskStatusAsync(TaskStatusMessage message);

    /// <summary>
    /// 发布自定义消息
    /// </summary>
    Task PublishAsync(string channel, string message);
}

/// <summary>
/// 实时消息订阅服务接口
/// </summary>
public interface IRealtimeSubscriber : IDisposable
{
    /// <summary>
    /// 订阅设备状态变更
    /// </summary>
    Task SubscribeDeviceStatusAsync(string tenant, string? device, Action<DeviceStatusMessage> handler);

    /// <summary>
    /// 订阅设备实时数据
    /// </summary>
    Task SubscribeDeviceDataAsync(string tenant, string? device, Action<DeviceDataMessage> handler);

    /// <summary>
    /// 订阅告警通知
    /// </summary>
    Task SubscribeAlertAsync(string tenant, Action<AlertMessage> handler);

    /// <summary>
    /// 订阅任务状态
    /// </summary>
    Task SubscribeTaskStatusAsync(string tenant, Action<TaskStatusMessage> handler);

    /// <summary>
    /// 订阅自定义频道
    /// </summary>
    Task SubscribeAsync(string pattern, Action<string, string> handler);

    /// <summary>
    /// 取消订阅
    /// </summary>
    Task UnsubscribeAsync(string pattern);

    /// <summary>
    /// 取消所有订阅
    /// </summary>
    Task UnsubscribeAllAsync();
}
