using Admin.Api.Domain.DTOs;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Entities;

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
                UpdatedAt = e.UpdatedAt
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
                UpdatedAt = e.UpdatedAt
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
}
