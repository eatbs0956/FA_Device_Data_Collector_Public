using System.Text.Json.Serialization;

namespace Shared.Realtime.Models;

/// <summary>
/// 设备状态变更消息
/// </summary>
public class DeviceStatusMessage
{
    /// <summary>
    /// 消息类型
    /// </summary>
    [JsonPropertyName("type")]
    public string Type => "device_status";

    /// <summary>
    /// 租户ID
    /// </summary>
    [JsonPropertyName("tenant")]
    public string Tenant { get; set; } = string.Empty;

    /// <summary>
    /// 设备ID
    /// </summary>
    [JsonPropertyName("device")]
    public string Device { get; set; } = string.Empty;

    /// <summary>
    /// 状态 (online/offline/error)
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = "offline";

    /// <summary>
    /// 状态消息
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>
    /// 时间戳
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// 告警通知消息
/// </summary>
public class AlertMessage
{
    /// <summary>
    /// 消息类型
    /// </summary>
    [JsonPropertyName("type")]
    public string Type => "alert";

    /// <summary>
    /// 告警ID
    /// </summary>
    [JsonPropertyName("alertId")]
    public string AlertId { get; set; } = string.Empty;

    /// <summary>
    /// 租户ID
    /// </summary>
    [JsonPropertyName("tenant")]
    public string Tenant { get; set; } = string.Empty;

    /// <summary>
    /// 设备ID
    /// </summary>
    [JsonPropertyName("device")]
    public string? Device { get; set; }

    /// <summary>
    /// 告警级别 (info/warning/error/critical)
    /// </summary>
    [JsonPropertyName("level")]
    public string Level { get; set; } = "info";

    /// <summary>
    /// 告警标题
    /// </summary>
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// 告警内容
    /// </summary>
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    /// <summary>
    /// 时间戳
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// 采集任务状态消息
/// </summary>
public class TaskStatusMessage
{
    /// <summary>
    /// 消息类型
    /// </summary>
    [JsonPropertyName("type")]
    public string Type => "task_status";

    /// <summary>
    /// 租户ID
    /// </summary>
    [JsonPropertyName("tenant")]
    public string Tenant { get; set; } = string.Empty;

    /// <summary>
    /// 任务ID
    /// </summary>
    [JsonPropertyName("taskId")]
    public string TaskId { get; set; } = string.Empty;

    /// <summary>
    /// 任务名称
    /// </summary>
    [JsonPropertyName("taskName")]
    public string TaskName { get; set; } = string.Empty;

    /// <summary>
    /// 任务状态 (running/completed/failed/cancelled)
    /// </summary>
    [JsonPropertyName("status")]
    public string Status { get; set; } = "running";

    /// <summary>
    /// 进度 (0-100)
    /// </summary>
    [JsonPropertyName("progress")]
    public int Progress { get; set; }

    /// <summary>
    /// 状态消息
    /// </summary>
    [JsonPropertyName("message")]
    public string? Message { get; set; }

    /// <summary>
    /// 时间戳
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// 设备实时数据消息 - 包含设备的所有启用实时推送的标签值
/// </summary>
public class DeviceDataMessage
{
    /// <summary>
    /// 消息类型
    /// </summary>
    [JsonPropertyName("type")]
    public string Type => "device_data";

    /// <summary>
    /// 租户ID
    /// </summary>
    [JsonPropertyName("tenant")]
    public string Tenant { get; set; } = string.Empty;

    /// <summary>
    /// 设备ID
    /// </summary>
    [JsonPropertyName("device")]
    public string Device { get; set; } = string.Empty;

    /// <summary>
    /// 设备名称
    /// </summary>
    [JsonPropertyName("deviceName")]
    public string DeviceName { get; set; } = string.Empty;

    /// <summary>
    /// 标签数据列表
    /// </summary>
    [JsonPropertyName("tags")]
    public List<TagDataItem> Tags { get; set; } = new();

    /// <summary>
    /// 时间戳
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// 标签数据项
/// </summary>
public class TagDataItem
{
    /// <summary>
    /// 标签ID
    /// </summary>
    [JsonPropertyName("tagId")]
    public string TagId { get; set; } = string.Empty;

    /// <summary>
    /// 标签名称
    /// </summary>
    [JsonPropertyName("tagName")]
    public string TagName { get; set; } = string.Empty;

    /// <summary>
    /// 数值 (数字类型)
    /// </summary>
    [JsonPropertyName("value")]
    public object? Value { get; set; }

    /// <summary>
    /// 单位
    /// </summary>
    [JsonPropertyName("unit")]
    public string? Unit { get; set; }

    /// <summary>
    /// 数据质量 (Good/Bad/Uncertain)
    /// </summary>
    [JsonPropertyName("quality")]
    public string Quality { get; set; } = "Good";

    /// <summary>
    /// 数据时间
    /// </summary>
    [JsonPropertyName("eventTime")]
    public DateTime EventTime { get; set; } = DateTime.UtcNow;
}
