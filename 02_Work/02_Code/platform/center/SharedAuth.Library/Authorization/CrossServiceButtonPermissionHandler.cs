using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SharedAuth.Data;

namespace SharedAuth.Authorization;

/// <summary>
/// 跨服务按钮权限授权处理器 - 通过访问Auth数据库验证用户按钮权限
/// </summary>
/// <remarks>
/// 工作流程：
/// 1. 从JWT Claims中提取用户ID
/// 2. 连接Auth数据库查询用户所有角色
/// 3. 查询这些角色绑定的所有按钮权限
/// 4. 验证是否包含所需的按钮权限编码
/// 特殊处理：R_SUPER角色拥有所有权限
/// 
/// 适用场景：
/// - 微服务架构下，业务API（如Device.Api）需要验证用户的按钮级权限
/// - 避免在每个服务中重复实现权限验证逻辑
/// </remarks>
public class CrossServiceButtonPermissionHandler : AuthorizationHandler<ButtonPermissionRequirement>
{
    private readonly IAuthDbContextFactory _contextFactory;
    private readonly ILogger<CrossServiceButtonPermissionHandler> _logger;

    public CrossServiceButtonPermissionHandler(
        IAuthDbContextFactory contextFactory,
        ILogger<CrossServiceButtonPermissionHandler> logger)
    {
        _contextFactory = contextFactory;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ButtonPermissionRequirement requirement)
    {
        // 1. 提取用户ID - 优先使用sub claim，回退到NameIdentifier
        var userIdClaim = context.User.FindFirst("sub") ?? context.User.FindFirst(ClaimTypes.NameIdentifier);
        if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
        {
            // 记录所有 Claims 以便调试
            var allClaims = string.Join(", ", context.User.Claims.Select(c => $"{c.Type}={c.Value}"));
            _logger.LogWarning("用户未认证或用户ID无效。IsAuthenticated={IsAuth}, Claims=[{Claims}]", 
                context.User.Identity?.IsAuthenticated ?? false, 
                allClaims);
            context.Fail();
            return;
        }

        try
        {
            // 2. 创建Auth数据库连接 - 使用工厂模式避免DbContext生命周期问题
            await using var db = _contextFactory.CreateDbContext();

            // 3. 查询用户的所有角色编码
            var userRoles = await db.UserRoles
                .Where(ur => ur.UserId == userId)
                .Join(db.Roles,
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

            // 4. 特殊处理：R_SUPER 角色拥有所有权限
            if (userRoles.Contains("R_SUPER"))
            {
                _logger.LogInformation("用户 {UserId} 拥有超级管理员角色，允许访问按钮 {ButtonCode}",
                    userId, requirement.ButtonCode);
                context.Succeed(requirement);
                return;
            }

            // 5. 查询用户角色绑定的所有按钮权限
            var roleIds = await db.Roles
                .Where(r => userRoles.Contains(r.Code))
                .Select(r => r.Id)
                .ToListAsync();

            var hasPermission = await db.RoleButtons
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
