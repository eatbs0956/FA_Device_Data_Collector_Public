using Microsoft.AspNetCore.Authorization;

namespace Auth.Api.Authorization;

/// <summary>
/// 按钮权限要求 - 用于基于按钮权限的授权策略
/// </summary>
/// <remarks>
/// 定义需要验证的按钮权限编码，格式为 "menuId:buttonCode"
/// 例如: "18:add" 表示菜单ID为18的add按钮权限
/// </remarks>
public class ButtonPermissionRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// 所需的按钮权限编码
    /// </summary>
    public string ButtonCode { get; }

    /// <summary>
    /// 构造函数 - 初始化按钮权限要求
    /// </summary>
    /// <param name="buttonCode">按钮权限编码，格式: "menuId:buttonCode"</param>
    public ButtonPermissionRequirement(string buttonCode)
    {
        ButtonCode = buttonCode ?? throw new ArgumentNullException(nameof(buttonCode));
    }
}
