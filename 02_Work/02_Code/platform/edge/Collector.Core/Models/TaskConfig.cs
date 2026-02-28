namespace Collector.Core.Models;

/// <summary>
/// 采集任务配置
/// </summary>
public class TaskConfig
{
    /// <summary>
    /// 任务ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 任务名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 任务编码
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// 任务描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 任务类型 - Periodic, Scheduled, EventDriven, Hybrid
    /// </summary>
    public string TaskType { get; set; } = "Periodic";

    /// <summary>
    /// 采集间隔（毫秒）
    /// </summary>
    public int? DefaultInterval { get; set; }

    /// <summary>
    /// Cron 表达式
    /// </summary>
    public string? CronExpression { get; set; }

    /// <summary>
    /// 优先级 (0-9)
    /// </summary>
    public int Priority { get; set; } = 5;

    /// <summary>
    /// 任务状态 - Draft, Active, Paused, Stopped
    /// </summary>
    public string Status { get; set; } = "Draft";

    /// <summary>
    /// 启用状态
    /// </summary>
    public bool IsEnabled { get; set; } = true;

    /// <summary>
    /// 关联的设备ID列表
    /// </summary>
    public List<Guid> DeviceIds { get; set; } = new();

    /// <summary>
    /// 生效开始时间
    /// </summary>
    public DateTimeOffset? EffectiveFrom { get; set; }

    /// <summary>
    /// 生效结束时间
    /// </summary>
    public DateTimeOffset? EffectiveTo { get; set; }
}

/// <summary>
/// 任务运行时状态
/// </summary>
public class TaskRuntimeStatus : System.ComponentModel.INotifyPropertyChanged
{
    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));

    public Guid TaskId { get; set; }
    public string TaskName { get; set; } = string.Empty;

    private TaskRunState _state = TaskRunState.Stopped;
    public TaskRunState State
    {
        get => _state;
        set { if (_state != value) { _state = value; OnPropertyChanged(); } }
    }

    private DateTimeOffset? _lastRunTime;
    public DateTimeOffset? LastRunTime
    {
        get => _lastRunTime;
        set { if (_lastRunTime != value) { _lastRunTime = value; OnPropertyChanged(); } }
    }

    private DateTimeOffset? _nextRunTime;
    public DateTimeOffset? NextRunTime
    {
        get => _nextRunTime;
        set { if (_nextRunTime != value) { _nextRunTime = value; OnPropertyChanged(); } }
    }

    private long _totalCollectionCount;
    public long TotalCollectionCount
    {
        get => _totalCollectionCount;
        set { if (_totalCollectionCount != value) { _totalCollectionCount = value; OnPropertyChanged(); } }
    }

    private long _errorCount;
    public long ErrorCount
    {
        get => _errorCount;
        set { if (_errorCount != value) { _errorCount = value; OnPropertyChanged(); } }
    }

    private string? _lastError;
    public string? LastError
    {
        get => _lastError;
        set { if (_lastError != value) { _lastError = value; OnPropertyChanged(); } }
    }
}

/// <summary>
/// 任务运行状态枚举
/// </summary>
public enum TaskRunState
{
    Stopped,
    Running,
    Paused,
    Error
}
