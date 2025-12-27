using Microsoft.EntityFrameworkCore;

namespace SharedAuth.Data;

/// <summary>
/// Auth数据库上下文实现 - 用于权限验证的轻量级DbContext
/// </summary>
/// <remarks>
/// 仅包含权限验证所需的表：UserRoles、Roles、RoleButtons
/// 避免加载完整的Auth数据库模型，减少依赖和性能开销
/// </remarks>
public class AuthDbContext : DbContext, IAuthDbContext
{
    public AuthDbContext(DbContextOptions<AuthDbContext> options) : base(options)
    {
    }

    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RoleButton> RoleButtons => Set<RoleButton>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // UserRoles表配置 - 使用snake_case命名（表名是单数形式）
        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.ToTable("user_role");
            entity.HasKey(e => new { e.UserId, e.RoleId });
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
        });

        // Roles表配置 - 使用snake_case命名（表名是单数形式）
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("role");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code).HasColumnName("code");
        });

        // RoleButtons表配置 - 使用snake_case命名（表名是单数形式）
        modelBuilder.Entity<RoleButton>(entity =>
        {
            entity.ToTable("role_button");
            entity.HasKey(e => new { e.RoleId, e.ButtonCode });
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.ButtonCode).HasColumnName("button_code");
        });
    }
}

/// <summary>
/// Auth数据库上下文工厂实现
/// </summary>
public class AuthDbContextFactory : IAuthDbContextFactory
{
    private readonly DbContextOptions<AuthDbContext> _options;

    public AuthDbContextFactory(DbContextOptions<AuthDbContext> options)
    {
        _options = options;
    }

    public IAuthDbContext CreateDbContext()
    {
        return new AuthDbContext(_options);
    }
}
