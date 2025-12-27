using Shared.Domain.Entities;
using Auth.Api.Contracts;

namespace Auth.Api.Services.Abstractions;

/// <summary>
/// 角色管理服务接口 - 定义角色相关业务逻辑的抽象契约
/// </summary>
public interface IRoleService
{
    /// <summary>
    /// 获取角色分页列表
    /// </summary>
    Task<(List<RoleDto> Items, int Total)> GetRoleListAsync(
        int current = 1,
        int size = 10,
        string? roleName = null,
        string? roleCode = null,
        int? status = null);

    /// <summary>
    /// 根据ID获取角色详情
    /// </summary>
    Task<Role?> GetRoleByIdAsync(Guid id);

    /// <summary>
    /// 创建新角色
    /// </summary>
    Task<Role> CreateRoleAsync(
        string roleName,
        string roleCode,
        string roleDesc = "",
        int status = 1);

    /// <summary>
    /// 更新角色信息
    /// </summary>
    Task<bool> UpdateRoleAsync(
        Guid id,
        string roleName,
        string roleCode,
        string roleDesc = "",
        int? status = null);

    /// <summary>
    /// 删除角色(软删除)
    /// </summary>
    Task<bool> DeleteRoleAsync(Guid id);

    /// <summary>
    /// 批量删除角色(软删除)
    /// </summary>
    Task<int> BatchDeleteRolesAsync(List<Guid> ids);
}
