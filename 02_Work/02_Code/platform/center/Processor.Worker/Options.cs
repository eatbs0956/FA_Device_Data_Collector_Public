namespace Processor.Worker;

/// <summary>
/// RabbitMQ 配置选项
/// </summary>
public class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    /// <summary>
    /// 主机地址
    /// </summary>
    public string Host { get; set; } = "localhost";

    /// <summary>
    /// 端口
    /// </summary>
    public int Port { get; set; } = 5672;

    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; } = "devdcp";

    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; set; } = "devdcp";

    /// <summary>
    /// 虚拟主机
    /// </summary>
    public string VirtualHost { get; set; } = "/";

    /// <summary>
    /// 采集数据 Exchange
    /// </summary>
    public string DataExchange { get; set; } = "data.collected";

    /// <summary>
    /// 处理器队列
    /// </summary>
    public string ProcessorQueue { get; set; } = "processor.queue";

    /// <summary>
    /// 预取数量
    /// </summary>
    public ushort PrefetchCount { get; set; } = 100;
}

/// <summary>
/// Redis 配置选项
/// </summary>
public class RedisOptions
{
    public const string SectionName = "Redis";

    /// <summary>
    /// 连接字符串
    /// </summary>
    public string ConnectionString { get; set; } = "localhost:6379";
}

/// <summary>
/// 实时推送配置选项
/// </summary>
public class RealtimeOptions
{
    public const string SectionName = "Realtime";

    /// <summary>
    /// 是否启用实时数据推送
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 推送节流间隔（毫秒）- 同一设备的数据至少间隔此时间才推送
    /// </summary>
    public int ThrottleIntervalMs { get; set; } = 500;

    /// <summary>
    /// 标签配置缓存刷新间隔（秒）
    /// </summary>
    public int TagConfigCacheSeconds { get; set; } = 60;
}
