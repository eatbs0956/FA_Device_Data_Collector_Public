using Shared.Domain.Entities;
using Auth.Api.Contracts;

namespace Auth.Api.Services.Abstractions;

/// <summary>
/// 用户管理服务接口 - 定义用户相关业务逻辑的抽象契约
/// </summary>
public interface IUserService
{
    /// <summary>
    /// 获取用户分页列表
    /// </summary>
    Task<(List<UserDto> Items, int Total)> GetUserListAsync(
        int current = 1,
        int size = 10,
        string? userName = null,
        string? nickName = null,
        string? userPhone = null,
        string? userEmail = null,
        int? userGender = null,
        int? status = null);

    /// <summary>
    /// 根据ID获取用户详情
    /// </summary>
    Task<User?> GetUserByIdAsync(Guid id);

    /// <summary>
    /// 创建新用户
    /// </summary>
    Task<User> CreateUserAsync(
        string userName,
        string nickName = "",
        int? userGender = null,
        string userPhone = "",
        string userEmail = "",
        int status = 1,
        string[]? userRoles = null,
        string? password = null);

    /// <summary>
    /// 更新用户信息
    /// </summary>
    Task<bool> UpdateUserAsync(
        Guid id,
        string userName,
        string nickName = "",
        int? userGender = null,
        string userPhone = "",
        string userEmail = "",
        int? status = null,
        string[]? userRoles = null,
        string? password = null);

    /// <summary>
    /// 删除用户(软删除)
    /// </summary>
    Task<bool> DeleteUserAsync(Guid id);

    /// <summary>
    /// 批量删除用户(软删除)
    /// </summary>
    Task<int> BatchDeleteUsersAsync(List<Guid> ids);

    /// <summary>
    /// 分配角色到用户
    /// </summary>
    Task AssignRolesToUserAsync(Guid userId, string[] roleCodes);

    /// <summary>
    /// 获取用户的角色编码数组
    /// </summary>
    Task<string[]> GetUserRoleCodesAsync(Guid userId);
}
