
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Collector.Agent.Localization;
using Collector.Agent.Services;
using Collector.Core.ApiClient;
using Collector.Core.Engine;
using Collector.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Collector.Agent.ViewModels;

/// <summary>
/// 主窗口 ViewModel
/// </summary>
public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IAuthApiClient _authApiClient;
    private readonly IAdminApiClient _adminApiClient;
    private readonly ICollectionEngine _collectionEngine;
    private readonly IConfigurationService _configService;
    private readonly INotificationService _notificationService;
    private readonly HeartbeatService _heartbeatService;
    private readonly ConfigPullService _configPullService;
    private readonly ILogger<MainWindowViewModel> _logger;

    // 采集运行中收到配置变更通知时置为 true，引擎停止后自动触发拉取
    private volatile bool _configDirty;

    [ObservableProperty]
    private ViewModelBase? _currentView;

    [ObservableProperty]
    private bool _isLoggedIn;

    [ObservableProperty]
    private string _userName = string.Empty;

    [ObservableProperty]
    private string _nodeStatus = "离线";

    [ObservableProperty]
    private string _engineStatus = "未启动";

    [ObservableProperty]
    private bool _hasConfigChangeNotification;

    [ObservableProperty]
    private string? _configChangeMessage;

    [ObservableProperty]
    private int _selectedMenuIndex = -1;

    // 当选中菜单索引变化时自动导航
    partial void OnSelectedMenuIndexChanged(int value)
    {
        if (value >= 0 && value < MenuItems.Count && IsLoggedIn)
        {
            var menuItem = MenuItems[value];
            NavigateToViewInternal(menuItem.Tag);
        }
    }

    public ObservableCollection<MenuItemViewModel> MenuItems { get; } = new();

    // 子 ViewModels
    private LoginViewModel? _loginViewModel;
    private DashboardViewModel? _dashboardViewModel;
    private TaskListViewModel? _taskListViewModel;
    private DeviceStatusViewModel? _deviceStatusViewModel;
    private DataPreviewViewModel? _dataPreviewViewModel;
    private DiagnosticsViewModel? _diagnosticsViewModel;
    private SettingsViewModel? _settingsViewModel;
    private LogViewModel? _logViewModel;

    public MainWindowViewModel(
        IAuthApiClient authApiClient,
        IAdminApiClient adminApiClient,
        ICollectionEngine collectionEngine,
        IConfigurationService configService,
        INotificationService notificationService,
        HeartbeatService heartbeatService,
        ConfigPullService configPullService,
        ILogger<MainWindowViewModel> logger)
    {
        _authApiClient = authApiClient;
        _adminApiClient = adminApiClient;
        _collectionEngine = collectionEngine;
        _configService = configService;
        _notificationService = notificationService;
        _heartbeatService = heartbeatService;
        _configPullService = configPullService;
        _logger = logger;

        // 初始化菜单
        InitializeMenuItems();

        // 订阅事件
        _notificationService.ConfigChanged += OnConfigChanged;
        _collectionEngine.StateChanged += OnEngineStateChanged;

        // 显示登录视图
        ShowLoginView();
    }

    private void InitializeMenuItems()
    {
        MenuItems.Add(new MenuItemViewModel { Title = LocalizationManager.T("Menu.Dashboard"), Icon = "📊", Tag = "Dashboard" });
        MenuItems.Add(new MenuItemViewModel { Title = LocalizationManager.T("Menu.Tasks"), Icon = "📋", Tag = "Tasks" });
        MenuItems.Add(new MenuItemViewModel { Title = LocalizationManager.T("Menu.Devices"), Icon = "🔌", Tag = "Devices" });
        MenuItems.Add(new MenuItemViewModel { Title = LocalizationManager.T("Menu.DataPreview"), Icon = "📈", Tag = "DataPreview" });
        MenuItems.Add(new MenuItemViewModel { Title = LocalizationManager.T("Menu.Diagnostics"), Icon = "🔧", Tag = "Diagnostics" });
        MenuItems.Add(new MenuItemViewModel { Title = LocalizationManager.T("Menu.Logs"), Icon = "📝", Tag = "Logs" });
        MenuItems.Add(new MenuItemViewModel { Title = LocalizationManager.T("Menu.Settings"), Icon = "⚙️", Tag = "Settings" });
    }

    // 本地化属性供 AXAML 绑定
    public string AppSubTitle => LocalizationManager.T("MainWindow.Title").Replace("DevDCP ", "");
    public string LogoutText => LocalizationManager.T("MainWindow.Logout");
    public string PullConfigText => LocalizationManager.T("MainWindow.PullConfig");
    public string StatusPrefix => LocalizationManager.CurrentLanguage == "en-US" ? "Status: " : "状态: ";

    private void ShowLoginView()
    {
        _loginViewModel ??= App.Services!.GetRequiredService<LoginViewModel>();
        _loginViewModel.LoginSucceeded += OnLoginSucceeded;
        CurrentView = _loginViewModel;
    }

    private async void OnLoginSucceeded(object? sender, EventArgs e)
    {
        IsLoggedIn = true;

        // 优先从本地 CurrentUser 取用户名
        UserName = _authApiClient.CurrentUser?.RealName ?? _authApiClient.CurrentUser?.UserName ?? string.Empty;
    
        // 若仍为空，从 JWT token 中解析 unique_name (用户名)
        if (string.IsNullOrWhiteSpace(UserName))
        {
            var token = _authApiClient.CurrentToken;
            if (string.IsNullOrEmpty(token) && sender is LoginViewModel loginVm)
            {
                // fallback: 使用登录时输入的用户名
                UserName = loginVm.UserName;
                _logger.LogDebug("从 LoginViewModel 取用户名: {UserName}", UserName);
            }
            else if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var parts = token.Split('.');
                    if (parts.Length >= 2)
                    {
                        var payload = parts[1];
                        payload = payload.Replace('-', '+').Replace('_', '/');
                        switch (payload.Length % 4)
                        {
                            case 2: payload += "=="; break;
                            case 3: payload += "="; break;
                        }
                        var bytes = Convert.FromBase64String(payload);
                        var jsonStr = System.Text.Encoding.UTF8.GetString(bytes);
                        var claims = Newtonsoft.Json.Linq.JObject.Parse(jsonStr);
                        UserName = claims.Value<string>("unique_name") ?? string.Empty;
                        _logger.LogDebug("从 JWT 解析用户名: {UserName}", UserName);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "从 JWT 解析用户名失败");
                }
            }
        }

        if (string.IsNullOrWhiteSpace(UserName))
        {
            UserName = "用户";
        }

        // 连接通知服务（通过 Gateway.Api 反向代理到 Admin.Api SignalR Hub）
        try
        {
            var settings = _configService.CurrentSettings;
            var hubUrl = $"{settings.ApiGatewayUrl.TrimEnd('/')}/hub/collector";
            var nodeId = settings.NodeId;
            var token = _authApiClient.CurrentToken!;

            // 为管理端接口设置认证令牌
            _adminApiClient.SetToken(token);

            _logger.LogInformation("连接 SignalR Hub: {HubUrl}，NodeId: {NodeId}", hubUrl, nodeId);
            await _notificationService.ConnectAsync(hubUrl, token, nodeId);

            // 成功连接后启动心跳
            _heartbeatService.Start();
            NodeStatus = "在线";

            // 登录成功并建立通知连接后，自动拉取一次最新配置
            _logger.LogInformation("登录成功，主动拉取节点配置: {NodeId}", nodeId);
            _ = PullConfigAsync(); 
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "连接通知服务失败，将使用轮询模式");
            NodeStatus = "离线";
        }

        // 切换到仪表板（设置索引会自动触发导航）
        SelectedMenuIndex = 0;
        // 确保导航生效（防止索引未变化时不触发 OnSelectedMenuIndexChanged）
        NavigateToViewInternal("Dashboard");
    }

    [RelayCommand]
    private void NavigateToView(string viewTag)
    {
        // 通过索引触发导航，避免重复调用
        var menuIndex = MenuItems.ToList().FindIndex(m => m.Tag == viewTag);
        if (menuIndex >= 0 && menuIndex != SelectedMenuIndex)
        {
            SelectedMenuIndex = menuIndex;
        }
        else
        {
            // 索引已经正确，直接导航
            NavigateToViewInternal(viewTag);
        }
    }

    /// <summary>
    /// 内部导航方法，只切换视图不更新索引
    /// </summary>
    private void NavigateToViewInternal(string viewTag)
    {
        CurrentView = viewTag switch
        {
            "Dashboard" => _dashboardViewModel ??= App.Services!.GetRequiredService<DashboardViewModel>(),
            "Tasks" => _taskListViewModel ??= App.Services!.GetRequiredService<TaskListViewModel>(),
            "Devices" => _deviceStatusViewModel ??= App.Services!.GetRequiredService<DeviceStatusViewModel>(),
            "DataPreview" => _dataPreviewViewModel ??= App.Services!.GetRequiredService<DataPreviewViewModel>(),
            "Diagnostics" => _diagnosticsViewModel ??= App.Services!.GetRequiredService<DiagnosticsViewModel>(),
            "Logs" => _logViewModel ??= App.Services!.GetRequiredService<LogViewModel>(),
            "Settings" => _settingsViewModel ??= App.Services!.GetRequiredService<SettingsViewModel>(),
            _ => CurrentView
        };
    }

    [RelayCommand]
    private async Task LogoutAsync()
    {
        _heartbeatService.Stop();
        await _authApiClient.LogoutAsync();
        await _notificationService.DisconnectAsync();
        
        IsLoggedIn = false;
        UserName = string.Empty;
        NodeStatus = "离线";
        
        ShowLoginView();
    }

    [RelayCommand]
    private async Task PullConfigAsync()
    {
        SetBusy(true, "正在拉取配置...");

        try
        {
            var success = await _configPullService.PullAndLoadConfigAsync();
            if (success)
            {
                HasConfigChangeNotification = false;
                ConfigChangeMessage = null;
            }
        }
        finally
        {
            SetBusy(false);
        }
    }

    [RelayCommand]
    private void DismissNotification()
    {
        HasConfigChangeNotification = false;
        ConfigChangeMessage = null;
    }

    private void OnConfigChanged(object? sender, ConfigChangeNotification notification)
    {
        _logger.LogInformation("收到配置变更通知: {ChangeType}", notification.ChangeType);

        if (_collectionEngine.State == EngineState.Running)
        {
            // 采集运行中：标记待更新，停止后自动拉取
            _configDirty = true;
            HasConfigChangeNotification = true;
            ConfigChangeMessage = $"配置已变更（{notification.ChangeType}），将在采集停止后自动更新";
            _logger.LogInformation("采集运行中，配置变更已标记（_configDirty=true），停止后将自动拉取");
        }
        else
        {
            // 引擎未运行：立即拉取
            HasConfigChangeNotification = true;
            ConfigChangeMessage = $"配置已变更（{notification.ChangeType}），正在自动拉取最新配置...";
            _ = PullConfigAsync();
        }
    }

    private void OnEngineStateChanged(object? sender, EngineState state)
    {
        EngineStatus = state switch
        {
            EngineState.Uninitialized => LocalizationManager.T("Engine.Uninitialized"),
            EngineState.Configured => LocalizationManager.T("Engine.Configured"),
            EngineState.Running => LocalizationManager.T("Engine.Running"),
            EngineState.Paused => LocalizationManager.T("Engine.Paused"),
            EngineState.Stopped => LocalizationManager.T("Engine.Stopped"),
            EngineState.Error => LocalizationManager.T("Engine.Error"),
            _ => LocalizationManager.T("Engine.Unknown")
        };

        // 采集停止后，如果有待更新的配置，自动触发拉取
        if (state == EngineState.Stopped && _configDirty)
        {
            _configDirty = false;
            _logger.LogInformation("采集已停止，检测到配置变更标记，自动拉取最新配置");
            ConfigChangeMessage = "采集已停止，正在自动拉取最新配置...";
            _ = PullConfigAsync();
        }
    }
}

/// <summary>
/// 菜单项 ViewModel
/// </summary>
public partial class MenuItemViewModel : ObservableObject
{
    [ObservableProperty]
    private string _title = string.Empty;

    [ObservableProperty]
    private string _icon = string.Empty;

    [ObservableProperty]
    private string _tag = string.Empty;

    [ObservableProperty]
    private bool _isSelected;
}
