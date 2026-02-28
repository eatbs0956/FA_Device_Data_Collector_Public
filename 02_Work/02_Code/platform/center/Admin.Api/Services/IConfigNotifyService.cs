namespace Admin.Api.Services;

/// <summary>
/// 配置变更通知服务 - 向对应 EdgeNode 的 Collector.Agent 推送 SignalR 通知。
/// 
/// 设计原则：调用方在 DbContext 存活期间查好 NodeId 列表，传入本服务。
/// 本服务不依赖 DbContext，因此可安全用于 fire-and-forget 场景。
/// </summary>
public interface IConfigNotifyService
{
    /// <summary>
    /// 向指定的 NodeId 列表推送配置变更通知。
    /// </summary>
    /// <param name="nodeIds">要通知的 NodeId（SignalR 分组名），可包含 null 值（自动忽略）</param>
    /// <param name="entityType">实体类型，如 "Device" / "Task"</param>
    /// <param name="entityId">变更的实体 ID（用于日志）</param>
    Task NotifyNodesAsync(IEnumerable<string?> nodeIds, string entityType, string entityId);
}
