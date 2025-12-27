using Microsoft.EntityFrameworkCore;

namespace SharedAuth.Data;

/// <summary>
/// Auth数据库上下文工厂接口 - 用于在微服务中创建Auth数据库连接
/// </summary>
public interface IAuthDbContextFactory
{
    /// <summary>
    /// 创建Auth数据库上下文实例
    /// </summary>
    IAuthDbContext CreateDbContext();
}

/// <summary>
/// Auth数据库上下文接口 - 定义权限验证所需的最小数据集
/// </summary>
public interface IAuthDbContext : IDisposable, IAsyncDisposable
{
    DbSet<UserRole> UserRoles { get; }
    DbSet<Role> Roles { get; }
    DbSet<RoleButton> RoleButtons { get; }
}

// Entity Models - 权限验证所需的最小实体定义
public class UserRole
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
}

public class Role
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
}

public class RoleButton
{
    public Guid RoleId { get; set; }
    public string ButtonCode { get; set; } = string.Empty;
}
