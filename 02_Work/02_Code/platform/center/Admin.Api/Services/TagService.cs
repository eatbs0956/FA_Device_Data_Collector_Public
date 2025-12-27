using Admin.Api.Domain.DTOs;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Entities;

namespace Admin.Api.Services;

/// <summary>
/// 标签服务实现
/// </summary>
public class TagService : ITagService
{
    private readonly DbContext _dbContext;
    private readonly ILogger<TagService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TagService(
        DbContext dbContext,
        ILogger<TagService> logger,
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

    public async Task<TagListResponse> GetTagsAsync(TagQueryRequest request)
    {
        var query = _dbContext.Set<TagDefinition>()
            .Where(t => !t.DeletedFlag)
            .Include(t => t.Device)
            .AsQueryable();

        // 按设备ID筛选（主要筛选条件）
        if (request.DeviceId.HasValue)
        {
            query = query.Where(t => t.DeviceId == request.DeviceId.Value);
        }

        // 应用其他筛选条件
        if (!string.IsNullOrWhiteSpace(request.TagName))
        {
            query = query.Where(t => EF.Functions.ILike(t.TagName, $"%{request.TagName}%"));
        }

        if (!string.IsNullOrWhiteSpace(request.TagId))
        {
            query = query.Where(t => t.TagId == request.TagId);
        }

        if (request.Enabled.HasValue)
        {
            query = query.Where(t => t.Enabled == request.Enabled.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.DataType))
        {
            query = query.Where(t => t.DataType == request.DataType);
        }

        // 总记录数
        var total = await query.CountAsync();

        // 排序
        query = ApplySorting(query, request.SortBy, request.SortOrder);

        // 分页并查询数据
        var tags = await query
            .Skip((request.Current - 1) * request.Size)
            .Take(request.Size)
            .ToListAsync();

        // 转换为DTO
        var items = tags.Select(t => MapToDto(t)).ToList();

        return new TagListResponse
        {
            Records = items,
            Total = total,
            Current = request.Current,
            Size = request.Size
        };
    }

    public async Task<TagDto?> GetTagByIdAsync(Guid id)
    {
        var tag = await _dbContext.Set<TagDefinition>()
            .Where(t => t.Id == id && !t.DeletedFlag)
            .Include(t => t.Device)
            .FirstOrDefaultAsync();

        if (tag == null)
            return null;

        return MapToDto(tag);
    }

    public async Task<Guid> CreateTagAsync(CreateTagRequest request)
    {
        // 检查设备是否存在
        var device = await _dbContext.Set<Device>()
            .FirstOrDefaultAsync(d => d.Id == request.DeviceId && !d.DeletedFlag);

        if (device == null)
        {
            throw new InvalidOperationException("所选设备不存在");
        }

        // 检查标签标识符是否已存在（同一设备内唯一）
        var exists = await _dbContext.Set<TagDefinition>()
            .AnyAsync(t => t.DeviceId == request.DeviceId && t.TagId == request.TagId && !t.DeletedFlag);

        if (exists)
        {
            throw new InvalidOperationException($"标签标识符 '{request.TagId}' 在该设备下已存在");
        }

        var userId = GetCurrentUserId();
        var tag = new TagDefinition
        {
            Id = Guid.NewGuid(),
            TagId = request.TagId,
            DeviceId = request.DeviceId,
            TagName = request.TagName,
            TagAddress = request.TagAddress,
            DataType = request.DataType,
            Unit = request.Unit,
            Description = request.Description,
            Enabled = request.Enabled,
            MinValue = request.MinValue,
            MaxValue = request.MaxValue,
            ScalingFactor = request.ScalingFactor,
            Offset = request.Offset,
            AccessMode = request.AccessMode,
            Deadband = request.Deadband,
            CreatedBy = userId,
            CreatedAt = DateTimeOffset.UtcNow,
            TenantId = device.TenantId // 继承设备的租户ID
        };

        _dbContext.Set<TagDefinition>().Add(tag);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("标签创建成功: {TagId}, DeviceId: {DeviceId}", request.TagId, request.DeviceId);

        return tag.Id;
    }

    public async Task UpdateTagAsync(Guid id, UpdateTagRequest request)
    {
        var tag = await _dbContext.Set<TagDefinition>()
            .FirstOrDefaultAsync(t => t.Id == id && !t.DeletedFlag);

        if (tag == null)
        {
            throw new InvalidOperationException("标签不存在");
        }

        // 检查标签标识符是否与其他标签冲突（同一设备内唯一）
        var exists = await _dbContext.Set<TagDefinition>()
            .AnyAsync(t => t.DeviceId == tag.DeviceId && t.TagId == request.TagId && t.Id != id && !t.DeletedFlag);

        if (exists)
        {
            throw new InvalidOperationException($"标签标识符 '{request.TagId}' 在该设备下已存在");
        }

        var userId = GetCurrentUserId();

        tag.TagId = request.TagId;
        tag.TagName = request.TagName;
        tag.TagAddress = request.TagAddress;
        tag.DataType = request.DataType;
        tag.Unit = request.Unit;
        tag.Description = request.Description;
        tag.Enabled = request.Enabled;
        tag.MinValue = request.MinValue;
        tag.MaxValue = request.MaxValue;
        tag.ScalingFactor = request.ScalingFactor;
        tag.Offset = request.Offset;
        tag.AccessMode = request.AccessMode;
        tag.Deadband = request.Deadband;
        tag.UpdatedBy = userId;
        tag.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("标签更新成功: {TagId}", id);
    }

    public async Task DeleteTagAsync(Guid id)
    {
        var tag = await _dbContext.Set<TagDefinition>()
            .FirstOrDefaultAsync(t => t.Id == id && !t.DeletedFlag);

        if (tag == null)
        {
            throw new InvalidOperationException("标签不存在");
        }

        var userId = GetCurrentUserId();
        tag.DeletedFlag = true;
        tag.UpdatedBy = userId;
        tag.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("标签删除成功: {TagId}", id);
    }

    public async Task BatchDeleteAsync(List<Guid> ids)
    {
        if (ids == null || ids.Count == 0)
        {
            throw new ArgumentException("请选择要删除的标签");
        }

        var tags = await _dbContext.Set<TagDefinition>()
            .Where(t => ids.Contains(t.Id) && !t.DeletedFlag)
            .ToListAsync();

        if (tags.Count == 0)
        {
            throw new InvalidOperationException("未找到要删除的标签");
        }

        var userId = GetCurrentUserId();
        var now = DateTimeOffset.UtcNow;

        foreach (var tag in tags)
        {
            tag.DeletedFlag = true;
            tag.UpdatedBy = userId;
            tag.UpdatedAt = now;
        }

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("批量删除标签成功, 数量: {Count}", tags.Count);
    }

    public async Task ToggleEnabledAsync(Guid id, bool enabled)
    {
        var tag = await _dbContext.Set<TagDefinition>()
            .FirstOrDefaultAsync(t => t.Id == id && !t.DeletedFlag);

        if (tag == null)
        {
            throw new InvalidOperationException("标签不存在");
        }

        var userId = GetCurrentUserId();
        tag.Enabled = enabled;
        tag.UpdatedBy = userId;
        tag.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("标签状态更新成功: {TagId}, Enabled: {Enabled}", id, enabled);
    }

    public async Task BatchToggleEnabledAsync(List<Guid> ids, bool enabled)
    {
        if (ids == null || ids.Count == 0)
        {
            throw new ArgumentException("请选择要操作的标签");
        }

        var tags = await _dbContext.Set<TagDefinition>()
            .Where(t => ids.Contains(t.Id) && !t.DeletedFlag)
            .ToListAsync();

        if (tags.Count == 0)
        {
            throw new InvalidOperationException("未找到要操作的标签");
        }

        var userId = GetCurrentUserId();
        var now = DateTimeOffset.UtcNow;

        foreach (var tag in tags)
        {
            tag.Enabled = enabled;
            tag.UpdatedBy = userId;
            tag.UpdatedAt = now;
        }

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("批量更新标签状态成功, 数量: {Count}, Enabled: {Enabled}", tags.Count, enabled);
    }

    public async Task<List<TagDto>> GetTagsByDeviceIdAsync(Guid deviceId)
    {
        var tags = await _dbContext.Set<TagDefinition>()
            .Where(t => t.DeviceId == deviceId && !t.DeletedFlag)
            .Include(t => t.Device)
            .OrderBy(t => t.TagName)
            .ToListAsync();

        return tags.Select(t => MapToDto(t)).ToList();
    }

    public async Task<int> BatchImportAsync(Guid deviceId, List<CreateTagRequest> tags)
    {
        if (tags == null || tags.Count == 0)
        {
            throw new ArgumentException("导入数据为空");
        }

        // 检查设备是否存在
        var device = await _dbContext.Set<Device>()
            .FirstOrDefaultAsync(d => d.Id == deviceId && !d.DeletedFlag);

        if (device == null)
        {
            throw new InvalidOperationException("所选设备不存在");
        }

        // 获取该设备现有的标签ID
        var existingTagIds = await _dbContext.Set<TagDefinition>()
            .Where(t => t.DeviceId == deviceId && !t.DeletedFlag)
            .Select(t => t.TagId)
            .ToListAsync();

        var userId = GetCurrentUserId();
        var now = DateTimeOffset.UtcNow;
        var importCount = 0;

        foreach (var tagRequest in tags)
        {
            // 跳过已存在的标签
            if (existingTagIds.Contains(tagRequest.TagId))
            {
                _logger.LogWarning("标签标识符已存在，跳过: {TagId}", tagRequest.TagId);
                continue;
            }

            var tag = new TagDefinition
            {
                Id = Guid.NewGuid(),
                TagId = tagRequest.TagId,
                DeviceId = deviceId,
                TagName = tagRequest.TagName,
                TagAddress = tagRequest.TagAddress,
                DataType = tagRequest.DataType,
                Unit = tagRequest.Unit,
                Description = tagRequest.Description,
                Enabled = tagRequest.Enabled,
                MinValue = tagRequest.MinValue,
                MaxValue = tagRequest.MaxValue,
                ScalingFactor = tagRequest.ScalingFactor,
                Offset = tagRequest.Offset,
                AccessMode = tagRequest.AccessMode,
                Deadband = tagRequest.Deadband,
                CreatedBy = userId,
                CreatedAt = now,
                TenantId = device.TenantId
            };

            _dbContext.Set<TagDefinition>().Add(tag);
            existingTagIds.Add(tagRequest.TagId); // 防止同批次重复
            importCount++;
        }

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("批量导入标签成功, 设备: {DeviceId}, 导入数量: {Count}", deviceId, importCount);

        return importCount;
    }

    /// <summary>
    /// 应用排序
    /// </summary>
    private static IQueryable<TagDefinition> ApplySorting(IQueryable<TagDefinition> query, string? sortBy, string? sortOrder)
    {
        var isDescending = sortOrder?.ToLower() == "desc";

        return sortBy?.ToLower() switch
        {
            "tagname" => isDescending ? query.OrderByDescending(t => t.TagName) : query.OrderBy(t => t.TagName),
            "tagid" => isDescending ? query.OrderByDescending(t => t.TagId) : query.OrderBy(t => t.TagId),
            "datatype" => isDescending ? query.OrderByDescending(t => t.DataType) : query.OrderBy(t => t.DataType),
            "enabled" => isDescending ? query.OrderByDescending(t => t.Enabled) : query.OrderBy(t => t.Enabled),
            _ => isDescending ? query.OrderByDescending(t => t.CreatedAt) : query.OrderBy(t => t.CreatedAt)
        };
    }

    /// <summary>
    /// 实体转DTO
    /// </summary>
    private static TagDto MapToDto(TagDefinition tag)
    {
        return new TagDto
        {
            Id = tag.Id,
            TagId = tag.TagId,
            DeviceId = tag.DeviceId,
            DeviceName = tag.Device?.DeviceName ?? string.Empty,
            ProtocolType = tag.Device?.ProtocolType ?? string.Empty,
            TagName = tag.TagName,
            TagAddress = tag.TagAddress,
            DataType = tag.DataType,
            Unit = tag.Unit,
            Description = tag.Description,
            Enabled = tag.Enabled,
            MinValue = tag.MinValue,
            MaxValue = tag.MaxValue,
            ScalingFactor = tag.ScalingFactor,
            Offset = tag.Offset,
            AccessMode = tag.AccessMode,
            Deadband = tag.Deadband,
            TenantId = tag.TenantId,
            CreatedBy = tag.CreatedBy,
            CreatedAt = tag.CreatedAt,
            UpdatedBy = tag.UpdatedBy,
            UpdatedAt = tag.UpdatedAt ?? tag.CreatedAt
        };
    }
}
