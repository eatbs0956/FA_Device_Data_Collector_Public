using System;
using System.Collections.Generic;

namespace Collector.Core.Models;

/// <summary>
/// 服务端节点配置响应 DTO（与 Admin.Api 的 EdgeNodeConfigResponse 一致）
/// 用于接收 API 原始 JSON 后再转换为 Core 内部模型
/// </summary>
public class ServerNodeConfigResponse
{
    public ServerNodeBasicInfo Node { get; set; } = new();
    public List<ServerDeviceConfigInfo> Devices { get; set; } = new();
    public List<ServerTaskConfigInfo> Tasks { get; set; } = new();
    public long ConfigVersion { get; set; }
    public DateTimeOffset LastUpdatedAt { get; set; }
}

/// <summary>
/// 服务端节点基本信息 DTO
/// </summary>
public class ServerNodeBasicInfo
{
    public string Id { get; set; } = string.Empty;
    public string NodeId { get; set; } = string.Empty;
    public string NodeName { get; set; } = string.Empty;
    public string? ResourceLimits { get; set; }
    public string? PlatformConfig { get; set; }
}

/// <summary>
/// 服务端设备配置 DTO（ProtocolConfig 是 JSON 字符串而非对象）
/// </summary>
public class ServerDeviceConfigInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string DeviceType { get; set; } = string.Empty;
    public string Protocol { get; set; } = string.Empty;
    public string? ConnectionString { get; set; }
    public string? IpAddress { get; set; }
    public int? Port { get; set; }
    /// <summary>
    /// 协议配置 - 服务端返回的是 JSON 字符串
    /// </summary>
    public string? ProtocolConfig { get; set; }
    public bool IsEnabled { get; set; }
    public List<ServerTagConfigInfo> Tags { get; set; } = new();
}

/// <summary>
/// 采集器状态报告 - 与 Admin.Api CollectorStatusReport 一致
/// </summary>
public class CollectorStatusReport
{
    /// <summary>
    /// 状态：Running, Stopped, Error
    /// </summary>
    public string Status { get; set; } = "Running";

    /// <summary>
    /// 正在运行的任务数
    /// </summary>
    public int RunningTaskCount { get; set; }

    /// <summary>
    /// CPU使用率
    /// </summary>
    public double CpuUsage { get; set; }

    /// <summary>
    /// 内存使用MB
    /// </summary>
    public double MemoryUsageMb { get; set; }

    /// <summary>
    /// 已处理数据点数
    /// </summary>
    public long DataPointsProcessed { get; set; }

    /// <summary>
    /// 最后采集时间
    /// </summary>
    public DateTimeOffset? LastCollectionTime { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 设备运行时状态列表
    /// </summary>
    public List<DeviceStatusItem>? DeviceStatuses { get; set; }

    /// <summary>
    /// 任务运行时状态列表
    /// </summary>
    public List<TaskStatusItem>? TaskStatuses { get; set; }
}

/// <summary>
/// 设备状态上报项
/// </summary>
public class DeviceStatusItem
{
    /// <summary>
    /// 设备标识符（device_id 字段，非主键 Id）
    /// </summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// 连接状态：Connected, Disconnected, Connecting, Error
    /// </summary>
    public string ConnectionStatus { get; set; } = "Disconnected";

    /// <summary>
    /// 错误次数
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// 最后错误信息
    /// </summary>
    public string? LastError { get; set; }
}

/// <summary>
/// 任务状态上报项
/// </summary>
public class TaskStatusItem
{
    /// <summary>
    /// 任务编码（对应 collection_tasks.code）
    /// </summary>
    public string TaskCode { get; set; } = string.Empty;

    /// <summary>
    /// 运行状态：Running, Stopped, Paused, Error
    /// </summary>
    public string Status { get; set; } = "Stopped";

    /// <summary>
    /// 采集次数
    /// </summary>
    public long TotalCollectionCount { get; set; }

    /// <summary>
    /// 错误次数
    /// </summary>
    public long ErrorCount { get; set; }

    /// <summary>
    /// 最后错误信息
    /// </summary>
    public string? LastError { get; set; }
}

/// <summary>
/// 服务端点位配置 DTO
/// </summary>
public class ServerTagConfigInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string? Address { get; set; }
    public int? SamplingInterval { get; set; }
    public double? ScalingFactor { get; set; }
    public double? Offset { get; set; }
    public string? Unit { get; set; }
    public bool IsEnabled { get; set; }
}

/// <summary>
/// 服务端采集任务配置 DTO
/// </summary>
public class ServerTaskConfigInfo
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public string TaskType { get; set; } = string.Empty;
    public string? CronExpression { get; set; }
    public int? IntervalMs { get; set; }
    public int? BatchSize { get; set; }
    public int? TimeoutMs { get; set; }
    public int? RetryCount { get; set; }
    public bool IsEnabled { get; set; }
    public string? TaskConfig { get; set; }
    public List<string> DeviceIds { get; set; } = new();
}

/// <summary>
/// 边缘节点注册请求（与 Admin.Api 一致）
/// </summary>
public class EdgeNodeRegisterRequest
{
    public string NodeName { get; set; } = string.Empty;
    public string Platform { get; set; } = ".NET 8.0";
    public string Version { get; set; } = "1.0.0";
    public string? IpAddress { get; set; }
    public int? Port { get; set; }
    public string? OsInfo { get; set; }
    public string? HardwareInfo { get; set; }
    public string? InstallPath { get; set; }
}
