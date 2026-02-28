using System.Collections.Concurrent;
using Collector.Core.Drivers;
using Collector.Core.Messaging;
using Collector.Core.Models;
using Microsoft.Extensions.Logging;

namespace Collector.Core.Engine;

/// <summary>
/// 采集引擎实现
/// </summary>
public class CollectionEngine : ICollectionEngine, IDisposable
{
    private readonly IDriverFactory _driverFactory;
    private readonly IRabbitMqPublisher _publisher;
    private readonly ILogger<CollectionEngine> _logger;
    private readonly AppSettings _appSettings;

    private NodeConfig? _currentConfig;
    private EngineState _state = EngineState.Uninitialized;
    private CancellationTokenSource? _cancellationTokenSource;

    // 设备驱动实例缓存
    private readonly ConcurrentDictionary<Guid, IProtocolDriver> _drivers = new();
    
    // 任务执行器
    private readonly ConcurrentDictionary<Guid, TaskExecutor> _taskExecutors = new();
    
    // 运行时状态
    private readonly ConcurrentDictionary<Guid, TaskRuntimeStatus> _taskStatuses = new();
    private readonly ConcurrentDictionary<Guid, DeviceRuntimeStatus> _deviceStatuses = new();

    public EngineState State
    {
        get => _state;
        private set
        {
            if (_state != value)
            {
                _state = value;
                StateChanged?.Invoke(this, value);
            }
        }
    }

    public NodeConfig? CurrentConfig => _currentConfig;
    public IReadOnlyList<TaskRuntimeStatus> TaskStatuses => _taskStatuses.Values.ToList();
    public IReadOnlyList<DeviceRuntimeStatus> DeviceStatuses => _deviceStatuses.Values.ToList();

    public event EventHandler<EngineState>? StateChanged;
    public event EventHandler? ConfigLoaded;
    public event EventHandler<TaskRuntimeStatus>? TaskStatusChanged;
    public event EventHandler<DeviceRuntimeStatus>? DeviceStatusChanged;
    public event EventHandler<CollectionData>? DataCollected;
    public event EventHandler<EngineErrorEventArgs>? ErrorOccurred;

    public CollectionEngine(
        IDriverFactory driverFactory,
        IRabbitMqPublisher publisher,
        AppSettings appSettings,
        ILogger<CollectionEngine> logger)
    {
        _driverFactory = driverFactory;
        _publisher = publisher;
        _appSettings = appSettings;
        _logger = logger;
    }

    public Task LoadConfigAsync(NodeConfig config)
    {
        _logger.LogInformation("加载节点配置: {NodeId}, Tasks: {TaskCount}, Devices: {DeviceCount}",
            config.Node.NodeId, config.Tasks.Count, config.Devices.Count);

        _currentConfig = config;

        // 清空旧的运行时状态，确保移除已删除的任务/设备
        _taskStatuses.Clear();
        _deviceStatuses.Clear();

        // 初始化任务状态
        foreach (var task in config.Tasks)
        {
            _taskStatuses[task.Id] = new TaskRuntimeStatus
            {
                TaskId = task.Id,
                TaskName = task.Name,
                State = TaskRunState.Stopped
            };
        }

        // 初始化设备状态
        foreach (var device in config.Devices)
        {
            _deviceStatuses[device.Id] = new DeviceRuntimeStatus
            {
                DeviceId = device.Id,
                DeviceName = device.DeviceName,
                State = DeviceConnectionState.Disconnected
            };
        }

        State = EngineState.Configured;

        // 始终触发 ConfigLoaded，即使 State 未变化（重复加载配置时 StateChanged 不会触发）
        ConfigLoaded?.Invoke(this, EventArgs.Empty);

        return Task.CompletedTask;
    }

    public async Task StartAsync()
    {
        if (_currentConfig == null)
        {
            throw new InvalidOperationException("未加载配置，无法启动");
        }

        _logger.LogInformation("启动采集引擎");
        _cancellationTokenSource = new CancellationTokenSource();

        try
        {
            // 连接 RabbitMQ
            await _publisher.ConnectAsync(_appSettings);

            // 初始化设备驱动
            await InitializeDriversAsync();

            // 启动所有启用的任务
            foreach (var task in _currentConfig.Tasks.Where(t => t.IsEnabled))
            {
                await StartTaskInternalAsync(task);
            }

            State = EngineState.Running;
            _logger.LogInformation("采集引擎已启动");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "启动采集引擎失败");
            State = EngineState.Error;
            throw;
        }
    }

    public async Task StopAsync()
    {
        _logger.LogInformation("停止采集引擎");

        _cancellationTokenSource?.Cancel();
        _cancellationTokenSource?.Dispose();
        _cancellationTokenSource = null;

        // 停止所有任务
        foreach (var executor in _taskExecutors.Values)
        {
            await executor.StopAsync();
        }
        _taskExecutors.Clear();

        // 断开所有设备连接
        foreach (var driver in _drivers.Values)
        {
            await driver.DisconnectAsync();
            driver.Dispose();
        }
        _drivers.Clear();

        // 断开 RabbitMQ
        await _publisher.DisconnectAsync();

        State = EngineState.Stopped;
        _logger.LogInformation("采集引擎已停止");
    }

    public Task PauseAsync()
    {
        _logger.LogInformation("暂停采集引擎");

        foreach (var executor in _taskExecutors.Values)
        {
            executor.Pause();
        }

        State = EngineState.Paused;
        return Task.CompletedTask;
    }

    public Task ResumeAsync()
    {
        _logger.LogInformation("恢复采集引擎");

        foreach (var executor in _taskExecutors.Values)
        {
            executor.Resume();
        }

        State = EngineState.Running;
        return Task.CompletedTask;
    }

    public async Task StartTaskAsync(Guid taskId)
    {
        var task = _currentConfig?.Tasks.FirstOrDefault(t => t.Id == taskId);
        if (task == null)
        {
            throw new ArgumentException($"任务不存在: {taskId}");
        }

        await StartTaskInternalAsync(task);
    }

    public async Task StopTaskAsync(Guid taskId)
    {
        if (_taskExecutors.TryRemove(taskId, out var executor))
        {
            await executor.StopAsync();
            UpdateTaskStatus(taskId, TaskRunState.Stopped);
            _logger.LogInformation("任务已停止: {TaskId}", taskId);
        }
    }

    public async Task TriggerCollectionAsync(Guid taskId)
    {
        if (_taskExecutors.TryGetValue(taskId, out var executor))
        {
            await executor.TriggerOnceAsync();
        }
        else
        {
            // 如果任务未运行，创建临时执行一次
            var task = _currentConfig?.Tasks.FirstOrDefault(t => t.Id == taskId);
            if (task != null)
            {
                await ExecuteCollectionAsync(task);
            }
        }
    }

    public async Task<ConnectionTestResult> TestDeviceConnectionAsync(Guid deviceId)
    {
        var device = _currentConfig?.Devices.FirstOrDefault(d => d.Id == deviceId);
        if (device == null)
        {
            return new ConnectionTestResult
            {
                Success = false,
                ErrorMessage = $"设备不存在: {deviceId}"
            };
        }

        return await TestDeviceConnectionAsync(device);
    }

    public async Task<ConnectionTestResult> TestDeviceConnectionAsync(DeviceConfig device)
    {
        try
        {
            var driver = _driverFactory.CreateDriver(device.ProtocolType);
            var result = await driver.TestConnectionAsync(device);
            driver.Dispose();
            return result;
        }
        catch (Exception ex)
        {
            return new ConnectionTestResult
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public async Task<List<TagReadResult>> ReadDeviceTagsAsync(Guid deviceId, IEnumerable<Guid>? tagIds = null)
    {
        var device = _currentConfig?.Devices.FirstOrDefault(d => d.Id == deviceId);
        if (device == null)
        {
            throw new ArgumentException($"设备不存在: {deviceId}");
        }

        return await ReadDeviceTagsAsync(device, tagIds);
    }

    public async Task<List<TagReadResult>> ReadDeviceTagsAsync(DeviceConfig device, IEnumerable<Guid>? tagIds = null)
    {
        var driver = GetOrCreateDriver(device);
        
        if (driver.ConnectionState != DeviceConnectionState.Connected)
        {
            var connected = await driver.ConnectAsync(device);
            if (!connected)
            {
                // 连接失败时，返回所有标签的失败结果，而不是在断开的 socket 上尝试读取
                var failedTags = (tagIds == null ? device.Tags : device.Tags.Where(t => tagIds.Contains(t.Id)));
                return failedTags.Select(t => new TagReadResult
                {
                    TagId = t.TagId,
                    TagName = t.TagName,
                    Success = false,
                    Quality = DataQuality.Bad,
                    ErrorMessage = $"设备连接失败: {driver.LastError ?? "未知错误"}",
                    Timestamp = DateTimeOffset.UtcNow
                }).ToList();
            }
        }

        var tagsToRead = tagIds == null
            ? device.Tags
            : device.Tags.Where(t => tagIds.Contains(t.Id));

        return await driver.ReadTagsAsync(tagsToRead);
    }

    private async Task InitializeDriversAsync()
    {
        if (_currentConfig == null) return;

        foreach (var device in _currentConfig.Devices.Where(d => d.Enabled))
        {
            try
            {
                var driver = GetOrCreateDriver(device);
                var connected = await driver.ConnectAsync(device);
                
                UpdateDeviceStatus(device.Id, connected 
                    ? DeviceConnectionState.Connected 
                    : DeviceConnectionState.Error);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "初始化设备驱动失败: {DeviceName}", device.DeviceName);
                UpdateDeviceStatus(device.Id, DeviceConnectionState.Error, ex.Message);
            }
        }
    }

    private IProtocolDriver GetOrCreateDriver(DeviceConfig device)
    {
        return _drivers.GetOrAdd(device.Id, _ =>
        {
            var driver = _driverFactory.CreateDriver(device.ProtocolType);
            driver.ConnectionStateChanged += (s, state) => UpdateDeviceStatus(device.Id, state);
            driver.ErrorOccurred += (s, e) => UpdateDeviceStatus(device.Id, DeviceConnectionState.Error, e.ErrorMessage);
            return driver;
        });
    }

    private async Task StartTaskInternalAsync(TaskConfig task)
    {
        if (_taskExecutors.ContainsKey(task.Id))
        {
            _logger.LogWarning("任务已在运行: {TaskName}", task.Name);
            return;
        }

        // 确保 CancellationTokenSource 已初始化（单独启动任务时引擎可能未执行 StartAsync）
        _cancellationTokenSource ??= new CancellationTokenSource();

        var executor = new TaskExecutor(task, this, _logger);
        _taskExecutors[task.Id] = executor;

        await executor.StartAsync(_cancellationTokenSource.Token);
        UpdateTaskStatus(task.Id, TaskRunState.Running);
        
        _logger.LogInformation("任务已启动: {TaskName}", task.Name);
    }

    internal async Task ExecuteCollectionAsync(TaskConfig task)
    {
        if (_currentConfig == null) return;

        var timestamp = DateTimeOffset.UtcNow;

        // 获取任务关联的设备
        var devices = _currentConfig.Devices
            .Where(d => task.DeviceIds.Contains(d.Id) && d.Enabled)
            .ToList();

        foreach (var device in devices)
        {
            try
            {
                var driver = GetOrCreateDriver(device);
                
                if (driver.ConnectionState != DeviceConnectionState.Connected)
                {
                    await driver.ConnectAsync(device);
                }

                // 读取数据
                var results = await driver.ReadTagsAsync(device.Tags.Where(t => t.Enabled));

                // 构建采集数据
                var collectionData = new CollectionData
                {
                    NodeId = _appSettings.NodeId,
                    TaskId = task.Id,
                    DeviceId = device.Id,
                    DeviceCode = device.DeviceId,
                    Timestamp = timestamp,
                    Quality = results.All(r => r.Success) ? CollectionQuality.Good : CollectionQuality.Uncertain,
                    DataPoints = results.Select(r => new DataPoint
                    {
                        TagId = r.TagId,
                        TagName = r.TagName,
                        RawValue = r.RawValue,
                        Value = r.Value,
                        Quality = r.Quality,
                        Timestamp = r.Timestamp
                    }).ToList()
                };

                // 发布数据
                await _publisher.PublishAsync(collectionData);

                // 触发事件
                DataCollected?.Invoke(this, collectionData);

                // 更新统计
                UpdateTaskCollectionCount(task.Id);
                UpdateDeviceStatus(device.Id, DeviceConnectionState.Connected);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "采集设备 {DeviceName} 数据失败", device.DeviceName);
                UpdateDeviceStatus(device.Id, DeviceConnectionState.Error, ex.Message);
                UpdateTaskError(task.Id, ex.Message);

                ErrorOccurred?.Invoke(this, new EngineErrorEventArgs
                {
                    Message = $"采集失败: {ex.Message}",
                    Exception = ex,
                    TaskId = task.Id,
                    DeviceId = device.Id
                });
            }
        }
    }

    private void UpdateTaskStatus(Guid taskId, TaskRunState state, string? error = null)
    {
        if (_taskStatuses.TryGetValue(taskId, out var status))
        {
            status.State = state;
            if (error != null) status.LastError = error;
            TaskStatusChanged?.Invoke(this, status);
        }
    }

    private void UpdateTaskCollectionCount(Guid taskId)
    {
        if (_taskStatuses.TryGetValue(taskId, out var status))
        {
            status.TotalCollectionCount++;
            status.LastRunTime = DateTimeOffset.UtcNow;
            TaskStatusChanged?.Invoke(this, status);
        }
    }

    private void UpdateTaskError(Guid taskId, string error)
    {
        if (_taskStatuses.TryGetValue(taskId, out var status))
        {
            status.ErrorCount++;
            status.LastError = error;
            TaskStatusChanged?.Invoke(this, status);
        }
    }

    private void UpdateDeviceStatus(Guid deviceId, DeviceConnectionState state, string? error = null)
    {
        if (_deviceStatuses.TryGetValue(deviceId, out var status))
        {
            // 状态未变化且无新错误时，不触发事件（避免重复日志）
            var stateChanged = status.State != state;
            var hasNewError = error != null;

            status.State = state;
            if (state == DeviceConnectionState.Connected)
            {
                status.LastConnectTime = DateTimeOffset.UtcNow;
            }
            if (hasNewError)
            {
                status.ErrorCount++;
                status.LastError = error;
            }

            // 仅在状态变化或有新错误时触发事件
            if (stateChanged || hasNewError)
            {
                DeviceStatusChanged?.Invoke(this, status);
            }
        }
    }

    public void Dispose()
    {
        StopAsync().GetAwaiter().GetResult();
    }
}

/// <summary>
/// 任务执行器 - 管理单个任务的执行
/// </summary>
internal class TaskExecutor
{
    private readonly TaskConfig _task;
    private readonly CollectionEngine _engine;
    private readonly ILogger _logger;
    private CancellationTokenSource? _cts;
    private Task? _runningTask;
    private bool _isPaused;
    private readonly ManualResetEventSlim _pauseEvent = new(true);

    public TaskExecutor(TaskConfig task, CollectionEngine engine, ILogger logger)
    {
        _task = task;
        _engine = engine;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        _runningTask = RunAsync(_cts.Token);
        return Task.CompletedTask;
    }

    public async Task StopAsync()
    {
        _cts?.Cancel();
        _pauseEvent.Set(); // 确保从暂停状态恢复
        
        if (_runningTask != null)
        {
            try
            {
                await _runningTask;
            }
            catch (OperationCanceledException)
            {
                // 正常取消
            }
        }
    }

    public void Pause()
    {
        _isPaused = true;
        _pauseEvent.Reset();
    }

    public void Resume()
    {
        _isPaused = false;
        _pauseEvent.Set();
    }

    public async Task TriggerOnceAsync()
    {
        await _engine.ExecuteCollectionAsync(_task);
    }

    private async Task RunAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("任务执行器启动: {TaskName}, 类型: {TaskType}, 间隔: {Interval}ms",
            _task.Name, _task.TaskType, _task.DefaultInterval);

        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                // 检查暂停状态
                _pauseEvent.Wait(cancellationToken);

                // 检查生效时间
                var now = DateTimeOffset.UtcNow;
                if (_task.EffectiveFrom.HasValue && now < _task.EffectiveFrom.Value)
                {
                    await Task.Delay(1000, cancellationToken);
                    continue;
                }
                if (_task.EffectiveTo.HasValue && now > _task.EffectiveTo.Value)
                {
                    _logger.LogInformation("任务已过期: {TaskName}", _task.Name);
                    break;
                }

                // 执行采集
                await _engine.ExecuteCollectionAsync(_task);

                // 等待下一个周期
                var interval = _task.DefaultInterval ?? 1000;
                await Task.Delay(interval, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "任务执行异常: {TaskName}", _task.Name);
                await Task.Delay(5000, cancellationToken); // 错误后等待 5 秒重试
            }
        }

        _logger.LogInformation("任务执行器停止: {TaskName}", _task.Name);
    }
}
