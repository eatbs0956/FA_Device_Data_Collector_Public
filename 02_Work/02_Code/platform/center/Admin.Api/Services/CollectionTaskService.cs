using Admin.Api.Domain.DTOs;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Entities;

namespace Admin.Api.Services;

/// <summary>
/// 采集任务服务实现
/// </summary>
public class CollectionTaskService : ICollectionTaskService
{
    private readonly DbContext _dbContext;
    private readonly ILogger<CollectionTaskService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    // 有效的任务类型
    private static readonly HashSet<string> ValidTaskTypes = new()
    {
        "Periodic",    // 周期任务
        "Scheduled",   // 定时执行
        "EventDriven", // 事件触发
        "Hybrid"       // 混合模式
    };

    // 有效的任务状态
    private static readonly HashSet<string> ValidStatuses = new()
    {
        "Draft",   // 草稿
        "Active",  // 运行中
        "Paused",  // 已暂停
        "Stopped"  // 已停止
    };

    public CollectionTaskService(
        DbContext dbContext,
        ILogger<CollectionTaskService> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
    }

    /// <summary>
    /// 获取当前用户ID
    /// </summary>
    private Guid? GetCurrentUserId()
    {
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    public async Task<CollectionTaskListResponse> GetTasksAsync(CollectionTaskQueryRequest request)
    {
        var query = _dbContext.Set<CollectionTask>()
            .Where(t => !t.DeletedFlag)
            .AsQueryable();

        // 应用筛选条件
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            query = query.Where(t => EF.Functions.ILike(t.Name, $"%{request.Name}%"));
        }

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            query = query.Where(t => t.Code == request.Code);
        }

        if (!string.IsNullOrWhiteSpace(request.TaskType))
        {
            query = query.Where(t => t.TaskType == request.TaskType);
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(t => t.Status == request.Status);
        }

        if (request.IsEnabled.HasValue)
        {
            query = query.Where(t => t.IsEnabled == request.IsEnabled.Value);
        }

        // 计算总数
        var total = await query.CountAsync();

        // 分页查询
        var pageIndex = (request.Current ?? 1) - 1;
        var pageSize = request.Size ?? 20;

        var tasks = await query
            .OrderByDescending(t => t.CreatedAt)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .Select(t => new CollectionTaskDto
            {
                Id = t.Id.ToString(),
                Name = t.Name,
                Code = t.Code,
                Description = t.Description,
                TaskType = t.TaskType,
                DefaultInterval = t.DefaultInterval,
                CronExpression = t.CronExpression,
                Priority = t.Priority,
                Status = t.Status,
                IsEnabled = t.IsEnabled,
                EffectiveFrom = t.EffectiveFrom,
                EffectiveTo = t.EffectiveTo,
                DeviceCount = t.TaskDevices.Count,
                DeviceIds = t.TaskDevices.Select(td => td.DeviceId.ToString()).ToList(),
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .ToListAsync();

        return new CollectionTaskListResponse
        {
            Records = tasks,
            Total = total,
            Current = request.Current ?? 1,
            Size = pageSize
        };
    }

    public async Task<CollectionTaskDto?> GetTaskByIdAsync(Guid id)
    {
        var task = await _dbContext.Set<CollectionTask>()
            .Where(t => t.Id == id && !t.DeletedFlag)
            .Select(t => new CollectionTaskDto
            {
                Id = t.Id.ToString(),
                Name = t.Name,
                Code = t.Code,
                Description = t.Description,
                TaskType = t.TaskType,
                DefaultInterval = t.DefaultInterval,
                CronExpression = t.CronExpression,
                Priority = t.Priority,
                Status = t.Status,
                IsEnabled = t.IsEnabled,
                EffectiveFrom = t.EffectiveFrom,
                EffectiveTo = t.EffectiveTo,
                DeviceCount = t.TaskDevices.Count,
                DeviceIds = t.TaskDevices.Select(td => td.DeviceId.ToString()).ToList(),
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt
            })
            .FirstOrDefaultAsync();

        return task;
    }

    public async Task<Guid> CreateTaskAsync(CreateCollectionTaskRequest request)
    {
        // 验证任务类型
        if (!ValidTaskTypes.Contains(request.TaskType))
        {
            throw new ArgumentException($"无效的任务类型: {request.TaskType}");
        }

        // 验证必填字段
        ValidateTaskConfig(request.TaskType, request.DefaultInterval, request.CronExpression);

        // 检查名称是否已存在
        var existingTask = await _dbContext.Set<CollectionTask>()
            .FirstOrDefaultAsync(t => t.Name == request.Name && !t.DeletedFlag);

        if (existingTask != null)
        {
            throw new InvalidOperationException($"任务名称 '{request.Name}' 已存在");
        }

        // 检查编码是否已存在（如果提供了编码）
        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var existingCode = await _dbContext.Set<CollectionTask>()
                .FirstOrDefaultAsync(t => t.Code == request.Code && !t.DeletedFlag);

            if (existingCode != null)
            {
                throw new InvalidOperationException($"任务编码 '{request.Code}' 已存在");
            }
        }

        // 创建任务实体
        var task = new CollectionTask
        {
            Name = request.Name,
            Code = request.Code,
            Description = request.Description,
            TaskType = request.TaskType,
            DefaultInterval = request.DefaultInterval,
            CronExpression = request.CronExpression,
            Priority = request.Priority,
            Status = "Draft",
            IsEnabled = true,
            EffectiveFrom = request.EffectiveFrom?.ToUniversalTime(),
            EffectiveTo = request.EffectiveTo?.ToUniversalTime(),
            CreatedBy = GetCurrentUserId()
        };

        _dbContext.Set<CollectionTask>().Add(task);

        // 关联设备
        if (request.DeviceIds?.Any() == true)
        {
            foreach (var deviceIdStr in request.DeviceIds)
            {
                if (Guid.TryParse(deviceIdStr, out var deviceId))
                {
                    var taskDevice = new CollectionTaskDevice
                    {
                        TaskId = task.Id,
                        DeviceId = deviceId
                    };
                    _dbContext.Set<CollectionTaskDevice>().Add(taskDevice);
                }
            }
        }

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("创建采集任务成功: {TaskId}, 名称: {Name}", task.Id, task.Name);

        return task.Id;
    }

    public async Task UpdateTaskAsync(Guid id, UpdateCollectionTaskRequest request)
    {
        var task = await _dbContext.Set<CollectionTask>()
            .Include(t => t.TaskDevices)
            .FirstOrDefaultAsync(t => t.Id == id && !t.DeletedFlag);

        if (task == null)
        {
            throw new KeyNotFoundException($"采集任务不存在: {id}");
        }

        // 更新基本字段
        if (!string.IsNullOrWhiteSpace(request.Name) && request.Name != task.Name)
        {
            // 检查名称是否已存在
            var existingTask = await _dbContext.Set<CollectionTask>()
                .FirstOrDefaultAsync(t => t.Name == request.Name && t.Id != id && !t.DeletedFlag);

            if (existingTask != null)
            {
                throw new InvalidOperationException($"任务名称 '{request.Name}' 已存在");
            }
            task.Name = request.Name;
        }

        if (request.Code != null && request.Code != task.Code)
        {
            if (!string.IsNullOrWhiteSpace(request.Code))
            {
                var existingCode = await _dbContext.Set<CollectionTask>()
                    .FirstOrDefaultAsync(t => t.Code == request.Code && t.Id != id && !t.DeletedFlag);

                if (existingCode != null)
                {
                    throw new InvalidOperationException($"任务编码 '{request.Code}' 已存在");
                }
            }
            task.Code = request.Code;
        }

        if (request.Description != null)
        {
            task.Description = request.Description;
        }

        if (!string.IsNullOrWhiteSpace(request.TaskType))
        {
            if (!ValidTaskTypes.Contains(request.TaskType))
            {
                throw new ArgumentException($"无效的任务类型: {request.TaskType}");
            }
            task.TaskType = request.TaskType;
        }

        if (request.DefaultInterval.HasValue)
        {
            task.DefaultInterval = request.DefaultInterval;
        }

        if (request.CronExpression != null)
        {
            task.CronExpression = request.CronExpression;
        }

        if (request.Priority.HasValue)
        {
            task.Priority = Math.Clamp(request.Priority.Value, 0, 9);
        }

        if (request.IsEnabled.HasValue)
        {
            task.IsEnabled = request.IsEnabled.Value;
        }

        if (request.EffectiveFrom.HasValue)
        {
            task.EffectiveFrom = request.EffectiveFrom.Value.ToUniversalTime();
        }

        if (request.EffectiveTo.HasValue)
        {
            task.EffectiveTo = request.EffectiveTo.Value.ToUniversalTime();
        }

        // 验证配置
        ValidateTaskConfig(task.TaskType, task.DefaultInterval, task.CronExpression);

        // 更新关联设备
        if (request.DeviceIds != null)
        {
            // 移除现有关联
            _dbContext.Set<CollectionTaskDevice>().RemoveRange(task.TaskDevices);

            // 添加新关联
            foreach (var deviceIdStr in request.DeviceIds)
            {
                if (Guid.TryParse(deviceIdStr, out var deviceId))
                {
                    var taskDevice = new CollectionTaskDevice
                    {
                        TaskId = task.Id,
                        DeviceId = deviceId
                    };
                    _dbContext.Set<CollectionTaskDevice>().Add(taskDevice);
                }
            }
        }

        task.UpdatedBy = GetCurrentUserId();
        task.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("更新采集任务成功: {TaskId}", id);
    }

    public async Task DeleteTaskAsync(Guid id)
    {
        var task = await _dbContext.Set<CollectionTask>()
            .FirstOrDefaultAsync(t => t.Id == id && !t.DeletedFlag);

        if (task == null)
        {
            throw new KeyNotFoundException($"采集任务不存在: {id}");
        }

        // 软删除
        task.DeletedFlag = true;
        task.UpdatedBy = GetCurrentUserId();
        task.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("删除采集任务成功: {TaskId}", id);
    }

    public async Task ChangeStatusAsync(Guid id, string targetStatus)
    {
        if (!ValidStatuses.Contains(targetStatus))
        {
            throw new ArgumentException($"无效的状态: {targetStatus}");
        }

        var task = await _dbContext.Set<CollectionTask>()
            .FirstOrDefaultAsync(t => t.Id == id && !t.DeletedFlag);

        if (task == null)
        {
            throw new KeyNotFoundException($"采集任务不存在: {id}");
        }

        // 验证状态转换是否有效
        ValidateStatusTransition(task.Status, targetStatus);

        task.Status = targetStatus;
        task.UpdatedBy = GetCurrentUserId();
        task.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("采集任务状态变更成功: {TaskId}, {OldStatus} -> {NewStatus}", 
            id, task.Status, targetStatus);
    }

    public async Task<List<TaskDeviceOptionDto>> GetAvailableDevicesAsync()
    {
        var devices = await _dbContext.Set<Device>()
            .Where(d => !d.DeletedFlag && d.Enabled)
            .OrderBy(d => d.DeviceName)
            .Select(d => new TaskDeviceOptionDto
            {
                Id = d.Id.ToString(),
                DeviceName = d.DeviceName,
                DeviceId = d.DeviceId,
                ProtocolType = d.ProtocolType,
                ConnectionStatus = d.ConnectionStatus
            })
            .ToListAsync();

        return devices;
    }

    /// <summary>
    /// 验证任务配置
    /// </summary>
    private void ValidateTaskConfig(string taskType, int? defaultInterval, string? cronExpression)
    {
        switch (taskType)
        {
            case "Periodic":
            case "Hybrid":
                if (!defaultInterval.HasValue)
                {
                    throw new ArgumentException($"{taskType} 类型任务必须配置采集间隔");
                }
                if (defaultInterval < 100 || defaultInterval > 3600000)
                {
                    throw new ArgumentException("采集间隔必须在 100ms - 3600000ms 之间");
                }
                break;

            case "Scheduled":
                if (string.IsNullOrWhiteSpace(cronExpression))
                {
                    throw new ArgumentException("Scheduled 类型任务必须配置 Cron 表达式");
                }
                // 可以添加 Cron 表达式格式验证
                break;

            case "EventDriven":
                // 事件触发类型不需要配置间隔
                break;
        }
    }

    /// <summary>
    /// 验证状态转换
    /// </summary>
    private void ValidateStatusTransition(string currentStatus, string targetStatus)
    {
        // 定义有效的状态转换
        var validTransitions = new Dictionary<string, HashSet<string>>
        {
            { "Draft", new HashSet<string> { "Active" } },
            { "Active", new HashSet<string> { "Paused", "Stopped" } },
            { "Paused", new HashSet<string> { "Active", "Stopped" } },
            { "Stopped", new HashSet<string> { "Active" } }
        };

        if (!validTransitions.TryGetValue(currentStatus, out var allowedTargets) || 
            !allowedTargets.Contains(targetStatus))
        {
            throw new InvalidOperationException(
                $"无效的状态转换: {currentStatus} -> {targetStatus}");
        }
    }
}
