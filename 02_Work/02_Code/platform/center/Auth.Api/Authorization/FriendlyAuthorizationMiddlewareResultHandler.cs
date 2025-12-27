using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Auth.Api.Contracts;
using System.Text.Json;

namespace Auth.Api.Authorization;

/// <summary>
/// 自定义授权结果处理器 - 返回友好的中文错误消息
/// </summary>
public class FriendlyAuthorizationMiddlewareResultHandler : IAuthorizationMiddlewareResultHandler
{
    private readonly ILogger<FriendlyAuthorizationMiddlewareResultHandler> _logger;

    public FriendlyAuthorizationMiddlewareResultHandler(ILogger<FriendlyAuthorizationMiddlewareResultHandler> logger)
    {
        _logger = logger;
    }

    public async Task HandleAsync(
        RequestDelegate next,
        HttpContext context,
        AuthorizationPolicy policy,
        PolicyAuthorizationResult authorizeResult)
    {
        // 如果授权成功，继续执行后续中间件
        if (authorizeResult.Succeeded)
        {
            await next(context);
            return;
        }

        // 授权失败，返回友好错误消息
        var path = context.Request.Path.Value ?? "";
        var method = context.Request.Method;
        
        // 提取操作类型
        string operation = GetOperationFromPath(path, method);
        var errorMessage = $"当前用户没有{operation}操作权限";
        
        _logger.LogWarning("授权失败: {Path} - {Message}", path, errorMessage);

        // 设置响应
        context.Response.StatusCode = 403;
        context.Response.ContentType = "application/json; charset=utf-8";
        
        var response = Envelope<object>.Fail("403", errorMessage);
        await context.Response.WriteAsJsonAsync(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }

    /// <summary>
    /// 根据路径和HTTP方法提取操作类型
    /// </summary>
    private string GetOperationFromPath(string path, string method)
    {
        // 先判断具体路径中的操作类型
        if (path.Contains("getUserList", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("getRoleList", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("getMenuList", StringComparison.OrdinalIgnoreCase))
        {
            return "查询";
        }
        
        if (path.Contains("addUsers", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("addRoles", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("addMenus", StringComparison.OrdinalIgnoreCase))
        {
            return "新增";
        }
        
        if (path.Contains("updateUsers", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("updateRoles", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("updateMenus", StringComparison.OrdinalIgnoreCase))
        {
            return "编辑";
        }
        
        if (path.Contains("deleteUsers", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("deleteRoles", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("deleteMenus", StringComparison.OrdinalIgnoreCase))
        {
            return "删除";
        }

        if (path.Contains("saveRoleMenus", StringComparison.OrdinalIgnoreCase) ||
            path.Contains("saveRoleButtons", StringComparison.OrdinalIgnoreCase))
        {
            return "保存权限配置";
        }

        // 根据HTTP方法判断操作类型（兜底逻辑）
        return method.ToUpper() switch
        {
            "GET" => "查询",
            "POST" => "新增",
            "PUT" => "编辑",
            "DELETE" => "删除",
            _ => "访问"
        };
    }
}
