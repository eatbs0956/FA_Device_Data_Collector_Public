using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Auth.Api.Authorization;

/// <summary>
/// 动态按钮权限策略提供器 - 在运行时动态创建按钮权限策略
/// </summary>
/// <remarks>
/// 当遇到 "ButtonPermission:{buttonCode}" 格式的策略时，自动创建对应的按钮权限验证策略
/// 无需在 Program.cs 中预定义所有按钮权限策略
/// </remarks>
public class DynamicButtonPermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private const string POLICY_PREFIX = "ButtonPermission:";
    private readonly DefaultAuthorizationPolicyProvider _fallbackPolicyProvider;

    public DynamicButtonPermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        _fallbackPolicyProvider = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync()
    {
        return _fallbackPolicyProvider.GetDefaultPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync()
    {
        return _fallbackPolicyProvider.GetFallbackPolicyAsync();
    }

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // 检查是否是按钮权限策略
        if (policyName.StartsWith(POLICY_PREFIX, StringComparison.OrdinalIgnoreCase))
        {
            // 提取按钮编码
            var buttonCode = policyName.Substring(POLICY_PREFIX.Length);

            // 动态创建策略
            var policy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .AddRequirements(new ButtonPermissionRequirement(buttonCode))
                .Build();

            return Task.FromResult<AuthorizationPolicy?>(policy);
        }

        // 对于非按钮权限策略，使用默认提供器
        return _fallbackPolicyProvider.GetPolicyAsync(policyName);
    }
}
