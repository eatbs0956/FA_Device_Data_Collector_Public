using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Collector.Agent.Services;
using Collector.Core.Engine;
using Collector.Core.Models;
using Microsoft.Extensions.Logging;

namespace Collector.Agent.ViewModels;

/// <summary>
/// 仪表板 ViewModel - 统计概览 + 实时数据预览
/// </summary>
public partial class DashboardViewModel : ViewModelBase
{
    private readonly ICollectionEngine _collectionEngine;
    private readonly ConfigPullService _configPullService;
    private readonly ILogger<DashboardViewModel> _logger;
    private const int MaxRecentDataPoints = 20;

    [ObservableProperty]
    private string _engineState = "未初始化";

    [ObservableProperty]
    private int _runningTaskCount;

    [ObservableProperty]
    private int _connectedDeviceCount;

    [ObservableProperty]
    private long _totalCollectionCount;

    [ObservableProperty]
    private long _errorCount;

    [ObservableProperty]
    private string _lastUpdateTime = "-";

    [ObservableProperty]
    private double _taskGridHeight = 100; // 初始高度

    [ObservableProperty]
    private double _deviceGridHeight = 100; // 初始高度

    [ObservableProperty]
    private double _dataGridHeight = 100; // 初始高度

    public ObservableCollection<TaskStatusDisplay> TaskStatuses { get; } = new();
    public ObservableCollection<DeviceStatusDisplay> DeviceStatuses { get; } = new();

    /// <summary>
    /// 最近采集的数据点预览
    /// </summary>
    public ObservableCollection<RecentDataPointDisplay> RecentDataPoints { get; } = new();

    public DashboardViewModel(
        ICollectionEngine collectionEngine,
        ConfigPullService configPullService,
        ILogger<DashboardViewModel> logger)
    {
        _collectionEngine = collectionEngine;
        _configPullService = configPullService;
        _logger = logger;

        // 订阅事件
        _collectionEngine.StateChanged += (s, state) =>
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() => UpdateEngineState(state));
        };
        _collectionEngine.ConfigLoaded += (s, e) =>
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() => LoadDataFromEngine());
        };
        _collectionEngine.TaskStatusChanged += (s, status) =>
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() => UpdateTaskStatus(status));
        };
        _collectionEngine.DeviceStatusChanged += (s, status) =>
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() => UpdateDeviceStatus(status));
        };
        _collectionEngine.DataCollected += OnDataCollected;

        // 监听集合变化，动态调整 DataGrid 高度
        TaskStatuses.CollectionChanged += (s, e) => UpdateTaskGridHeight();
        DeviceStatuses.CollectionChanged += (s, e) => UpdateDeviceGridHeight();
        RecentDataPoints.CollectionChanged += (s, e) => UpdateDataGridHeight();

        // 初始化数据
        LoadDataFromEngine();
    }

    /// <summary>
    /// 刷新按钮：从引擎内存重新读取数据到 UI
    /// </summary>
    [RelayCommand]
    private void RefreshData()
    {
        _logger.LogInformation("刷新仪表板数据");
        LoadDataFromEngine();
    }

    /// <summary>
    /// 从引擎内存中读取数据到 UI
    /// </summary>
    private void LoadDataFromEngine()
    {
        EngineState = _collectionEngine.State.ToString();
        _logger.LogDebug("更新仪表板数据，引擎状态: {State}, 任务数: {TaskCount}, 设备数: {DeviceCount}",
            EngineState, _collectionEngine.TaskStatuses.Count, _collectionEngine.DeviceStatuses.Count);

        TaskStatuses.Clear();
        foreach (var status in _collectionEngine.TaskStatuses)
        {
            TaskStatuses.Add(new TaskStatusDisplay(status));
        }

        DeviceStatuses.Clear();
        foreach (var status in _collectionEngine.DeviceStatuses)
        {
            DeviceStatuses.Add(new DeviceStatusDisplay(status));
        }

        RunningTaskCount = TaskStatuses.Count(t => t.Status.State == TaskRunState.Running);
        ConnectedDeviceCount = DeviceStatuses.Count(d => d.Status.State == DeviceConnectionState.Connected);
        TotalCollectionCount = TaskStatuses.Sum(t => t.Status.TotalCollectionCount);
        ErrorCount = TaskStatuses.Sum(t => t.Status.ErrorCount);
        LastUpdateTime = DateTime.Now.ToString("HH:mm:ss");
        
        _logger.LogInformation("仪表板已更新: 运行任务={RunningCount}, 连接设备={ConnectedCount}",
            RunningTaskCount, ConnectedDeviceCount);
    }

    [RelayCommand]
    private void ClearRecentData()
    {
        RecentDataPoints.Clear();
    }

    /// <summary>
    /// 接收采集数据，更新最近数据点预览
    /// </summary>
    private void OnDataCollected(object? sender, CollectionData data)
    {
        var deviceName = _collectionEngine.CurrentConfig?.Devices
            .FirstOrDefault(d => d.Id == data.DeviceId)?.DeviceName ?? data.DeviceCode;

        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            foreach (var dp in data.DataPoints)
            {
                var display = new RecentDataPointDisplay
                {
                    Timestamp = data.Timestamp.LocalDateTime,
                    DeviceName = deviceName,
                    TagName = dp.TagName,
                    Value = LogViewModel.FormatTagValue(dp.Value),
                    Quality = data.Quality.ToString()
                };

                // 插入到顶部
                RecentDataPoints.Insert(0, display);

                // 限制数量
                while (RecentDataPoints.Count > MaxRecentDataPoints)
                {
                    RecentDataPoints.RemoveAt(RecentDataPoints.Count - 1);
                }
            }

            LastUpdateTime = DateTime.Now.ToString("HH:mm:ss");
        });
    }

    private void UpdateEngineState(Collector.Core.Engine.EngineState state)
    {
        EngineState = state.ToString();
        // 如果是从未初始化变为已配置，或者配置有变更，需要刷新数据
        if (state == Collector.Core.Engine.EngineState.Configured || state == Collector.Core.Engine.EngineState.Running)
        {
            LoadDataFromEngine();
        }
    }

    private void UpdateTaskStatus(TaskRuntimeStatus status)
    {
        var existing = TaskStatuses.FirstOrDefault(t => t.Status.TaskId == status.TaskId);
        if (existing == null)
        {
            // 新增任务：设置闪烁标记
            var display = new TaskStatusDisplay(status) { IsNew = true };
            TaskStatuses.Add(display);
            
            // 1 秒后取消闪烁标记
            _ = Task.Delay(1000).ContinueWith(_ =>
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    display.IsNew = false;
                });
            });
        }
        // TaskRuntimeStatus 已实现 INPC，DataGrid 行内数据会自动更新
        // 此处仅更新顶部统计卡片

        RunningTaskCount = TaskStatuses.Count(t => t.Status.State == TaskRunState.Running);
        TotalCollectionCount = TaskStatuses.Sum(t => t.Status.TotalCollectionCount);
        ErrorCount = TaskStatuses.Sum(t => t.Status.ErrorCount);
    }

    private void UpdateDeviceStatus(DeviceRuntimeStatus status)
    {
        var existing = DeviceStatuses.FirstOrDefault(d => d.Status.DeviceId == status.DeviceId);
        if (existing == null)
        {
            // 新增设备：设置闪烁标记
            var display = new DeviceStatusDisplay(status) { IsNew = true };
            DeviceStatuses.Add(display);
            
            // 1 秒后取消闪烁标记
            _ = Task.Delay(1000).ContinueWith(_ =>
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() =>
                {
                    display.IsNew = false;
                });
            });
        }
        // DeviceRuntimeStatus 已实现 INPC，DataGrid 行内数据会自动更新

        ConnectedDeviceCount = DeviceStatuses.Count(d => d.Status.State == DeviceConnectionState.Connected);
    }

    /// <summary>
    /// 计算任务 DataGrid 的高度：基于行数动态调整，最大 400px
    /// 高度 = Min(行数 × 32 + 表头高度, 400)
    /// </summary>
    private void UpdateTaskGridHeight()
    {
        const int rowHeight = 32;
        const int headerHeight = 40; // DataGrid 表头高度
        const int maxHeight = 400;
        
        var calculatedHeight = TaskStatuses.Count * rowHeight + headerHeight;
        TaskGridHeight = Math.Min(calculatedHeight, maxHeight);
    }

    /// <summary>
    /// 计算设备 DataGrid 的高度：基于行数动态调整，最大 400px
    /// 高度 = Min(行数 × 32 + 表头高度, 400)
    /// </summary>
    private void UpdateDeviceGridHeight()
    {
        const int rowHeight = 32;
        const int headerHeight = 40;
        const int maxHeight = 400;
        
        var calculatedHeight = DeviceStatuses.Count * rowHeight + headerHeight;
        DeviceGridHeight = Math.Min(calculatedHeight, maxHeight);
    }

    /// <summary>
    /// 计算实时数据 DataGrid 的高度：基于行数动态调整，最大 500px
    /// 高度 = Min(行数 × 32 + 表头高度, 500)
    /// </summary>
    private void UpdateDataGridHeight()
    {
        const int rowHeight = 32;
        const int headerHeight = 40;
        const int maxHeight = 500;
        
        var calculatedHeight = RecentDataPoints.Count * rowHeight + headerHeight;
        DataGridHeight = Math.Min(calculatedHeight, maxHeight);
    }
}

/// <summary>
/// 最近数据点显示模型
/// </summary>
public class RecentDataPointDisplay
{
    public DateTime Timestamp { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public string TagName { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Quality { get; set; } = string.Empty;

    public string FormattedTime => Timestamp.ToString("HH:mm:ss.fff");
}

/// <summary>
/// 任务状态显示包装类（支持新行闪烁）
/// </summary>
public partial class TaskStatusDisplay : ObservableObject
{
    public TaskRuntimeStatus Status { get; set; }

    [ObservableProperty]
    private bool _isNew;

    public TaskStatusDisplay(TaskRuntimeStatus status)
    {
        Status = status;
        IsNew = false;
    }

    // 代理属性，便于 XAML 直接绑定
    public Guid TaskId => Status.TaskId;
    public string TaskName => Status.TaskName;
    public TaskRunState State => Status.State;
    public DateTimeOffset? LastRunTime => Status.LastRunTime;
    public long TotalCollectionCount => Status.TotalCollectionCount;
    public long ErrorCount => Status.ErrorCount;
    public string? LastError => Status.LastError;
}

/// <summary>
/// 设备状态显示包装类（支持新行闪烁）
/// </summary>
public partial class DeviceStatusDisplay : ObservableObject
{
    public DeviceRuntimeStatus Status { get; set; }

    [ObservableProperty]
    private bool _isNew;

    public DeviceStatusDisplay(DeviceRuntimeStatus status)
    {
        Status = status;
        IsNew = false;
    }

    // 代理属性，便于 XAML 直接绑定
    public Guid DeviceId => Status.DeviceId;
    public string DeviceName => Status.DeviceName;
    public DeviceConnectionState State => Status.State;
    public DateTimeOffset? LastConnectTime => Status.LastConnectTime;
    public int ErrorCount => Status.ErrorCount;
    public string? LastError => Status.LastError;
}
