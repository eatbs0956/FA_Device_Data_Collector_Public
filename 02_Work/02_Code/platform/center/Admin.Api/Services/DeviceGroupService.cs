using Admin.Api.Domain.DTOs;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Entities;

namespace Admin.Api.Services;

/// <summary>
/// 设备分组服务实现
/// </summary>
public class DeviceGroupService : IDeviceGroupService
{
    private readonly DbContext _dbContext;
    private readonly ILogger<DeviceGroupService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public DeviceGroupService(
        DbContext dbContext,
        ILogger<DeviceGroupService> logger,
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

    /// <summary>
    /// 最大层级限制
    /// </summary>
    private const int MaxLevel = 4;

    public async Task<DeviceGroupListResponse> GetGroupsAsync(DeviceGroupQueryRequest request)
    {
        var query = _dbContext.Set<DeviceGroup>()
            .Where(g => !g.DeletedFlag);

        // 按父分组ID筛选
        if (request.ParentId.HasValue)
        {
            query = query.Where(g => g.ParentId == request.ParentId.Value);
        }
        else if (request.TopLevelOnly == true)
        {
            // 只查询顶级分组
            query = query.Where(g => g.ParentId == null);
        }

        // 按名称搜索
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            query = query.Where(g => g.Name.Contains(request.Name));
        }

        // 获取总数
        var total = await query.CountAsync();

        // 排序
        query = query.OrderBy(g => g.SortOrder).ThenBy(g => g.Name);

        // 分页
        var groups = await query
            .Skip((request.Current - 1) * request.Size)
            .Take(request.Size)
            .ToListAsync();

        // 获取设备数量
        var groupIds = groups.Select(g => g.Id).ToList();
        var deviceCounts = await _dbContext.Set<Shared.Domain.Entities.Device>()
            .Where(d => !d.DeletedFlag && d.GroupId != null && groupIds.Contains(d.GroupId.Value))
            .GroupBy(d => d.GroupId)
            .Select(g => new { GroupId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.GroupId!.Value, x => x.Count);

        // 获取子分组数量
        var childCounts = await _dbContext.Set<DeviceGroup>()
            .Where(g => !g.DeletedFlag && g.ParentId != null && groupIds.Contains(g.ParentId.Value))
            .GroupBy(g => g.ParentId)
            .Select(g => new { ParentId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.ParentId!.Value, x => x.Count);

        var items = groups.Select(g => new DeviceGroupDto
        {
            Id = g.Id,
            Name = g.Name,
            ParentId = g.ParentId,
            Level = g.Level,
            SortOrder = g.SortOrder,
            Description = g.Description,
            DeviceCount = deviceCounts.GetValueOrDefault(g.Id, 0),
            ChildCount = childCounts.GetValueOrDefault(g.Id, 0),
            TenantId = g.TenantId,
            CreatedBy = g.CreatedBy,
            CreatedAt = g.CreatedAt,
            UpdatedBy = g.UpdatedBy,
            UpdatedAt = g.UpdatedAt
        }).ToList();

        return new DeviceGroupListResponse
        {
            Records = items,
            Total = total
        };
    }

    public async Task<List<DeviceGroupDto>> GetGroupTreeAsync()
    {
        // 获取所有分组
        var allGroups = await _dbContext.Set<DeviceGroup>()
            .Where(g => !g.DeletedFlag)
            .OrderBy(g => g.SortOrder)
            .ThenBy(g => g.Name)
            .ToListAsync();

        // 获取每个分组的设备数量
        var deviceCounts = await _dbContext.Set<Shared.Domain.Entities.Device>()
            .Where(d => !d.DeletedFlag && d.GroupId != null)
            .GroupBy(d => d.GroupId)
            .Select(g => new { GroupId = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.GroupId!.Value, x => x.Count);

        // 转换为 DTO
        var groupDtos = allGroups.Select(g => new DeviceGroupDto
        {
            Id = g.Id,
            Name = g.Name,
            ParentId = g.ParentId,
            Level = g.Level,
            SortOrder = g.SortOrder,
            Description = g.Description,
            DeviceCount = deviceCounts.GetValueOrDefault(g.Id, 0),
            TenantId = g.TenantId,
            CreatedBy = g.CreatedBy,
            CreatedAt = g.CreatedAt,
            UpdatedBy = g.UpdatedBy,
            UpdatedAt = g.UpdatedAt
        }).ToList();

        // 构建树形结构
        return BuildTree(groupDtos);
    }

    public async Task<List<DeviceGroupTreeNode>> GetGroupTreeSimpleAsync()
    {
        var allGroups = await _dbContext.Set<DeviceGroup>()
            .Where(g => !g.DeletedFlag)
            .OrderBy(g => g.SortOrder)
            .ThenBy(g => g.Name)
            .Select(g => new DeviceGroupTreeNode
            {
                Id = g.Id,
                Name = g.Name,
                ParentId = g.ParentId,
                Level = g.Level,
                SortOrder = g.SortOrder
            })
            .ToListAsync();

        return BuildTreeSimple(allGroups);
    }

    public async Task<DeviceGroupDto?> GetGroupByIdAsync(Guid id)
    {
        var group = await _dbContext.Set<DeviceGroup>()
            .Where(g => g.Id == id && !g.DeletedFlag)
            .FirstOrDefaultAsync();

        if (group == null)
            return null;

        // 获取该分组的设备数量
        var deviceCount = await _dbContext.Set<Shared.Domain.Entities.Device>()
            .CountAsync(d => !d.DeletedFlag && d.GroupId == id);

        return new DeviceGroupDto
        {
            Id = group.Id,
            Name = group.Name,
            ParentId = group.ParentId,
            Level = group.Level,
            SortOrder = group.SortOrder,
            Description = group.Description,
            DeviceCount = deviceCount,
            TenantId = group.TenantId,
            CreatedBy = group.CreatedBy,
            CreatedAt = group.CreatedAt,
            UpdatedBy = group.UpdatedBy,
            UpdatedAt = group.UpdatedAt
        };
    }

    public async Task<Guid> CreateGroupAsync(CreateDeviceGroupRequest request)
    {
        // 验证父分组是否存在
        int level = 1;
        if (request.ParentId.HasValue)
        {
            var parent = await _dbContext.Set<DeviceGroup>()
                .FirstOrDefaultAsync(g => g.Id == request.ParentId.Value && !g.DeletedFlag);

            if (parent == null)
            {
                throw new InvalidOperationException("父分组不存在");
            }

            level = parent.Level + 1;

            // 验证层级限制
            if (level > MaxLevel)
            {
                throw new InvalidOperationException($"分组层级不能超过 {MaxLevel} 级");
            }
        }

        // 检查同级下名称是否重复
        var nameExists = await _dbContext.Set<DeviceGroup>()
            .AnyAsync(g => g.ParentId == request.ParentId 
                        && g.Name == request.Name 
                        && !g.DeletedFlag);

        if (nameExists)
        {
            throw new InvalidOperationException($"同级分组下已存在名称 '{request.Name}'");
        }

        var group = new DeviceGroup
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            ParentId = request.ParentId,
            Level = level,
            SortOrder = request.SortOrder,
            Description = request.Description,
            CreatedBy = GetCurrentUserId(),
            CreatedAt = DateTimeOffset.UtcNow
        };

        _dbContext.Set<DeviceGroup>().Add(group);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("设备分组创建成功: {GroupId} ({GroupName})", group.Id, group.Name);

        return group.Id;
    }

    public async Task UpdateGroupAsync(Guid id, UpdateDeviceGroupRequest request)
    {
        var group = await _dbContext.Set<DeviceGroup>()
            .FirstOrDefaultAsync(g => g.Id == id && !g.DeletedFlag);

        if (group == null)
        {
            throw new InvalidOperationException("分组不存在");
        }

        // 如果更新父分组
        if (request.UpdateParent)
        {
            // 不能将分组移动到自己或自己的子分组下
            if (request.ParentId == id)
            {
                throw new InvalidOperationException("不能将分组移动到自身");
            }

            if (request.ParentId.HasValue)
            {
                // 检查是否移动到自己的子分组下（会形成循环）
                var isDescendant = await IsDescendantAsync(request.ParentId.Value, id);
                if (isDescendant)
                {
                    throw new InvalidOperationException("不能将分组移动到其子分组下");
                }

                var parent = await _dbContext.Set<DeviceGroup>()
                    .FirstOrDefaultAsync(g => g.Id == request.ParentId.Value && !g.DeletedFlag);

                if (parent == null)
                {
                    throw new InvalidOperationException("目标父分组不存在");
                }

                group.ParentId = request.ParentId;
                group.Level = parent.Level + 1;
            }
            else
            {
                group.ParentId = null;
                group.Level = 1;
            }

            // 更新所有子分组的层级
            await UpdateChildrenLevelAsync(id, group.Level);
        }

        // 如果更新名称，检查同级重名
        if (!string.IsNullOrWhiteSpace(request.Name) && request.Name != group.Name)
        {
            var nameExists = await _dbContext.Set<DeviceGroup>()
                .AnyAsync(g => g.ParentId == group.ParentId 
                            && g.Name == request.Name 
                            && g.Id != id
                            && !g.DeletedFlag);

            if (nameExists)
            {
                throw new InvalidOperationException($"同级分组下已存在名称 '{request.Name}'");
            }

            group.Name = request.Name;
        }

        if (request.SortOrder.HasValue)
        {
            group.SortOrder = request.SortOrder.Value;
        }

        if (request.Description != null)
        {
            group.Description = request.Description;
        }

        group.UpdatedBy = GetCurrentUserId();
        group.UpdatedAt = DateTimeOffset.UtcNow;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("设备分组更新成功: {GroupId}", id);
    }

    public async Task DeleteGroupAsync(Guid id)
    {
        var group = await _dbContext.Set<DeviceGroup>()
            .FirstOrDefaultAsync(g => g.Id == id && !g.DeletedFlag);

        if (group == null)
        {
            throw new InvalidOperationException("分组不存在");
        }

        // 软删除该分组及其所有子分组（级联删除）
        await SoftDeleteGroupAndChildrenAsync(id);

        _logger.LogInformation("设备分组删除成功（含子分组）: {GroupId}", id);
    }

    public async Task MoveGroupAsync(Guid id, MoveDeviceGroupRequest request)
    {
        var updateRequest = new UpdateDeviceGroupRequest
        {
            ParentId = request.TargetParentId,
            UpdateParent = true,
            SortOrder = request.TargetSortOrder
        };

        await UpdateGroupAsync(id, updateRequest);
    }

    #region Private Methods

    /// <summary>
    /// 构建树形结构（完整版）
    /// </summary>
    private List<DeviceGroupDto> BuildTree(List<DeviceGroupDto> groups)
    {
        var lookup = groups.ToLookup(g => g.ParentId);
        
        foreach (var group in groups)
        {
            group.Children = lookup[group.Id].ToList();
        }

        return groups.Where(g => g.ParentId == null).ToList();
    }

    /// <summary>
    /// 构建树形结构（简化版）
    /// </summary>
    private List<DeviceGroupTreeNode> BuildTreeSimple(List<DeviceGroupTreeNode> groups)
    {
        var lookup = groups.ToLookup(g => g.ParentId);
        
        foreach (var group in groups)
        {
            group.Children = lookup[group.Id].ToList();
        }

        return groups.Where(g => g.ParentId == null).ToList();
    }

    /// <summary>
    /// 检查 targetId 是否是 ancestorId 的后代
    /// </summary>
    private async Task<bool> IsDescendantAsync(Guid targetId, Guid ancestorId)
    {
        var target = await _dbContext.Set<DeviceGroup>()
            .FirstOrDefaultAsync(g => g.Id == targetId && !g.DeletedFlag);

        while (target != null && target.ParentId.HasValue)
        {
            if (target.ParentId == ancestorId)
                return true;

            target = await _dbContext.Set<DeviceGroup>()
                .FirstOrDefaultAsync(g => g.Id == target.ParentId.Value && !g.DeletedFlag);
        }

        return false;
    }

    /// <summary>
    /// 更新所有子分组的层级
    /// </summary>
    private async Task UpdateChildrenLevelAsync(Guid parentId, int parentLevel)
    {
        var children = await _dbContext.Set<DeviceGroup>()
            .Where(g => g.ParentId == parentId && !g.DeletedFlag)
            .ToListAsync();

        foreach (var child in children)
        {
            child.Level = parentLevel + 1;
            child.UpdatedAt = DateTimeOffset.UtcNow;
            
            // 递归更新子分组
            await UpdateChildrenLevelAsync(child.Id, child.Level);
        }

        await _dbContext.SaveChangesAsync();
    }

    /// <summary>
    /// 软删除分组及其所有子分组
    /// </summary>
    private async Task SoftDeleteGroupAndChildrenAsync(Guid groupId)
    {
        var userId = GetCurrentUserId();
        var now = DateTimeOffset.UtcNow;

        // 获取该分组
        var group = await _dbContext.Set<DeviceGroup>()
            .FirstOrDefaultAsync(g => g.Id == groupId && !g.DeletedFlag);

        if (group != null)
        {
            group.DeletedFlag = true;
            group.UpdatedBy = userId;
            group.UpdatedAt = now;

            // 递归删除子分组
            var children = await _dbContext.Set<DeviceGroup>()
                .Where(g => g.ParentId == groupId && !g.DeletedFlag)
                .ToListAsync();

            foreach (var child in children)
            {
                await SoftDeleteGroupAndChildrenAsync(child.Id);
            }
        }

        await _dbContext.SaveChangesAsync();
    }

    #endregion
}
