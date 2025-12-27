using Microsoft.AspNetCore.Authorization;

namespace Auth.Api.Authorization;

/// <summary>
/// 授权策略构建器扩展 - 简化按钮权限策略的添加
/// </summary>
public static class AuthorizationPolicyBuilderExtensions
{
    /// <summary>
    /// 要求指定的按钮权限
    /// </summary>
    /// <param name="builder">策略构建器</param>
    /// <param name="buttonCode">按钮权限编码，格式: "menuId:buttonCode"</param>
    /// <returns>策略构建器实例，支持链式调用</returns>
    public static AuthorizationPolicyBuilder RequireButtonPermission(
        this AuthorizationPolicyBuilder builder,
        string buttonCode)
    {
        builder.Requirements.Add(new ButtonPermissionRequirement(buttonCode));
        return builder;
    }
}
