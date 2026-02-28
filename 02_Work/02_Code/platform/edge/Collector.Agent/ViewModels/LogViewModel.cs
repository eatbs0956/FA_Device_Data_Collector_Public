using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Collector.Agent.Localization;
using Collector.Agent.Services;
using Collector.Core.Engine;
using Collector.Core.Models;
using Microsoft.Extensions.Logging;
using Serilog.Events;

namespace Collector.Agent.ViewModels;

/// <summary>
/// 日志 ViewModel - 实时显示系统全部 Serilog 日志 + 采集引擎事件日志
/// </summary>
public partial class LogViewModel : ViewModelBase
{
    private readonly ICollectionEngine _collectionEngine;
    private readonly ILogger<LogViewModel> _logger;

    // 所有日志（未过滤）
    private readonly List<LogEntry> _allLogs = new();
    private readonly object _logLock = new();

    private const int MaxLogCount = 2000;

    [ObservableProperty]
    private string _selectedLogLevel = "";

    [ObservableProperty]
    private string _searchKeyword = string.Empty;

    [ObservableProperty]
    private bool _autoScroll = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CountSummary))]
    private int _totalCount;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CountSummary))]
    private int _filteredCount;

    /// <summary>
    /// 统计摘要文本（本地化）
    /// </summary>
    public string CountSummary => string.Format(LocalizationManager.T("Logs.ShowCount"), FilteredCount, TotalCount);

    /// <summary>
    /// 过滤后的日志列表（绑定到 DataGrid）
    /// </summary>
    public ObservableCollection<LogEntry> LogEntries { get; } = new();

    /// <summary>
    /// "全部" 级别的本地化文本
    /// </summary>
    private string AllLevelText => LocalizationManager.T("Logs.All");

    /// <summary>
    /// 日志级别选项
    /// </summary>
    public List<string> LogLevels => new() { AllLevelText, "Debug", "Info", "Warning", "Error", "Fatal" };

    /// <summary>
    /// 当日志新增时通知 View 滚动到底部
    /// </summary>
    public event Action? ScrollToBottomRequested;

    public LogViewModel(ICollectionEngine collectionEngine, ILogger<LogViewModel> logger)
    {
        _collectionEngine = collectionEngine;
        _logger = logger;

        // 初始化默认选中级别
        _selectedLogLevel = AllLevelText;

        // 订阅 Serilog 全局日志
        InMemoryLogSink.Instance.LogReceived += OnSerilogEvent;

        // 订阅采集引擎特有事件
        _collectionEngine.StateChanged += OnEngineStateChanged;
        _collectionEngine.DeviceStatusChanged += OnDeviceStatusChanged;
    }

    // ─── Serilog 日志接收 ────────────────────────────────────────────────

    private void OnSerilogEvent(LogEvent logEvent)
    {
        var level = MapLevel(logEvent.Level);
        var message = logEvent.RenderMessage();

        // 过滤 HttpMessageHandler cleanup 等高频噪声日志
        if (level == "Debug" && message.Contains("HttpMessageHandler"))
            return;

        var entry = new LogEntry
        {
            Timestamp = logEvent.Timestamp.LocalDateTime,
            Level = level,
            Source = ExtractSource(logEvent),
            Message = message
        };

        AddLogEntry(entry);
    }

    // ─── 引擎事件 ────────────────────────────────────────────────────────

    private void OnEngineStateChanged(object? sender, EngineState state)
    {
        var stateText = state switch
        {
            EngineState.Uninitialized => LocalizationManager.T("Engine.Uninitialized"),
            EngineState.Configured => LocalizationManager.T("Engine.Configured"),
            EngineState.Running => LocalizationManager.T("Engine.Running"),
            EngineState.Paused => LocalizationManager.T("Engine.Paused"),
            EngineState.Stopped => LocalizationManager.T("Engine.Stopped"),
            EngineState.Error => LocalizationManager.T("Engine.Error"),
            _ => state.ToString()
        };

        AddLogEntry(new LogEntry
        {
            Timestamp = DateTime.Now,
            Level = "Info",
            Source = "引擎",
            Message = $"采集引擎状态变更: {stateText}"
        });
    }

    private void OnDeviceStatusChanged(object? sender, DeviceRuntimeStatus status)
    {
        var stateText = status.State switch
        {
            DeviceConnectionState.Connected => "✅ 已连接",
            DeviceConnectionState.Disconnected => "⚪ 已断开",
            DeviceConnectionState.Connecting => "🔄 连接中",
            DeviceConnectionState.Error => $"❌ 错误: {status.LastError}",
            _ => status.State.ToString()
        };

        AddLogEntry(new LogEntry
        {
            Timestamp = DateTime.Now,
            Level = status.State == DeviceConnectionState.Error ? "Error" : "Info",
            Source = "设备",
            Message = $"[{status.DeviceName}] {stateText}"
        });
    }

    // ─── 核心：添加日志条目 ──────────────────────────────────────────────

    private void AddLogEntry(LogEntry entry)
    {
        lock (_logLock)
        {
            _allLogs.Add(entry);

            // 超过上限时移除最早的
            if (_allLogs.Count > MaxLogCount)
            {
                _allLogs.RemoveRange(0, _allLogs.Count - MaxLogCount);
            }
        }

        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            TotalCount = _allLogs.Count;

            if (MatchesFilter(entry))
            {
                if (LogEntries.Count > MaxLogCount)
                {
                    LogEntries.RemoveAt(0);
                }

                LogEntries.Add(entry);
                FilteredCount = LogEntries.Count;

                if (AutoScroll)
                {
                    ScrollToBottomRequested?.Invoke();
                }
            }
        });
    }

    // ─── 过滤逻辑 ────────────────────────────────────────────────────────

    private bool MatchesFilter(LogEntry entry)
    {
        // 级别过滤
        if (SelectedLogLevel != AllLevelText && entry.Level != SelectedLogLevel)
            return false;

        // 关键字搜索
        if (!string.IsNullOrWhiteSpace(SearchKeyword))
        {
            var kw = SearchKeyword;
            if (!entry.Message.Contains(kw, StringComparison.OrdinalIgnoreCase)
                && !entry.Source.Contains(kw, StringComparison.OrdinalIgnoreCase))
                return false;
        }

        return true;
    }

    /// <summary>
    /// 重新应用过滤条件（级别或关键字变化时调用）
    /// </summary>
    private void ApplyFilter()
    {
        LogEntries.Clear();

        List<LogEntry> snapshot;
        lock (_logLock)
        {
            snapshot = _allLogs.ToList();
        }

        foreach (var entry in snapshot)
        {
            if (MatchesFilter(entry))
            {
                LogEntries.Add(entry);
            }
        }

        FilteredCount = LogEntries.Count;
    }

    partial void OnSelectedLogLevelChanged(string value)
    {
        ApplyFilter();
    }

    partial void OnSearchKeywordChanged(string value)
    {
        ApplyFilter();
    }

    // ─── 命令 ────────────────────────────────────────────────────────────

    [RelayCommand]
    private void ClearLogs()
    {
        lock (_logLock)
        {
            _allLogs.Clear();
        }

        LogEntries.Clear();
        TotalCount = 0;
        FilteredCount = 0;
    }

    // ─── 工具方法 ────────────────────────────────────────────────────────

    /// <summary>
    /// 格式化标签值（供 DashboardViewModel、DeviceStatusViewModel 等外部调用）
    /// </summary>
    public static string FormatTagValue(object? value)
    {
        if (value == null) return "[NULL]";
        if (value is Array array)
        {
            var items = new List<string>();
            for (int i = 0; i < array.Length; i++)
                items.Add(array.GetValue(i)?.ToString() ?? "[NULL]");
            return $"[{string.Join(", ", items)}]";
        }
        return value.ToString() ?? "[NULL]";
    }

    private static string MapLevel(LogEventLevel level) => level switch
    {
        LogEventLevel.Verbose => "Debug",
        LogEventLevel.Debug => "Debug",
        LogEventLevel.Information => "Info",
        LogEventLevel.Warning => "Warning",
        LogEventLevel.Error => "Error",
        LogEventLevel.Fatal => "Fatal",
        _ => "Info"
    };

    private static string ExtractSource(LogEvent logEvent)
    {
        if (logEvent.Properties.TryGetValue("SourceContext", out var prop))
        {
            var full = prop.ToString().Trim('"');
            // 取类名最后一段：Collector.Core.Drivers.ModbusTcpDriver → ModbusTcpDriver
            var lastDot = full.LastIndexOf('.');
            return lastDot >= 0 ? full[(lastDot + 1)..] : full;
        }
        return "System";
    }
}

/// <summary>
/// 日志条目模型
/// </summary>
public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public string Level { get; set; } = string.Empty;
    public string Source { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public string FormattedTime => Timestamp.ToString("HH:mm:ss.fff");

    /// <summary>
    /// 级别对应的图标
    /// </summary>
    public string LevelIcon => Level switch
    {
        "Fatal" => "💀",
        "Error" => "❌",
        "Warning" => "⚠️",
        "Info" => "ℹ️",
        "Debug" => "🔍",
        _ => "📝"
    };
}
