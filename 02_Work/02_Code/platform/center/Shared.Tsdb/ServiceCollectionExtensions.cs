using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Shared.Tsdb;

/// <summary>
/// 依赖注入扩展
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// 添加 InfluxDB 服务
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configureOptions">配置选项</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddInfluxDb(
        this IServiceCollection services,
        Action<InfluxDbOptions> configureOptions)
    {
        services.Configure(configureOptions);
        services.AddSingleton<IInfluxDbService, InfluxDbService>();
        return services;
    }

    /// <summary>
    /// 添加 InfluxDB 服务（从配置节读取）
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="configuration">配置</param>
    /// <returns>服务集合</returns>
    public static IServiceCollection AddInfluxDb(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var section = configuration.GetSection(InfluxDbOptions.SectionName);
        services.Configure<InfluxDbOptions>(options =>
        {
            options.Url = section.GetValue<string>("Url") ?? options.Url;
            options.Token = section.GetValue<string>("Token") ?? options.Token;
            options.Org = section.GetValue<string>("Org") ?? options.Org;
            options.DefaultBucket = section.GetValue<string>("DefaultBucket") ?? options.DefaultBucket;
        });
        services.AddSingleton<IInfluxDbService, InfluxDbService>();
        return services;
    }
}
