using Collector.Core.Models;

namespace Collector.Agent.Services;

/// <summary>
/// 通知服务接口 - 处理来自服务端的配置变更通知
/// </summary>
public interface INotificationService
{
    /// <summary>
    /// 是否已连接
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// 连接到通知服务
    /// </summary>
    Task ConnectAsync(string hubUrl, string accessToken);

    /// <summary>
    /// 连接到通知服务，并在连接成功后自动注册节点
    /// </summary>
    Task ConnectAsync(string hubUrl, string accessToken, string? nodeId);

    /// <summary>
    /// 报告节点状态（心跳）
    /// </summary>
    /// <param name="nodeId">节点ID</param>
    /// <param name="report">状态报告</param>
    Task ReportStatusAsync(string nodeId, CollectorStatusReport report);

    /// <summary>
    /// 断开连接
    /// </summary>
    Task DisconnectAsync();

    /// <summary>
    /// 配置变更通知事件
    /// </summary>
    event EventHandler<ConfigChangeNotification>? ConfigChanged;

    /// <summary>
    /// 连接状态变更事件
    /// </summary>
    event EventHandler<bool>? ConnectionStateChanged;
}
