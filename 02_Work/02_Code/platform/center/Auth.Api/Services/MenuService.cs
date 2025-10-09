using Auth.Api.Models;
using Auth.Api.Contracts;
using Microsoft.EntityFrameworkCore;

namespace Auth.Api.Services;

/// <summary>
/// 菜单服务类 - 提供菜单管理的CRUD操作和业务逻辑
/// </summary>
public class MenuService(AuthDbContext db)
{
    /// <summary>
    /// 获取菜单列表 - 分页查询菜单数据（树形结构）
    /// </summary>
    /// <param name="page">页码 - 当前页数</param>
    /// <param name="pageSize">页大小 - 每页记录数</param>
    /// <returns>菜单树形列表和总数</returns>
    public async Task<(List<Menu> Items, int Total)> GetMenuListAsync(int page = 1, int pageSize = 10)
    {
        // 获取所有菜单数据 - 用于构建完整树形结构
        var allMenus = await db.Menus
            .OrderBy(x => x.Order)
            .ToListAsync();

        // 总记录数 - 只统计顶级菜单数量
        var rootMenus = allMenus.Where(m => m.ParentId == null).ToList();
        var total = rootMenus.Count;

        // 分页获取顶级菜单 - 按照排序号排序
        var pagedRootMenus = rootMenus
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        // 为每个顶级菜单递归填充子菜单
        var items = new List<Menu>();
        foreach (var rootMenu in pagedRootMenus)
        {
            items.Add(BuildMenuTree(rootMenu, allMenus));
        }

        return (items, total);
    }

    /// <summary>
    /// 递归构建菜单树 - 为菜单填充所有子菜单
    /// </summary>
    /// <param name="menu">当前菜单</param>
    /// <param name="allMenus">所有菜单列表</param>
    /// <returns>包含子菜单的菜单对象</returns>
    private Menu BuildMenuTree(Menu menu, List<Menu> allMenus)
    {
        // 查找当前菜单的所有子菜单
        var children = allMenus
            .Where(m => m.ParentId == menu.Id)
            .OrderBy(m => m.Order)
            .ToList();

        // 如果有子菜单，递归构建子菜单树
        if (children.Any())
        {
            menu.Children = new List<Menu>();
            foreach (var child in children)
            {
                menu.Children.Add(BuildMenuTree(child, allMenus));
            }
        }

        return menu;
    }

    /// <summary>
    /// 获取所有菜单 - 返回所有菜单的树形结构
    /// </summary>
    /// <returns>菜单树形列表</returns>
    public async Task<List<Menu>> GetAllMenusAsync()
    {
        // 获取所有菜单 - 按照父级ID和排序号排序
        return await db.Menus
            .OrderBy(x => x.ParentId)
            .ThenBy(x => x.Order)
            .ToListAsync();
    }

    /// <summary>
    /// 获取菜单树 - 返回树形结构的菜单列表,用于角色权限配置
    /// </summary>
    /// <returns>菜单树形列表</returns>
    public async Task<List<MenuTreeDto>> GetMenuTreeAsync()
    {
        // 获取所有菜单
        var allMenus = await db.Menus
            .OrderBy(x => x.Order)
            .ToListAsync();

        // 构建树形结构 - 只返回顶级菜单,子菜单通过Children属性关联
        var rootMenus = allMenus.Where(m => m.ParentId == null).ToList();
        
        // 转换为 MenuTreeDto 并递归填充子菜单
        var treeDtos = new List<MenuTreeDto>();
        foreach (var rootMenu in rootMenus)
        {
            treeDtos.Add(ConvertToTreeDto(rootMenu, allMenus));
        }

        return treeDtos;
    }

    /// <summary>
    /// 转换菜单实体为菜单树DTO
    /// </summary>
    /// <param name="menu">菜单实体</param>
    /// <param name="allMenus">所有菜单列表</param>
    /// <returns>菜单树DTO</returns>
    private MenuTreeDto ConvertToTreeDto(Menu menu, List<Menu> allMenus)
    {
        var dto = new MenuTreeDto
        {
            Id = menu.Id,
            Label = menu.MenuName,
            PId = menu.ParentId ?? 0
        };

        // 查找子菜单
        var children = allMenus.Where(m => m.ParentId == menu.Id).OrderBy(m => m.Order).ToList();
        
        if (children.Any())
        {
            dto.Children = new List<MenuTreeDto>();
            foreach (var child in children)
            {
                dto.Children.Add(ConvertToTreeDto(child, allMenus));
            }
        }

        return dto;
    }

    /// <summary>
    /// 根据ID获取菜单 - 获取单个菜单详情
    /// </summary>
    /// <param name="id">菜单ID - 菜单唯一标识符</param>
    /// <returns>菜单实体或null</returns>
    public async Task<Menu?> GetMenuByIdAsync(int id)
    {
        return await db.Menus.FindAsync(id);
    }

    /// <summary>
    /// 创建菜单 - 新增菜单记录
    /// </summary>
    /// <param name="menu">菜单实体 - 要创建的菜单对象</param>
    /// <returns>创建的菜单实体</returns>
    public async Task<Menu> CreateMenuAsync(Menu menu)
    {
        // 查询当前最大ID并生成新ID
        var maxId = await db.Menus.AnyAsync() 
            ? await db.Menus.MaxAsync(m => m.Id) 
            : 0;
        menu.Id = maxId + 1;

        // 设置创建和更新时间
        menu.CreatedAt = DateTimeOffset.UtcNow;
        menu.UpdatedAt = DateTimeOffset.UtcNow;

        // 添加到数据库
        db.Menus.Add(menu);
        await db.SaveChangesAsync();

        return menu;
    }

    /// <summary>
    /// 更新菜单 - 修改菜单记录
    /// </summary>
    /// <param name="id">菜单ID - 要更新的菜单标识</param>
    /// <param name="menu">菜单实体 - 更新的菜单数据</param>
    /// <returns>是否更新成功</returns>
    public async Task<bool> UpdateMenuAsync(int id, Menu menu)
    {
        // 查找现有菜单
        var existing = await db.Menus.FindAsync(id);
        if (existing == null)
        {
            return false;
        }

        // 更新字段
        existing.MenuType = menu.MenuType;
        existing.MenuName = menu.MenuName;
        existing.RouteName = menu.RouteName;
        existing.RoutePath = menu.RoutePath;
        existing.Component = menu.Component;
        existing.I18nKey = menu.I18nKey;
        existing.Icon = menu.Icon;
        existing.IconType = menu.IconType;
        existing.ParentId = menu.ParentId;
        existing.Order = menu.Order;
        existing.Status = menu.Status;
        existing.HideInMenu = menu.HideInMenu;
        existing.ActiveMenu = menu.ActiveMenu;
        existing.MultiTab = menu.MultiTab;
        existing.FixedIndexInTab = menu.FixedIndexInTab;
        existing.Query = menu.Query;
        existing.UpdatedAt = DateTimeOffset.UtcNow;

        // 保存更改
        await db.SaveChangesAsync();
        return true;
    }

    /// <summary>
    /// 删除菜单 - 删除单个菜单记录
    /// </summary>
    /// <param name="id">菜单ID - 要删除的菜单标识</param>
    /// <returns>是否删除成功</returns>
    public async Task<bool> DeleteMenuAsync(int id)
    {
        // 查找菜单
        var menu = await db.Menus.FindAsync(id);
        if (menu == null)
        {
            return false;
        }

        // 检查是否有子菜单
        var hasChildren = await db.Menus.AnyAsync(x => x.ParentId == id);
        if (hasChildren)
        {
            throw new InvalidOperationException("Cannot delete menu with children. Please delete children first.");
        }

        // 删除菜单
        db.Menus.Remove(menu);
        await db.SaveChangesAsync();

        return true;
    }

    /// <summary>
    /// 批量删除菜单 - 删除多个菜单记录
    /// </summary>
    /// <param name="ids">菜单ID列表 - 要删除的菜单标识集合</param>
    /// <returns>删除的记录数</returns>
    public async Task<int> BatchDeleteMenusAsync(List<int> ids)
    {
        var count = 0;

        foreach (var id in ids)
        {
            try
            {
                if (await DeleteMenuAsync(id))
                {
                    count++;
                }
            }
            catch
            {
                // 忽略有子菜单的删除失败
                continue;
            }
        }

        return count;
    }

    /// <summary>
    /// 获取所有页面组件列表 - 从数据库中获取所有菜单的路由名称
    /// </summary>
    /// <returns>路由名称列表</returns>
    public async Task<List<string>> GetAllPagesAsync()
    {
        // 从数据库中获取所有菜单的路由名称（RouteName字段）
        var routeNames = await db.Menus
            .Where(m => !string.IsNullOrEmpty(m.RouteName))
            .Select(m => m.RouteName)
            .Distinct()
            .OrderBy(r => r)
            .ToListAsync();

        return routeNames;
    }

    /// <summary>
    /// 获取角色的菜单权限 - 查询指定角色已授权的菜单ID列表
    /// </summary>
    /// <param name="roleId">角色ID</param>
    /// <returns>菜单ID列表</returns>
    public async Task<List<int>> GetRoleMenusAsync(Guid roleId)
    {
        var menuIds = await db.RoleMenus
            .Where(rm => rm.RoleId == roleId)
            .Select(rm => rm.MenuId)
            .ToListAsync();

        return menuIds;
    }

    /// <summary>
    /// 保存角色的菜单权限 - 先删除旧权限,再添加新权限
    /// </summary>
    /// <param name="roleId">角色ID</param>
    /// <param name="menuIds">菜单ID列表</param>
    /// <returns>影响的行数</returns>
    public async Task<int> SaveRoleMenusAsync(Guid roleId, List<int> menuIds)
    {
        // 1. 删除该角色所有旧的菜单权限
        var oldRoleMenus = await db.RoleMenus
            .Where(rm => rm.RoleId == roleId)
            .ToListAsync();

        db.RoleMenus.RemoveRange(oldRoleMenus);

        // 2. 添加新的菜单权限
        var newRoleMenus = menuIds.Select(menuId => new RoleMenu
        {
            RoleId = roleId,
            MenuId = menuId
        }).ToList();

        await db.RoleMenus.AddRangeAsync(newRoleMenus);

        // 3. 保存更改
        var affectedRows = await db.SaveChangesAsync();

        return affectedRows;
    }
}
