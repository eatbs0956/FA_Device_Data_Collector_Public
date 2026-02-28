namespace Collector.Core.Models;

/// <summary>
/// 登录请求
/// </summary>
public class LoginRequest
{
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

/// <summary>
/// 登录响应
/// </summary>
public class LoginResponse
{
    /// <summary>
    /// 访问令牌 (后端字段名为 token)
    /// </summary>
    [Newtonsoft.Json.JsonProperty("token")]
    public string AccessToken { get; set; } = string.Empty;

    public string RefreshToken { get; set; } = string.Empty;
    public int ExpiresIn { get; set; }
    public string TokenType { get; set; } = "Bearer";
    public UserInfo? User { get; set; }
}

/// <summary>
/// 用户信息
/// </summary>
public class UserInfo
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string? RealName { get; set; }
    public List<string> Roles { get; set; } = new();
}

/// <summary>
/// API 响应基类
/// </summary>
/// <typeparam name="T">数据类型</typeparam>
public class ApiResponse<T>
{
    public string Code { get; set; } = string.Empty;
    public string Msg { get; set; } = string.Empty;
    public T? Data { get; set; }

    public bool IsSuccess => Code == "0000";
}

/// <summary>
/// 节点注册请求
/// </summary>
public class NodeRegisterRequest
{
    public string NodeId { get; set; } = string.Empty;
    public string NodeName { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string Version { get; set; } = string.Empty;
    public string? IpAddress { get; set; }
    public string? OsInfo { get; set; }
}

/// <summary>
/// 心跳请求
/// </summary>
public class HeartbeatRequest
{
    public string Status { get; set; } = "Online";
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
    public int RunningTaskCount { get; set; }
    public long TotalCollectionCount { get; set; }
    public long ErrorCount { get; set; }
}

/// <summary>
/// 配置变更通知
/// </summary>
public class ConfigChangeNotification
{
    public string NodeId { get; set; } = string.Empty;
    public ConfigChangeType ChangeType { get; set; }
    public string? Message { get; set; }
    public DateTimeOffset Timestamp { get; set; }
}

/// <summary>
/// 配置变更类型枚举
/// </summary>
public enum ConfigChangeType
{
    TaskAdded,
    TaskUpdated,
    TaskDeleted,
    DeviceAdded,
    DeviceUpdated,
    DeviceDeleted,
    TagAdded,
    TagUpdated,
    TagDeleted,
    FullSync
}
