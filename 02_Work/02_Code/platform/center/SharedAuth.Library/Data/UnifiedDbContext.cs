using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Shared.Domain.Entities;
using Shared.Domain.Helpers;
using System.Linq.Expressions;

namespace Shared.Domain.Data;

/// <summary>
/// 统一数据库上下文 - 整合 Auth 和 Device 两个模块的所有实体
/// </summary>
/// <remarks>
/// 核心功能：
/// 1. 多租户数据隔离（Global Query Filter）
/// 2. 审计字段自动填充（CreatedBy, UpdatedBy, DeletedFlag）
/// 3. 软删除自动处理
/// 4. 支持超级管理员跨租户查询（IgnoreQueryFilters）
/// </remarks>
public class UnifiedDbContext : DbContext
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    public UnifiedDbContext(DbContextOptions<UnifiedDbContext> options) : base(options)
    {
    }

    public UnifiedDbContext(
        DbContextOptions<UnifiedDbContext> options,
        IHttpContextAccessor httpContextAccessor) : base(options)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    // ==================== Auth 模块实体集合 ====================
    
    /// <summary>
    /// 用户数据集
    /// </summary>
    public DbSet<User> Users => Set<User>();
    
    /// <summary>
    /// 角色数据集
    /// </summary>
    public DbSet<Role> Roles => Set<Role>();
    
    /// <summary>
    /// 用户角色关联数据集
    /// </summary>
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    
    /// <summary>
    /// 刷新令牌数据集
    /// </summary>
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    
    /// <summary>
    /// 菜单数据集
    /// </summary>
    public DbSet<Menu> Menus => Set<Menu>();
    
    /// <summary>
    /// 角色菜单关联数据集
    /// </summary>
    public DbSet<RoleMenu> RoleMenus => Set<RoleMenu>();

    /// <summary>
    /// 角色按钮权限关联数据集
    /// </summary>
    public DbSet<RoleButton> RoleButtons => Set<RoleButton>();

    /// <summary>
    /// 会话数据集
    /// </summary>
    public DbSet<Session> Sessions => Set<Session>();

    /// <summary>
    /// 审计日志数据集
    /// </summary>
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    // ==================== Device 模块实体集合 ====================

    /// <summary>
    /// 边缘节点集合
    /// </summary>
    public DbSet<EdgeNode> EdgeNodes => Set<EdgeNode>();

    /// <summary>
    /// 设备集合
    /// </summary>
    public DbSet<Device> Devices => Set<Device>();

    /// <summary>
    /// 设备分组集合
    /// </summary>
    public DbSet<DeviceGroup> DeviceGroups => Set<DeviceGroup>();

    /// <summary>
    /// 标签定义集合
    /// </summary>
    public DbSet<TagDefinition> TagDefinitions => Set<TagDefinition>();

    // ==================== Collection 模块实体集合 ====================

    /// <summary>
    /// 采集任务集合
    /// </summary>
    public DbSet<CollectionTask> CollectionTasks => Set<CollectionTask>();

    /// <summary>
    /// 采集任务-设备关联集合
    /// </summary>
    public DbSet<CollectionTaskDevice> CollectionTaskDevices => Set<CollectionTaskDevice>();

    // ==================== 当前上下文信息 ====================

    /// <summary>
    /// 当前租户ID - 从 JWT Token 的 tenant_id Claim 中提取
    /// </summary>
    public string CurrentTenantId => 
        AuditHelper.GetCurrentTenantId(_httpContextAccessor?.HttpContext);

    /// <summary>
    /// 当前用户ID - 从 JWT Token 的 sub Claim 中提取
    /// </summary>
    public Guid? CurrentUserId => 
        AuditHelper.GetCurrentUserId(_httpContextAccessor?.HttpContext);

    /// <summary>
    /// 是否为超级管理员 - 检查是否拥有 R_SUPER 角色
    /// </summary>
    public bool IsSuperAdmin => 
        AuditHelper.IsSuperAdmin(_httpContextAccessor?.HttpContext);

    // ==================== EF Core 配置 ====================

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 配置实体关系
        ConfigureRelationships(modelBuilder);

        // 配置唯一索引
        ConfigureUniqueIndexes(modelBuilder);

        // 配置性能索引
        ConfigurePerformanceIndexes(modelBuilder);

        // 配置审计字段默认值
        ConfigureAuditFields(modelBuilder);

        // 配置多租户过滤器（核心功能）
        ConfigureTenantFilter(modelBuilder);

        // 配置软删除过滤器
        ConfigureSoftDeleteFilter(modelBuilder);

        // 配置JSONB字段（PostgreSQL特定）
        ConfigureJsonbColumns(modelBuilder);
    }

    /// <summary>
    /// 配置实体关系
    /// </summary>
    private void ConfigureRelationships(ModelBuilder modelBuilder)
    {
        // === 用户角色关联配置 ===
        modelBuilder.Entity<UserRole>().HasKey(x => new { x.UserId, x.RoleId });

        // === 角色菜单关联配置 ===
        modelBuilder.Entity<RoleMenu>().HasKey(x => new { x.RoleId, x.MenuId });

        // === 角色按钮权限关联配置 ===
        modelBuilder.Entity<RoleButton>().HasKey(x => new { x.RoleId, x.ButtonCode });

        // === EdgeNode - Device 一对多关系 ===
        modelBuilder.Entity<EdgeNode>()
            .HasMany(e => e.Devices)
            .WithOne(d => d.EdgeNode)
            .HasForeignKey(d => d.EdgeNodeId)
            .OnDelete(DeleteBehavior.Cascade);

        // === Device - TagDefinition 一对多关系 ===
        modelBuilder.Entity<Device>()
            .HasMany(d => d.TagDefinitions)
            .WithOne(t => t.Device)
            .HasForeignKey(t => t.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);

        // === DeviceGroup - Device 一对多关系 ===
        modelBuilder.Entity<DeviceGroup>()
            .HasMany(g => g.Devices)
            .WithOne(d => d.Group)
            .HasForeignKey(d => d.GroupId)
            .OnDelete(DeleteBehavior.SetNull);

        // === DeviceGroup - DeviceGroup 自关联（树形结构）===
        modelBuilder.Entity<DeviceGroup>()
            .HasMany(g => g.Children)
            .WithOne(g => g.Parent)
            .HasForeignKey(g => g.ParentId)
            .OnDelete(DeleteBehavior.Cascade);

        // === CollectionTask - Device 多对多关系 ===
        modelBuilder.Entity<CollectionTaskDevice>()
            .HasKey(x => new { x.TaskId, x.DeviceId });

        modelBuilder.Entity<CollectionTaskDevice>()
            .HasOne(ctd => ctd.Task)
            .WithMany(t => t.TaskDevices)
            .HasForeignKey(ctd => ctd.TaskId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<CollectionTaskDevice>()
            .HasOne(ctd => ctd.Device)
            .WithMany()
            .HasForeignKey(ctd => ctd.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    /// <summary>
    /// 配置唯一索引
    /// </summary>
    private void ConfigureUniqueIndexes(ModelBuilder modelBuilder)
    {
        // 用户名唯一索引（同一租户内）
        modelBuilder.Entity<User>()
            .HasIndex(x => new { x.TenantId, x.UserName })
            .IsUnique()
            .HasFilter("\"deleted_flag\" = false");

        // 角色编码唯一索引（同一租户内）
        modelBuilder.Entity<Role>()
            .HasIndex(x => new { x.TenantId, x.Code })
            .IsUnique()
            .HasFilter("\"deleted_flag\" = false");

        // 菜单路由名称唯一索引（同一租户内）
        modelBuilder.Entity<Menu>()
            .HasIndex(x => new { x.TenantId, x.RouteName })
            .IsUnique()
            .HasFilter("\"deleted_flag\" = false");

        // 边缘节点ID唯一索引
        modelBuilder.Entity<EdgeNode>()
            .HasIndex(e => e.NodeId)
            .IsUnique();

        // 设备ID唯一索引
        modelBuilder.Entity<Device>()
            .HasIndex(d => d.DeviceId)
            .IsUnique();

        // 设备分组名称唯一索引（同一租户和父分组下）
        modelBuilder.Entity<DeviceGroup>()
            .HasIndex(g => new { g.TenantId, g.ParentId, g.Name })
            .IsUnique()
            .HasFilter("\"deleted_flag\" = false");

        // 标签ID唯一索引（同一设备内）
        modelBuilder.Entity<TagDefinition>()
            .HasIndex(t => new { t.DeviceId, t.TagId })
            .IsUnique();

        // 采集任务名称唯一索引（同一租户内）
        modelBuilder.Entity<CollectionTask>()
            .HasIndex(t => new { t.TenantId, t.Name })
            .IsUnique()
            .HasFilter("\"deleted_flag\" = false");

        // 采集任务编码唯一索引（同一租户内，编码不为空时）
        modelBuilder.Entity<CollectionTask>()
            .HasIndex(t => new { t.TenantId, t.Code })
            .IsUnique()
            .HasFilter("\"deleted_flag\" = false AND \"code\" IS NOT NULL");

        // AccessToken JTI唯一索引
        modelBuilder.Entity<Session>()
            .HasIndex(x => x.AccessTokenJti)
            .IsUnique();

        // RefreshToken哈希唯一索引
        modelBuilder.Entity<Session>()
            .HasIndex(x => x.RefreshTokenHash)
            .IsUnique();
    }

    /// <summary>
    /// 配置性能索引
    /// </summary>
    private void ConfigurePerformanceIndexes(ModelBuilder modelBuilder)
    {
        // 菜单父级ID索引
        modelBuilder.Entity<Menu>().HasIndex(x => x.ParentId);

        // 角色按钮索引
        modelBuilder.Entity<RoleButton>().HasIndex(x => x.RoleId);

        // 刷新令牌复合索引
        modelBuilder.Entity<RefreshToken>().HasIndex(x => new { x.UserId, x.Token });

        // 用户会话索引
        modelBuilder.Entity<Session>().HasIndex(x => new { x.UserId, x.ExpiresAt });

        // 审计日志索引
        modelBuilder.Entity<AuditLog>().HasIndex(x => new { x.UserId, x.CreatedAt });
        modelBuilder.Entity<AuditLog>().HasIndex(x => new { x.Action, x.CreatedAt });
        modelBuilder.Entity<AuditLog>().HasIndex(x => new { x.TenantId, x.CreatedAt });

        // 边缘节点性能索引
        modelBuilder.Entity<EdgeNode>().HasIndex(e => new { e.Platform, e.Status });
        modelBuilder.Entity<EdgeNode>().HasIndex(e => e.TenantId);

        // 设备性能索引
        modelBuilder.Entity<Device>().HasIndex(d => new { d.EdgeNodeId, d.ConnectionStatus });
        modelBuilder.Entity<Device>().HasIndex(d => d.TenantId);
        modelBuilder.Entity<Device>().HasIndex(d => d.GroupId).HasFilter("\"group_id\" IS NOT NULL");
        modelBuilder.Entity<Device>().HasIndex(d => d.Enabled);

        // 设备分组性能索引
        modelBuilder.Entity<DeviceGroup>().HasIndex(g => g.ParentId).HasFilter("\"parent_id\" IS NOT NULL");
        modelBuilder.Entity<DeviceGroup>().HasIndex(g => g.TenantId);

        // 标签定义性能索引
        modelBuilder.Entity<TagDefinition>().HasIndex(t => new { t.DeviceId, t.Enabled });
        modelBuilder.Entity<TagDefinition>().HasIndex(t => t.TenantId);
    }

    /// <summary>
    /// 配置审计字段的默认值和约束
    /// </summary>
    private void ConfigureAuditFields(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                // CreatedAt 默认值为当前时间
                modelBuilder.Entity(entityType.ClrType)
                    .Property<DateTimeOffset>("CreatedAt")
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                // DeletedFlag 默认值为false
                modelBuilder.Entity(entityType.ClrType)
                    .Property<bool>("DeletedFlag")
                    .HasDefaultValue(false);

                // TenantId 默认值为 "t1"
                modelBuilder.Entity(entityType.ClrType)
                    .Property<string>("TenantId")
                    .HasMaxLength(64)
                    .HasDefaultValue("t1")
                    .IsRequired();
            }
        }
    }

    /// <summary>
    /// 配置多租户过滤器（核心功能）
    /// </summary>
    /// <remarks>
    /// 自动过滤：e => e.TenantId == CurrentTenantId
    /// 超级管理员绕过：使用 .IgnoreQueryFilters() 方法
    /// </remarks>
    private void ConfigureTenantFilter(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var filter = BuildTenantFilter(entityType.ClrType);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }
    }

    /// <summary>
    /// 构建租户过滤器表达式
    /// </summary>
    private LambdaExpression BuildTenantFilter(Type entityType)
    {
        // 构建表达式：e => e.TenantId == CurrentTenantId
        var parameter = Expression.Parameter(entityType, "e");
        var property = Expression.Property(parameter, nameof(BaseEntity.TenantId));
        var currentTenantId = Expression.Property(Expression.Constant(this), nameof(CurrentTenantId));
        var condition = Expression.Equal(property, currentTenantId);
        return Expression.Lambda(condition, parameter);
    }

    /// <summary>
    /// 配置软删除过滤器
    /// </summary>
    /// <remarks>
    /// 自动过滤：e => e.DeletedFlag == false
    /// 查询已删除数据：使用 .IgnoreQueryFilters() 方法
    /// </remarks>
    private void ConfigureSoftDeleteFilter(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
            {
                var filter = BuildSoftDeleteFilter(entityType.ClrType);
                modelBuilder.Entity(entityType.ClrType).HasQueryFilter(filter);
            }
        }
    }

    /// <summary>
    /// 构建软删除过滤器表达式
    /// </summary>
    private LambdaExpression BuildSoftDeleteFilter(Type entityType)
    {
        // 构建表达式：e => e.DeletedFlag == false
        var parameter = Expression.Parameter(entityType, "e");
        var property = Expression.Property(parameter, nameof(BaseEntity.DeletedFlag));
        var condition = Expression.Equal(property, Expression.Constant(false));
        return Expression.Lambda(condition, parameter);
    }

    /// <summary>
    /// 配置JSONB类型字段（PostgreSQL特定）
    /// </summary>
    private void ConfigureJsonbColumns(ModelBuilder modelBuilder)
    {
        // EdgeNode JSONB字段
        modelBuilder.Entity<EdgeNode>()
            .Property(e => e.PlatformConfig)
            .HasColumnType("jsonb");
        modelBuilder.Entity<EdgeNode>()
            .Property(e => e.ResourceLimits)
            .HasColumnType("jsonb");
        modelBuilder.Entity<EdgeNode>()
            .Property(e => e.HardwareInfo)
            .HasColumnType("jsonb");

        // Device JSONB字段
        modelBuilder.Entity<Device>()
            .Property(d => d.ConnectionConfig)
            .HasColumnType("jsonb");
        modelBuilder.Entity<Device>()
            .Property(d => d.ProtocolConfig)
            .HasColumnType("jsonb");
        modelBuilder.Entity<Device>()
            .Property(d => d.TagsConfig)
            .HasColumnType("jsonb");
    }

    // ==================== SaveChanges 自动审计 ====================

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        UpdateAuditFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges()
    {
        UpdateAuditFields();
        return base.SaveChanges();
    }

    /// <summary>
    /// 更新审计字段 - 自动从 JWT Token 中提取当前用户信息
    /// </summary>
    private void UpdateAuditFields()
    {
        var entries = ChangeTracker.Entries<BaseEntity>();
        var httpContext = _httpContextAccessor?.HttpContext;

        foreach (var entry in entries)
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    // 创建操作：设置创建人和创建时间
                    AuditHelper.SetCreateAudit(entry.Entity, httpContext);
                    break;

                case EntityState.Modified:
                    // 更新操作：设置更新人和更新时间
                    if (entry.Entity.DeletedFlag)
                    {
                        // 这是软删除操作
                        var deletedFlagProperty = entry.Property(nameof(BaseEntity.DeletedFlag));
                        if (deletedFlagProperty.OriginalValue is bool originalDeleted && !originalDeleted)
                        {
                            AuditHelper.SetDeleteAudit(entry.Entity, httpContext);
                        }
                        else
                        {
                            AuditHelper.SetUpdateAudit(entry.Entity, httpContext);
                        }
                    }
                    else
                    {
                        AuditHelper.SetUpdateAudit(entry.Entity, httpContext);
                    }
                    break;

                case EntityState.Deleted:
                    // 实现软删除：将删除操作转换为更新操作
                    entry.State = EntityState.Modified;
                    AuditHelper.SetDeleteAudit(entry.Entity, httpContext);
                    break;
            }
        }
    }
}
