using Admin.Api.Domain.DTOs;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Entities;
using System.Diagnostics;
using System.Text.Json;

namespace Admin.Api.Services;

/// <summary>
/// 设备服务实现
/// </summary>
public class DeviceService : IDeviceService
{
    private readonly DbContext _dbContext;
    private readonly ILogger<DeviceService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DeviceService(
        DbContext dbContext,
        ILogger<DeviceService> logger,
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

    public async Task<DeviceListResponse> GetDevicesAsync(DeviceQueryRequest request)
    {
        var query = _dbContext.Set<Shared.Domain.Entities.Device>()
            .Where(d => !d.DeletedFlag)
            .Include(d => d.EdgeNode)
            .Include(d => d.Group)
            .AsQueryable();

        // 应用筛选条件
        if (!string.IsNullOrWhiteSpace(request.DeviceName))
        {
            query = query.Where(d => EF.Functions.ILike(d.DeviceName, $"%{request.DeviceName}%"));
        }

        if (!string.IsNullOrWhiteSpace(request.DeviceId))
        {
            query = query.Where(d => d.DeviceId == request.DeviceId);
        }

        if (!string.IsNullOrWhiteSpace(request.ProtocolType))
        {
            query = query.Where(d => d.ProtocolType == request.ProtocolType);
        }

        if (!string.IsNullOrWhiteSpace(request.DeviceType))
        {
            query = query.Where(d => d.DeviceType == request.DeviceType);
        }

        if (!string.IsNullOrWhiteSpace(request.ConnectionStatus))
        {
            query = query.Where(d => d.ConnectionStatus == request.ConnectionStatus);
        }

        if (request.EdgeNodeId.HasValue)
        {
            query = query.Where(d => d.EdgeNodeId == request.EdgeNodeId.Value);
        }

        if (request.GroupId.HasValue)
        {
            query = query.Where(d => d.GroupId == request.GroupId.Value);
        }

        if (request.Enabled.HasValue)
        {
            query = query.Where(d => d.Enabled == request.Enabled.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Location))
        {
            query = query.Where(d => d.Location != null && EF.Functions.ILike(d.Location, $"%{request.Location}%"));
        }

        if (!string.IsNullOrWhiteSpace(request.Vendor))
        {
            query = query.Where(d => d.Vendor != null && EF.Functions.ILike(d.Vendor, $"%{request.Vendor}%"));
        }

        // 总记录数
        var total = await query.CountAsync();

        // 排序
        query = ApplySorting(query, request.SortBy, request.SortOrder);

        // 分页并查询数据（先加载到内存，避免 EF Core 无法转换 CountTagsFromJson 方法）
        var devices = await query
            .Skip((request.Current - 1) * request.Size)
            .Take(request.Size)
            .Include(d => d.EdgeNode)
            .Include(d => d.Group)
            .ToListAsync();

        // 在内存中执行投影
        var items = devices.Select(d => new DeviceDto
        {
            Id = d.Id,
            DeviceId = d.DeviceId,
            DeviceName = d.DeviceName,
            DeviceType = d.DeviceType,
            ProtocolType = d.ProtocolType,
            EdgeNodeId = d.EdgeNodeId,
            EdgeNodeName = d.EdgeNode?.NodeName,
            ConnectionConfig = d.ConnectionConfig,
            ProtocolConfig = d.ProtocolConfig,
            ConnectionStatus = d.ConnectionStatus,
            LastConnectTime = d.LastConnectTime,
            ErrorCount = d.ErrorCount,
            LastError = d.LastError,
            TagCount = CountTagsFromJson(d.TagsConfig),
            Vendor = d.Vendor,
            Model = d.Model,
            FirmwareVersion = d.FirmwareVersion,
            Description = d.Description,
            Location = d.Location,
            GroupId = d.GroupId,
            GroupName = d.Group?.Name,
            Enabled = d.Enabled,
            TenantId = d.TenantId,
            CreatedBy = d.CreatedBy,
            CreatedAt = d.CreatedAt,
            UpdatedBy = d.UpdatedBy,
            UpdatedAt = d.UpdatedAt ?? d.CreatedAt
        }).ToList();

        return new DeviceListResponse
        {
            Records = items,
            Total = total,
            Current = request.Current,
            Size = request.Size
        };
    }

    public async Task<DeviceDto?> GetDeviceByIdAsync(Guid id)
    {
        var device = await _dbContext.Set<Shared.Domain.Entities.Device>()
            .Where(d => d.Id == id && !d.DeletedFlag)
            .Include(d => d.EdgeNode)
            .Include(d => d.Group)
            .FirstOrDefaultAsync();

        if (device == null)
            return null;

        return new DeviceDto
        {
            Id = device.Id,
            DeviceId = device.DeviceId,
            DeviceName = device.DeviceName,
            DeviceType = device.DeviceType,
            ProtocolType = device.ProtocolType,
            EdgeNodeId = device.EdgeNodeId,
            EdgeNodeName = device.EdgeNode?.NodeName,
            ConnectionConfig = device.ConnectionConfig,
            ProtocolConfig = device.ProtocolConfig,
            ConnectionStatus = device.ConnectionStatus,
            LastConnectTime = device.LastConnectTime,
            ErrorCount = device.ErrorCount,
            LastError = device.LastError,
            TagCount = CountTagsFromJson(device.TagsConfig),
            Vendor = device.Vendor,
            Model = device.Model,
            FirmwareVersion = device.FirmwareVersion,
            Description = device.Description,
            Location = device.Location,
            GroupId = device.GroupId,
            GroupName = device.Group?.Name,
            Enabled = device.Enabled,
            TenantId = device.TenantId,
            CreatedBy = device.CreatedBy,
            CreatedAt = device.CreatedAt,
            UpdatedBy = device.UpdatedBy,
            UpdatedAt = device.UpdatedAt ?? device.CreatedAt
        };
    }

    public async Task<Guid> CreateDeviceAsync(CreateDeviceRequest request)
    {
        // 检查设备编码是否已存在
        var exists = await _dbContext.Set<Shared.Domain.Entities.Device>()
            .AnyAsync(d => d.DeviceId == request.DeviceId && !d.DeletedFlag);

        if (exists)
        {
            throw new InvalidOperationException($"设备编码 '{request.DeviceId}' 已存在");
        }

        // 验证边缘节点是否存在（仅当指定了边缘节点时）
        if (request.EdgeNodeId.HasValue)
        {
            var edgeNodeExists = await _dbContext.Set<EdgeNode>()
                .AnyAsync(e => e.Id == request.EdgeNodeId.Value && !e.DeletedFlag);

            if (!edgeNodeExists)
            {
                throw new InvalidOperationException($"边缘节点不存在");
            }
        }

        // 如果指定了分组,验证分组是否存在
        if (request.GroupId.HasValue)
        {
            var groupExists = await _dbContext.Set<DeviceGroup>()
                .AnyAsync(g => g.Id == request.GroupId.Value && !g.DeletedFlag);

            if (!groupExists)
            {
                throw new InvalidOperationException($"设备分组不存在");
            }
        }

        var device = new Shared.Domain.Entities.Device
        {
            Id = Guid.NewGuid(),
            DeviceId = request.DeviceId,
            DeviceName = request.DeviceName,
            DeviceType = request.DeviceType,
            ProtocolType = request.ProtocolType,
            EdgeNodeId = request.EdgeNodeId,
            ConnectionConfig = request.ConnectionConfig,
            ProtocolConfig = request.ProtocolConfig,
            TagsConfig = request.TagsConfig,
            Vendor = request.Vendor,
            Model = request.Model,
            FirmwareVersion = request.FirmwareVersion,
            Description = request.Description,
            Location = request.Location,
            GroupId = request.GroupId,
            Enabled = request.Enabled,
            ConnectionStatus = "Disconnected",
            ErrorCount = 0,
            CreatedBy = GetCurrentUserId(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.Set<Shared.Domain.Entities.Device>().Add(device);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("设备创建成功: {DeviceId} ({DeviceName})", device.DeviceId, device.DeviceName);

        return device.Id;
    }

    public async Task UpdateDeviceAsync(Guid id, UpdateDeviceRequest request)
    {
        var device = await _dbContext.Set<Shared.Domain.Entities.Device>()
            .FirstOrDefaultAsync(d => d.Id == id && !d.DeletedFlag);

        if (device == null)
        {
            throw new InvalidOperationException("设备不存在");
        }

        // 如果更新边缘节点,验证其存在性
        if (request.EdgeNodeId.HasValue)
        {
            var edgeNodeExists = await _dbContext.Set<EdgeNode>()
                .AnyAsync(e => e.Id == request.EdgeNodeId.Value && !e.DeletedFlag);

            if (!edgeNodeExists)
            {
                throw new InvalidOperationException("边缘节点不存在");
            }
        }

        // 如果更新分组,验证其存在性
        if (request.GroupId.HasValue)
        {
            var groupExists = await _dbContext.Set<DeviceGroup>()
                .AnyAsync(g => g.Id == request.GroupId.Value && !g.DeletedFlag);

            if (!groupExists)
            {
                throw new InvalidOperationException("设备分组不存在");
            }
        }

        // 更新字段（仅更新非null的字段）
        if (!string.IsNullOrWhiteSpace(request.DeviceName))
            device.DeviceName = request.DeviceName;

        if (!string.IsNullOrWhiteSpace(request.DeviceType))
            device.DeviceType = request.DeviceType;

        if (!string.IsNullOrWhiteSpace(request.ProtocolType))
            device.ProtocolType = request.ProtocolType;

        if (request.EdgeNodeId.HasValue)
            device.EdgeNodeId = request.EdgeNodeId.Value;

        if (!string.IsNullOrWhiteSpace(request.ConnectionConfig))
            device.ConnectionConfig = request.ConnectionConfig;

        if (!string.IsNullOrWhiteSpace(request.ProtocolConfig))
            device.ProtocolConfig = request.ProtocolConfig;

        if (!string.IsNullOrWhiteSpace(request.TagsConfig))
            device.TagsConfig = request.TagsConfig;

        if (request.Vendor != null)
            device.Vendor = request.Vendor;

        if (request.Model != null)
            device.Model = request.Model;

        if (request.FirmwareVersion != null)
            device.FirmwareVersion = request.FirmwareVersion;

        if (request.Description != null)
            device.Description = request.Description;

        if (request.Location != null)
            device.Location = request.Location;

        if (request.GroupId.HasValue)
            device.GroupId = request.GroupId;

        if (request.Enabled.HasValue)
            device.Enabled = request.Enabled.Value;

        device.UpdatedBy = GetCurrentUserId();
        device.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("设备更新成功: {DeviceId}", id);
    }

    public async Task DeleteDeviceAsync(Guid id)
    {
        var device = await _dbContext.Set<Shared.Domain.Entities.Device>()
            .FirstOrDefaultAsync(d => d.Id == id && !d.DeletedFlag);

        if (device == null)
        {
            throw new InvalidOperationException("设备不存在");
        }

        device.DeletedFlag = true;
        device.UpdatedBy = GetCurrentUserId();
        device.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("设备删除成功: {DeviceId}", id);
    }

    public async Task BatchDeleteAsync(List<Guid> ids)
    {
        var devices = await _dbContext.Set<Shared.Domain.Entities.Device>()
            .Where(d => ids.Contains(d.Id) && !d.DeletedFlag)
            .ToListAsync();

        var userId = GetCurrentUserId();
        var now = DateTimeOffset.UtcNow;

        foreach (var device in devices)
        {
            device.DeletedFlag = true;
            device.UpdatedBy = userId;
            device.UpdatedAt = now;
        }

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("批量删除设备成功, 数量: {Count}", devices.Count);
    }

    public async Task<DeviceConnectionTestResult> TestConnectionAsync(Guid id)
    {
        var device = await _dbContext.Set<Shared.Domain.Entities.Device>()
            .FirstOrDefaultAsync(d => d.Id == id && !d.DeletedFlag);

        if (device == null)
        {
            throw new InvalidOperationException("设备不存在");
        }

        var stopwatch = Stopwatch.StartNew();

        try
        {
            // TODO: 实现真实的设备连接测试逻辑
            // 这里仅作为示例，返回模拟结果
            await Task.Delay(100); // 模拟网络延迟

            stopwatch.Stop();

            // 模拟成功结果
            return new DeviceConnectionTestResult
            {
                Success = true,
                ResponseTime = (int)stopwatch.ElapsedMilliseconds,
                Message = "设备连接正常",
                Protocol = device.ProtocolType,
                ServerInfo = new Dictionary<string, string>
                {
                    { "DeviceId", device.DeviceId },
                    { "DeviceName", device.DeviceName },
                    { "Vendor", device.Vendor ?? "Unknown" }
                },
                TestedAt = DateTimeOffset.UtcNow
            };
        }
        catch (Exception ex)
        {
            stopwatch.Stop();

            _logger.LogError(ex, "设备连接测试失败: {DeviceId}", id);

            return new DeviceConnectionTestResult
            {
                Success = false,
                ResponseTime = (int)stopwatch.ElapsedMilliseconds,
                Message = "连接失败",
                Protocol = device.ProtocolType,
                ErrorDetails = ex.Message,
                TestedAt = DateTimeOffset.UtcNow
            };
        }
    }

    public async Task ToggleEnabledAsync(Guid id, bool enabled)
    {
        var device = await _dbContext.Set<Shared.Domain.Entities.Device>()
            .FirstOrDefaultAsync(d => d.Id == id && !d.DeletedFlag);

        if (device == null)
        {
            throw new InvalidOperationException("设备不存在");
        }

        device.Enabled = enabled;
        device.UpdatedBy = GetCurrentUserId();
        device.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("设备状态切换成功: {DeviceId}, Enabled: {Enabled}", id, enabled);
    }

    /// <summary>
    /// 应用排序
    /// </summary>
    private IQueryable<Shared.Domain.Entities.Device> ApplySorting(
        IQueryable<Shared.Domain.Entities.Device> query,
        string? sortBy,
        string? sortOrder)
    {
        var isDescending = sortOrder?.ToLower() == "desc";

        return (sortBy?.ToLower()) switch
        {
            "devicename" => isDescending
                ? query.OrderByDescending(d => d.DeviceName)
                : query.OrderBy(d => d.DeviceName),
            "lastconnecttime" => isDescending
                ? query.OrderByDescending(d => d.LastConnectTime)
                : query.OrderBy(d => d.LastConnectTime),
            "connectionstatus" => isDescending
                ? query.OrderByDescending(d => d.ConnectionStatus)
                : query.OrderBy(d => d.ConnectionStatus),
            _ => isDescending
                ? query.OrderByDescending(d => d.CreatedAt)
                : query.OrderBy(d => d.CreatedAt)
        };
    }

    /// <summary>
    /// 从JSON字符串中统计标签数量
    /// </summary>
    private static int CountTagsFromJson(string? tagsConfigJson)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(tagsConfigJson))
                return 0;

            var tags = JsonSerializer.Deserialize<JsonElement>(tagsConfigJson);
            return tags.ValueKind == JsonValueKind.Array ? tags.GetArrayLength() : 0;
        }
        catch
        {
            return 0;
        }
    }
}
