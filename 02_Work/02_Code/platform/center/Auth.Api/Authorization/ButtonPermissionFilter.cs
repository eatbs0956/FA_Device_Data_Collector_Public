using Microsoft.AspNetCore.Authorization;

namespace Auth.Api.Authorization;

/// <summary>
/// 按钮权限端点过滤器 - 用于 Minimal API 的按钮权限验证
/// </summary>
/// <remarks>
/// 使用方式：
/// app.MapPost("/admin/users", handler)
///    .AddEndpointFilter(new ButtonPermissionFilter("18:add"));
/// </remarks>
public class ButtonPermissionFilter : IEndpointFilter
{
    private readonly string _buttonCode;

    /// <summary>
    /// 构造函数
    /// </summary>
    /// <param name="buttonCode">按钮权限编码，格式: "menuId:buttonCode"</param>
    public ButtonPermissionFilter(string buttonCode)
    {
        _buttonCode = buttonCode ?? throw new ArgumentNullException(nameof(buttonCode));
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;
        var authService = httpContext.RequestServices.GetRequiredService<IAuthorizationService>();

        // 执行按钮权限验证
        var authResult = await authService.AuthorizeAsync(
            httpContext.User,
            null,
            new ButtonPermissionRequirement(_buttonCode));

        if (!authResult.Succeeded)
        {
            // 返回 403 Forbidden
            httpContext.Response.StatusCode = 403;
            await httpContext.Response.WriteAsJsonAsync(new
            {
                code = "403",
                msg = "您没有权限执行此操作",
                data = (object?)null
            });
            return null;
        }

        // 权限验证通过，继续执行
        return await next(context);
    }
}
