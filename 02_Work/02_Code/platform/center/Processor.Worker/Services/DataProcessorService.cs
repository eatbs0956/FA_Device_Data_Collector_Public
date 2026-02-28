using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Processor.Worker.Models;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Shared.Domain.Data;
using Shared.Domain.Entities;
using Shared.Realtime;
using Shared.Realtime.Models;
using Shared.Tsdb;
using Shared.Tsdb.Models;
using StackExchange.Redis;

namespace Processor.Worker.Services;

/// <summary>
/// 数据处理后台服务
/// 从 RabbitMQ 消费采集数据，写入 InfluxDB 和 Redis，并推送实时数据到前端
/// </summary>
public class DataProcessorService : BackgroundService
{
    private readonly ILogger<DataProcessorService> _logger;
    private readonly RabbitMqOptions _rabbitOptions;
    private readonly RedisOptions _redisOptions;
    private readonly RealtimeOptions _realtimeOptions;
    private readonly IInfluxDbService _influxService;
    private readonly IServiceProvider _serviceProvider;
    
    private IConnection? _rabbitConnection;
    private IModel? _rabbitChannel;
    private ConnectionMultiplexer? _redisConnection;
    
    // 数据批处理缓冲
    private readonly Dictionary<string, DeviceDataBatch> _batchBuffer = new();
    private readonly object _bufferLock = new();
    private DateTime _lastFlushTime = DateTime.UtcNow;
    private const int BatchFlushIntervalMs = 1000; // 1秒刷新一次
    private const int MaxBatchSize = 100; // 最大批量大小

    // 实时推送相关
    private readonly ConcurrentDictionary<string, RealtimeTagConfig> _tagConfigCache = new();
    private readonly ConcurrentDictionary<string, DeviceRealtimeBuffer> _realtimeBuffers = new();
    private DateTime _lastTagConfigRefresh = DateTime.MinValue;

    public DataProcessorService(
        ILogger<DataProcessorService> logger,
        IOptions<RabbitMqOptions> rabbitOptions,
        IOptions<RedisOptions> redisOptions,
        IOptions<RealtimeOptions> realtimeOptions,
        IInfluxDbService influxService,
        IServiceProvider serviceProvider)
    {
        _logger = logger;
        _rabbitOptions = rabbitOptions.Value;
        _redisOptions = redisOptions.Value;
        _realtimeOptions = realtimeOptions.Value;
        _influxService = influxService;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("数据处理服务启动...");

        // 初始化连接
        await InitializeConnectionsAsync(stoppingToken);

        // 加载标签实时配置
        await RefreshTagConfigCacheAsync();

        // 启动批处理定时器
        _ = Task.Run(() => BatchFlushLoopAsync(stoppingToken), stoppingToken);

        // 启动实时推送定时器
        if (_realtimeOptions.Enabled)
        {
            _ = Task.Run(() => RealtimePushLoopAsync(stoppingToken), stoppingToken);
        }

        // 等待取消
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task InitializeConnectionsAsync(CancellationToken stoppingToken)
    {
        // 初始化 RabbitMQ
        await InitializeRabbitMqAsync(stoppingToken);

        // 初始化 Redis
        await InitializeRedisAsync();

        // 检查 InfluxDB 连接
        var influxOk = await _influxService.PingAsync();
        if (influxOk)
        {
            _logger.LogInformation("InfluxDB 连接成功");
            
            // 确保 Bucket 存在
            await _influxService.EnsureBucketExistsAsync(TsdbBuckets.Collected, 30);
            await _influxService.EnsureBucketExistsAsync(TsdbBuckets.Aggregated, 365);
            await _influxService.EnsureBucketExistsAsync(TsdbBuckets.Metrics, 90);
        }
        else
        {
            _logger.LogError("InfluxDB 连接失败");
        }
    }

    private async Task InitializeRabbitMqAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _rabbitOptions.Host,
            Port = _rabbitOptions.Port,
            UserName = _rabbitOptions.Username,
            Password = _rabbitOptions.Password,
            VirtualHost = _rabbitOptions.VirtualHost,
            DispatchConsumersAsync = true
        };

        // 重试连接
        int retryCount = 0;
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _rabbitConnection = factory.CreateConnection();
                _rabbitChannel = _rabbitConnection.CreateModel();

                // 声明 Exchange 和 Queue
                _rabbitChannel.ExchangeDeclare(
                    exchange: _rabbitOptions.DataExchange,
                    type: ExchangeType.Topic,
                    durable: true);

                _rabbitChannel.QueueDeclare(
                    queue: _rabbitOptions.ProcessorQueue,
                    durable: true,
                    exclusive: false,
                    autoDelete: false);

                _rabbitChannel.QueueBind(
                    queue: _rabbitOptions.ProcessorQueue,
                    exchange: _rabbitOptions.DataExchange,
                    routingKey: "#"); // 接收所有消息

                _rabbitChannel.BasicQos(0, _rabbitOptions.PrefetchCount, false);

                // 设置消费者
                var consumer = new AsyncEventingBasicConsumer(_rabbitChannel);
                consumer.Received += OnMessageReceivedAsync;

                _rabbitChannel.BasicConsume(
                    queue: _rabbitOptions.ProcessorQueue,
                    autoAck: false,
                    consumer: consumer);

                _logger.LogInformation("RabbitMQ 连接成功，开始消费消息");
                break;
            }
            catch (Exception ex)
            {
                retryCount++;
                _logger.LogWarning(ex, "RabbitMQ 连接失败，{RetryCount} 秒后重试...", retryCount * 5);
                await Task.Delay(retryCount * 5000, stoppingToken);
            }
        }
    }

    private async Task InitializeRedisAsync()
    {
        try
        {
            _redisConnection = await ConnectionMultiplexer.ConnectAsync(_redisOptions.ConnectionString);
            _logger.LogInformation("Redis 连接成功");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Redis 连接失败");
        }
    }

    private async Task OnMessageReceivedAsync(object sender, BasicDeliverEventArgs ea)
    {
        try
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);
            var message = JsonSerializer.Deserialize<DataPointMessage>(json);

            if (message != null)
            {
                await ProcessDataPointAsync(message);
            }

            _rabbitChannel?.BasicAck(ea.DeliveryTag, false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "处理消息失败");
            // Nack 消息，不重新入队（避免无限循环）
            _rabbitChannel?.BasicNack(ea.DeliveryTag, false, false);
        }
    }

    private async Task ProcessDataPointAsync(DataPointMessage message)
    {
        var deviceId = message.GetDeviceId();
        var tagName = message.GetTagName();
        var batchKey = $"{message.TenantId}:{deviceId}";

        // 添加到批处理缓冲
        lock (_bufferLock)
        {
            if (!_batchBuffer.TryGetValue(batchKey, out var batch))
            {
                batch = new DeviceDataBatch
                {
                    TenantId = message.TenantId,
                    DeviceId = deviceId,
                    Source = message.Source ?? "collector",
                    Timestamp = message.EventTime
                };
                _batchBuffer[batchKey] = batch;
            }

            // 更新时间戳为最新
            if (message.EventTime > batch.Timestamp)
            {
                batch.Timestamp = message.EventTime;
            }

            // 添加字段值
            if (message.Value != null)
            {
                batch.Fields[tagName] = ConvertValue(message.Value);
            }
        }

        // 更新 Redis 缓存（实时数据）
        await UpdateRedisCacheAsync(message.TenantId, deviceId, tagName, message.Value, message.EventTime);

        // 添加到实时推送缓冲区
        AddToRealtimeBuffer(message.TenantId, deviceId, tagName, message.Value, message.EventTime);

        // 检查是否需要立即刷新
        bool shouldFlush;
        lock (_bufferLock)
        {
            shouldFlush = _batchBuffer.Count >= MaxBatchSize;
        }

        if (shouldFlush)
        {
            await FlushBatchAsync();
        }
    }

    private static object ConvertValue(object value)
    {
        // 处理 JsonElement 类型
        if (value is JsonElement jsonElement)
        {
            return jsonElement.ValueKind switch
            {
                JsonValueKind.Number when jsonElement.TryGetInt64(out var longVal) => longVal,
                JsonValueKind.Number when jsonElement.TryGetDouble(out var doubleVal) => doubleVal,
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.String => jsonElement.GetString() ?? "",
                _ => jsonElement.ToString()
            };
        }
        return value;
    }

    private async Task UpdateRedisCacheAsync(string tenantId, string deviceId, string tagName, object? value, DateTime timestamp)
    {
        if (_redisConnection == null || !_redisConnection.IsConnected)
            return;

        try
        {
            var db = _redisConnection.GetDatabase();
            var key = $"device:data:{tenantId}:{deviceId}";
            
            // 使用 Hash 存储设备最新数据
            await db.HashSetAsync(key, new[]
            {
                new HashEntry(tagName, JsonSerializer.Serialize(value)),
                new HashEntry($"{tagName}:time", timestamp.ToString("O"))
            });

            // 设置过期时间（1小时）
            await db.KeyExpireAsync(key, TimeSpan.FromHours(1));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "更新 Redis 缓存失败");
        }
    }

    private async Task BatchFlushLoopAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(BatchFlushIntervalMs, stoppingToken);
            
            if ((DateTime.UtcNow - _lastFlushTime).TotalMilliseconds >= BatchFlushIntervalMs)
            {
                await FlushBatchAsync();
            }
        }
    }

    private async Task FlushBatchAsync()
    {
        List<DeviceDataBatch> batches;
        lock (_bufferLock)
        {
            if (_batchBuffer.Count == 0)
                return;

            batches = _batchBuffer.Values.ToList();
            _batchBuffer.Clear();
            _lastFlushTime = DateTime.UtcNow;
        }

        try
        {
            // 转换为 InfluxDB 数据点
            var dataPoints = batches.Select(batch => new DeviceDataPoint
            {
                Tenant = batch.TenantId,
                Device = batch.DeviceId,
                Source = batch.Source,
                Timestamp = batch.Timestamp,
                Fields = batch.Fields
            }).ToList();

            // 批量写入 InfluxDB
            await _influxService.WriteDeviceDataBatchAsync(dataPoints);

            _logger.LogDebug("批量写入 InfluxDB: {Count} 个设备数据", dataPoints.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量写入 InfluxDB 失败");
        }
    }

    #region 实时推送相关

    /// <summary>
    /// 刷新标签配置缓存
    /// </summary>
    private async Task RefreshTagConfigCacheAsync()
    {
        if ((DateTime.UtcNow - _lastTagConfigRefresh).TotalSeconds < _realtimeOptions.TagConfigCacheSeconds)
            return;

        try
        {
            using var scope = _serviceProvider.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<UnifiedDbContext>();

            // 查询所有启用实时推送的标签
            var tags = await dbContext.Set<TagDefinition>()
                .AsNoTracking()
                .Where(t => t.EnableRealtime && t.Enabled && !t.DeletedFlag)
                .Include(t => t.Device)
                .Select(t => new
                {
                    t.DeviceId,
                    DeviceDbId = t.Device!.DeviceId,
                    DeviceName = t.Device.DeviceName,
                    t.TagId,
                    t.TagName,
                    t.Unit,
                    t.TenantId
                })
                .ToListAsync();

            // 更新缓存
            _tagConfigCache.Clear();
            foreach (var tag in tags)
            {
                var key = $"{tag.TenantId}:{tag.DeviceDbId}:{tag.TagName}";
                _tagConfigCache[key] = new RealtimeTagConfig
                {
                    TenantId = tag.TenantId,
                    DeviceId = tag.DeviceDbId,
                    DeviceName = tag.DeviceName,
                    TagId = tag.TagId,
                    TagName = tag.TagName,
                    Unit = tag.Unit
                };
            }

            _lastTagConfigRefresh = DateTime.UtcNow;
            _logger.LogInformation("刷新实时标签配置缓存: {Count} 个标签", tags.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "刷新标签配置缓存失败");
        }
    }

    /// <summary>
    /// 实时推送循环
    /// </summary>
    private async Task RealtimePushLoopAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("实时推送服务启动，节流间隔: {Interval}ms", _realtimeOptions.ThrottleIntervalMs);

        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(_realtimeOptions.ThrottleIntervalMs, stoppingToken);

            // 定期刷新标签配置
            await RefreshTagConfigCacheAsync();

            // 推送缓冲区中的数据
            await FlushRealtimeBuffersAsync();
        }
    }

    /// <summary>
    /// 将数据添加到实时推送缓冲区
    /// </summary>
    private void AddToRealtimeBuffer(string tenantId, string deviceId, string tagName, object? value, DateTime eventTime)
    {
        if (!_realtimeOptions.Enabled)
            return;

        // 检查标签是否启用实时推送
        var configKey = $"{tenantId}:{deviceId}:{tagName}";
        if (!_tagConfigCache.TryGetValue(configKey, out var tagConfig))
            return;

        // 获取或创建设备缓冲区
        var bufferKey = $"{tenantId}:{deviceId}";
        var buffer = _realtimeBuffers.GetOrAdd(bufferKey, _ => new DeviceRealtimeBuffer
        {
            TenantId = tenantId,
            DeviceId = deviceId,
            DeviceName = tagConfig.DeviceName
        });

        // 添加标签数据
        buffer.Tags[tagName] = new TagDataItem
        {
            TagId = tagConfig.TagId,
            TagName = tagConfig.TagName,
            Value = value,
            Unit = tagConfig.Unit,
            Quality = "Good",
            EventTime = eventTime
        };
        buffer.LastUpdate = DateTime.UtcNow;
    }

    /// <summary>
    /// 刷新实时推送缓冲区
    /// </summary>
    private async Task FlushRealtimeBuffersAsync()
    {
        if (_realtimeBuffers.IsEmpty)
            return;

        using var scope = _serviceProvider.CreateScope();
        var publisher = scope.ServiceProvider.GetRequiredService<IRealtimePublisher>();

        var now = DateTime.UtcNow;
        var buffersToFlush = _realtimeBuffers
            .Where(kv => (now - kv.Value.LastUpdate).TotalMilliseconds >= _realtimeOptions.ThrottleIntervalMs / 2)
            .ToList();

        foreach (var (key, buffer) in buffersToFlush)
        {
            if (buffer.Tags.Count == 0)
                continue;

            try
            {
                // 构建消息
                var message = new DeviceDataMessage
                {
                    Tenant = buffer.TenantId,
                    Device = buffer.DeviceId,
                    DeviceName = buffer.DeviceName,
                    Tags = buffer.Tags.Values.ToList(),
                    Timestamp = now
                };

                // 发布到 Redis
                await publisher.PublishDeviceDataAsync(message);

                _logger.LogDebug("推送设备实时数据: {Device}, Tags: {Count}", buffer.DeviceId, message.Tags.Count);

                // 清空已推送的数据
                buffer.Tags.Clear();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "推送设备实时数据失败: {Device}", buffer.DeviceId);
            }
        }
    }

    #endregion

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("数据处理服务停止中...");

        // 刷新剩余数据
        await FlushBatchAsync();

        // 关闭连接
        _rabbitChannel?.Close();
        _rabbitConnection?.Close();
        _redisConnection?.Close();
        _influxService.Dispose();

        await base.StopAsync(cancellationToken);
    }
}

/// <summary>
/// 实时标签配置缓存项
/// </summary>
internal class RealtimeTagConfig
{
    public string TenantId { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string TagId { get; set; } = string.Empty;
    public string TagName { get; set; } = string.Empty;
    public string? Unit { get; set; }
}

/// <summary>
/// 设备实时推送缓冲区
/// </summary>
internal class DeviceRealtimeBuffer
{
    public string TenantId { get; set; } = string.Empty;
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public ConcurrentDictionary<string, TagDataItem> Tags { get; set; } = new();
    public DateTime LastUpdate { get; set; } = DateTime.UtcNow;
}
