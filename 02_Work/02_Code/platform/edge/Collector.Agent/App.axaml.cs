using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Collector.Agent.Services;
using Collector.Agent.ViewModels;
using Collector.Agent.Views;
using Collector.Core.ApiClient;
using Collector.Core.Drivers;
using Collector.Core.Engine;
using Collector.Core.Messaging;
using Collector.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Serilog;

namespace Collector.Agent;

public partial class App : Application
{
    public static IServiceProvider? Services { get; private set; }
    public static AppSettings AppSettings { get; private set; } = new();

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // 加载配置
        LoadAppSettings();

        // 配置依赖注入
        var services = new ServiceCollection();
        ConfigureServices(services);
        Services = services.BuildServiceProvider();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetRequiredService<MainWindowViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void LoadAppSettings()
    {
        try
        {
            var configPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
            
            // 用户运行时配置文件（不会被编译覆盖）
            var userConfigPath = Path.Combine(AppContext.BaseDirectory, "usersettings.json");

            // 1. 先加载基础配置（随项目分发的默认值）
            if (File.Exists(configPath))
            {
                var json = File.ReadAllText(configPath);
                AppSettings = JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
            }

            // 2. 再加载用户运行时配置（覆盖基础配置中的同名字段）
            if (File.Exists(userConfigPath))
            {
                var userJson = File.ReadAllText(userConfigPath);
                // 使用 PopulateObject 仅覆盖用户配置中有值的字段
                JsonConvert.PopulateObject(userJson, AppSettings);
                Log.Information("已加载用户运行时配置: {Path}", userConfigPath);
            }

            // 自动生成节点ID（首次/NodeId为空时）
            if (string.IsNullOrWhiteSpace(AppSettings.NodeId))
            {
                // 使用计算机名作为默认 ID 前缀或直接使用计算机名
                var computerName = Environment.MachineName;
                var random = new Random().Next(100, 999);
                AppSettings.NodeId = $"{computerName}-{random}"; 
                Log.Information("首次启动，自动生成节点ID: {NodeId}", AppSettings.NodeId);
            }

            // 采集节点名称默认设为计算机名
            if (string.IsNullOrWhiteSpace(AppSettings.NodeName))
            {
                AppSettings.NodeName = Environment.MachineName;
                Log.Information("首次启动，自动设置节点名称: {NodeName}", AppSettings.NodeName);
            }

            // 保存到用户运行时配置（不影响源目录的 appsettings.json）
            try
            {
                var json = JsonConvert.SerializeObject(AppSettings, Formatting.Indented);
                File.WriteAllText(userConfigPath, json);
            }
            catch (Exception saveEx)
            {
                Log.Warning(saveEx, "保存用户运行时配置失败");
            }

            // 设置语言（如果配置中未设置语言则回退到 zh-CN）
            Localization.LocalizationManager.CurrentLanguage =
                string.IsNullOrWhiteSpace(AppSettings.Language) ? "zh-CN" : AppSettings.Language;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "加载配置文件失败");
        }
    }

    private void ConfigureServices(IServiceCollection services)
    {
        // 日志
        services.AddLogging(builder =>
        {
            builder.ClearProviders();
            builder.AddSerilog(dispose: true);
        });

        // 配置
        services.AddSingleton(AppSettings);

        // HTTP 客户端 - 统一使用网关地址
        // AuthApiClient 必须为 Singleton，确保登录状态（token）在所有注入点共享
        services.AddHttpClient("AuthApiClient", client =>
        {
            client.BaseAddress = new Uri(AppSettings.ApiGatewayUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });
        services.AddSingleton<IAuthApiClient>(sp =>
        {
            var factory = sp.GetRequiredService<IHttpClientFactory>();
            var httpClient = factory.CreateClient("AuthApiClient");
            var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<AuthApiClient>>();
            return new AuthApiClient(httpClient, logger);
        });

        services.AddHttpClient<IAdminApiClient, AdminApiClient>(client =>
        {
            client.BaseAddress = new Uri(AppSettings.ApiGatewayUrl);
            client.Timeout = TimeSpan.FromSeconds(30);
        });

        // 核心服务
        services.AddSingleton<IDriverFactory, DriverFactory>();
        services.AddSingleton<IRabbitMqPublisher, RabbitMqPublisher>();
        services.AddSingleton<ICollectionEngine, CollectionEngine>();

        // 应用服务
        services.AddSingleton<IConfigurationService, ConfigurationService>();
        services.AddSingleton<INotificationService, NotificationService>();
        services.AddSingleton<HeartbeatService>();
        services.AddSingleton<ConfigPullService>();

        // ViewModels
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<LoginViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<TaskListViewModel>();
        services.AddTransient<DeviceStatusViewModel>();
        services.AddTransient<DataPreviewViewModel>();
        services.AddTransient<DiagnosticsViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddTransient<LogViewModel>();
    }
}
