using Admin.Api.Domain.DTOs;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Entities;
using System.Text.Json;

namespace Admin.Api.Services;

/// <summary>
/// 边缘节点服务实现
/// </summary>
public class EdgeNodeService : IEdgeNodeService
{
    private readonly DbContext _dbContext;
    private readonly ILogger<EdgeNodeService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public EdgeNodeService(
        DbContext dbContext,
        ILogger<EdgeNodeService> logger,
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

    public async Task<EdgeNodeListResponse> GetEdgeNodesAsync(EdgeNodeQueryRequest request)
    {
        var query = _dbContext.Set<EdgeNode>()
            .Where(e => !e.DeletedFlag)
            .AsQueryable();

        // 应用筛选条件
        if (!string.IsNullOrWhiteSpace(request.NodeName))
        {
            query = query.Where(e => EF.Functions.ILike(e.NodeName, $"%{request.NodeName}%"));
        }

        if (!string.IsNullOrWhiteSpace(request.NodeId))
        {
            query = query.Where(e => e.NodeId == request.NodeId);
        }

        if (!string.IsNullOrWhiteSpace(request.Platform))
        {
            query = query.Where(e => e.Platform == request.Platform);
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            query = query.Where(e => e.Status == request.Status);
        }

        // 计算总数
        var total = await query.CountAsync();

        // 分页查询
        var pageIndex = (request.Current ?? 1) - 1;
        var pageSize = request.Size ?? 20;

        var edgeNodes = await query
            .OrderByDescending(e => e.CreatedAt)
            .Skip(pageIndex * pageSize)
            .Take(pageSize)
            .Select(e => new EdgeNodeDto
            {
                Id = e.Id.ToString(),
                NodeId = e.NodeId,
                NodeName = e.NodeName,
                Platform = e.Platform,
                Version = e.Version,
                Location = e.Location,
                IpAddress = e.IpAddress,
                Port = e.Port,
                Status = e.Status,
                PlatformConfig = e.PlatformConfig,
                ResourceLimits = e.ResourceLimits,
                OsInfo = e.OsInfo,
                HardwareInfo = e.HardwareInfo,
                InstallPath = e.InstallPath,
                LastHeartbeat = e.LastHeartbeat,
                RegistrationType = e.RegistrationType,
                DeviceCount = e.Devices.Count(d => !d.DeletedFlag),
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
                ServiceUserId = e.ServiceUserId.HasValue ? e.ServiceUserId.ToString() : null,
                ServiceUserName = e.ServiceUser != null ? e.ServiceUser.UserName : null
            })
            .ToListAsync();

        return new EdgeNodeListResponse
        {
            Records = edgeNodes,
            Total = total,
            Current = request.Current ?? 1,
            Size = pageSize
        };
    }

    public async Task<EdgeNodeDto?> GetEdgeNodeByIdAsync(Guid id)
    {
        var edgeNode = await _dbContext.Set<EdgeNode>()
            .Where(e => e.Id == id && !e.DeletedFlag)
            .Select(e => new EdgeNodeDto
            {
                Id = e.Id.ToString(),
                NodeId = e.NodeId,
                NodeName = e.NodeName,
                Platform = e.Platform,
                Version = e.Version,
                Location = e.Location,
                IpAddress = e.IpAddress,
                Port = e.Port,
                Status = e.Status,
                PlatformConfig = e.PlatformConfig,
                ResourceLimits = e.ResourceLimits,
                OsInfo = e.OsInfo,
                HardwareInfo = e.HardwareInfo,
                InstallPath = e.InstallPath,
                LastHeartbeat = e.LastHeartbeat,
                RegistrationType = e.RegistrationType,
                DeviceCount = e.Devices.Count(d => !d.DeletedFlag),
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
                ServiceUserId = e.ServiceUserId.HasValue ? e.ServiceUserId.ToString() : null,
                ServiceUserName = e.ServiceUser != null ? e.ServiceUser.UserName : null
            })
            .FirstOrDefaultAsync();

        return edgeNode;
    }

    public async Task<Guid> CreateEdgeNodeAsync(CreateEdgeNodeRequest request)
    {
        // 检查 NodeId 是否已存在
        var existingNode = await _dbContext.Set<EdgeNode>()
            .FirstOrDefaultAsync(e => e.NodeId == request.NodeId);

        if (existingNode != null)
        {
            if (existingNode.DeletedFlag)
            {
                throw new InvalidOperationException($"节点ID '{request.NodeId}' 已被删除，请联系管理员恢复或使用其他ID");
            }
            throw new InvalidOperationException($"节点ID '{request.NodeId}' 已存在");
        }

        var currentUserId = GetCurrentUserId();

        var edgeNode = new EdgeNode
        {
            Id = Guid.NewGuid(),
            NodeId = request.NodeId,
            NodeName = request.NodeName,
            Platform = request.Platform,
            Version = request.Version ?? "1.0.0",
            Location = request.Location,
            IpAddress = request.IpAddress,
            Port = request.Port,
            Status = "Offline", // 手动添加的节点初始状态为离线
            PlatformConfig = request.PlatformConfig ?? "{}",
            ResourceLimits = request.ResourceLimits ?? "{\"maxMemoryMB\": 512, \"maxConcurrentTasks\": 5}",
            OsInfo = request.OsInfo,
            HardwareInfo = request.HardwareInfo,
            InstallPath = request.InstallPath,
            RegistrationType = "manual", // 手动添加
            CreatedBy = currentUserId,
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.Set<EdgeNode>().Add(edgeNode);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("手动创建边缘节点: {NodeId}, {NodeName}", edgeNode.NodeId, edgeNode.NodeName);

        return edgeNode.Id;
    }

    public async Task UpdateEdgeNodeAsync(Guid id, UpdateEdgeNodeRequest request)
    {
        var edgeNode = await _dbContext.Set<EdgeNode>()
            .FirstOrDefaultAsync(e => e.Id == id && !e.DeletedFlag);

        if (edgeNode == null)
        {
            throw new KeyNotFoundException($"边缘节点 {id} 不存在");
        }

        var currentUserId = GetCurrentUserId();

        // 始终可编辑的字段
        edgeNode.NodeName = request.NodeName;
        edgeNode.Location = request.Location;
        edgeNode.ResourceLimits = request.ResourceLimits ?? edgeNode.ResourceLimits;
        edgeNode.UpdatedBy = currentUserId;
        edgeNode.UpdatedAt = DateTimeOffset.UtcNow;

        // 系统字段：仅手动添加且未连接过的节点可编辑
        var isManualNode = edgeNode.RegistrationType == "manual";
        var hasConnected = edgeNode.LastHeartbeat != null;

        if (isManualNode && !hasConnected)
        {
            // 可以更新系统字段
            if (!string.IsNullOrEmpty(request.Platform))
            {
                edgeNode.Platform = request.Platform;
            }
            if (request.Version != null)
            {
                edgeNode.Version = request.Version;
            }
            if (request.IpAddress != null)
            {
                edgeNode.IpAddress = request.IpAddress;
            }
            if (request.Port.HasValue)
            {
                edgeNode.Port = request.Port;
            }
            if (request.OsInfo != null)
            {
                edgeNode.OsInfo = request.OsInfo;
            }
            if (request.HardwareInfo != null)
            {
                edgeNode.HardwareInfo = request.HardwareInfo;
            }
            if (request.InstallPath != null)
            {
                edgeNode.InstallPath = request.InstallPath;
            }

            _logger.LogInformation("更新边缘节点(含系统字段): {NodeId}, {NodeName}", edgeNode.NodeId, edgeNode.NodeName);
        }
        else
        {
            _logger.LogInformation("更新边缘节点(仅基本字段): {NodeId}, {NodeName}", edgeNode.NodeId, edgeNode.NodeName);
        }

        await _dbContext.SaveChangesAsync();
    }

    public async Task<int> DeleteEdgeNodeAsync(Guid id)
    {
        var edgeNode = await _dbContext.Set<EdgeNode>()
            .Include(e => e.Devices)
            .FirstOrDefaultAsync(e => e.Id == id && !e.DeletedFlag);

        if (edgeNode == null)
        {
            throw new KeyNotFoundException($"边缘节点 {id} 不存在");
        }

        var currentUserId = GetCurrentUserId();

        // 统计关联设备数量
        var deviceCount = edgeNode.Devices.Count(d => !d.DeletedFlag);

        // 将关联设备的 edge_node_id 置为 NULL
        foreach (var device in edgeNode.Devices.Where(d => !d.DeletedFlag))
        {
            device.EdgeNodeId = null;
            device.UpdatedBy = currentUserId;
            device.UpdatedAt = DateTimeOffset.UtcNow;
        }

        // 软删除节点
        edgeNode.DeletedFlag = true;
        edgeNode.UpdatedBy = currentUserId;
        edgeNode.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("删除边缘节点: {NodeId}, {NodeName}, 已解除 {DeviceCount} 个设备关联", 
            edgeNode.NodeId, edgeNode.NodeName, deviceCount);

        return deviceCount;
    }

    public async Task<int> GetDeviceCountAsync(Guid id)
    {
        return await _dbContext.Set<Device>()
            .CountAsync(d => d.EdgeNodeId == id && !d.DeletedFlag);
    }

    // ============ Collector API 新增方法实现 ============

    public async Task<EdgeNodeRegisterResponse> RegisterEdgeNodeAsync(string nodeId, EdgeNodeRegisterRequest request)
    {
        var existingNode = await _dbContext.Set<EdgeNode>()
            .FirstOrDefaultAsync(e => e.NodeId.ToUpper() == nodeId.ToUpper() && !e.DeletedFlag);

        if (existingNode != null)
        {
            // 节点已存在，更新系统信息
            existingNode.NodeName = request.NodeName;
            existingNode.Platform = request.Platform;
            existingNode.Version = request.Version;
            existingNode.IpAddress = request.IpAddress;
            existingNode.Port = request.Port;
            existingNode.OsInfo = request.OsInfo;
            existingNode.HardwareInfo = request.HardwareInfo;
            existingNode.InstallPath = request.InstallPath;
            existingNode.Status = "Online";
            existingNode.LastHeartbeat = DateTimeOffset.UtcNow;
            existingNode.UpdatedAt = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("边缘节点重新注册（更新）: {NodeId}, {NodeName}, 类型: {RegistrationType}", 
                nodeId, request.NodeName, existingNode.RegistrationType);

            return new EdgeNodeRegisterResponse
            {
                Id = existingNode.Id.ToString(),
                NodeId = existingNode.NodeId,
                NodeName = existingNode.NodeName,
                IsNewNode = false,
                RegistrationType = existingNode.RegistrationType
            };
        }

        // 节点不存在，自动创建（auto类型）
        var newNode = new EdgeNode
        {
            Id = Guid.NewGuid(),
            NodeId = nodeId,
            NodeName = request.NodeName,
            Platform = request.Platform,
            Version = request.Version,
            IpAddress = request.IpAddress,
            Port = request.Port,
            OsInfo = request.OsInfo,
            HardwareInfo = request.HardwareInfo,
            InstallPath = request.InstallPath,
            Status = "Online",
            RegistrationType = "auto", // 自动注册
            LastHeartbeat = DateTimeOffset.UtcNow,
            PlatformConfig = "{}",
            ResourceLimits = "{\"maxMemoryMB\": 512, \"maxConcurrentTasks\": 5}",
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.Set<EdgeNode>().Add(newNode);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("边缘节点自动注册（新建）: {NodeId}, {NodeName}", nodeId, request.NodeName);

        return new EdgeNodeRegisterResponse
        {
            Id = newNode.Id.ToString(),
            NodeId = newNode.NodeId,
            NodeName = newNode.NodeName,
            IsNewNode = true,
            RegistrationType = newNode.RegistrationType
        };
    }

    public async Task<EdgeNodeHeartbeatResponse> UpdateHeartbeatAsync(string nodeId, EdgeNodeHeartbeatRequest? request)
    {
        var node = await _dbContext.Set<EdgeNode>()
            .FirstOrDefaultAsync(e => e.NodeId.ToUpper() == nodeId.ToUpper() && !e.DeletedFlag);

        if (node == null)
        {
            _logger.LogWarning("心跳更新失败：节点不存在 {NodeId}", nodeId);
            return new EdgeNodeHeartbeatResponse
            {
                Success = false,
                HasConfigUpdate = false,
                ServerTime = DateTimeOffset.UtcNow
            };
        }

        // 更新心跳时间和状态
        node.LastHeartbeat = DateTimeOffset.UtcNow;
        node.Status = "Online";
        node.UpdatedAt = DateTimeOffset.UtcNow;

        _logger.LogInformation("收到节点心跳: {NodeId}, 状态已更新为 Online", nodeId);

        // 如果有运行时信息，可以存储到扩展字段（这里简化处理）
        if (request != null)
        {
            // 可以考虑将运行时状态存储到独立的监控表
            _logger.LogDebug("节点 {NodeId} 心跳: RunningTasks={TaskCount}, CPU={Cpu}%, Memory={Memory}MB",
                nodeId,
                request.RunningTaskIds?.Count ?? 0,
                request.CpuUsage,
                request.MemoryUsageMb);
        }

        await _dbContext.SaveChangesAsync();

        // TODO: 检查是否有配置更新（可以基于配置版本号实现）
        var hasConfigUpdate = false;

        return new EdgeNodeHeartbeatResponse
        {
            Success = true,
            HasConfigUpdate = hasConfigUpdate,
            ServerTime = DateTimeOffset.UtcNow
        };
    }

    public async Task<EdgeNodeConfigResponse?> GetNodeConfigAsync(string nodeId)
    {
        var node = await _dbContext.Set<EdgeNode>()
            .FirstOrDefaultAsync(e => e.NodeId == nodeId && !e.DeletedFlag);

        if (node == null)
        {
            return null;
        }

        // 获取分配给该节点的设备及其标签
        var devices = await _dbContext.Set<Device>()
            .Include(d => d.TagDefinitions.Where(t => !t.DeletedFlag))
            .Where(d => d.EdgeNodeId == node.Id && !d.DeletedFlag && d.Enabled)
            .ToListAsync();

        // 获取与该节点关联设备相关的采集任务
        var deviceIds = devices.Select(d => d.Id).ToList();
        var taskDevices = await _dbContext.Set<CollectionTaskDevice>()
            .Include(td => td.Task)
            .Where(td => deviceIds.Contains(td.DeviceId) && 
                        td.Task != null && 
                        !td.Task.DeletedFlag && 
                        td.Task.IsEnabled)
            .ToListAsync();

        var tasks = taskDevices
            .Where(td => td.Task != null)
            .Select(td => td.Task!)
            .Distinct()
            .ToList();

        // 构建配置响应
        var config = new EdgeNodeConfigResponse
        {
            Node = new EdgeNodeBasicInfo
            {
                Id = node.Id.ToString(),
                NodeId = node.NodeId,
                NodeName = node.NodeName,
                ResourceLimits = node.ResourceLimits,
                PlatformConfig = node.PlatformConfig
            },
            Devices = devices.Select(d => MapDeviceToConfigInfo(d)).ToList(),
            Tasks = tasks.Select(t => MapTaskToConfigInfo(t, taskDevices)).ToList(),
            ConfigVersion = node.UpdatedAt?.ToUnixTimeSeconds() ?? node.CreatedAt.ToUnixTimeSeconds(),
            LastUpdatedAt = node.UpdatedAt ?? node.CreatedAt
        };

        return config;
    }

    public async Task<Guid?> GetNodeIdByNodeIdAsync(string nodeId)
    {
        var node = await _dbContext.Set<EdgeNode>()
            .Where(e => e.NodeId == nodeId && !e.DeletedFlag)
            .Select(e => e.Id)
            .FirstOrDefaultAsync();

        return node == Guid.Empty ? null : node;
    }

    // ============ 私有辅助方法 ============

    private DeviceConfigInfo MapDeviceToConfigInfo(Device device)
    {
        // 解析连接配置
        string? ipAddress = null;
        int? port = null;
        string? connectionString = null;

        if (!string.IsNullOrEmpty(device.ConnectionConfig))
        {
            try
            {
                using var doc = JsonDocument.Parse(device.ConnectionConfig);
                var root = doc.RootElement;
                
                if (root.TryGetProperty("ipAddress", out var ipProp) || 
                    root.TryGetProperty("ip", out ipProp) ||
                    root.TryGetProperty("host", out ipProp))
                {
                    ipAddress = ipProp.GetString();
                }
                
                if (root.TryGetProperty("port", out var portProp))
                {
                    port = portProp.GetInt32();
                }
                
                if (root.TryGetProperty("connectionString", out var connProp))
                {
                    connectionString = connProp.GetString();
                }
            }
            catch (JsonException)
            {
                // 解析失败，保持默认值
            }
        }

        return new DeviceConfigInfo
        {
            Id = device.Id.ToString(),
            Name = device.DeviceName,
            Code = device.DeviceId,
            DeviceType = device.DeviceType,
            Protocol = device.ProtocolType,
            ConnectionString = connectionString,
            IpAddress = ipAddress,
            Port = port,
            ProtocolConfig = device.ProtocolConfig,
            IsEnabled = device.Enabled,
            Tags = device.TagDefinitions
                .Where(t => t.Enabled)
                .Select(t => new TagConfigInfo
                {
                    Id = t.Id.ToString(),
                    Name = t.TagName,
                    Code = t.TagId,
                    DataType = t.DataType,
                    Address = t.TagAddress,
                    ScalingFactor = (double)t.ScalingFactor,
                    Offset = (double)t.Offset,
                    Unit = t.Unit,
                    IsEnabled = t.Enabled
                })
                .ToList()
        };
    }

    private CollectionTaskConfigInfo MapTaskToConfigInfo(CollectionTask task, List<CollectionTaskDevice> taskDevices)
    {
        return new CollectionTaskConfigInfo
        {
            Id = task.Id.ToString(),
            Name = task.Name,
            Code = task.Code,
            TaskType = task.TaskType,
            CronExpression = task.CronExpression,
            IntervalMs = task.DefaultInterval,
            IsEnabled = task.IsEnabled,
            DeviceIds = taskDevices
                .Where(td => td.TaskId == task.Id)
                .Select(td => td.DeviceId.ToString())
                .ToList()
        };
    }

    // ============ 服务账号管理方法 ============

    public async Task BindServiceAccountAsync(Guid edgeNodeId, Guid? serviceUserId)
    {
        var edgeNode = await _dbContext.Set<EdgeNode>()
            .FirstOrDefaultAsync(e => e.Id == edgeNodeId && !e.DeletedFlag);

        if (edgeNode == null)
        {
            throw new InvalidOperationException("边缘节点不存在");
        }

        // 如果指定了服务账号，验证其存在且类型为 service
        if (serviceUserId.HasValue)
        {
            var serviceUser = await _dbContext.Set<User>()
                .FirstOrDefaultAsync(u => u.Id == serviceUserId.Value && !u.DeletedFlag);

            if (serviceUser == null)
            {
                throw new InvalidOperationException("服务账号不存在");
            }

            if (serviceUser.UserType != "service")
            {
                throw new InvalidOperationException("只能绑定服务类型的账号");
            }
        }

        edgeNode.ServiceUserId = serviceUserId;
        edgeNode.UpdatedAt = DateTimeOffset.UtcNow;
        edgeNode.UpdatedBy = GetCurrentUserId();

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("边缘节点 {EdgeNodeId} 已{Action}服务账号 {ServiceUserId}",
            edgeNodeId,
            serviceUserId.HasValue ? "绑定" : "解绑",
            serviceUserId);
    }

    public async Task<List<EdgeNodeDto>> GetEdgeNodesByServiceUserAsync(Guid serviceUserId)
    {
        var edgeNodes = await _dbContext.Set<EdgeNode>()
            .Where(e => e.ServiceUserId == serviceUserId && !e.DeletedFlag)
            .OrderBy(e => e.NodeName)
            .Select(e => new EdgeNodeDto
            {
                Id = e.Id.ToString(),
                NodeId = e.NodeId,
                NodeName = e.NodeName,
                Platform = e.Platform,
                Version = e.Version,
                Location = e.Location,
                IpAddress = e.IpAddress,
                Port = e.Port,
                Status = e.Status,
                PlatformConfig = e.PlatformConfig,
                ResourceLimits = e.ResourceLimits,
                OsInfo = e.OsInfo,
                HardwareInfo = e.HardwareInfo,
                InstallPath = e.InstallPath,
                LastHeartbeat = e.LastHeartbeat,
                RegistrationType = e.RegistrationType,
                DeviceCount = e.Devices.Count(d => !d.DeletedFlag),
                CreatedAt = e.CreatedAt,
                UpdatedAt = e.UpdatedAt,
                ServiceUserId = e.ServiceUserId.HasValue ? e.ServiceUserId.ToString() : null,
                ServiceUserName = e.ServiceUser != null ? e.ServiceUser.UserName : null
            })
            .ToListAsync();

        return edgeNodes;
    }
}
