using System.Text.Json.Serialization;

namespace Processor.Worker.Models;

/// <summary>
/// RabbitMQ 数据点消息
/// </summary>
public class DataPointMessage
{
    /// <summary>
    /// 租户ID
    /// </summary>
    [JsonPropertyName("tenantId")]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// 标签ID（格式: deviceId.tagName）
    /// </summary>
    [JsonPropertyName("tagId")]
    public string TagId { get; set; } = string.Empty;

    /// <summary>
    /// 事件时间
    /// </summary>
    [JsonPropertyName("eventTime")]
    public DateTime EventTime { get; set; }

    /// <summary>
    /// 数据质量 (0=Good, 其他=Bad)
    /// </summary>
    [JsonPropertyName("quality")]
    public int Quality { get; set; }

    /// <summary>
    /// 数据来源
    /// </summary>
    [JsonPropertyName("source")]
    public string? Source { get; set; }

    /// <summary>
    /// 值（支持多种类型）
    /// </summary>
    [JsonPropertyName("value")]
    public object? Value { get; set; }

    /// <summary>
    /// 信封ID（用于批量消息追踪）
    /// </summary>
    [JsonPropertyName("envelopeId")]
    public string? EnvelopeId { get; set; }

    /// <summary>
    /// 关联ID
    /// </summary>
    [JsonPropertyName("correlationId")]
    public string? CorrelationId { get; set; }

    /// <summary>
    /// 序列号
    /// </summary>
    [JsonPropertyName("seq")]
    public int? Seq { get; set; }

    /// <summary>
    /// 从 TagId 解析设备ID
    /// </summary>
    public string GetDeviceId()
    {
        var parts = TagId.Split('.');
        return parts.Length > 0 ? parts[0] : TagId;
    }

    /// <summary>
    /// 从 TagId 解析标签名
    /// </summary>
    public string GetTagName()
    {
        var parts = TagId.Split('.');
        return parts.Length > 1 ? string.Join(".", parts.Skip(1)) : TagId;
    }
}

/// <summary>
/// 批量数据点消息（用于按设备聚合）
/// </summary>
public class DeviceDataBatch
{
    /// <summary>
    /// 租户ID
    /// </summary>
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// 设备ID
    /// </summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// 数据来源
    /// </summary>
    public string Source { get; set; } = "collector";

    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// 字段值（标签名 -> 值）
    /// </summary>
    public Dictionary<string, object> Fields { get; set; } = new();
}
