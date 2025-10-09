using Microsoft.EntityFrameworkCore;

namespace Auth.Api.Models;

/// <summary>
/// 认证数据库上下文 - Entity Framework Core 数据库访问上下文
/// </summary>
/// <param name="options">数据库上下文配置选项 - 包含连接字符串和其他配置</param>
public class AuthDbContext(DbContextOptions<AuthDbContext> options) : DbContext(options)
{
    /// <summary>
    /// 用户数据集 - 系统用户信息表
    /// </summary>
    public DbSet<User> Users => Set<User>();
    
    /// <summary>
    /// 角色数据集 - 系统角色权限表
    /// </summary>
    public DbSet<Role> Roles => Set<Role>();
    
    /// <summary>
    /// 用户角色关联数据集 - 用户与角色的多对多关系表
    /// </summary>
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    
    /// <summary>
    /// 刷新令牌数据集 - JWT刷新令牌存储表
    /// </summary>
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    
    /// <summary>
    /// 菜单数据集 - 系统菜单导航表
    /// </summary>
    public DbSet<Menu> Menus => Set<Menu>();
    
    /// <summary>
    /// 角色菜单关联数据集 - 角色与菜单的多对多关系表
    /// </summary>
    public DbSet<RoleMenu> RoleMenus => Set<RoleMenu>();

    /// <summary>
    /// 模型构建配置 - 配置实体关系、索引和约束
    /// </summary>
    /// <param name="modelBuilder">模型构建器 - 用于配置实体模型</param>
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // 用户名唯一索引配置 - 确保用户名不重复
        modelBuilder.Entity<User>().HasIndex(x => x.UserName).IsUnique();
        // 角色编码唯一索引配置 - 确保角色编码不重复
        modelBuilder.Entity<Role>().HasIndex(x => x.Code).IsUnique();
        // 用户角色复合主键配置 - 用户ID和角色ID组合主键
        modelBuilder.Entity<UserRole>().HasKey(x => new { x.UserId, x.RoleId });
        // 刷新令牌复合索引配置 - 用户ID和令牌组合索引，提高查询效率
        modelBuilder.Entity<RefreshToken>().HasIndex(x => new { x.UserId, x.Token });
        // 菜单路由名称唯一索引配置 - 确保路由名称不重复
        modelBuilder.Entity<Menu>().HasIndex(x => x.RouteName).IsUnique();
        // 菜单父级ID索引配置 - 提高菜单树查询效率
        modelBuilder.Entity<Menu>().HasIndex(x => x.ParentId);
        // 角色菜单复合主键配置 - 角色ID和菜单ID组合主键
        modelBuilder.Entity<RoleMenu>().HasKey(x => new { x.RoleId, x.MenuId });
    }
}
