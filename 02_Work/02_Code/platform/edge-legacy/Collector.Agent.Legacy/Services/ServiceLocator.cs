using System;
using Collector.Core.ApiClient;
using Collector.Core.Drivers;
using Collector.Core.Engine;
using Collector.Core.Messaging;
using Collector.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;

namespace Collector.Agent.Legacy.Services
{
    /// <summary>
    /// 服务定位器 - 管理 DI 容器的生命周期
    /// 在 WinForms 应用中提供 DI 支持（类似 edge 项目的 App.Services）
    /// </summary>
    public static class ServiceLocator
    {
        private static IServiceProvider _serviceProvider;
        private static ServiceCollection _services;

        /// <summary>
        /// 全局服务提供者
        /// </summary>
        public static IServiceProvider Services
        {
            get
            {
                if (_serviceProvider == null)
                    throw new InvalidOperationException("ServiceLocator 尚未初始化，请先调用 Initialize()");
                return _serviceProvider;
            }
        }

        /// <summary>
        /// 初始化 DI 容器
        /// </summary>
        /// <param name="localSettings">本地设置（包含网关地址、RabbitMQ 配置等）</param>
        public static void Initialize(LocalSettings localSettings)
        {
            _services = new ServiceCollection();
            ConfigureServices(_services, localSettings);
            _serviceProvider = _services.BuildServiceProvider();
        }

        /// <summary>
        /// 获取服务实例
        /// </summary>
        public static T GetService<T>()
        {
            return Services.GetRequiredService<T>();
        }

        /// <summary>
        /// 尝试获取服务实例（不存在返回 default）
        /// </summary>
        public static T GetServiceOrDefault<T>() where T : class
        {
            return Services.GetService<T>();
        }

        private static void ConfigureServices(IServiceCollection services, LocalSettings localSettings)
        {
            // === 日志 ===
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.SetMinimumLevel(LogLevel.Debug);
                builder.AddNLog();
            });

            // === 配置 ===
            var appSettings = localSettings.ToAppSettings();
            services.AddSingleton(appSettings);
            services.AddSingleton(localSettings);

            // === HTTP 客户端 - 通过网关访问 API ===
            var gatewayUrl = localSettings.ApiGatewayUrl ?? "http://localhost:60620";

            services.AddHttpClient<IAuthApiClient, AuthApiClient>(client =>
            {
                client.BaseAddress = new Uri(gatewayUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            services.AddHttpClient<IAdminApiClient, AdminApiClient>(client =>
            {
                client.BaseAddress = new Uri(gatewayUrl);
                client.Timeout = TimeSpan.FromSeconds(30);
            });

            // === 核心服务 ===
            services.AddSingleton<IDriverFactory, DriverFactory>();
            services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
            services.AddSingleton<ICollectionEngine, CollectionEngine>();
        }

        /// <summary>
        /// 释放 DI 容器
        /// </summary>
        public static void Dispose()
        {
            if (_serviceProvider is IDisposable disposable)
            {
                disposable.Dispose();
            }
            _serviceProvider = null;
        }
    }
}
