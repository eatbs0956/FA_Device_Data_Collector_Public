using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Collector.Agent.Localization;
using Collector.Agent.Services;
using Collector.Core.Models;
using Microsoft.Extensions.Logging;

namespace Collector.Agent.ViewModels;

/// <summary>
/// 设置 ViewModel
/// </summary>
public partial class SettingsViewModel : ViewModelBase
{
    private readonly IConfigurationService _configService;
    private readonly ILogger<SettingsViewModel> _logger;

    [ObservableProperty]
    private string _nodeId = string.Empty;

    [ObservableProperty]
    private string _nodeName = string.Empty;

    [ObservableProperty]
    private string _apiGatewayUrl = string.Empty;

    [ObservableProperty]
    private string _rabbitMqHost = string.Empty;

    [ObservableProperty]
    private int _rabbitMqPort;

    [ObservableProperty]
    private string _rabbitMqUser = string.Empty;

    [ObservableProperty]
    private string _rabbitMqPassword = string.Empty;

    [ObservableProperty]
    private int _heartbeatIntervalSeconds;

    [ObservableProperty]
    private string _selectedLanguage = "zh-CN";

    [ObservableProperty]
    private string? _saveMessage;

    public IReadOnlyList<LanguageInfo> SupportedLanguages => LocalizationManager.SupportedLanguages;

    public SettingsViewModel(IConfigurationService configService, ILogger<SettingsViewModel> logger)
    {
        _configService = configService;
        _logger = logger;

        LoadSettings();
    }

    private void LoadSettings()
    {
        var settings = _configService.CurrentSettings;

    NodeId = settings.NodeId;
    NodeName = settings.NodeName;
    ApiGatewayUrl = settings.ApiGatewayUrl;
    RabbitMqHost = settings.RabbitMqHost;
    RabbitMqPort = settings.RabbitMqPort;
    RabbitMqUser = settings.RabbitMqUser;
    RabbitMqPassword = settings.RabbitMqPassword;
    HeartbeatIntervalSeconds = settings.HeartbeatIntervalSeconds;
    SelectedLanguage = string.IsNullOrWhiteSpace(settings.Language) ? "zh-CN" : settings.Language;
    }

    [RelayCommand]
    private void GenerateNodeId()
    {
        NodeId = _configService.GenerateNodeId();
    }

    partial void OnSelectedLanguageChanged(string value)
    {
        LocalizationManager.CurrentLanguage = value;
    }

    [RelayCommand]
    private async Task SaveSettingsAsync()
    {
        SetBusy(true, "正在保存...");
        SaveMessage = null;

        try
        {
            var settings = new AppSettings
            {
                NodeId = NodeId,
                NodeName = NodeName,
                ApiGatewayUrl = ApiGatewayUrl,
                RabbitMqHost = RabbitMqHost,
                RabbitMqPort = RabbitMqPort,
                RabbitMqUser = RabbitMqUser,
                RabbitMqPassword = RabbitMqPassword,
                HeartbeatIntervalSeconds = HeartbeatIntervalSeconds,
                Language = SelectedLanguage
            };

            await _configService.SaveSettingsAsync(settings);
            SaveMessage = LocalizationManager.T("Settings.SaveSuccess");
            _logger.LogInformation("配置已保存");
        }
        catch (Exception ex)
        {
            SaveMessage = LocalizationManager.T("Settings.SaveFailed") + $": {ex.Message}";
            _logger.LogError(ex, "保存配置失败");
        }
        finally
        {
            SetBusy(false);
        }
    }

    [RelayCommand]
    private void ResetSettings()
    {
        LoadSettings();
        SaveMessage = "配置已重置";
    }
}
