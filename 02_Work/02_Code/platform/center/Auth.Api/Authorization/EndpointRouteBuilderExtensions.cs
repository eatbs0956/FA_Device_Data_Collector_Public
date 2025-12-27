using Microsoft.AspNetCore.Builder;

namespace Auth.Api.Authorization;

/// <summary>
/// 路由端点扩展方法 - 简化按钮权限验证的使用
/// </summary>
public static class EndpointRouteBuilderExtensions
{
    /// <summary>
    /// 要求指定的按钮权限
    /// </summary>
    /// <typeparam name="TBuilder">端点构建器类型</typeparam>
    /// <param name="builder">端点构建器</param>
    /// <param name="buttonCode">按钮权限编码，格式: "menuId:buttonCode"</param>
    /// <returns>端点构建器实例，支持链式调用</returns>
    /// <example>
    /// app.MapPost("/admin/users", handler)
    ///    .RequireButtonPermission("18:add");
    /// </example>
    public static TBuilder RequireButtonPermission<TBuilder>(this TBuilder builder, string buttonCode)
        where TBuilder : IEndpointConventionBuilder
    {
        return builder.RequireAuthorization($"ButtonPermission:{buttonCode}");
    }
}
