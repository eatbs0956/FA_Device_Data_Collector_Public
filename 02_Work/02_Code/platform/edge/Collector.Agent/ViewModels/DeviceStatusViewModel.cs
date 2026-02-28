using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Collector.Agent.Services;
using Collector.Core.Engine;
using Collector.Core.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;

namespace Collector.Agent.ViewModels;

/// <summary>
/// 设备状态 ViewModel - 设备列表 + 标签实时值显示
/// </summary>
public partial class DeviceStatusViewModel : ViewModelBase
{
    private readonly ICollectionEngine _collectionEngine;
    private readonly ConfigPullService _configPullService;
    private readonly ILogger<DeviceStatusViewModel> _logger;

    [ObservableProperty]
    private DeviceRuntimeStatus? _selectedDevice;

    [ObservableProperty]
    private string? _connectionTestResult;

    public ObservableCollection<DeviceRuntimeStatus> Devices { get; } = new();

    /// <summary>
    /// 选中设备的标签实时值列表
    /// </summary>
    public ObservableCollection<TagValueDisplay> SelectedDeviceTags { get; } = new();

    public DeviceStatusViewModel(
        ICollectionEngine collectionEngine,
        ConfigPullService configPullService,
        ILogger<DeviceStatusViewModel> logger)
    {
        _collectionEngine = collectionEngine;
        _configPullService = configPullService;
        _logger = logger;

        // 订阅单个设备状态变更
        _collectionEngine.DeviceStatusChanged += (s, status) =>
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() => UpdateDeviceStatus(status));
        };
        
        // 订阅数据采集事件
        _collectionEngine.DataCollected += OnDataCollected;
        
        // 订阅引擎状态变更，在配置加载时刷新设备列表
        _collectionEngine.StateChanged += (s, state) =>
        {
            if (state == EngineState.Configured || state == EngineState.Running)
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() => LoadDevicesFromEngine());
            }
        };

        // 订阅配置重载事件（State 不变时 StateChanged 不触发，但 ConfigLoaded 始终触发）
        _collectionEngine.ConfigLoaded += (s, e) =>
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() => LoadDevicesFromEngine());
        };

        // 初始化设备列表（如果配置已加载则显示）
        LoadDevicesFromEngine();
    }

    /// <summary>
    /// 当选中设备变更时，加载该设备的标签列表
    /// </summary>
    partial void OnSelectedDeviceChanged(DeviceRuntimeStatus? value)
    {
        LoadDeviceTags(value);
    }

    /// <summary>
    /// 刷新按钮：先从服务端拉取最新配置，再刷新 UI
    /// </summary>
    [RelayCommand]
    private async Task RefreshDevicesAsync()
    {
        _logger.LogInformation("刷新按钮：从服务端拉取最新配置");
        await _configPullService.PullAndLoadConfigAsync();
        // LoadConfigAsync 完成后会触发 ConfigLoaded 事件，自动调用 LoadDevicesFromEngine
    }

    /// <summary>
    /// 从引擎内存中读取设备状态到 UI
    /// </summary>
    private void LoadDevicesFromEngine()
    {
        _logger.LogDebug("刷新设备列表，当前设备数: {Count}", _collectionEngine.DeviceStatuses.Count);
        Devices.Clear();
        
        if (_collectionEngine.DeviceStatuses.Count > 0)
        {
            foreach (var status in _collectionEngine.DeviceStatuses)
            {
                Devices.Add(status);
                _logger.LogDebug("添加设备: {DeviceName} (ID: {DeviceId})", status.DeviceName, status.DeviceId);
            }
        }
        else if (_collectionEngine.State == EngineState.Configured || _collectionEngine.State == EngineState.Running)
        {
            _logger.LogWarning("采集引擎已配置但没有设备数据");
        }
    }

    [RelayCommand]
    private async Task TestConnectionAsync()
    {
        if (SelectedDevice == null) return;

        SetBusy(true, "正在测试连接...");
        ConnectionTestResult = null;

        try
        {
            var result = await _collectionEngine.TestDeviceConnectionAsync(SelectedDevice.DeviceId);
            
            ConnectionTestResult = result.Success
                ? $"✅ 连接成功 (响应时间: {result.ResponseTimeMs}ms)"
                : $"❌ 连接失败: {result.ErrorMessage}";
        }
        catch (Exception ex)
        {
            ConnectionTestResult = $"❌ 测试异常: {ex.Message}";
            _logger.LogError(ex, "连接测试失败");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void UpdateDeviceStatus(DeviceRuntimeStatus status)
    {
        var existing = Devices.FirstOrDefault(d => d.DeviceId == status.DeviceId);
        if (existing != null)
        {
            var index = Devices.IndexOf(existing);
            Devices[index] = status;
        }
    }

    /// <summary>
    /// 加载设备的标签配置到标签列表
    /// </summary>
    private void LoadDeviceTags(DeviceRuntimeStatus? device)
    {
        SelectedDeviceTags.Clear();
        if (device == null) return;

        var deviceConfig = _collectionEngine.CurrentConfig?.Devices
            .FirstOrDefault(d => d.Id == device.DeviceId);

        if (deviceConfig == null) return;

        foreach (var tag in deviceConfig.Tags.Where(t => t.Enabled))
        {
            SelectedDeviceTags.Add(new TagValueDisplay
            {
                TagId = tag.TagId,
                TagName = tag.TagName,
                Address = FormatAddress(tag.TagAddress),
                DataType = tag.DataType.ToString(),
                Unit = tag.Unit ?? "",
                Value = "--",
                Quality = "等待采集",
                LastUpdateTime = null
            });
        }
    }

    /// <summary>
    /// 将 TagAddress JSON 字符串转换为人类可读格式
    /// 例如: {"functionCode":"03","address":0,"quantity":5} → FC03 Addr:0 Qty:5
    /// </summary>
    private static string FormatAddress(string? jsonAddress)
    {
        if (string.IsNullOrWhiteSpace(jsonAddress)) return "--";
        try
        {
            var obj = JObject.Parse(jsonAddress);
            var parts = new List<string>();

            // Modbus 功能码
            var fc = obj["functionCode"]?.ToString();
            if (!string.IsNullOrEmpty(fc))
                parts.Add($"FC{fc.PadLeft(2, '0')}");

            // 起始地址
            if (obj["address"] != null)
                parts.Add($"Addr:{obj["address"]}");

            // 寄存器数量
            if (obj["quantity"] != null)
                parts.Add($"Qty:{obj["quantity"]}");

            // SlaveId (如果存在)
            if (obj["slaveId"] != null)
                parts.Add($"Slave:{obj["slaveId"]}");

            return parts.Count > 0 ? string.Join(" ", parts) : jsonAddress;
        }
        catch
        {
            // 解析失败则原样返回
            return jsonAddress;
        }
    }

    /// <summary>
    /// 接收采集数据，更新选中设备的标签实时值
    /// </summary>
    private void OnDataCollected(object? sender, CollectionData data)
    {
        if (SelectedDevice == null || data.DeviceId != SelectedDevice.DeviceId) return;

        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            foreach (var dp in data.DataPoints)
            {
                var tagDisplay = SelectedDeviceTags.FirstOrDefault(t => t.TagId == dp.TagId);
                if (tagDisplay != null)
                {
                    tagDisplay.Value = LogViewModel.FormatTagValue(dp.Value);
                    tagDisplay.Quality = data.Quality.ToString();
                    tagDisplay.LastUpdateTime = data.Timestamp.LocalDateTime;
                }
            }
        });
    }
}

/// <summary>
/// 标签实时值显示模型
/// </summary>
public partial class TagValueDisplay : ObservableObject
{
    public string TagId { get; set; } = string.Empty;
    public string TagName { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;

    [ObservableProperty]
    private string _value = "--";

    [ObservableProperty]
    private string _quality = "等待采集";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(FormattedUpdateTime))]
    private DateTime? _lastUpdateTime;

    public string FormattedUpdateTime => LastUpdateTime?.ToString("HH:mm:ss.fff") ?? "--";
}
