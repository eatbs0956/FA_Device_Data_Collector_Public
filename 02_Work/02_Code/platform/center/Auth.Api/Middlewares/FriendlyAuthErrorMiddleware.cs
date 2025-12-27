using System.Text.Json;
using Auth.Api.Contracts;

namespace Auth.Api.Middlewares;

/// <summary>
/// 友好授权错误中间件 - 处理授权失败的友好错误消息
/// </summary>
public class FriendlyAuthErrorMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<FriendlyAuthErrorMiddleware> _logger;

    public FriendlyAuthErrorMiddleware(RequestDelegate next, ILogger<FriendlyAuthErrorMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // 使用临时的内存流来捕获原始响应
        var originalBodyStream = context.Response.Body;
        
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        await _next(context);

        // 检查是否是403 Forbidden
        if (context.Response.StatusCode == 403)
        {
            var path = context.Request.Path.Value ?? "";
            
            // 提取操作类型
            string operation = GetOperationFromPath(path, context.Request.Method);
            
            var errorMessage = $"当前用户没有{operation}操作权限";
            
            _logger.LogWarning("授权失败: {Path} - {Message}", path, errorMessage);

            // 清空临时流，准备写入新的响应
            responseBody.SetLength(0);
            responseBody.Seek(0, SeekOrigin.Begin);
            
            context.Response.ContentType = "application/json; charset=utf-8";
            
            var response = Envelope<object>.Fail("403", errorMessage);
            var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });

            // 写入友好错误消息到临时流
            var writer = new StreamWriter(responseBody, leaveOpen: true);
            await writer.WriteAsync(json);
            await writer.FlushAsync();
            
            // 复制到原始流
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
        else
        {
            // 将临时流中的内容复制回原始流
            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
        
        // 恢复原始流引用
        context.Response.Body = originalBodyStream;
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
