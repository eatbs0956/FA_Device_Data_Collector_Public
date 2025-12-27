namespace Admin.Api.Domain.DTOs;

/// <summary>
/// 采集任务查询请求
/// </summary>
public class CollectionTaskQueryRequest
{
    /// <summary>
    /// 当前页码
    /// </summary>
    public int? Current { get; set; } = 1;

    /// <summary>
    /// 每页大小
    /// </summary>
    public int? Size { get; set; } = 20;

    /// <summary>
    /// 任务名称（模糊匹配）
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 任务编码（精确匹配）
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// 任务类型
    /// </summary>
    public string? TaskType { get; set; }

    /// <summary>
    /// 任务状态
    /// </summary>
    public string? Status { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }
}

/// <summary>
/// 采集任务列表响应
/// </summary>
public class CollectionTaskListResponse
{
    /// <summary>
    /// 任务列表
    /// </summary>
    public List<CollectionTaskDto> Records { get; set; } = new();

    /// <summary>
    /// 总数
    /// </summary>
    public int Total { get; set; }

    /// <summary>
    /// 当前页码
    /// </summary>
    public int Current { get; set; }

    /// <summary>
    /// 每页大小
    /// </summary>
    public int Size { get; set; }
}

/// <summary>
/// 采集任务DTO
/// </summary>
public class CollectionTaskDto
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

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
    /// 任务类型
    /// </summary>
    /// <remarks>
    /// Periodic: 周期任务
    /// Scheduled: 定时执行
    /// EventDriven: 事件触发
    /// Hybrid: 混合模式
    /// </remarks>
    public string TaskType { get; set; } = string.Empty;

    /// <summary>
    /// 采集间隔（毫秒）
    /// </summary>
    public int? DefaultInterval { get; set; }

    /// <summary>
    /// Cron表达式
    /// </summary>
    public string? CronExpression { get; set; }

    /// <summary>
    /// 优先级（0-9）
    /// </summary>
    public int Priority { get; set; }

    /// <summary>
    /// 任务状态
    /// </summary>
    /// <remarks>
    /// Draft: 草稿
    /// Active: 运行中
    /// Paused: 已暂停
    /// Stopped: 已停止
    /// </remarks>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 生效开始时间
    /// </summary>
    public DateTimeOffset? EffectiveFrom { get; set; }

    /// <summary>
    /// 生效结束时间
    /// </summary>
    public DateTimeOffset? EffectiveTo { get; set; }

    /// <summary>
    /// 关联设备数量
    /// </summary>
    public int DeviceCount { get; set; }

    /// <summary>
    /// 关联设备ID列表
    /// </summary>
    public List<string> DeviceIds { get; set; } = new();

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// 更新时间
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }
}

/// <summary>
/// 创建采集任务请求
/// </summary>
public class CreateCollectionTaskRequest
{
    /// <summary>
    /// 任务名称（必填）
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 任务编码（可选，唯一）
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// 任务描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 任务类型（必填）
    /// </summary>
    /// <remarks>
    /// Periodic: 周期任务
    /// Scheduled: 定时执行
    /// EventDriven: 事件触发
    /// Hybrid: 混合模式
    /// </remarks>
    public string TaskType { get; set; } = "Periodic";

    /// <summary>
    /// 采集间隔（毫秒）
    /// </summary>
    /// <remarks>
    /// Periodic和Hybrid类型必填，范围: 100-3600000
    /// </remarks>
    public int? DefaultInterval { get; set; }

    /// <summary>
    /// Cron表达式
    /// </summary>
    /// <remarks>
    /// Scheduled类型必填
    /// </remarks>
    public string? CronExpression { get; set; }

    /// <summary>
    /// 优先级（0-9，默认5）
    /// </summary>
    public int Priority { get; set; } = 5;

    /// <summary>
    /// 生效开始时间（可选）
    /// </summary>
    public DateTimeOffset? EffectiveFrom { get; set; }

    /// <summary>
    /// 生效结束时间（可选）
    /// </summary>
    public DateTimeOffset? EffectiveTo { get; set; }

    /// <summary>
    /// 关联设备ID列表
    /// </summary>
    public List<string> DeviceIds { get; set; } = new();
}

/// <summary>
/// 更新采集任务请求
/// </summary>
public class UpdateCollectionTaskRequest
{
    /// <summary>
    /// 任务名称
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// 任务编码
    /// </summary>
    public string? Code { get; set; }

    /// <summary>
    /// 任务描述
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 任务类型
    /// </summary>
    public string? TaskType { get; set; }

    /// <summary>
    /// 采集间隔（毫秒）
    /// </summary>
    public int? DefaultInterval { get; set; }

    /// <summary>
    /// Cron表达式
    /// </summary>
    public string? CronExpression { get; set; }

    /// <summary>
    /// 优先级（0-9）
    /// </summary>
    public int? Priority { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool? IsEnabled { get; set; }

    /// <summary>
    /// 生效开始时间
    /// </summary>
    public DateTimeOffset? EffectiveFrom { get; set; }

    /// <summary>
    /// 生效结束时间
    /// </summary>
    public DateTimeOffset? EffectiveTo { get; set; }

    /// <summary>
    /// 关联设备ID列表
    /// </summary>
    public List<string>? DeviceIds { get; set; }
}

/// <summary>
/// 任务状态变更请求
/// </summary>
public class TaskStatusChangeRequest
{
    /// <summary>
    /// 目标状态
    /// </summary>
    /// <remarks>
    /// Active: 启动
    /// Paused: 暂停
    /// Stopped: 停止
    /// </remarks>
    public string Status { get; set; } = string.Empty;
}
