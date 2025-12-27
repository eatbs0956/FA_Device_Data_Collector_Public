using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Shared.Domain.Entities;

namespace Auth.Api.Services;

/// <summary>
/// 审计辅助类 - 用于设置实体的审计字段
/// </summary>
/// <remarks>
/// 这是实体级审计，负责填充实体的审计字段（CreatedBy, UpdatedBy, CreatedAt, UpdatedAt等）
/// 与 AuditService 的 HTTP 请求级审计互补，形成双层审计机制
/// </remarks>
public static class AuditHelper
{
    /// <summary>
    /// 设置创建审计信息
    /// </summary>
    /// <param name="entity">实体对象（必须包含审计字段）</param>
    /// <param name="httpContext">HTTP 上下文，用于获取当前用户</param>
    public static void SetCreateAudit(object entity, HttpContext? httpContext)
    {
        var userId = GetCurrentUserId(httpContext);
        var now = DateTimeOffset.UtcNow;

        // 使用反射设置审计字段
        var type = entity.GetType();
        
        type.GetProperty("CreatedBy")?.SetValue(entity, userId);
        type.GetProperty("CreatedAt")?.SetValue(entity, now);
        type.GetProperty("UpdatedBy")?.SetValue(entity, null);
        type.GetProperty("UpdatedAt")?.SetValue(entity, now);
        type.GetProperty("DeletedFlag")?.SetValue(entity, false);
    }

    /// <summary>
    /// 设置更新审计信息
    /// </summary>
    /// <param name="entity">实体对象（必须包含审计字段）</param>
    /// <param name="httpContext">HTTP 上下文，用于获取当前用户</param>
    public static void SetUpdateAudit(object entity, HttpContext? httpContext)
    {
        var userId = GetCurrentUserId(httpContext);
        var now = DateTimeOffset.UtcNow;

        // 使用反射设置审计字段
        var type = entity.GetType();
        
        type.GetProperty("UpdatedBy")?.SetValue(entity, userId);
        type.GetProperty("UpdatedAt")?.SetValue(entity, now);
    }

    /// <summary>
    /// 设置删除审计信息（软删除）
    /// </summary>
    /// <param name="entity">实体对象（必须包含审计字段）</param>
    /// <param name="httpContext">HTTP 上下文，用于获取当前用户</param>
    public static void SetDeleteAudit(object entity, HttpContext? httpContext)
    {
        var userId = GetCurrentUserId(httpContext);
        var now = DateTimeOffset.UtcNow;

        // 使用反射设置审计字段
        var type = entity.GetType();
        
        type.GetProperty("UpdatedBy")?.SetValue(entity, userId);
        type.GetProperty("UpdatedAt")?.SetValue(entity, now);
        type.GetProperty("DeletedFlag")?.SetValue(entity, true);
    }

    /// <summary>
    /// 从 HTTP 上下文中获取当前用户 ID
    /// </summary>
    /// <param name="httpContext">HTTP 上下文</param>
    /// <returns>用户 ID（Guid），如果无法获取则返回 null</returns>
    private static Guid? GetCurrentUserId(HttpContext? httpContext)
    {
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        // 尝试从多个可能的 Claim 类型中读取用户 ID
        var userIdString = httpContext.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                          ?? httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier)
                          ?? httpContext.User.FindFirstValue("sub");

        if (string.IsNullOrEmpty(userIdString))
        {
            return null;
        }

        return Guid.TryParse(userIdString, out var userId) ? userId : null;
    }
}
