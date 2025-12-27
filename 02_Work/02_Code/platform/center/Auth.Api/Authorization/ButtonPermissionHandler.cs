using System.Security.Claims;
using Shared.Domain.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;

namespace Auth.Api.Authorization;

/// <summary>
/// 按钮权限授权处理器 - 验证用户是否拥有指定的按钮权限
/// </summary>
/// <remarks>
/// 工作流程：
/// 1. 从JWT Claims中提取用户ID
/// 2. 查询用户所有角色
/// 3. 查询这些角色绑定的所有按钮权限
/// 4. 验证是否包含所需的按钮权限编码
/// 特殊处理：R_SUPER角色拥有所有权限
/// </remarks>
public class ButtonPermissionHandler : AuthorizationHandler<ButtonPermissionRequirement>
{
    private readonly UnifiedDbContext _db;
    private readonly ILogger<ButtonPermissionHandler> _logger;

    public ButtonPermissionHandler(UnifiedDbContext db, ILogger<ButtonPermissionHandler> logger)
    {
        _db = db;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ButtonPermissionRequirement requirement)
    {
        // 1. 提取用户ID
        var userIdClaim = context.User.FindFirst("sub") ?? context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            _logger.LogWarning("用户未认证或用户ID无效");
            context.Fail();
            return;
        }

        try
        {
            // 2. 查询用户的所有角色编码
            var userRoles = await _db.UserRoles
                .Where(ur => ur.UserId == userId)
                .Join(_db.Roles,
                    ur => ur.RoleId,
                    r => r.Id,
                    (ur, r) => r.Code)
                .ToListAsync();

            if (!userRoles.Any())
            {
                _logger.LogWarning("用户 {UserId} 没有分配任何角色", userId);
                context.Fail();
                return;
            }

            // 3. 特殊处理：R_SUPER 角色拥有所有权限
            if (userRoles.Contains("R_SUPER"))
            {
                _logger.LogInformation("用户 {UserId} 拥有超级管理员角色，允许访问按钮 {ButtonCode}", 
                    userId, requirement.ButtonCode);
                context.Succeed(requirement);
                return;
            }

            // 4. 查询用户角色绑定的所有按钮权限
            var roleIds = await _db.Roles
                .Where(r => userRoles.Contains(r.Code))
                .Select(r => r.Id)
                .ToListAsync();

            var hasPermission = await _db.RoleButtons
                .AnyAsync(rb => roleIds.Contains(rb.RoleId) && rb.ButtonCode == requirement.ButtonCode);

            if (hasPermission)
            {
                _logger.LogInformation("用户 {UserId} 拥有按钮权限 {ButtonCode}", userId, requirement.ButtonCode);
                context.Succeed(requirement);
            }
            else
            {
                _logger.LogWarning("用户 {UserId} 没有按钮权限 {ButtonCode}，用户角色: {Roles}", 
                    userId, requirement.ButtonCode, string.Join(", ", userRoles));
                context.Fail();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "验证用户 {UserId} 的按钮权限 {ButtonCode} 时发生错误", userId, requirement.ButtonCode);
            context.Fail();
        }
    }
}
