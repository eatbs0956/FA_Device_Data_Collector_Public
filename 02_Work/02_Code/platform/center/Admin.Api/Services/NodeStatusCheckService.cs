using Microsoft.EntityFrameworkCore;
using Shared.Domain.Entities;

namespace Admin.Api.Services;

/// <summary>
/// 节点状态检查后台服务
/// 定期检查节点心跳超时，自动更新节点状态为离线
/// </summary>
public class NodeStatusCheckService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<NodeStatusCheckService> _logger;
    private readonly TimeSpan _checkInterval = TimeSpan.FromMinutes(1); // 每分钟检查一次
    private readonly TimeSpan _heartbeatTimeout = TimeSpan.FromMinutes(2); // 心跳超时阈值：2分钟

    public NodeStatusCheckService(
        IServiceProvider serviceProvider,
        ILogger<NodeStatusCheckService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("节点状态检查服务已启动");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await CheckNodeStatusAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "检查节点状态时发生错误");
            }

            // 等待下一次检查
            await Task.Delay(_checkInterval, stoppingToken);
        }

        _logger.LogInformation("节点状态检查服务已停止");
    }

    private async Task CheckNodeStatusAsync(CancellationToken cancellationToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DbContext>();

        var now = DateTimeOffset.UtcNow;
        var timeoutThreshold = now - _heartbeatTimeout;

        // 查找所有状态为在线的节点进行时间检查，离线的节点如果心跳没超时也可以在这里尝试恢复（通常由被动心跳恢复）
        var onlineNodes = await dbContext.Set<EdgeNode>()
            .Where(e => !e.DeletedFlag && e.Status == "Online")
            .ToListAsync(cancellationToken);

        _logger.LogDebug("状态检查服务：当前在线节点数量 {Count}", onlineNodes.Count);

        var offlineCount = 0;

        foreach (var node in onlineNodes)
        {
            // 如果没有心跳记录或心跳超时，将状态更新为离线
            if (!node.LastHeartbeat.HasValue || node.LastHeartbeat.Value < timeoutThreshold)
            {
                var originalHeartbeat = node.LastHeartbeat;
                node.Status = "Offline";
                node.UpdatedAt = now;
                offlineCount++;

                _logger.LogInformation(
                    "节点 {NodeId} ({NodeName}) 心跳超时，状态更新为离线。最后心跳时间: {LastHeartbeat}, 超时阈值: {Threshold}",
                    node.NodeId,
                    node.NodeName,
                    originalHeartbeat?.ToString("yyyy-MM-dd HH:mm:ss.fff zzz") ?? "从未连接",
                    timeoutThreshold.ToString("yyyy-MM-dd HH:mm:ss.fff zzz")
                );
            }
        }

        if (offlineCount > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
            _logger.LogInformation("共有 {Count} 个节点状态更新为离线", offlineCount);
        }
    }
}
