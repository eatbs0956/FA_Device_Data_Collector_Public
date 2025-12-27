using System.Diagnostics;
using Serilog;

namespace Gateway.Api.Middleware;

/// <summary>
/// 请求日志中间件 - 记录请求路径、响应时间、状态码等信息
/// </summary>
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private static readonly HashSet<string> SensitiveHeaders = new(StringComparer.OrdinalIgnoreCase)
    {
        "Authorization",
        "Cookie",
        "Set-Cookie",
        "X-Api-Key"
    };

    public RequestLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var stopwatch = Stopwatch.StartNew();
        var requestId = Guid.NewGuid().ToString("N")[..8];
        
        // 获取客户端 IP
        var clientIp = GetClientIp(context);
        
        // 记录请求开始
        var hasAuth = context.Request.Headers.ContainsKey("Authorization");
        var authHeader = hasAuth ? context.Request.Headers["Authorization"].ToString() : "None";
        var authPreview = hasAuth && authHeader.Length > 20 ? authHeader.Substring(0, 20) + "..." : authHeader;
        
        Log.Information(
            "[{RequestId}] --> {Method} {Path}{QueryString} | IP: {ClientIp} | UA: {UserAgent} | Auth: {AuthHeader}",
            requestId,
            context.Request.Method,
            context.Request.Path,
            context.Request.QueryString,
            clientIp,
            GetSafeUserAgent(context),
            authPreview);

        try
        {
            await _next(context);
            
            stopwatch.Stop();
            
            // 记录请求完成
            var level = context.Response.StatusCode >= 400 ? Serilog.Events.LogEventLevel.Warning : Serilog.Events.LogEventLevel.Information;
            
            Log.Write(
                level,
                "[{RequestId}] <-- {Method} {Path} | Status: {StatusCode} | Duration: {Duration}ms",
                requestId,
                context.Request.Method,
                context.Request.Path,
                context.Response.StatusCode,
                stopwatch.ElapsedMilliseconds);
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            Log.Error(
                ex,
                "[{RequestId}] <-- {Method} {Path} | Status: 500 | Duration: {Duration}ms | Error: {ErrorMessage}",
                requestId,
                context.Request.Method,
                context.Request.Path,
                stopwatch.ElapsedMilliseconds,
                ex.Message);
            
            throw;
        }
    }

    private static string GetClientIp(HttpContext context)
    {
        // 优先从转发头获取真实 IP
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }
        
        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }
        
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private static string GetSafeUserAgent(HttpContext context)
    {
        var userAgent = context.Request.Headers.UserAgent.FirstOrDefault();
        if (string.IsNullOrEmpty(userAgent))
        {
            return "-";
        }
        
        // 截断过长的 User-Agent
        return userAgent.Length > 100 ? userAgent[..100] + "..." : userAgent;
    }
}

/// <summary>
/// 中间件扩展方法
/// </summary>
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<RequestLoggingMiddleware>();
    }
}
