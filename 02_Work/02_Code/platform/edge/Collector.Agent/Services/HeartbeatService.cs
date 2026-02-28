using System.Diagnostics;
using Collector.Core.Engine;
using Collector.Core.Models;
using Microsoft.Extensions.Logging;

namespace Collector.Agent.Services;

/// <summary>
/// 心跳上报服务 - 定期向服务端汇报节点状态和资源使用情况
/// </summary>
public class HeartbeatService : IAsyncDisposable
{
    private readonly INotificationService _notificationService;
    private readonly ICollectionEngine _engine;
    private readonly AppSettings _settings;
    private readonly ILogger<HeartbeatService> _logger;
    private readonly CancellationTokenSource _cts = new();
    private Task? _timerTask;

    public HeartbeatService(
        INotificationService notificationService,
        ICollectionEngine engine,
        AppSettings settings,
        ILogger<HeartbeatService> logger)
    {
        _notificationService = notificationService;
        _engine = engine;
        _settings = settings;
        _logger = logger;
    }

    public void Start()
    {
        if (_timerTask != null) return;

        _logger.LogInformation("启动心跳服务，间隔: {Interval}s", _settings.HeartbeatIntervalSeconds);
        _timerTask = RunHeartbeatLoopAsync(_cts.Token);
    }

    public void Stop()
    {
        _cts.Cancel();
    }

    private async Task RunHeartbeatLoopAsync(CancellationToken cancellationToken)
    {
        var process = Process.GetCurrentProcess();
        
        while (!cancellationToken.IsCancellationRequested)
        {
            try
            {
                if (_notificationService.IsConnected)
                {
                    // 构建设备状态列表
                    var deviceStatuses = new List<DeviceStatusItem>();
                    var config = _engine.CurrentConfig;
                    if (config != null)
                    {
                        var deviceConfigMap = config.Devices.ToDictionary(d => d.Id, d => d);
                        foreach (var ds in _engine.DeviceStatuses)
                        {
                            if (deviceConfigMap.TryGetValue(ds.DeviceId, out var deviceConfig))
                            {
                                deviceStatuses.Add(new DeviceStatusItem
                                {
                                    DeviceId = deviceConfig.DeviceId, // 使用字符串标识符
                                    ConnectionStatus = ds.State.ToString(),
                                    ErrorCount = ds.ErrorCount,
                                    LastError = ds.LastError
                                });
                            }
                        }
                    }

                    // 构建任务状态列表
                    var taskStatuses = new List<TaskStatusItem>();
                    if (config != null)
                    {
                        var taskConfigMap = config.Tasks.ToDictionary(t => t.Id, t => t);
                        foreach (var ts in _engine.TaskStatuses)
                        {
                            if (taskConfigMap.TryGetValue(ts.TaskId, out var taskConfig) && !string.IsNullOrEmpty(taskConfig.Code))
                            {
                                taskStatuses.Add(new TaskStatusItem
                                {
                                    TaskCode = taskConfig.Code,
                                    Status = ts.State.ToString(), // Running, Stopped, Paused, Error
                                    TotalCollectionCount = ts.TotalCollectionCount,
                                    ErrorCount = ts.ErrorCount,
                                    LastError = ts.LastError
                                });
                            }
                        }
                    }

                    var report = new CollectorStatusReport
                    {
                        Status = _engine.State.ToString(),
                        RunningTaskCount = _engine.TaskStatuses.Count(t => t.State == TaskRunState.Running),
                        CpuUsage = await GetCpuUsageAsync(),
                        MemoryUsageMb = process.WorkingSet64 / 1024.0 / 1024.0,
                        DataPointsProcessed = _engine.TaskStatuses.Sum(t => t.TotalCollectionCount),
                        LastCollectionTime = DateTimeOffset.UtcNow,
                        DeviceStatuses = deviceStatuses.Count > 0 ? deviceStatuses : null,
                        TaskStatuses = taskStatuses.Count > 0 ? taskStatuses : null
                    };

                    await _notificationService.ReportStatusAsync(_settings.NodeId, report);
                }
                else
                {
                    _logger.LogTrace("心跳跳过：SignalR 未连接");
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "心跳上报异常");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(_settings.HeartbeatIntervalSeconds), cancellationToken);
            }
            catch (TaskCanceledException)
            {
                break;
            }
        }
    }

    private async Task<double> GetCpuUsageAsync()
    {
        // 跨平台简单实现：两次采样计算差值
        try
        {
            var process = Process.GetCurrentProcess();
            var startCpuTime = process.TotalProcessorTime;
            var startTime = DateTime.UtcNow;

            await Task.Delay(500); // 采样 500ms

            var endCpuTime = process.TotalProcessorTime;
            var endTime = DateTime.UtcNow;

            var cpuUsedMs = (endCpuTime - startCpuTime).TotalMilliseconds;
            var totalMs = (endTime - startTime).TotalMilliseconds * Environment.ProcessorCount;

            return Math.Round(cpuUsedMs / totalMs * 100, 2);
        }
        catch
        {
            return 0;
        }
    }

    public async ValueTask DisposeAsync()
    {
        Stop();
        if (_timerTask != null)
        {
            try { await _timerTask; } catch { }
        }
        _cts.Dispose();
    }
}
