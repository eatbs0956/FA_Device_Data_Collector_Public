using Shared.Domain.Entities;
using Auth.Api.Contracts;

namespace Auth.Api.Services.Abstractions;

/// <summary>
/// 菜单管理服务接口 - 定义菜单相关业务逻辑的抽象契约
/// </summary>
public interface IMenuService
{
    /// <summary>
    /// 获取菜单分页列表
    /// </summary>
    Task<(List<Menu> Items, int Total)> GetMenuListAsync(int page = 1, int pageSize = 10);

    /// <summary>
    /// 获取所有菜单(不分页)
    /// </summary>
    Task<List<Menu>> GetAllMenusAsync();

    /// <summary>
    /// 获取菜单树结构
    /// </summary>
    Task<List<MenuTreeDto>> GetMenuTreeAsync();

    /// <summary>
    /// 根据ID获取菜单详情
    /// </summary>
    Task<Menu?> GetMenuByIdAsync(int id);

    /// <summary>
    /// 创建新菜单
    /// </summary>
    Task<Menu> CreateMenuAsync(Menu menu);

    /// <summary>
    /// 更新菜单信息
    /// </summary>
    Task<bool> UpdateMenuAsync(int id, Menu menu);

    /// <summary>
    /// 删除菜单(软删除)
    /// </summary>
    Task<bool> DeleteMenuAsync(int id);

    /// <summary>
    /// 批量删除菜单(软删除)
    /// </summary>
    Task<int> BatchDeleteMenusAsync(List<int> ids);

    /// <summary>
    /// 获取所有页面权限
    /// </summary>
    Task<List<string>> GetAllPagesAsync();

    /// <summary>
    /// 获取角色的菜单ID列表
    /// </summary>
    Task<List<int>> GetRoleMenusAsync(Guid roleId);

    /// <summary>
    /// 保存角色菜单权限
    /// </summary>
    Task<int> SaveRoleMenusAsync(Guid roleId, List<int> menuIds);
}
