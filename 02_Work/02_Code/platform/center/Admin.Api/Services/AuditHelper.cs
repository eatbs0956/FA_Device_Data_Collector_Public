using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Admin.Api.Services;

/// <summary>
/// 审计辅助类 - 用于设置实体的审计字段
/// 符合 LLD 1.3.2 审计追踪规范要求
/// </summary>
/// <remarks>
/// 这是实体级审计，负责填充实体的审计字段（CreatedBy, UpdatedBy, CreatedAt, UpdatedAt等）
/// 从 JWT Token 中提取当前用户信息，确保审计数据的准确性
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
        var tenantId = GetCurrentTenantId(httpContext);
        var now = DateTimeOffset.UtcNow;

        // 使用反射设置审计字段（类型与 Auth.Api 保持一致）
        var type = entity.GetType();
        
        type.GetProperty("CreatedBy")?.SetValue(entity, userId);  // Guid?
        type.GetProperty("CreatedAt")?.SetValue(entity, now);
        type.GetProperty("UpdatedBy")?.SetValue(entity, null);
        type.GetProperty("UpdatedAt")?.SetValue(entity, now);
        type.GetProperty("DeletedFlag")?.SetValue(entity, false);
        type.GetProperty("TenantId")?.SetValue(entity, tenantId ?? "t0");  // string, 默认"t0"
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

        // 使用反射设置审计字段（类型与 Auth.Api 保持一致）
        var type = entity.GetType();
        
        type.GetProperty("UpdatedBy")?.SetValue(entity, userId);  // Guid?
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

        // 使用反射设置审计字段（类型与 Auth.Api 保持一致）
        var type = entity.GetType();
        
        type.GetProperty("UpdatedBy")?.SetValue(entity, userId);  // Guid?
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
                          ?? httpContext.User.FindFirstValue("sub")
                          ?? httpContext.User.FindFirstValue("user_id");

        if (string.IsNullOrEmpty(userIdString))
        {
            return null;
        }

        return Guid.TryParse(userIdString, out var userId) ? userId : null;
    }

    /// <summary>
    /// 从 HTTP 上下文中获取租户 ID
    /// </summary>
    /// <param name="httpContext">HTTP 上下文</param>
    /// <returns>租户 ID（string），如果无法获取则返回 null</returns>
    public static string? GetCurrentTenantId(HttpContext? httpContext)
    {
        if (httpContext?.User?.Identity?.IsAuthenticated != true)
        {
            return null;
        }

        // 尝试从 Claim 中读取租户 ID（与 Auth.Api 保持一致，类型为 string）
        var tenantId = httpContext.User.FindFirstValue("tenant_id")
                      ?? httpContext.User.FindFirstValue("TenantId");

        return tenantId;
    }
}
