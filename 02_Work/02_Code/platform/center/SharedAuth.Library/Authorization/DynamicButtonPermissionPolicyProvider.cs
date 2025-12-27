using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace SharedAuth.Authorization;

/// <summary>
/// 动态按钮权限策略提供器 - 根据按钮权限编码动态生成授权策略
/// </summary>
/// <remarks>
/// 工作原理：
/// 1. 当控制器使用 [Authorize("ButtonPermission:18:delete")] 时
/// 2. 系统调用 GetPolicyAsync("ButtonPermission:18:delete")
/// 3. 提供器解析出按钮编码 "18:delete"
/// 4. 动态创建包含 ButtonPermissionRequirement("18:delete") 的授权策略
/// 
/// 策略命名规范：
/// - 必须以 "ButtonPermission:" 前缀开头
/// - 格式：ButtonPermission:{menuId}:{buttonCode}
/// - 例如：ButtonPermission:18:select、ButtonPermission:25:delete
/// </remarks>
public class DynamicButtonPermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;
    private const string POLICY_PREFIX = "ButtonPermission:";

    public DynamicButtonPermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
        => _fallbackPolicyProvider.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
        => _fallbackPolicyProvider.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (policyName.StartsWith(POLICY_PREFIX, StringComparison.OrdinalIgnoreCase))
        {
            // 提取按钮权限编码 - 从策略名称中移除前缀
            var buttonCode = policyName[POLICY_PREFIX.Length..];

            // 动态创建授权策略 - 包含按钮权限需求
            var policy = new AuthorizationPolicyBuilder()
                .AddRequirements(new ButtonPermissionRequirement(buttonCode))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        // 其他策略（如 "Admin"、"User" 等）使用默认提供器
        return _fallbackPolicyProvider.GetPolicyAsync(policyName);
    }
}
