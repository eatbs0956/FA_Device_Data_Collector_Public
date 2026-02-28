namespace Collector.Core.Models;

/// <summary>
/// 节点配置 - 从服务端拉取的完整配置
/// </summary>
public class NodeConfig
{
    /// <summary>
    /// 节点信息
    /// </summary>
    public NodeInfo Node { get; set; } = new();

    /// <summary>
    /// 采集任务列表
    /// </summary>
    public List<TaskConfig> Tasks { get; set; } = new();

    /// <summary>
    /// 设备配置列表（包含标签）
    /// </summary>
    public List<DeviceConfig> Devices { get; set; } = new();

    /// <summary>
    /// 配置版本号（用于变更检测）
    /// </summary>
    public long ConfigVersion { get; set; }

    /// <summary>
    /// 配置获取时间
    /// </summary>
    public DateTimeOffset FetchedAt { get; set; }
}

/// <summary>
/// 节点基本信息
/// </summary>
public class NodeInfo
{
    public Guid Id { get; set; }
    public string NodeId { get; set; } = string.Empty;
    public string NodeName { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string? Location { get; set; }
    public string Status { get; set; } = "Offline";
}
