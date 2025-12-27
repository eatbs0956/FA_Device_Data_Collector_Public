using Microsoft.EntityFrameworkCore;
using Shared.Domain.Data;
using Shared.Domain.Entities;

namespace Shared.Domain.Seeders;

/// <summary>
/// 数据库初始化种子类
/// </summary>
/// <remarks>
/// 用于在系统首次启动时初始化默认数据：
/// 1. 超级管理员用户（super, 租户 t0）
/// 2. 默认角色（Super, Admin, User）
/// 3. 默认菜单（首页、设备管理、采集管理、监控管理、告警管理、系统管理）
/// 4. 角色菜单权限分配
/// 
/// 租户说明：
/// - t0：超级管理员租户（仅超级管理员使用）
/// - t1：默认租户/系统租户（普通用户默认租户）
/// </remarks>
public static class DataSeeder
{
    /// <summary>
    /// 执行数据库初始化
    /// </summary>
    /// <param name="context">数据库上下文</param>
    /// <param name="passwordHashFunc">密码哈希函数（由外部 PasswordService 提供）</param>
    public static void SeedData(UnifiedDbContext context, Func<string, string> passwordHashFunc)
    {
        // 应用数据库迁移
        context.Database.Migrate();

        // 初始化用户
        SeedUsers(context, passwordHashFunc);

        // 初始化默认角色
        SeedRoles(context);

        // 为超级管理员分配角色
        SeedUserRoles(context);

        // 初始化默认菜单
        SeedMenus(context);

        // 初始化角色菜单权限
        SeedRoleMenus(context);
    }

    /// <summary>
    /// 初始化用户
    /// </summary>
    private static void SeedUsers(UnifiedDbContext context, Func<string, string> passwordHashFunc)
    {
        // 检查 super 用户是否已存在
        var superUser = context.Users
            .IgnoreQueryFilters() // 绕过租户过滤器，查找 t0 租户的用户
            .FirstOrDefault(u => u.UserName == "super" && u.TenantId == "t0");

        // 检查 admin 用户是否已存在
        var adminUser = context.Users
            .IgnoreQueryFilters() // 绕过租户过滤器，查找 t1 租户的用户
            .FirstOrDefault(u => u.UserName == "admin" && u.TenantId == "t1");

        // 检查 user 用户是否已存在
        var normalUser = context.Users
            .IgnoreQueryFilters() // 绕过租户过滤器，查找 t1 租户的用户
            .FirstOrDefault(u => u.UserName == "user" && u.TenantId == "t1");

        var hasNewUsers = false;

        // 创建超级管理员用户
        if (superUser == null)
        {
            superUser = new User
            {
                UserName = "super",
                NickName = "超级管理员",
                Gender = 1,
                Phone = "",
                Email = "",
                Status = 1,
                Enabled = true,
                PasswordHash = passwordHashFunc("Super@123"),
                PasswordUpdatedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                CreatedBy = null, // 超级管理员自己创建自己
                UpdatedBy = null,
                DeletedFlag = false,
                TenantId = "t0" // 超级管理员租户
            };
            context.Users.Add(superUser);
            hasNewUsers = true;
            Console.WriteLine("[DataSeeder] ✓ 创建超级管理员用户: super (租户: t0)");
        }

        // 创建管理员
        if (adminUser == null)
        {
            adminUser = new User
            {
                UserName = "admin",
                NickName = "管理员",
                Gender = 1,
                Phone = "",
                Email = "",
                Status = 1,
                Enabled = true,
                PasswordHash = passwordHashFunc("Admin@123"),
                PasswordUpdatedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                CreatedBy = null, // 初始化时 super 的 ID 还未生成
                UpdatedBy = null,
                DeletedFlag = false,
                TenantId = "t1" // 默认租户
            };
            context.Users.Add(adminUser);
            hasNewUsers = true;
            Console.WriteLine("[DataSeeder] ✓ 创建管理员用户: admin (租户: t1)");
        }

        // 创建普通用户
        if (normalUser == null)
        {
            normalUser = new User
            {
                UserName = "user",
                NickName = "普通用户",
                Gender = 1,
                Phone = "",
                Email = "",
                Status = 1,
                Enabled = true,
                PasswordHash = passwordHashFunc("User@123"),
                PasswordUpdatedAt = DateTimeOffset.UtcNow,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                CreatedBy = null, // 初始化时 super 的 ID 还未生成
                UpdatedBy = null,
                DeletedFlag = false,
                TenantId = "t1" // 默认租户
            };
            context.Users.Add(normalUser);
            hasNewUsers = true;
            Console.WriteLine("[DataSeeder] ✓ 创建普通用户: user (租户: t1)");
        }

        if (hasNewUsers)
        {
            context.SaveChanges();
        }
        else
        {
            Console.WriteLine("[DataSeeder] ○ 用户已存在,跳过创建");
        }
    }


    /// <summary>
    /// 初始化默认角色
    /// </summary>
    private static void SeedRoles(UnifiedDbContext context)
    {
        var superUser = context.Users
            .IgnoreQueryFilters()
            .FirstOrDefault(u => u.UserName == "super" && u.TenantId == "t0");

        if (superUser == null)
        {
            throw new InvalidOperationException("超级管理员用户不存在,无法创建角色");
        }

        // 创建 t0 租户的超级管理员角色
        if (!context.Roles.IgnoreQueryFilters().Any(r => r.Code == "R_SUPER" && r.TenantId == "t0"))
        {
            context.Roles.Add(new Role
            {
                Name = "Super",
                Code = "R_SUPER",
                Status = 1,
                Description = "超级管理员角色",
                CreatedBy = superUser.Id,
                UpdatedBy = null,
                CreatedAt = DateTimeOffset.UtcNow,
                UpdatedAt = DateTimeOffset.UtcNow,
                DeletedFlag = false,
                TenantId = "t0" // 超级管理员租户
            });
            Console.WriteLine("[DataSeeder] ✓ 创建超级管理员角色: R_SUPER (租户: t0)");
        }

        // 创建 t1 租户的普通角色
        if (!context.Roles.IgnoreQueryFilters().Any(r => r.Code == "R_ADMIN" && r.TenantId == "t1"))
        {
            context.Roles.AddRange(
                new Role
                {
                    Name = "Admin",
                    Code = "R_ADMIN",
                    Status = 1,
                    Description = "管理员角色",
                    CreatedBy = superUser.Id,
                    UpdatedBy = null,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow,
                    DeletedFlag = false,
                    TenantId = "t1" // 默认租户
                },
                new Role
                {
                    Name = "User",
                    Code = "R_USER",
                    Status = 1,
                    Description = "普通用户角色",
                    CreatedBy = superUser.Id,
                    UpdatedBy = null,
                    CreatedAt = DateTimeOffset.UtcNow,
                    UpdatedAt = DateTimeOffset.UtcNow,
                    DeletedFlag = false,
                    TenantId = "t1" // 默认租户
                }
            );
            Console.WriteLine("[DataSeeder] ✓ 创建默认租户角色: R_ADMIN, R_USER (租户: t1)");
        }

        context.SaveChanges();
    }

    /// <summary>
    /// 为用户分配角色
    /// </summary>
    private static void SeedUserRoles(UnifiedDbContext context)
    {
        // 获取所有用户
        var superUser = context.Users
            .IgnoreQueryFilters()
            .FirstOrDefault(u => u.UserName == "super" && u.TenantId == "t0");

        var adminUser = context.Users
            .IgnoreQueryFilters()
            .FirstOrDefault(u => u.UserName == "admin" && u.TenantId == "t1");

        var normalUser = context.Users
            .IgnoreQueryFilters()
            .FirstOrDefault(u => u.UserName == "user" && u.TenantId == "t1");

        // 获取所有角色
        var superRole = context.Roles
            .IgnoreQueryFilters()
            .FirstOrDefault(r => r.Code == "R_SUPER" && r.TenantId == "t0");

        var adminRole = context.Roles
            .IgnoreQueryFilters()
            .FirstOrDefault(r => r.Code == "R_ADMIN" && r.TenantId == "t1");

        var userRole = context.Roles
            .IgnoreQueryFilters()
            .FirstOrDefault(r => r.Code == "R_USER" && r.TenantId == "t1");

        // 验证数据完整性
        if (superUser == null)
            throw new InvalidOperationException("超级管理员用户不存在,无法分配角色");
        if (adminUser == null)
            throw new InvalidOperationException("管理员用户不存在,无法分配角色");
        if (normalUser == null)
            throw new InvalidOperationException("普通用户不存在,无法分配角色");
        if (superRole == null)
            throw new InvalidOperationException("超级管理员角色不存在,无法分配角色");
        if (adminRole == null)
            throw new InvalidOperationException("管理员角色不存在,无法分配角色");
        if (userRole == null)
            throw new InvalidOperationException("普通用户角色不存在,无法分配角色");

        var hasNewAssignments = false;

        // 为 super 用户分配 R_SUPER 角色
        if (!context.UserRoles.Any(ur => ur.UserId == superUser.Id && ur.RoleId == superRole.Id))
        {
            context.UserRoles.Add(new UserRole
            {
                UserId = superUser.Id,
                RoleId = superRole.Id
            });
            hasNewAssignments = true;
            Console.WriteLine("[DataSeeder] ✓ 为超级管理员(super)分配角色: R_SUPER");
        }
        else
        {
            Console.WriteLine("[DataSeeder] ○ 超级管理员(super)已拥有角色 R_SUPER,跳过分配");
        }

        // 为 admin 用户分配 R_ADMIN 角色
        if (!context.UserRoles.Any(ur => ur.UserId == adminUser.Id && ur.RoleId == adminRole.Id))
        {
            context.UserRoles.Add(new UserRole
            {
                UserId = adminUser.Id,
                RoleId = adminRole.Id
            });
            hasNewAssignments = true;
            Console.WriteLine("[DataSeeder] ✓ 为管理员(admin)分配角色: R_ADMIN");
        }
        else
        {
            Console.WriteLine("[DataSeeder] ○ 管理员(admin)已拥有角色 R_ADMIN,跳过分配");
        }

        // 为 user 用户分配 R_USER 角色
        if (!context.UserRoles.Any(ur => ur.UserId == normalUser.Id && ur.RoleId == userRole.Id))
        {
            context.UserRoles.Add(new UserRole
            {
                UserId = normalUser.Id,
                RoleId = userRole.Id
            });
            hasNewAssignments = true;
            Console.WriteLine("[DataSeeder] ✓ 为普通用户(user)分配角色: R_USER");
        }
        else
        {
            Console.WriteLine("[DataSeeder] ○ 普通用户(user)已拥有角色 R_USER,跳过分配");
        }

        if (hasNewAssignments)
        {
            context.SaveChanges();
        }
    }

    /// <summary>
    /// 初始化默认菜单
    /// </summary>
    /// <remarks>
    /// 注意：菜单数据存储在 t1（默认租户）中，超级管理员可以通过 IgnoreQueryFilters() 查看所有租户的菜单
    /// </remarks>
    private static void SeedMenus(UnifiedDbContext context)
    {
        var superUser = context.Users
            .IgnoreQueryFilters()
            .FirstOrDefault(u => u.UserName == "super" && u.TenantId == "t0");

        if (superUser == null)
        {
            throw new InvalidOperationException("超级管理员用户不存在,无法创建菜单");
        }

        // 检查是否已有菜单数据
        if (context.Menus.IgnoreQueryFilters().Any())
        {
            Console.WriteLine("[DataSeeder] ○ 菜单数据已存在,跳过创建");
            return;
        }

        // 创建默认菜单（存储在 t1 默认租户）
        var menus = new List<Menu>
        {
            // 首页
            new Menu { Id = 1, MenuType = 2, MenuName = "首页", RouteName = "home", RoutePath = "/home", 
                Component = "view.home", I18nKey = "route.home", Icon = "mdi:monitor-dashboard", IconType = "1", 
                Order = 1, Status = 1, ParentId = null, TenantId = "t1", CreatedBy = superUser.Id },

            // 设备管理（目录）
            new Menu { Id = 2, MenuType = 1, MenuName = "设备管理", RouteName = "device", RoutePath = "/device",
                Component = "view.device", I18nKey = "route.device", Icon = "tabler:devices-cog", IconType = "1",
                Order = 2, Status = 1, ParentId = null, TenantId = "t1", CreatedBy = superUser.Id },

            // 设备列表
            new Menu { Id = 3, MenuType = 2, MenuName = "设备列表", RouteName = "device_list", RoutePath = "/device/list",
                Component = "view.device_list", I18nKey = "route.device_list", Icon = "ri:list-settings-line", IconType = "1",
                Order = 1, Status = 1, ParentId = 2, 
                Buttons = "[{\"code\":\"select\",\"desc\":\"查询\"},{\"code\":\"add\",\"desc\":\"新增\"},{\"code\":\"edit\",\"desc\":\"编辑\"},{\"code\":\"delete\",\"desc\":\"删除\"}]",
                TenantId = "t1", CreatedBy = superUser.Id },

            // 设备分组
            new Menu { Id = 4, MenuType = 2, MenuName = "设备分组", RouteName = "device_group", RoutePath = "/device/group",
                Component = "view.device_group", I18nKey = "route.device_group", Icon = "mdi:format-list-group-plus", IconType = "1",
                Order = 2, Status = 1, ParentId = 2, 
                Buttons = "[{\"code\":\"select\",\"desc\":\"查询\"},{\"code\":\"add\",\"desc\":\"新增\"},{\"code\":\"edit\",\"desc\":\"编辑\"},{\"code\":\"delete\",\"desc\":\"删除\"}]",
                TenantId = "t1", CreatedBy = superUser.Id },

            // 设备标签
            new Menu { Id = 5, MenuType = 2, MenuName = "设备标签", RouteName = "device_tag", RoutePath = "/device/tag",
                Component = "view.device_tag", I18nKey = "route.device_tag", Icon = "mdi:tag-multiple-outline", IconType = "1",
                Order = 3, Status = 1, ParentId = 2, 
                Buttons = "[{\"code\":\"select\",\"desc\":\"查询\"},{\"code\":\"add\",\"desc\":\"新增\"},{\"code\":\"edit\",\"desc\":\"编辑\"},{\"code\":\"delete\",\"desc\":\"删除\"}]",
                TenantId = "t1", CreatedBy = superUser.Id },

            // 采集管理（目录）
            new Menu { Id = 6, MenuType = 1, MenuName = "采集管理", RouteName = "collection", RoutePath = "/collection",
                Component = "view.collection", I18nKey = "route.collection", Icon = "carbon:partition-collection", IconType = "1",
                Order = 3, Status = 1, ParentId = null, TenantId = "t1", CreatedBy = superUser.Id },

            // 采集任务
            new Menu { Id = 7, MenuType = 2, MenuName = "采集任务", RouteName = "collection_task", RoutePath = "/collection/task",
                Component = "view.collection_task", I18nKey = "route.collection_task", Icon = "carbon:task-settings", IconType = "1",
                Order = 1, Status = 1, ParentId = 6, 
                Buttons = "[{\"code\":\"select\",\"desc\":\"查询\"},{\"code\":\"add\",\"desc\":\"新增\"},{\"code\":\"edit\",\"desc\":\"编辑\"},{\"code\":\"delete\",\"desc\":\"删除\"}]",
                TenantId = "t1", CreatedBy = superUser.Id },

            // 采集节点
            new Menu { Id = 8, MenuType = 2, MenuName = "采集节点", RouteName = "collection_node", RoutePath = "/collection/node",
                Component = "view.collection_node", I18nKey = "route.collection_node", Icon = "carbon:kubernetes-worker-node", IconType = "1",
                Order = 2, Status = 1, ParentId = 6, 
                Buttons = "[{\"code\":\"select\",\"desc\":\"查询\"},{\"code\":\"add\",\"desc\":\"新增\"},{\"code\":\"edit\",\"desc\":\"编辑\"},{\"code\":\"delete\",\"desc\":\"删除\"}]",
                TenantId = "t1", CreatedBy = superUser.Id },

            // 监控管理（目录）
            new Menu { Id = 9, MenuType = 1, MenuName = "监控管理", RouteName = "monitor", RoutePath = "/monitor",
                Component = "view.monitor", I18nKey = "route.monitor", Icon = "carbon:cloud-monitoring", IconType = "1",
                Order = 4, Status = 1, ParentId = null, TenantId = "t1", CreatedBy = superUser.Id },

            // 实时监控
            new Menu { Id = 10, MenuType = 2, MenuName = "实时监控", RouteName = "monitor_realtime", RoutePath = "/monitor/realtime",
                Component = "view.monitor_realtime", I18nKey = "route.monitor_realtime", Icon = "solar:monitor-camera-broken", IconType = "1",
                Order = 1, Status = 1, ParentId = 9, 
                Buttons = "[{\"code\":\"select\",\"desc\":\"查询\"},{\"code\":\"add\",\"desc\":\"新增\"},{\"code\":\"edit\",\"desc\":\"编辑\"},{\"code\":\"delete\",\"desc\":\"删除\"}]",
                TenantId = "t1", CreatedBy = superUser.Id },

            // 历史数据
            new Menu { Id = 11, MenuType = 2, MenuName = "历史数据", RouteName = "monitor_historical", RoutePath = "/monitor/historical",
                Component = "view.monitor_historical", I18nKey = "route.monitor_historical", Icon = "iconoir:database-monitor", IconType = "1",
                Order = 2, Status = 1, ParentId = 9, 
                Buttons = "[{\"code\":\"select\",\"desc\":\"查询\"},{\"code\":\"add\",\"desc\":\"新增\"},{\"code\":\"edit\",\"desc\":\"编辑\"},{\"code\":\"delete\",\"desc\":\"删除\"}]",
                TenantId = "t1", CreatedBy = superUser.Id },

            // 统计报表
            new Menu { Id = 12, MenuType = 2, MenuName = "统计报表", RouteName = "monitor_statistics", RoutePath = "/monitor/statistics",
                Component = "view.monitor_statistics", I18nKey = "route.monitor_statistics", Icon = "mdi:chart-box-outline", IconType = "1",
                Order = 3, Status = 1, ParentId = 9, 
                Buttons = "[{\"code\":\"select\",\"desc\":\"查询\"},{\"code\":\"add\",\"desc\":\"新增\"},{\"code\":\"edit\",\"desc\":\"编辑\"},{\"code\":\"delete\",\"desc\":\"删除\"}]",
                TenantId = "t1", CreatedBy = superUser.Id },

            // 告警管理（目录）
            new Menu { Id = 13, MenuType = 1, MenuName = "告警管理", RouteName = "alarm", RoutePath = "/alarm",
                Component = "view.alarm", I18nKey = "route.alarm", Icon = "lets-icons:alarm-light", IconType = "1",
                Order = 5, Status = 1, ParentId = null, TenantId = "t1", CreatedBy = superUser.Id },

            // 实时告警
            new Menu { Id = 14, MenuType = 2, MenuName = "实时告警", RouteName = "alarm_realtime", RoutePath = "/alarm/realtime",
                Component = "view.alarm_realtime", I18nKey = "route.alarm_realtime", Icon = "material-symbols-light:alarm-outline", IconType = "1",
                Order = 1, Status = 1, ParentId = 13, 
                Buttons = "[{\"code\":\"select\",\"desc\":\"查询\"},{\"code\":\"add\",\"desc\":\"新增\"},{\"code\":\"edit\",\"desc\":\"编辑\"},{\"code\":\"delete\",\"desc\":\"删除\"}]",
                TenantId = "t1", CreatedBy = superUser.Id },

            // 告警规则
            new Menu { Id = 15, MenuType = 2, MenuName = "告警规则", RouteName = "alarm_rule", RoutePath = "/alarm/rule",
                Component = "view.alarm_rule", I18nKey = "route.alarm_rule", Icon = "bi:file-earmark-ruled", IconType = "1",
                Order = 2, Status = 1, ParentId = 13, 
                Buttons = "[{\"code\":\"select\",\"desc\":\"查询\"},{\"code\":\"add\",\"desc\":\"新增\"},{\"code\":\"edit\",\"desc\":\"编辑\"},{\"code\":\"delete\",\"desc\":\"删除\"}]",
                TenantId = "t1", CreatedBy = superUser.Id },

            // 告警历史
            new Menu { Id = 16, MenuType = 2, MenuName = "告警历史", RouteName = "alarm_history", RoutePath = "/alarm/history",
                Component = "view.alarm_history", I18nKey = "route.alarm_history", Icon = "material-symbols-light:deployed-code-history-outline-sharp", IconType = "1",
                Order = 3, Status = 1, ParentId = 13, 
                Buttons = "[{\"code\":\"select\",\"desc\":\"查询\"},{\"code\":\"add\",\"desc\":\"新增\"},{\"code\":\"edit\",\"desc\":\"编辑\"},{\"code\":\"delete\",\"desc\":\"删除\"}]",
                TenantId = "t1", CreatedBy = superUser.Id },

            // 系统管理（目录）
            new Menu { Id = 17, MenuType = 1, MenuName = "系统管理", RouteName = "manage", RoutePath = "/manage",
                Component = "layout.base$view.manage", I18nKey = "route.manage", Icon = "carbon:cloud-service-management", IconType = "1",
                Order = 99, Status = 1, ParentId = null, TenantId = "t1", CreatedBy = superUser.Id },

            // 用户管理
            new Menu { Id = 18, MenuType = 2, MenuName = "用户管理", RouteName = "manage_user", RoutePath = "/manage/user",
                Component = "view.manage_user", I18nKey = "route.manage_user", Icon = "ic:round-manage-accounts", IconType = "1",
                Order = 1, Status = 1, ParentId = 17, 
                Buttons = "[{\"code\":\"select\",\"desc\":\"查询\"},{\"code\":\"add\",\"desc\":\"新增\"},{\"code\":\"edit\",\"desc\":\"编辑\"},{\"code\":\"delete\",\"desc\":\"删除\"}]",
                TenantId = "t1", CreatedBy = superUser.Id },

            // 角色管理
            new Menu { Id = 19, MenuType = 2, MenuName = "角色管理", RouteName = "manage_role", RoutePath = "/manage/role",
                Component = "view.manage_role", I18nKey = "route.manage_role", Icon = "carbon:user-role", IconType = "1",
                Order = 2, Status = 1, ParentId = 17,
                Buttons = "[{\"code\":\"select\",\"desc\":\"查询\"},{\"code\":\"add\",\"desc\":\"新增\"},{\"code\":\"edit\",\"desc\":\"编辑\"},{\"code\":\"delete\",\"desc\":\"删除\"}]",
                TenantId = "t1", CreatedBy = superUser.Id },

            // 菜单管理
            new Menu { Id = 20, MenuType = 2, MenuName = "菜单管理", RouteName = "manage_menu", RoutePath = "/manage/menu",
                Component = "view.manage_menu", I18nKey = "route.manage_menu", Icon = "material-symbols:route", IconType = "1",
                Order = 3, Status = 1, ParentId = 17,
                Buttons = "[{\"code\":\"select\",\"desc\":\"查询\"},{\"code\":\"add\",\"desc\":\"新增\"},{\"code\":\"edit\",\"desc\":\"编辑\"},{\"code\":\"delete\",\"desc\":\"删除\"}]",
                TenantId = "t1", CreatedBy = superUser.Id }
        };

        context.Menus.AddRange(menus);
        context.SaveChanges();
        Console.WriteLine($"[DataSeeder] ✓ 创建默认菜单: {menus.Count} 条 (租户: t1)");
    }

    /// <summary>
    /// 初始化角色菜单权限
    /// </summary>
    private static void SeedRoleMenus(UnifiedDbContext context)
    {
        var superRole = context.Roles
            .IgnoreQueryFilters()
            .FirstOrDefault(r => r.Code == "R_SUPER" && r.TenantId == "t0");

        var adminRole = context.Roles
            .IgnoreQueryFilters()
            .FirstOrDefault(r => r.Code == "R_ADMIN" && r.TenantId == "t1");

        var userRole = context.Roles
            .IgnoreQueryFilters()
            .FirstOrDefault(r => r.Code == "R_USER" && r.TenantId == "t1");

        if (context.RoleMenus.Any())
        {
            Console.WriteLine("[DataSeeder] ○ 角色菜单权限已存在,跳过创建");
            return;
        }

        var roleMenus = new List<RoleMenu>();

        // 超级管理员拥有所有菜单权限
        if (superRole != null)
        {
            var allMenuIds = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20 };
            foreach (var menuId in allMenuIds)
            {
                roleMenus.Add(new RoleMenu { RoleId = superRole.Id, MenuId = menuId });
            }
        }

        // 管理员拥有业务菜单 + 用户管理权限
        if (adminRole != null)
        {
            var adminMenuIds = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16, 17, 18 };
            foreach (var menuId in adminMenuIds)
            {
                roleMenus.Add(new RoleMenu { RoleId = adminRole.Id, MenuId = menuId });
            }
        }

        // 普通用户只有业务菜单权限
        if (userRole != null)
        {
            var userMenuIds = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15, 16 };
            foreach (var menuId in userMenuIds)
            {
                roleMenus.Add(new RoleMenu { RoleId = userRole.Id, MenuId = menuId });
            }
        }

        context.RoleMenus.AddRange(roleMenus);
        context.SaveChanges();
        Console.WriteLine($"[DataSeeder] ✓ 创建角色菜单权限: {roleMenus.Count} 条");
    }
}
