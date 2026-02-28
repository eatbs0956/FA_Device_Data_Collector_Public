using Admin.Api.Domain.DTOs;
using Admin.Api.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Admin.Api.Services;

/// <summary>
/// 配置变更通知服务实现。
/// 
/// 不依赖 DbContext：调用方在 DbContext 存活期间查好 NodeId 列表，传入本服务。
/// 因此可安全用于 fire-and-forget 场景。
/// </summary>
public class ConfigNotifyService : IConfigNotifyService
{
    private readonly IHubContext<CollectorHub> _hubContext;
    private readonly ILogger<ConfigNotifyService> _logger;

    public ConfigNotifyService(
        IHubContext<CollectorHub> hubContext,
        ILogger<ConfigNotifyService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <inheritdoc/>
    public async Task NotifyNodesAsync(IEnumerable<string?> nodeIds, string entityType, string entityId)
    {
        // 去重，去 null/空
        var distinctNodeIds = nodeIds
            .Where(n => !string.IsNullOrWhiteSpace(n))
            .Distinct()
            .ToList();

        if (distinctNodeIds.Count == 0)
        {
            _logger.LogDebug("没有需要通知的节点，EntityType={EntityType}, EntityId={EntityId}", entityType, entityId);
            return;
        }

        var notification = new ConfigChangeNotification
        {
            ChangeType = ConfigChangeType.ConfigUpdated,
            EntityType = entityType,
            EntityId = entityId,
            ChangedAt = DateTimeOffset.UtcNow,
            Message = $"{entityType} 配置已变更，请重新拉取"
        };

        foreach (var nodeId in distinctNodeIds)
        {
            if (!CollectorHub.IsNodeOnline(nodeId!))
            {
                _logger.LogDebug(
                    "节点 {NodeId} 当前离线，跳过推送（Agent 重连后将通过轮询同步配置）", nodeId);
                continue;
            }

            try
            {
                await _hubContext.NotifyConfigChange(nodeId!, notification);
                _logger.LogInformation(
                    "已推送配置变更通知 → NodeId={NodeId}, EntityType={EntityType}, EntityId={EntityId}",
                    nodeId, entityType, entityId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "推送配置变更通知失败: NodeId={NodeId}", nodeId);
            }
        }
    }
}
