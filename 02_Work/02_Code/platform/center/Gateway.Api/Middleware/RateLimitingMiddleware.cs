using System.Collections.Concurrent;
using Serilog;

namespace Gateway.Api.Middleware;

/// <summary>
/// IP 限流中间件 - 基于滑动窗口算法的 IP 限流
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RateLimitOptions _options;
    private readonly ConcurrentDictionary<string, RateLimitCounter> _counters = new();

    public RateLimitingMiddleware(RequestDelegate next, RateLimitOptions options)
    {
        _next = next;
        _options = options;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var clientIp = GetClientIp(context);
        var now = DateTime.UtcNow;
        
        // 获取或创建计数器
        var counter = _counters.GetOrAdd(clientIp, _ => new RateLimitCounter());
        
        bool isRateLimited;
        int requestCount;
        
        lock (counter)
        {
            // 清理过期的请求记录
            var windowStart = now.AddSeconds(-_options.WindowSeconds);
            while (counter.Requests.Count > 0 && counter.Requests.Peek() < windowStart)
            {
                counter.Requests.Dequeue();
            }
            
            requestCount = counter.Requests.Count;
            isRateLimited = requestCount >= _options.MaxRequests;
            
            if (!isRateLimited)
            {
                // 记录本次请求
                counter.Requests.Enqueue(now);
            }
        }
        
        if (isRateLimited)
        {
            Log.Warning(
                "Rate limit exceeded for IP: {ClientIp} | Requests: {RequestCount}/{MaxRequests} in {WindowSeconds}s",
                clientIp,
                requestCount,
                _options.MaxRequests,
                _options.WindowSeconds);
            
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers.RetryAfter = _options.WindowSeconds.ToString();
            await context.Response.WriteAsJsonAsync(new
            {
                code = "429",
                msg = "请求过于频繁，请稍后重试",
                data = (object?)null
            });
            return;
        }
        
        await _next(context);
    }

    private static string GetClientIp(HttpContext context)
    {
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

    private class RateLimitCounter
    {
        public Queue<DateTime> Requests { get; } = new();
    }
}

/// <summary>
/// 限流配置选项
/// </summary>
public class RateLimitOptions
{
    /// <summary>
    /// 时间窗口（秒）
    /// </summary>
    public int WindowSeconds { get; set; } = 60;
    
    /// <summary>
    /// 窗口内最大请求数
    /// </summary>
    public int MaxRequests { get; set; } = 1000;
}

/// <summary>
/// 限流中间件扩展方法
/// </summary>
public static class RateLimitingMiddlewareExtensions
{
    public static IApplicationBuilder UseRateLimiting(this IApplicationBuilder builder, Action<RateLimitOptions>? configure = null)
    {
        var options = new RateLimitOptions();
        configure?.Invoke(options);
        
        return builder.UseMiddleware<RateLimitingMiddleware>(options);
    }
}
