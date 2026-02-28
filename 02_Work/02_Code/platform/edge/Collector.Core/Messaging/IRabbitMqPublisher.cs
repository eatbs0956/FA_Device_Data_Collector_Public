using Collector.Core.Models;

namespace Collector.Core.Messaging;

/// <summary>
/// RabbitMQ 消息发布器接口
/// </summary>
public interface IRabbitMqPublisher : IDisposable
{
    /// <summary>
    /// 是否已连接
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// 连接到 RabbitMQ
    /// </summary>
    Task<bool> ConnectAsync(AppSettings settings);

    /// <summary>
    /// 断开连接
    /// </summary>
    Task DisconnectAsync();

    /// <summary>
    /// 发布采集数据
    /// </summary>
    /// <param name="data">采集数据</param>
    /// <returns>是否发送成功</returns>
    Task<bool> PublishAsync(CollectionData data);

    /// <summary>
    /// 批量发布采集数据
    /// </summary>
    /// <param name="dataList">采集数据列表</param>
    /// <returns>成功发送的数量</returns>
    Task<int> PublishBatchAsync(IEnumerable<CollectionData> dataList);

    /// <summary>
    /// 连接状态变更事件
    /// </summary>
    event EventHandler<bool>? ConnectionStateChanged;
}
