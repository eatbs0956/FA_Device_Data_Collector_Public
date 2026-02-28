using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Realtime.Models;
using StackExchange.Redis;

namespace Shared.Realtime;

/// <summary>
/// Redis 实时消息发布服务实现
/// </summary>
public class RedisRealtimePublisher : IRealtimePublisher
{
    private readonly ConnectionMultiplexer _redis;
    private readonly ISubscriber _subscriber;
    private readonly ILogger<RedisRealtimePublisher> _logger;

    public RedisRealtimePublisher(
        ConnectionMultiplexer redis,
        ILogger<RedisRealtimePublisher> logger)
    {
        _redis = redis;
        _subscriber = redis.GetSubscriber();
        _logger = logger;
    }

    public async Task PublishDeviceStatusAsync(DeviceStatusMessage message)
    {
        var channel = $"{RealtimeChannels.DeviceStatusPrefix}:{message.Tenant}:{message.Device}";
        var json = JsonSerializer.Serialize(message);
        
        await _subscriber.PublishAsync(RedisChannel.Literal(channel), json);
        _logger.LogDebug("发布设备状态: {Channel}", channel);
    }

    public async Task PublishDeviceDataAsync(DeviceDataMessage message)
    {
        var channel = $"{RealtimeChannels.DeviceDataPrefix}:{message.Tenant}:{message.Device}";
        var json = JsonSerializer.Serialize(message);
        
        await _subscriber.PublishAsync(RedisChannel.Literal(channel), json);
        _logger.LogDebug("发布设备数据: {Channel}, Tags: {TagCount}", channel, message.Tags.Count);
    }

    public async Task PublishAlertAsync(AlertMessage message)
    {
        var channel = RealtimeChannels.GetAlertChannel(message.Tenant);
        var json = JsonSerializer.Serialize(message);
        
        await _subscriber.PublishAsync(RedisChannel.Literal(channel), json);
        _logger.LogDebug("发布告警: {Channel}", channel);
    }

    public async Task PublishTaskStatusAsync(TaskStatusMessage message)
    {
        var channel = RealtimeChannels.GetTaskStatusChannel(message.Tenant);
        var json = JsonSerializer.Serialize(message);
        
        await _subscriber.PublishAsync(RedisChannel.Literal(channel), json);
        _logger.LogDebug("发布任务状态: {Channel}", channel);
    }

    public async Task PublishAsync(string channel, string message)
    {
        await _subscriber.PublishAsync(RedisChannel.Literal(channel), message);
        _logger.LogDebug("发布消息: {Channel}", channel);
    }
}

/// <summary>
/// Redis 实时消息订阅服务实现
/// </summary>
public class RedisRealtimeSubscriber : IRealtimeSubscriber
{
    private readonly ConnectionMultiplexer _redis;
    private readonly ISubscriber _subscriber;
    private readonly ILogger<RedisRealtimeSubscriber> _logger;
    private readonly List<string> _subscribedPatterns = new();

    public RedisRealtimeSubscriber(
        ConnectionMultiplexer redis,
        ILogger<RedisRealtimeSubscriber> logger)
    {
        _redis = redis;
        _subscriber = redis.GetSubscriber();
        _logger = logger;
    }

    public async Task SubscribeDeviceStatusAsync(string tenant, string? device, Action<DeviceStatusMessage> handler)
    {
        var pattern = device == null
            ? $"{RealtimeChannels.DeviceStatusPrefix}:{tenant}:*"
            : $"{RealtimeChannels.DeviceStatusPrefix}:{tenant}:{device}";

        await SubscribeInternalAsync(pattern, (channel, message) =>
        {
            try
            {
                var statusMessage = JsonSerializer.Deserialize<DeviceStatusMessage>(message);
                if (statusMessage != null)
                {
                    handler(statusMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "解析设备状态消息失败: {Message}", message);
            }
        });
    }

    public async Task SubscribeDeviceDataAsync(string tenant, string? device, Action<DeviceDataMessage> handler)
    {
        var pattern = device == null
            ? $"{RealtimeChannels.DeviceDataPrefix}:{tenant}:*"
            : $"{RealtimeChannels.DeviceDataPrefix}:{tenant}:{device}";

        await SubscribeInternalAsync(pattern, (channel, message) =>
        {
            try
            {
                var dataMessage = JsonSerializer.Deserialize<DeviceDataMessage>(message);
                if (dataMessage != null)
                {
                    handler(dataMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "解析设备数据消息失败: {Message}", message);
            }
        });
    }

    public async Task SubscribeAlertAsync(string tenant, Action<AlertMessage> handler)
    {
        var pattern = RealtimeChannels.GetAlertChannel(tenant);

        await SubscribeInternalAsync(pattern, (channel, message) =>
        {
            try
            {
                var alertMessage = JsonSerializer.Deserialize<AlertMessage>(message);
                if (alertMessage != null)
                {
                    handler(alertMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "解析告警消息失败: {Message}", message);
            }
        });
    }

    public async Task SubscribeTaskStatusAsync(string tenant, Action<TaskStatusMessage> handler)
    {
        var pattern = RealtimeChannels.GetTaskStatusChannel(tenant);

        await SubscribeInternalAsync(pattern, (channel, message) =>
        {
            try
            {
                var taskMessage = JsonSerializer.Deserialize<TaskStatusMessage>(message);
                if (taskMessage != null)
                {
                    handler(taskMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "解析任务状态消息失败: {Message}", message);
            }
        });
    }

    public async Task SubscribeAsync(string pattern, Action<string, string> handler)
    {
        await SubscribeInternalAsync(pattern, handler);
    }

    private async Task SubscribeInternalAsync(string pattern, Action<string, string> handler)
    {
        // 判断是否为模式订阅
        if (pattern.Contains('*'))
        {
            await _subscriber.SubscribeAsync(RedisChannel.Pattern(pattern), (channel, message) =>
            {
                handler(channel.ToString(), message.ToString());
            });
        }
        else
        {
            await _subscriber.SubscribeAsync(RedisChannel.Literal(pattern), (channel, message) =>
            {
                handler(channel.ToString(), message.ToString());
            });
        }

        lock (_subscribedPatterns)
        {
            _subscribedPatterns.Add(pattern);
        }

        _logger.LogDebug("订阅频道: {Pattern}", pattern);
    }

    public async Task UnsubscribeAsync(string pattern)
    {
        if (pattern.Contains('*'))
        {
            await _subscriber.UnsubscribeAsync(RedisChannel.Pattern(pattern));
        }
        else
        {
            await _subscriber.UnsubscribeAsync(RedisChannel.Literal(pattern));
        }

        lock (_subscribedPatterns)
        {
            _subscribedPatterns.Remove(pattern);
        }

        _logger.LogDebug("取消订阅: {Pattern}", pattern);
    }

    public async Task UnsubscribeAllAsync()
    {
        await _subscriber.UnsubscribeAllAsync();

        lock (_subscribedPatterns)
        {
            _subscribedPatterns.Clear();
        }

        _logger.LogDebug("取消所有订阅");
    }

    public void Dispose()
    {
        UnsubscribeAllAsync().GetAwaiter().GetResult();
    }
}
