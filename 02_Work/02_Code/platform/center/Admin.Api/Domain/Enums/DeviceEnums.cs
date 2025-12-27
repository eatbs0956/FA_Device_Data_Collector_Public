namespace Admin.Api.Domain.Enums;

/// <summary>
/// 平台类型枚举
/// </summary>
public enum PlatformType
{
    /// <summary>
    /// .NET 8.0 现代平台 - 完整功能支持
    /// </summary>
    NET80,

    /// <summary>
    /// .NET Framework 4.5+ 老旧平台 - 基础功能支持
    /// </summary>
    NET45
}

/// <summary>
/// 节点状态枚�?
/// </summary>
public enum NodeStatus
{
    /// <summary>
    /// 在线
    /// </summary>
    Online,

    /// <summary>
    /// 离线
    /// </summary>
    Offline,

    /// <summary>
    /// 错误
    /// </summary>
    Error
}

/// <summary>
/// 连接状态枚�?
/// </summary>
public enum ConnectionStatus
{
    /// <summary>
    /// 已连�?
    /// </summary>
    Connected,

    /// <summary>
    /// 断开连接
    /// </summary>
    Disconnected,

    /// <summary>
    /// 正在连接
    /// </summary>
    Connecting,

    /// <summary>
    /// 重新连接
    /// </summary>
    Reconnecting,

    /// <summary>
    /// 错误
    /// </summary>
    Error
}

/// <summary>
/// 数据类型枚举 - 对应LLD文档中的DataType定义
/// </summary>
public enum DataType
{
    Boolean,
    Int16,
    UInt16,
    Int32,
    UInt32,
    Int64,
    UInt64,
    Float,
    Double,
    String,
    DateTime,
    ByteArray
}

/// <summary>
/// 访问模式枚举
/// </summary>
public enum AccessMode
{
    /// <summary>
    /// 只读
    /// </summary>
    ReadOnly,

    /// <summary>
    /// 只写
    /// </summary>
    WriteOnly,

    /// <summary>
    /// 读写
    /// </summary>
    ReadWrite
}

/// <summary>
/// 质量码枚�?
/// </summary>
public enum QualityCode
{
    /// <summary>
    /// 良好
    /// </summary>
    Good = 0,

    /// <summary>
    /// 错误
    /// </summary>
    Bad = 1,

    /// <summary>
    /// 不确�?
    /// </summary>
    Uncertain = 2,

    /// <summary>
    /// 超时
    /// </summary>
    Timeout = 3,

    /// <summary>
    /// 通信错误
    /// </summary>
    CommunicationError = 4,

    /// <summary>
    /// 设备错误
    /// </summary>
    DeviceError = 5,

    /// <summary>
    /// 配置错误
    /// </summary>
    ConfigurationError = 6
}

/// <summary>
/// 任务类型枚举
/// </summary>
public enum TaskType
{
    /// <summary>
    /// 轮询
    /// </summary>
    Polling,

    /// <summary>
    /// 订阅
    /// </summary>
    Subscription,

    /// <summary>
    /// 事件触发
    /// </summary>
    Event
}

/// <summary>
/// 任务状态枚�?
/// </summary>
public enum TaskStatus
{
    /// <summary>
    /// 已创�?
    /// </summary>
    Created,

    /// <summary>
    /// 已调�?
    /// </summary>
    Scheduled,

    /// <summary>
    /// 运行�?
    /// </summary>
    Running,

    /// <summary>
    /// 已暂�?
    /// </summary>
    Paused,

    /// <summary>
    /// 已停�?
    /// </summary>
    Stopped,

    /// <summary>
    /// 失败
    /// </summary>
    Failed,

    /// <summary>
    /// 已取�?
    /// </summary>
    Cancelled
}

/// <summary>
/// 告警类型枚举
/// </summary>
public enum AlarmType
{
    /// <summary>
    /// 通信告警
    /// </summary>
    Communication,

    /// <summary>
    /// 阈值告�?
    /// </summary>
    Threshold,

    /// <summary>
    /// 质量告警
    /// </summary>
    Quality,

    /// <summary>
    /// 系统告警
    /// </summary>
    System
}

/// <summary>
/// 告警级别枚举
/// </summary>
public enum AlarmLevel
{
    /// <summary>
    /// 信息
    /// </summary>
    Info,

    /// <summary>
    /// 警告
    /// </summary>
    Warning,

    /// <summary>
    /// 错误
    /// </summary>
    Error,

    /// <summary>
    /// 严重
    /// </summary>
    Critical
}

/// <summary>
/// 告警状态枚�?
/// </summary>
public enum AlarmStatus
{
    /// <summary>
    /// 活动
    /// </summary>
    Active,

    /// <summary>
    /// 已确�?
    /// </summary>
    Acknowledged,

    /// <summary>
    /// 已恢�?
    /// </summary>
    Recovered,

    /// <summary>
    /// 已关�?
    /// </summary>
    Closed
}
