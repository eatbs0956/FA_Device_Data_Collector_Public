using Auth.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Auth.Api.Services;

/// <summary>
/// 用户管理服务 - 提供用户的CRUD操作和业务逻辑处理
/// </summary>
public class UserService
{
    private readonly AuthDbContext _db;
    private readonly PasswordService _passwordService;

    public UserService(AuthDbContext db, PasswordService passwordService)
    {
        _db = db;
        _passwordService = passwordService;
    }

    /// <summary>
    /// 获取用户分页列表
    /// </summary>
    /// <param name="current">当前页码</param>
    /// <param name="size">每页数量</param>
    /// <param name="userName">用户名(模糊搜索)</param>
    /// <param name="nickName">昵称(模糊搜索)</param>
    /// <param name="userPhone">手机号(模糊搜索)</param>
    /// <param name="userEmail">邮箱(模糊搜索)</param>
    /// <param name="userGender">性别</param>
    /// <param name="status">状态</param>
    /// <returns>用户列表和总数</returns>
    public async Task<(List<UserDto> Items, int Total)> GetUserListAsync(
        int current = 1,
        int size = 10,
        string? userName = null,
        string? nickName = null,
        string? userPhone = null,
        string? userEmail = null,
        int? userGender = null,
        int? status = null)
    {
        // 参数校验
        current = current <= 0 ? 1 : current;
        size = size <= 0 ? 10 : size;

        // 构建查询
        var query = _db.Users.AsQueryable();

        // 用户名模糊搜索
        if (!string.IsNullOrWhiteSpace(userName))
        {
            query = query.Where(x => x.UserName.Contains(userName));
        }

        // 昵称模糊搜索
        if (!string.IsNullOrWhiteSpace(nickName))
        {
            query = query.Where(x => x.NickName.Contains(nickName));
        }

        // 手机号模糊搜索
        if (!string.IsNullOrWhiteSpace(userPhone))
        {
            query = query.Where(x => x.Phone.Contains(userPhone));
        }

        // 邮箱模糊搜索
        if (!string.IsNullOrWhiteSpace(userEmail))
        {
            query = query.Where(x => x.Email.Contains(userEmail));
        }

        // 性别筛选
        if (userGender.HasValue)
        {
            query = query.Where(x => x.Gender == userGender.Value);
        }

        // 状态筛选
        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        // 获取总数
        var total = await query.CountAsync();

        // 分页查询
        var users = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((current - 1) * size)
            .Take(size)
            .ToListAsync();

        // 获取这些用户的所有角色
        var userIds = users.Select(u => u.Id).ToList();
        var userRoles = await _db.UserRoles
            .Where(ur => userIds.Contains(ur.UserId))
            .Join(_db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => new { ur.UserId, r.Code })
            .ToListAsync();

        // 按用户ID分组角色代码
        var userRoleCodesDict = userRoles
            .GroupBy(x => x.UserId)
            .ToDictionary(
                g => g.Key,
                g => g.Select(x => x.Code).ToArray()
            );

        // 转换为DTO
        var items = users.Select(x => new UserDto
        {
            Id = x.Id,
            UserName = x.UserName,
            NickName = x.NickName,
            UserGender = x.Gender?.ToString(),
            UserPhone = x.Phone,
            UserEmail = x.Email,
            Status = x.Status,
            UserRoles = userRoleCodesDict.GetValueOrDefault(x.Id, Array.Empty<string>())
        }).ToList();

        return (items, total);
    }

    /// <summary>
    /// 根据ID获取用户详情
    /// </summary>
    /// <param name="id">用户ID</param>
    /// <returns>用户信息,不存在返回null</returns>
    public async Task<User?> GetUserByIdAsync(Guid id)
    {
        return await _db.Users
            .Include(x => x.UserRoles)
            .FirstOrDefaultAsync(x => x.Id == id);
    }

    /// <summary>
    /// 创建新用户
    /// </summary>
    /// <param name="userName">用户名</param>
    /// <param name="nickName">昵称</param>
    /// <param name="userGender">性别</param>
    /// <param name="userPhone">手机号</param>
    /// <param name="userEmail">邮箱</param>
    /// <param name="status">状态</param>
    /// <param name="userRoles">角色编码数组</param>
    /// <param name="password">密码</param>
    /// <returns>创建的用户</returns>
    public async Task<User> CreateUserAsync(
        string userName,
        string nickName = "",
        int? userGender = null,
        string userPhone = "",
        string userEmail = "",
        int status = 1,
        string[]? userRoles = null,
        string? password = null)
    {
        // 验证必填字段
        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new ArgumentException("用户名不能为空", nameof(userName));
        }

        // 检查用户名是否已存在
        var exists = await _db.Users.AnyAsync(x => x.UserName == userName);
        if (exists)
        {
            throw new InvalidOperationException($"用户名 '{userName}' 已存在");
        }

        // 创建用户
        var user = new User
        {
            UserName = userName,
            NickName = nickName,
            Gender = userGender,
            Phone = userPhone,
            Email = userEmail,
            Status = status,
            Enabled = status == 1,
            PasswordHash = string.IsNullOrWhiteSpace(password) 
                ? _passwordService.Hash("Admin@123") 
                : _passwordService.Hash(password),
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        // 分配角色
        if (userRoles != null && userRoles.Length > 0)
        {
            await AssignRolesToUserAsync(user.Id, userRoles);
        }

        return user;
    }

    /// <summary>
    /// 更新用户信息
    /// </summary>
    /// <param name="id">用户ID</param>
    /// <param name="userName">用户名</param>
    /// <param name="nickName">昵称</param>
    /// <param name="userGender">性别</param>
    /// <param name="userPhone">手机号</param>
    /// <param name="userEmail">邮箱</param>
    /// <param name="status">状态</param>
    /// <param name="userRoles">角色编码数组</param>
    /// <param name="password">密码（可选，如果提供则更新密码）</param>
    /// <returns>是否更新成功</returns>
    public async Task<bool> UpdateUserAsync(
        Guid id,
        string userName,
        string nickName = "",
        int? userGender = null,
        string userPhone = "",
        string userEmail = "",
        int? status = null,
        string[]? userRoles = null,
        string? password = null)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null)
        {
            return false;
        }

        // 如果用户名发生变化,检查新用户名是否已存在
        if (user.UserName != userName)
        {
            var userId = user.Id; // 创建局部变量避免闭包捕获方法参数
            var exists = await _db.Users.AnyAsync(x => x.UserName == userName && x.Id != userId);
            if (exists)
            {
                throw new InvalidOperationException($"用户名 '{userName}' 已存在");
            }
        }

        // 更新字段
        user.UserName = userName;
        user.NickName = nickName;
        user.Gender = userGender;
        user.Phone = userPhone;
        user.Email = userEmail;
        if (status.HasValue)
        {
            user.Status = status.Value;
            user.Enabled = status.Value == 1;
        }
        // 更新密码（如果提供了新密码）
        if (!string.IsNullOrWhiteSpace(password))
        {
            user.PasswordHash = _passwordService.Hash(password);
            user.PasswordUpdatedAt = DateTimeOffset.UtcNow;
        }
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await _db.SaveChangesAsync();

        // 更新角色
        if (userRoles != null)
        {
            await AssignRolesToUserAsync(id, userRoles);
        }

        return true;
    }

    /// <summary>
    /// 删除用户
    /// </summary>
    /// <param name="id">用户ID</param>
    /// <returns>是否删除成功</returns>
    public async Task<bool> DeleteUserAsync(Guid id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null)
        {
            return false;
        }

        // 删除用户角色关联
        var userRoles = await _db.UserRoles.Where(x => x.UserId == id).ToListAsync();
        _db.UserRoles.RemoveRange(userRoles);

        // 删除刷新令牌
        var refreshTokens = await _db.RefreshTokens.Where(x => x.UserId == id).ToListAsync();
        _db.RefreshTokens.RemoveRange(refreshTokens);

        // 删除用户
        _db.Users.Remove(user);
        await _db.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// 批量删除用户
    /// </summary>
    /// <param name="ids">用户ID数组</param>
    /// <returns>成功删除的数量</returns>
    public async Task<int> BatchDeleteUsersAsync(List<Guid> ids)
    {
        int deletedCount = 0;

        foreach (var id in ids)
        {
            var success = await DeleteUserAsync(id);
            if (success)
            {
                deletedCount++;
            }
        }

        return deletedCount;
    }

    /// <summary>
    /// 为用户分配角色
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <param name="roleCodes">角色编码数组</param>
    /// <returns>是否分配成功</returns>
    private async Task<bool> AssignRolesToUserAsync(Guid userId, string[] roleCodes)
    {
        // 验证用户是否存在
        var userIdLocal = userId; // 创建局部变量避免闭包捕获方法参数
        var exists = await _db.Users.AnyAsync(x => x.Id == userIdLocal);
        if (!exists)
        {
            return false;
        }

        // 删除现有的用户角色关联
        var existingUserRoles = await _db.UserRoles
            .Where(x => x.UserId == userId)
            .ToListAsync();
        _db.UserRoles.RemoveRange(existingUserRoles);

        // 如果没有角色，直接保存并返回
        if (roleCodes == null || roleCodes.Length == 0)
        {
            await _db.SaveChangesAsync();
            return true;
        }

        // 创建局部变量避免闭包捕获方法参数导致的EF Core表达式树转换问题
        var roleCodesLocal = roleCodes.ToList();
        
        // 获取角色ID
        var roleIds = await _db.Roles
            .Where(r => roleCodesLocal.Contains(r.Code))
            .Select(r => r.Id)
            .ToListAsync();

        // 添加新的用户角色关联
        foreach (var roleId in roleIds)
        {
            _db.UserRoles.Add(new UserRole
            {
                UserId = userId,
                RoleId = roleId
            });
        }

        await _db.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// 获取用户的角色编码列表
    /// </summary>
    /// <param name="userId">用户ID</param>
    /// <returns>角色编码数组</returns>
    public async Task<string[]> GetUserRoleCodesAsync(Guid userId)
    {
        return await _db.UserRoles
            .Where(ur => ur.UserId == userId)
            .Join(_db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Code)
            .ToArrayAsync();
    }
}

/// <summary>
/// 用户数据传输对象 - 用于API响应
/// </summary>
public class UserDto
{
    public Guid Id { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string NickName { get; set; } = string.Empty;
    public string? UserGender { get; set; }
    public string UserPhone { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public int Status { get; set; }
    public string[] UserRoles { get; set; } = Array.Empty<string>();
}
