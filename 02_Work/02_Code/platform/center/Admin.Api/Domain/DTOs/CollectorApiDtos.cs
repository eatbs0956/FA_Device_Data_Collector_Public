namespace Admin.Api.Domain.DTOs;

/// <summary>
/// 边缘节点注册请求（采集器启动时调用）
/// </summary>
public class EdgeNodeRegisterRequest
{
    /// <summary>
    /// 节点名称（必填）
    /// </summary>
    public string NodeName { get; set; } = string.Empty;

    /// <summary>
    /// 平台类型（NET8.0 或 NET45）
    /// </summary>
    public string Platform { get; set; } = "NET8.0";

    /// <summary>
    /// 版本号
    /// </summary>
    public string Version { get; set; } = "1.0.0";

    /// <summary>
    /// IP地址
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// 端口
    /// </summary>
    public int? Port { get; set; }

    /// <summary>
    /// 操作系统信息
    /// </summary>
    public string? OsInfo { get; set; }

    /// <summary>
    /// 硬件信息（JSON）
    /// </summary>
    public string? HardwareInfo { get; set; }

    /// <summary>
    /// 安装路径
    /// </summary>
    public string? InstallPath { get; set; }
}

/// <summary>
/// 边缘节点注册响应
/// </summary>
public class EdgeNodeRegisterResponse
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 节点标识
    /// </summary>
    public string NodeId { get; set; } = string.Empty;

    /// <summary>
    /// 节点名称
    /// </summary>
    public string NodeName { get; set; } = string.Empty;

    /// <summary>
    /// 是否为新创建的节点
    /// </summary>
    public bool IsNewNode { get; set; }

    /// <summary>
    /// 注册类型
    /// </summary>
    public string RegistrationType { get; set; } = string.Empty;
}

/// <summary>
/// 心跳请求
/// </summary>
public class EdgeNodeHeartbeatRequest
{
    /// <summary>
    /// 当前正在执行的任务ID列表
    /// </summary>
    public List<string>? RunningTaskIds { get; set; }

    /// <summary>
    /// 最后采集时间
    /// </summary>
    public DateTimeOffset? LastCollectionTime { get; set; }

    /// <summary>
    /// CPU使用率百分比
    /// </summary>
    public double? CpuUsage { get; set; }

    /// <summary>
    /// 内存使用MB
    /// </summary>
    public double? MemoryUsageMb { get; set; }

    /// <summary>
    /// 已处理的数据点数量
    /// </summary>
    public long? DataPointsProcessed { get; set; }
}

/// <summary>
/// 心跳响应
/// </summary>
public class EdgeNodeHeartbeatResponse
{
    /// <summary>
    /// 是否成功
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 是否有配置更新
    /// </summary>
    public bool HasConfigUpdate { get; set; }

    /// <summary>
    /// 服务器时间（用于时钟同步）
    /// </summary>
    public DateTimeOffset ServerTime { get; set; }
}

/// <summary>
/// 节点配置响应（完整配置）
/// </summary>
public class EdgeNodeConfigResponse
{
    /// <summary>
    /// 节点基本信息
    /// </summary>
    public EdgeNodeBasicInfo Node { get; set; } = new();

    /// <summary>
    /// 分配给该节点的设备列表
    /// </summary>
    public List<DeviceConfigInfo> Devices { get; set; } = new();

    /// <summary>
    /// 分配给该节点的采集任务列表
    /// </summary>
    public List<CollectionTaskConfigInfo> Tasks { get; set; } = new();

    /// <summary>
    /// 配置版本号（用于增量更新判断）
    /// </summary>
    public long ConfigVersion { get; set; }

    /// <summary>
    /// 最后更新时间
    /// </summary>
    public DateTimeOffset LastUpdatedAt { get; set; }
}

/// <summary>
/// 节点基本信息
/// </summary>
public class EdgeNodeBasicInfo
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 节点标识
    /// </summary>
    public string NodeId { get; set; } = string.Empty;

    /// <summary>
    /// 节点名称
    /// </summary>
    public string NodeName { get; set; } = string.Empty;

    /// <summary>
    /// 资源限制配置（JSON）
    /// </summary>
    public string? ResourceLimits { get; set; }

    /// <summary>
    /// 平台配置（JSON）
    /// </summary>
    public string? PlatformConfig { get; set; }
}

/// <summary>
/// 设备配置信息
/// </summary>
public class DeviceConfigInfo
{
    /// <summary>
    /// 设备ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 设备名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 设备编码
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 设备类型
    /// </summary>
    public string DeviceType { get; set; } = string.Empty;

    /// <summary>
    /// 通信协议
    /// </summary>
    public string Protocol { get; set; } = string.Empty;

    /// <summary>
    /// 连接字符串/地址
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>
    /// IP地址
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// 端口
    /// </summary>
    public int? Port { get; set; }

    /// <summary>
    /// 协议配置（JSON）
    /// </summary>
    public string? ProtocolConfig { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 设备下的标签列表
    /// </summary>
    public List<TagConfigInfo> Tags { get; set; } = new();
}

/// <summary>
/// 标签配置信息
/// </summary>
public class TagConfigInfo
{
    /// <summary>
    /// 标签ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 标签名称
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// 标签编码
    /// </summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>
    /// 数据类型
    /// </summary>
    public string DataType { get; set; } = string.Empty;

    /// <summary>
    /// 地址
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// 采集间隔（秒）
    /// </summary>
    public int? SamplingInterval { get; set; }

    /// <summary>
    /// 缩放因子
    /// </summary>
    public double? ScalingFactor { get; set; }

    /// <summary>
    /// 偏移量
    /// </summary>
    public double? Offset { get; set; }

    /// <summary>
    /// 单位
    /// </summary>
    public string? Unit { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }
}

/// <summary>
/// 采集任务配置信息
/// </summary>
public class CollectionTaskConfigInfo
{
    /// <summary>
    /// 任务ID
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
    /// 任务类型（Periodic/Scheduled/RealTime）
    /// </summary>
    public string TaskType { get; set; } = string.Empty;

    /// <summary>
    /// Cron表达式
    /// </summary>
    public string? CronExpression { get; set; }

    /// <summary>
    /// 采集间隔（毫秒）
    /// </summary>
    public int? IntervalMs { get; set; }

    /// <summary>
    /// 批次大小
    /// </summary>
    public int? BatchSize { get; set; }

    /// <summary>
    /// 超时时间（毫秒）
    /// </summary>
    public int? TimeoutMs { get; set; }

    /// <summary>
    /// 重试次数
    /// </summary>
    public int? RetryCount { get; set; }

    /// <summary>
    /// 是否启用
    /// </summary>
    public bool IsEnabled { get; set; }

    /// <summary>
    /// 任务配置（JSON）
    /// </summary>
    public string? TaskConfig { get; set; }

    /// <summary>
    /// 关联的设备ID列表
    /// </summary>
    public List<string> DeviceIds { get; set; } = new();
}

/// <summary>
/// SignalR 配置变更通知消息
/// </summary>
public class ConfigChangeNotification
{
    /// <summary>
    /// 通知ID（用于去重）
    /// </summary>
    public string NotificationId { get; set; } = Guid.NewGuid().ToString();

    /// <summary>
    /// 变更类型
    /// </summary>
    public ConfigChangeType ChangeType { get; set; }

    /// <summary>
    /// 变更的实体类型（Device/Tag/Task/Node）
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// 变更的实体ID
    /// </summary>
    public string? EntityId { get; set; }

    /// <summary>
    /// 新的配置版本号
    /// </summary>
    public long NewConfigVersion { get; set; }

    /// <summary>
    /// 变更时间
    /// </summary>
    public DateTimeOffset ChangedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// 附加信息
    /// </summary>
    public string? Message { get; set; }
}

/// <summary>
/// 配置变更类型
/// </summary>
public enum ConfigChangeType
{
    /// <summary>
    /// 配置已更新，需要重新拉取
    /// </summary>
    ConfigUpdated,

    /// <summary>
    /// 请求重启采集器
    /// </summary>
    RestartRequested,

    /// <summary>
    /// 紧急停止所有采集
    /// </summary>
    EmergencyStop,

    /// <summary>
    /// 恢复采集
    /// </summary>
    ResumeCollection,

    /// <summary>
    /// 单个任务启动
    /// </summary>
    TaskStart,

    /// <summary>
    /// 单个任务停止
    /// </summary>
    TaskStop
}
