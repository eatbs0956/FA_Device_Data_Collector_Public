using System.Text;
using Collector.Core.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RabbitMQ.Client;

namespace Collector.Core.Messaging;

/// <summary>
/// RabbitMQ 消息发布器实现
/// </summary>
public class RabbitMqPublisher : IRabbitMqPublisher
{
    private readonly ILogger<RabbitMqPublisher> _logger;
    private IConnection? _connection;
    private IModel? _channel;
    private string _exchangeName = "devdcp.collection";
    private readonly object _lockObject = new();

    public bool IsConnected => _connection?.IsOpen == true && _channel?.IsOpen == true;

    public event EventHandler<bool>? ConnectionStateChanged;

    public RabbitMqPublisher(ILogger<RabbitMqPublisher> logger)
    {
        _logger = logger;
    }

    public Task<bool> ConnectAsync(AppSettings settings)
    {
        try
        {
            _logger.LogInformation("连接 RabbitMQ: {Host}:{Port}", settings.RabbitMqHost, settings.RabbitMqPort);

            var factory = new ConnectionFactory
            {
                HostName = settings.RabbitMqHost,
                Port = settings.RabbitMqPort,
                UserName = settings.RabbitMqUser,
                Password = settings.RabbitMqPassword,
                AutomaticRecoveryEnabled = true,
                NetworkRecoveryInterval = TimeSpan.FromSeconds(10)
            };

            _exchangeName = settings.RabbitMqExchange;

            _connection = factory.CreateConnection();
            _connection.ConnectionShutdown += OnConnectionShutdown;

            _channel = _connection.CreateModel();
            
            // 声明交换机
            _channel.ExchangeDeclare(
                exchange: _exchangeName,
                type: ExchangeType.Topic,
                durable: true,
                autoDelete: false);

            _logger.LogInformation("RabbitMQ 连接成功");
            ConnectionStateChanged?.Invoke(this, true);
            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "RabbitMQ 连接失败");
            ConnectionStateChanged?.Invoke(this, false);
            return Task.FromResult(false);
        }
    }

    public Task DisconnectAsync()
    {
        try
        {
            _channel?.Close();
            _channel?.Dispose();
            _channel = null;

            _connection?.Close();
            _connection?.Dispose();
            _connection = null;

            _logger.LogInformation("RabbitMQ 已断开连接");
            ConnectionStateChanged?.Invoke(this, false);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "断开 RabbitMQ 连接时发生错误");
        }

        return Task.CompletedTask;
    }

    public Task<bool> PublishAsync(CollectionData data)
    {
        if (!IsConnected)
        {
            _logger.LogWarning("RabbitMQ 未连接，无法发送消息");
            return Task.FromResult(false);
        }

        try
        {
            var json = JsonConvert.SerializeObject(data);
            var body = Encoding.UTF8.GetBytes(json);

            // 路由键格式: collection.{nodeId}.{deviceId}
            var routingKey = $"collection.{data.NodeId}.{data.DeviceCode}";

            lock (_lockObject)
            {
                var properties = _channel!.CreateBasicProperties();
                properties.Persistent = true;
                properties.ContentType = "application/json";
                properties.MessageId = data.MessageId;
                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                _channel.BasicPublish(
                    exchange: _exchangeName,
                    routingKey: routingKey,
                    basicProperties: properties,
                    body: body);
            }

            _logger.LogDebug("发送采集数据: {MessageId}, Device: {DeviceCode}, Points: {Count}",
                data.MessageId, data.DeviceCode, data.DataPoints.Count);

            return Task.FromResult(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "发送消息失败");
            return Task.FromResult(false);
        }
    }

    public async Task<int> PublishBatchAsync(IEnumerable<CollectionData> dataList)
    {
        var successCount = 0;

        foreach (var data in dataList)
        {
            if (await PublishAsync(data))
            {
                successCount++;
            }
        }

        return successCount;
    }

    private void OnConnectionShutdown(object? sender, ShutdownEventArgs e)
    {
        _logger.LogWarning("RabbitMQ 连接断开: {Reason}", e.ReplyText);
        ConnectionStateChanged?.Invoke(this, false);
    }

    public void Dispose()
    {
        DisconnectAsync().GetAwaiter().GetResult();
    }
}
