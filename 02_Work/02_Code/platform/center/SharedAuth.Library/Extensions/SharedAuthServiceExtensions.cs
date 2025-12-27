using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using SharedAuth.Authorization;
using SharedAuth.Data;

namespace SharedAuth.Extensions;

/// <summary>
/// 共享授权服务扩展方法
/// </summary>
public static class SharedAuthServiceExtensions
{
    /// <summary>
    /// 添加跨服务按钮权限授权支持
    /// </summary>
    /// <param name="services">服务集合</param>
    /// <param name="authDbConnectionString">Auth数据库连接字符串</param>
    /// <returns>服务集合</returns>
    /// <remarks>
    /// 使用示例：
    /// builder.Services.AddCrossServiceButtonPermission("Host=localhost;Database=devdcp;...");
    /// 
    /// 注册的服务：
    /// 1. Auth数据库上下文（只读，用于权限查询）
    /// 2. 动态按钮权限策略提供器
    /// 3. 跨服务按钮权限授权处理器
    /// </remarks>
    public static IServiceCollection AddCrossServiceButtonPermission(
        this IServiceCollection services,
        string authDbConnectionString)
    {
        // 注册Auth数据库上下文 - 用于权限查询
        services.AddDbContext<AuthDbContext>(options =>
            options.UseNpgsql(authDbConnectionString));

        // 注册Auth数据库上下文工厂
        services.AddSingleton<IAuthDbContextFactory>(provider =>
        {
            var optionsBuilder = new DbContextOptionsBuilder<AuthDbContext>();
            optionsBuilder.UseNpgsql(authDbConnectionString);
            return new AuthDbContextFactory(optionsBuilder.Options);
        });

        // 注册动态按钮权限策略提供器
        services.AddSingleton<IAuthorizationPolicyProvider, DynamicButtonPermissionPolicyProvider>();

        // 注册跨服务按钮权限授权处理器
        services.AddScoped<IAuthorizationHandler, CrossServiceButtonPermissionHandler>();

        return services;
    }
}
