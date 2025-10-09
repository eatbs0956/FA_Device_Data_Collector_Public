using Auth.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Auth.Api.Services;

/// <summary>
/// 角色管理服务 - 提供角色的CRUD操作和业务逻辑处理
/// </summary>
public class RoleService
{
    private readonly AuthDbContext _db;

    public RoleService(AuthDbContext db)
    {
        _db = db;
    }

    /// <summary>
    /// 获取角色分页列表
    /// </summary>
    /// <param name="current">当前页码</param>
    /// <param name="size">每页数量</param>
    /// <param name="roleName">角色名称(模糊搜索)</param>
    /// <param name="roleCode">角色编码(模糊搜索)</param>
    /// <param name="status">角色状态</param>
    /// <returns>角色列表和总数</returns>
    public async Task<(List<RoleDto> Items, int Total)> GetRoleListAsync(
        int current = 1, 
        int size = 10, 
        string? roleName = null, 
        string? roleCode = null, 
        int? status = null)
    {
        // 参数校验
        current = current <= 0 ? 1 : current;
        size = size <= 0 ? 10 : size;

        // 构建查询
        var query = _db.Roles.AsQueryable();

        // 角色名称模糊搜索
        if (!string.IsNullOrWhiteSpace(roleName))
        {
            query = query.Where(x => x.Name.Contains(roleName));
        }

        // 角色编码模糊搜索
        if (!string.IsNullOrWhiteSpace(roleCode))
        {
            query = query.Where(x => x.Code.Contains(roleCode));
        }

        // 状态筛选
        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        // 获取总数
        var total = await query.CountAsync();

        // 分页查询
        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((current - 1) * size)
            .Take(size)
            .Select(x => new RoleDto
            {
                Id = x.Id,
                RoleName = x.Name,
                RoleCode = x.Code,
                RoleDesc = x.Description,
                Status = x.Status
            })
            .ToListAsync();

        return (items, total);
    }

    /// <summary>
    /// 根据ID获取角色详情
    /// </summary>
    /// <param name="id">角色ID</param>
    /// <returns>角色信息,不存在返回null</returns>
    public async Task<Role?> GetRoleByIdAsync(Guid id)
    {
        return await _db.Roles.FindAsync(id);
    }

    /// <summary>
    /// 创建新角色
    /// </summary>
    /// <param name="roleName">角色名称</param>
    /// <param name="roleCode">角色编码</param>
    /// <param name="roleDesc">角色描述</param>
    /// <param name="status">角色状态</param>
    /// <returns>创建的角色</returns>
    /// <exception cref="InvalidOperationException">角色编码已存在时抛出</exception>
    public async Task<Role> CreateRoleAsync(string roleName, string roleCode, string roleDesc = "", int status = 1)
    {
        // 验证必填字段
        if (string.IsNullOrWhiteSpace(roleName))
        {
            throw new ArgumentException("角色名称不能为空", nameof(roleName));
        }

        if (string.IsNullOrWhiteSpace(roleCode))
        {
            throw new ArgumentException("角色编码不能为空", nameof(roleCode));
        }

        // 检查编码是否已存在
        var exists = await _db.Roles.AnyAsync(x => x.Code == roleCode);
        if (exists)
        {
            throw new InvalidOperationException($"角色编码 '{roleCode}' 已存在");
        }

        // 创建角色
        var role = new Role
        {
            Name = roleName,
            Code = roleCode,
            Description = roleDesc,
            Status = status,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _db.Roles.Add(role);
        await _db.SaveChangesAsync();

        return role;
    }

    /// <summary>
    /// 更新角色信息
    /// </summary>
    /// <param name="id">角色ID</param>
    /// <param name="roleName">角色名称</param>
    /// <param name="roleCode">角色编码</param>
    /// <param name="roleDesc">角色描述</param>
    /// <param name="status">角色状态</param>
    /// <returns>是否更新成功</returns>
    public async Task<bool> UpdateRoleAsync(Guid id, string roleName, string roleCode, string roleDesc = "", int? status = null)
    {
        var role = await _db.Roles.FindAsync(id);
        if (role == null)
        {
            return false;
        }

        // 如果角色编码发生变化,检查新编码是否已存在
        if (role.Code != roleCode)
        {
            var roleId = role.Id; // 创建局部变量避免闭包捕获方法参数
            var exists = await _db.Roles.AnyAsync(x => x.Code == roleCode && x.Id != roleId);
            if (exists)
            {
                throw new InvalidOperationException($"角色编码 '{roleCode}' 已存在");
            }
        }

        // 更新字段
        role.Name = roleName;
        role.Code = roleCode;
        role.Description = roleDesc;
        if (status.HasValue)
        {
            role.Status = status.Value;
        }
        role.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// 删除角色
    /// </summary>
    /// <param name="id">角色ID</param>
    /// <returns>是否删除成功</returns>
    /// <exception cref="InvalidOperationException">角色已分配给用户时抛出</exception>
    public async Task<bool> DeleteRoleAsync(Guid id)
    {
        var role = await _db.Roles.FindAsync(id);
        if (role == null)
        {
            return false;
        }

        // 检查角色是否已分配给用户
        var roleId = role.Id; // 创建局部变量避免闭包捕获方法参数
        var hasUsers = await _db.UserRoles.AnyAsync(x => x.RoleId == roleId);
        if (hasUsers)
        {
            throw new InvalidOperationException("该角色已分配给用户,无法删除");
        }

        // 删除角色
        _db.Roles.Remove(role);
        await _db.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// 批量删除角色
    /// </summary>
    /// <param name="ids">角色ID数组</param>
    /// <returns>成功删除的数量</returns>
    public async Task<int> BatchDeleteRolesAsync(List<Guid> ids)
    {
        int deletedCount = 0;

        foreach (var id in ids)
        {
            try
            {
                var success = await DeleteRoleAsync(id);
                if (success)
                {
                    deletedCount++;
                }
            }
            catch (InvalidOperationException)
            {
                // 如果角色已分配给用户,跳过该角色
                continue;
            }
        }

        return deletedCount;
    }
}

/// <summary>
/// 角色数据传输对象 - 用于API响应
/// </summary>
public class RoleDto
{
    public Guid Id { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string RoleCode { get; set; } = string.Empty;
    public string RoleDesc { get; set; } = string.Empty;
    public int Status { get; set; }
}
