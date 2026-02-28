using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Collector.Agent.Services;
using Collector.Core.Engine;
using Collector.Core.Models;
using Microsoft.Extensions.Logging;

namespace Collector.Agent.ViewModels;

/// <summary>
/// 任务列表 ViewModel
/// </summary>
public partial class TaskListViewModel : ViewModelBase
{
    private readonly ICollectionEngine _collectionEngine;
    private readonly ConfigPullService _configPullService;
    private readonly ILogger<TaskListViewModel> _logger;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(StartTaskCommand))]
    [NotifyCanExecuteChangedFor(nameof(StopTaskCommand))]
    [NotifyCanExecuteChangedFor(nameof(TriggerCollectionCommand))]
    private TaskRuntimeStatus? _selectedTask;

    public ObservableCollection<TaskRuntimeStatus> Tasks { get; } = new();

    public TaskListViewModel(
        ICollectionEngine collectionEngine,
        ConfigPullService configPullService,
        ILogger<TaskListViewModel> logger)
    {
        _collectionEngine = collectionEngine;
        _configPullService = configPullService;
        _logger = logger;

        // 订阅单个任务状态变更
        _collectionEngine.TaskStatusChanged += (s, status) => UpdateTaskStatus(status);
        
        // 订阅引擎状态变更，在配置加载时刷新任务列表
        _collectionEngine.StateChanged += (s, state) =>
        {
            if (state == EngineState.Configured || state == EngineState.Running)
            {
                Avalonia.Threading.Dispatcher.UIThread.Post(() => LoadTasksFromEngine());
            }
        };

        // 订阅配置重载事件（State 不变时 StateChanged 不触发，但 ConfigLoaded 始终触发）
        _collectionEngine.ConfigLoaded += (s, e) =>
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() => LoadTasksFromEngine());
        };

        // 初始化任务列表（如果配置已加载则显示）
        LoadTasksFromEngine();
    }

    /// <summary>
    /// 刷新按钮：从引擎内存重新读取任务状态到 UI
    /// </summary>
    [RelayCommand]
    private void RefreshTasks()
    {
        _logger.LogInformation("刷新任务列表");
        LoadTasksFromEngine();
    }

    /// <summary>
    /// 从引擎内存中读取任务状态到 UI
    /// </summary>
    private void LoadTasksFromEngine()
    {
        _logger.LogDebug("刷新任务列表，当前任务数: {Count}", _collectionEngine.TaskStatuses.Count);

        // 记住当前选中任务的 ID，刷新后恢复选中
        var selectedTaskId = SelectedTask?.TaskId;

        Tasks.Clear();
        
        if (_collectionEngine.TaskStatuses.Count > 0)
        {
            foreach (var status in _collectionEngine.TaskStatuses)
            {
                Tasks.Add(status);
                _logger.LogDebug("添加任务: {TaskName} (ID: {TaskId})", status.TaskName, status.TaskId);
            }

            // 恢复选中状态
            if (selectedTaskId.HasValue)
            {
                SelectedTask = Tasks.FirstOrDefault(t => t.TaskId == selectedTaskId.Value);
            }
        }
        else if (_collectionEngine.State == EngineState.Configured || _collectionEngine.State == EngineState.Running)
        {
            _logger.LogWarning("采集引擎已配置但没有任务数据");
        }
    }

    private bool CanExecuteTaskAction() => SelectedTask != null;

    [RelayCommand(CanExecute = nameof(CanExecuteTaskAction))]
    private async Task StartTaskAsync()
    {
        var task = SelectedTask;
        if (task == null) return;

        try
        {
            await _collectionEngine.StartTaskAsync(task.TaskId);
            _logger.LogInformation("任务已启动: {TaskName}", task.TaskName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "启动任务失败: {TaskName}", task.TaskName);
        }
    }

    [RelayCommand(CanExecute = nameof(CanExecuteTaskAction))]
    private async Task StopTaskAsync()
    {
        var task = SelectedTask;
        if (task == null) return;

        try
        {
            await _collectionEngine.StopTaskAsync(task.TaskId);
            _logger.LogInformation("任务已停止: {TaskName}", task.TaskName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "停止任务失败: {TaskName}", task.TaskName);
        }
    }

    [RelayCommand(CanExecute = nameof(CanExecuteTaskAction))]
    private async Task TriggerCollectionAsync()
    {
        var task = SelectedTask;
        if (task == null) return;

        try
        {
            await _collectionEngine.TriggerCollectionAsync(task.TaskId);
            _logger.LogInformation("手动触发采集: {TaskName}", task.TaskName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "触发采集失败: {TaskName}", task.TaskName);
        }
    }

    private void UpdateTaskStatus(TaskRuntimeStatus status)
    {
        // TaskRuntimeStatus 已实现 INotifyPropertyChanged，
        // 引擎直接修改属性时 DataGrid 绑定会自动刷新。
        // 此处仅处理新增任务的情况（例如运行时动态添加的任务）。
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            var existing = Tasks.FirstOrDefault(t => t.TaskId == status.TaskId);
            if (existing == null)
            {
                Tasks.Add(status);
            }
        });
    }
}
