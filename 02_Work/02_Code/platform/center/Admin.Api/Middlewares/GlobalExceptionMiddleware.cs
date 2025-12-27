using System.Net;
using System.Text.Json;

namespace Admin.Api.Middlewares;

/// <summary>
/// 全局异常处理中间件
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _env;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment env)
    {
        _next = next;
        _logger = logger;
        _env = env;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        // 记录详细的异常信息
        _logger.LogError(exception,
            "Unhandled exception occurred. Path: {Path}, Method: {Method}, User: {User}, TraceId: {TraceId}",
            context.Request.Path,
            context.Request.Method,
            context.User.Identity?.Name ?? "Anonymous",
            context.TraceIdentifier);

        // 构造错误响应
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        var response = new
        {
            code = "500",
            msg = _env.IsDevelopment() 
                ? $"Internal Server Error: {exception.Message}" 
                : "Internal Server Error",
            data = (object?)null,
            traceId = context.TraceIdentifier,
            // 开发环境返回堆栈信息
            stackTrace = _env.IsDevelopment() ? exception.StackTrace : null
        };

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }
}

/// <summary>
/// 全局异常处理中间件扩展方法
/// </summary>
public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionMiddleware>();
    }
}
