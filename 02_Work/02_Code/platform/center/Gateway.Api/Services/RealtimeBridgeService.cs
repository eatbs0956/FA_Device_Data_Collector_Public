using Microsoft.AspNetCore.SignalR;
using Shared.Realtime;
using Shared.Realtime.Models;
using Gateway.Api.Hubs;

namespace Gateway.Api.Services;

/// <summary>
/// Redis 订阅桥接服务
/// 将 Redis Pub/Sub 消息转发到 SignalR 客户端
/// </summary>
public class RealtimeBridgeService : BackgroundService
{
    private readonly ILogger<RealtimeBridgeService> _logger;
    private readonly IServiceProvider _serviceProvider;
    private readonly IHubContext<RealtimeHub> _hubContext;
    private IRealtimeSubscriber? _subscriber;

    public RealtimeBridgeService(
        ILogger<RealtimeBridgeService> logger,
        IServiceProvider serviceProvider,
        IHubContext<RealtimeHub> hubContext)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
        _hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("实时消息桥接服务启动...");

        // 创建 Scoped 服务
        using var scope = _serviceProvider.CreateScope();
        _subscriber = scope.ServiceProvider.GetRequiredService<IRealtimeSubscriber>();

        // 订阅所有实时消息频道
        await SubscribeToChannelsAsync();

        // 等待取消
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task SubscribeToChannelsAsync()
    {
        // 订阅设备状态变更（所有租户）
        await _subscriber!.SubscribeAsync($"{RealtimeChannels.DeviceStatusPrefix}:*", 
            async (channel, message) =>
            {
                await OnDeviceStatusReceivedAsync(channel, message);
            });

        // 订阅告警通知（所有租户）
        await _subscriber.SubscribeAsync($"{RealtimeChannels.AlertPrefix}:*",
            async (channel, message) =>
            {
                await OnAlertReceivedAsync(channel, message);
            });

        // 订阅任务状态（所有租户）
        await _subscriber.SubscribeAsync($"{RealtimeChannels.TaskStatusPrefix}:*",
            async (channel, message) =>
            {
                await OnTaskStatusReceivedAsync(channel, message);
            });

        // 订阅设备实时数据（所有租户）
        await _subscriber.SubscribeAsync($"{RealtimeChannels.DeviceDataPrefix}:*",
            async (channel, message) =>
            {
                await OnDeviceDataReceivedAsync(channel, message);
            });

        _logger.LogInformation("已订阅所有实时消息频道");
    }

    private async Task OnDeviceStatusReceivedAsync(string channel, string message)
    {
        try
        {
            // 解析 channel: realtime:status:{tenant}:{device}
            var parts = channel.Split(':');
            if (parts.Length < 4) return;

            var tenant = parts[2];
            var device = parts[3];

            // 发送到租户组
            await _hubContext.Clients.Group($"tenant:{tenant}")
                .SendAsync("DeviceStatus", message);

            // 发送到特定设备订阅组
            await _hubContext.Clients.Group($"status:{tenant}")
                .SendAsync("DeviceStatus", message);
            await _hubContext.Clients.Group($"status:{tenant}:{device}")
                .SendAsync("DeviceStatus", message);

            _logger.LogDebug("转发设备状态: {Tenant}/{Device}", tenant, device);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "转发设备状态失败: {Message}", message);
        }
    }

    private async Task OnAlertReceivedAsync(string channel, string message)
    {
        try
        {
            // 解析 channel: realtime:alert:{tenant}
            var parts = channel.Split(':');
            if (parts.Length < 3) return;

            var tenant = parts[2];

            // 发送到租户组
            await _hubContext.Clients.Group($"tenant:{tenant}")
                .SendAsync("Alert", message);

            // 发送到告警订阅组
            await _hubContext.Clients.Group($"alert:{tenant}")
                .SendAsync("Alert", message);

            _logger.LogDebug("转发告警: {Tenant}", tenant);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "转发告警失败: {Message}", message);
        }
    }

    private async Task OnTaskStatusReceivedAsync(string channel, string message)
    {
        try
        {
            // 解析 channel: realtime:task:{tenant}
            var parts = channel.Split(':');
            if (parts.Length < 3) return;

            var tenant = parts[2];

            // 发送到租户组
            await _hubContext.Clients.Group($"tenant:{tenant}")
                .SendAsync("TaskStatus", message);

            // 发送到任务状态订阅组
            await _hubContext.Clients.Group($"task:{tenant}")
                .SendAsync("TaskStatus", message);

            _logger.LogDebug("转发任务状态: {Tenant}", tenant);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "转发任务状态失败: {Message}", message);
        }
    }

    private async Task OnDeviceDataReceivedAsync(string channel, string message)
    {
        try
        {
            // 解析 channel: realtime:data:{tenant}:{device}
            var parts = channel.Split(':');
            if (parts.Length < 4) return;

            var tenant = parts[2];
            var device = parts[3];

            // 发送到租户组（所有该租户的客户端都会收到）
            await _hubContext.Clients.Group($"tenant:{tenant}")
                .SendAsync("DeviceData", message);

            // 发送到特定设备订阅组
            await _hubContext.Clients.Group($"data:{tenant}")
                .SendAsync("DeviceData", message);
            await _hubContext.Clients.Group($"data:{tenant}:{device}")
                .SendAsync("DeviceData", message);

            _logger.LogDebug("转发设备数据: {Tenant}/{Device}", tenant, device);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "转发设备数据失败: {Message}", message);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("实时消息桥接服务停止中...");

        if (_subscriber != null)
        {
            await _subscriber.UnsubscribeAllAsync();
        }

        await base.StopAsync(cancellationToken);
    }
}
