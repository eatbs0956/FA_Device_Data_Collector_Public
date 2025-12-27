using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Shared.Domain.Entities;

namespace Shared.Domain.Helpers;

/// <summary>
/// 审计辅助类 - 自动填充审计字段
/// </summary>
/// <remarks>
/// 从 HTTP 上下文的 JWT Token 中提取当前用户信息，自动填充审计字段
/// 符合 LLD 1.3.2 审计追踪规范
/// </remarks>
public static class AuditHelper
{
    /// <summary>
    /// 设置创建审计字段
    /// </summary>
    /// <param name="entity">需要设置审计字段的实体</param>
    /// <param name="httpContext">HTTP 上下文（可选）</param>
    public static void SetCreateAudit(BaseEntity entity, HttpContext? httpContext)
    {
        var currentUserId = GetCurrentUserId(httpContext);
        var currentTenantId = GetCurrentTenantId(httpContext);

        entity.CreatedBy = currentUserId;
        entity.CreatedAt = DateTimeOffset.UtcNow;
        
        // 如果实体的 TenantId 未设置或为默认值，则自动设置为当前租户
        if (string.IsNullOrEmpty(entity.TenantId) || entity.TenantId == "t1")
        {
            entity.TenantId = currentTenantId;
        }
    }

    /// <summary>
    /// 设置更新审计字段
    /// </summary>
    /// <param name="entity">需要设置审计字段的实体</param>
    /// <param name="httpContext">HTTP 上下文（可选）</param>
    public static void SetUpdateAudit(BaseEntity entity, HttpContext? httpContext)
    {
        var currentUserId = GetCurrentUserId(httpContext);

        entity.UpdatedBy = currentUserId;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// 设置删除审计字段（软删除）
    /// </summary>
    /// <param name="entity">需要设置审计字段的实体</param>
    /// <param name="httpContext">HTTP 上下文（可选）</param>
    public static void SetDeleteAudit(BaseEntity entity, HttpContext? httpContext)
    {
        var currentUserId = GetCurrentUserId(httpContext);

        entity.DeletedFlag = true;
        entity.UpdatedBy = currentUserId;
        entity.UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// 从 HTTP 上下文的 JWT Token 中提取当前用户 ID
    /// </summary>
    /// <param name="httpContext">HTTP 上下文</param>
    /// <returns>当前用户 ID，如果无法获取则返回 null</returns>
    public static Guid? GetCurrentUserId(HttpContext? httpContext)
    {
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        // 尝试从 JWT Claims 中读取用户 ID
        // 支持多种 Claim 类型：sub（JWT标准）、NameIdentifier（ASP.NET Core默认）
        var userIdClaim = httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                          ?? httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? httpContext.User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(userIdClaim))
        {
            return null;
        }

        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    /// <summary>
    /// 从 HTTP 上下文的 JWT Token 中提取当前租户 ID
    /// </summary>
    /// <param name="httpContext">HTTP 上下文</param>
    /// <returns>当前租户 ID，默认返回 "t1"（默认租户）</returns>
    public static string GetCurrentTenantId(HttpContext? httpContext)
    {
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return "t1"; // 默认租户
        }

        // 从 JWT Claims 中读取租户 ID
        var tenantIdClaim = httpContext.User.FindFirstValue("tenant_id")
                            ?? httpContext.User.FindFirstValue("TenantId");

        return string.IsNullOrEmpty(tenantIdClaim) ? "t1" : tenantIdClaim;
    }

    /// <summary>
    /// 判断当前用户是否为超级管理员
    /// </summary>
    /// <param name="httpContext">HTTP 上下文</param>
    /// <returns>是否为超级管理员</returns>
    public static bool IsSuperAdmin(HttpContext? httpContext)
    {
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return false;
        }

        // 检查用户是否拥有 R_SUPER 角色
        return httpContext.User.IsInRole("R_SUPER");
    }
}
