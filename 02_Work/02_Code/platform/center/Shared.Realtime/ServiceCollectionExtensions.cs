using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;

namespace Shared.Realtime;

/// <summary>
/// 依赖注入扩展
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加实时消息服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="connectionString">Redis 连接字符串</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddRealtime(
        this IServiceCollection services,
        string connectionString)
    {
        // 添加 Redis 连接
        services.AddSingleton<ConnectionMultiplexer>(sp =>
            ConnectionMultiplexer.Connect(connectionString));

        // 添加发布服务
        services.AddSingleton<IRealtimePublisher, RedisRealtimePublisher>();

        // 添加订阅服务（Scoped，每个连接一个实例）
        services.AddScoped<IRealtimeSubscriber, RedisRealtimeSubscriber>();

        return services;
    }

    /// <summary>
    /// 添加实时消息服务（从配置节读取）
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddRealtime(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetSection(RealtimeRedisOptions.SectionName)
            .GetValue<string>("ConnectionString") ?? "localhost:6379";

        return services.AddRealtime(connectionString);
    }
}
