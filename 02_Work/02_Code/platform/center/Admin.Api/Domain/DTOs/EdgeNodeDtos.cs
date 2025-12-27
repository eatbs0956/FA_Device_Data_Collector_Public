namespace Admin.Api.Domain.DTOs;

/// <summary>
/// 边缘节点查询请求
/// </summary>
public class EdgeNodeQueryRequest
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
    /// 节点ID（精确匹配）
    /// </summary>
    public string? NodeId { get; set; }

    /// <summary>
    /// 节点名称（模糊匹配）
    /// </summary>
    public string? NodeName { get; set; }

    /// <summary>
    /// 平台类型
    /// </summary>
    public string? Platform { get; set; }

    /// <summary>
    /// 节点状态
    /// </summary>
    public string? Status { get; set; }
}

/// <summary>
/// 边缘节点列表响应
/// </summary>
public class EdgeNodeListResponse
{
    /// <summary>
    /// 边缘节点列表
    /// </summary>
    public List<EdgeNodeDto> Records { get; set; } = new();

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
/// 边缘节点DTO
/// </summary>
public class EdgeNodeDto
{
    /// <summary>
    /// 主键ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// 节点标识符
    /// </summary>
    public string NodeId { get; set; } = string.Empty;

    /// <summary>
    /// 节点名称
    /// </summary>
    public string NodeName { get; set; } = string.Empty;

    /// <summary>
    /// 平台类型
    /// </summary>
    public string Platform { get; set; } = string.Empty;

    /// <summary>
    /// 版本号
    /// </summary>
    public string Version { get; set; } = string.Empty;

    /// <summary>
    /// 部署位置
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// IP地址
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// 端口
    /// </summary>
    public int? Port { get; set; }

    /// <summary>
    /// 状态
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// 平台配置（JSON）
    /// </summary>
    public string? PlatformConfig { get; set; }

    /// <summary>
    /// 资源限制（JSON）
    /// </summary>
    public string? ResourceLimits { get; set; }

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

    /// <summary>
    /// 最后心跳时间
    /// </summary>
    public DateTimeOffset? LastHeartbeat { get; set; }

    /// <summary>
    /// 注册类型
    /// </summary>
    public string RegistrationType { get; set; } = string.Empty;

    /// <summary>
    /// 关联设备数量
    /// </summary>
    public int DeviceCount { get; set; }

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
/// 创建边缘节点请求
/// </summary>
public class CreateEdgeNodeRequest
{
    /// <summary>
    /// 节点标识符（必填，唯一）
    /// </summary>
    public string NodeId { get; set; } = string.Empty;

    /// <summary>
    /// 节点名称（必填）
    /// </summary>
    public string NodeName { get; set; } = string.Empty;

    /// <summary>
    /// 平台类型（必填）
    /// </summary>
    public string Platform { get; set; } = string.Empty;

    /// <summary>
    /// 版本号
    /// </summary>
    public string? Version { get; set; }

    /// <summary>
    /// 部署位置
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// IP地址
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// 端口
    /// </summary>
    public int? Port { get; set; }

    /// <summary>
    /// 平台配置（JSON）
    /// </summary>
    public string? PlatformConfig { get; set; }

    /// <summary>
    /// 资源限制（JSON）
    /// </summary>
    public string? ResourceLimits { get; set; }

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
/// 更新边缘节点请求
/// </summary>
public class UpdateEdgeNodeRequest
{
    /// <summary>
    /// 节点名称（必填，始终可编辑）
    /// </summary>
    public string NodeName { get; set; } = string.Empty;

    /// <summary>
    /// 部署位置（始终可编辑）
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// 资源限制（JSON，始终可编辑）
    /// </summary>
    public string? ResourceLimits { get; set; }

    // ===== 以下字段仅手动添加且未连接时可编辑 =====

    /// <summary>
    /// 平台类型
    /// </summary>
    public string? Platform { get; set; }

    /// <summary>
    /// 版本号
    /// </summary>
    public string? Version { get; set; }

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
