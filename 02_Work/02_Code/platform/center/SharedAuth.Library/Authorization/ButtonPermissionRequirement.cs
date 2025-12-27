using Microsoft.AspNetCore.Authorization;

namespace SharedAuth.Authorization;

/// <summary>
/// 按钮权限需求 - 定义需要验证的按钮权限编码
/// </summary>
/// <remarks>
/// 按钮权限编码格式：{menuId}:{buttonCode}
/// 例如："18:select", "18:add", "18:edit", "18:delete"
/// 对应菜单ID和按钮操作类型的组合
/// </remarks>
public class ButtonPermissionRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// 按钮权限编码 - 格式为 "{menuId}:{buttonCode}"
    /// </summary>
    public string ButtonCode { get; }

    public ButtonPermissionRequirement(string buttonCode)
    {
        ButtonCode = buttonCode;
    }
}
