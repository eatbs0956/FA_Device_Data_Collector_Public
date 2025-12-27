using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Auth.Api.Services.Abstractions;

namespace Auth.Api.Services;

/// <summary>
/// 审计中间件 - 自动记录系统操作审计日志（异步非阻塞）
/// </summary>
/// <remarks>
/// 记录所有关键操作，包括：
/// - 所有POST/PUT/DELETE操作
/// - 登录/登出相关操作
/// - 用户信息访问
/// - 用户角色相关操作(/user)
/// - 超级管理员操作(/super)
/// 
/// 设计改进：
/// - 使用异步队列，不阻塞主请求流程
/// - 审计日志由后台服务批量写入数据库
/// </remarks>
public class AuditMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AuditMiddleware> _logger;

    /// <summary>
    /// 需要审计的路径前缀列表
    /// </summary>
    private static readonly string[] AuditPaths = new[]
    {
        "/auth",
        "/admin",
        "/systemManage",
        "/user",
        "/super"
    };

    public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, IAuditService auditService)
    {
        // 检查是否需要审计
        if (!ShouldAudit(context))
        {
            await _next(context);
            return;
        }

        var sw = System.Diagnostics.Stopwatch.StartNew();
        var originalBodyStream = context.Response.Body;
        var requestBody = await CaptureRequestBody(context);

        try
        {
            using var responseBody = new MemoryStream();
            context.Response.Body = responseBody;

            await _next(context);

            sw.Stop();

            // 记录审计日志（异步非阻塞）
            LogAuditAsync(context, auditService, requestBody, sw.ElapsedMilliseconds);

            responseBody.Seek(0, SeekOrigin.Begin);
            await responseBody.CopyToAsync(originalBodyStream);
        }
        catch (Exception ex)
        {
            sw.Stop();
            // 记录异常的审计日志（异步非阻塞）
            LogAuditAsync(context, auditService, requestBody, sw.ElapsedMilliseconds, ex);
            throw;
        }
        finally
        {
            context.Response.Body = originalBodyStream;
        }
    }

    /// <summary>
    /// 判断请求是否需要审计
    /// </summary>
    private bool ShouldAudit(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        // 健康检查和公钥端点不审计
        if (path.StartsWith("/health") || path.StartsWith("/.well-known"))
        {
            return false;
        }

        // 审计所有POST/PUT/DELETE操作
        if (context.Request.Method != "GET")
        {
            return AuditPaths.Any(p => path.StartsWith(p, StringComparison.OrdinalIgnoreCase));
        }

        // 审计关键的GET操作
        return path.StartsWith("/auth/getUserInfo", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/systemManage", StringComparison.OrdinalIgnoreCase)
            || path.StartsWith("/admin", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// 捕获请求体内容
    /// </summary>
    private async Task<string?> CaptureRequestBody(HttpContext context)
    {
        if (context.Request.ContentLength == null || context.Request.ContentLength == 0)
        {
            return null;
        }

        context.Request.EnableBuffering();
        var buffer = new byte[Convert.ToInt32(context.Request.ContentLength)];
        await context.Request.Body.ReadAsync(buffer, 0, buffer.Length);
        context.Request.Body.Position = 0;

        var requestBody = Encoding.UTF8.GetString(buffer);

        // 脱敏敏感信息
        requestBody = MaskSensitiveData(requestBody);

        return requestBody;
    }

    /// <summary>
    /// 脱敏敏感信息
    /// </summary>
    private string MaskSensitiveData(string content)
    {
        if (string.IsNullOrEmpty(content))
        {
            return content;
        }

        try
        {
            using var doc = JsonDocument.Parse(content);
            var root = doc.RootElement;

            // 简单替换敏感字段（实际应用中可能需要更复杂的逻辑）
            content = content.Replace(GetJsonValue(root, "password"), "***MASKED***");
            content = content.Replace(GetJsonValue(root, "refreshToken"), "***MASKED***");
        }
        catch
        {
            // 如果不是JSON或解析失败，保持原样
        }

        return content;
    }

    private string GetJsonValue(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var value))
        {
            return value.GetString() ?? string.Empty;
        }
        return string.Empty;
    }

    /// <summary>
    /// 记录审计日志（异步非阻塞）
    /// </summary>
    private void LogAuditAsync(
        HttpContext context,
        IAuditService auditService,
        string? requestBody,
        long elapsedMs,
        Exception? exception = null)
    {
        try
        {
            var userId = GetUserId(context);
            var action = GetAction(context);
            var (resourceType, resourceId) = GetResource(context);

            var entry = new AuditLogEntry
            {
                TenantId = "t0", // 默认租户，后续可从用户token中提取
                UserId = userId,
                Action = action,
                ResourceType = resourceType,
                ResourceId = resourceId,
                IpAddress = GetClientIp(context),
                UserAgent = context.Request.Headers["User-Agent"].ToString(),
                RequestBody = requestBody,
                ResponseStatus = context.Response.StatusCode,
                ErrorMessage = exception?.Message,
                Timestamp = DateTimeOffset.UtcNow
            };

            // 入队（非阻塞）
            auditService.EnqueueAuditLog(entry);

            _logger.LogDebug(
                "Audit queued: {Action} by User {UserId} - Status {Status} - {ElapsedMs}ms - Queue size: {QueueSize}",
                action, userId, context.Response.StatusCode, elapsedMs, auditService.GetQueueSize());
        }
        catch (Exception ex)
        {
            // 审计日志记录失败不应该影响主流程
            _logger.LogError(ex, "Failed to enqueue audit log");
        }
    }

    /// <summary>
    /// 获取用户ID
    /// </summary>
    private Guid? GetUserId(HttpContext context)
    {
        var userId = context.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                     ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? context.User.FindFirstValue("sub");

        return Guid.TryParse(userId, out var id) ? id : null;
    }

    /// <summary>
    /// 获取操作类型
    /// </summary>
    private string GetAction(HttpContext context)
    {
        var method = context.Request.Method;
        var path = context.Request.Path.Value ?? string.Empty;

        // 特殊路径映射
        if (path.Contains("/login", StringComparison.OrdinalIgnoreCase))
            return "Login";
        if (path.Contains("/logout", StringComparison.OrdinalIgnoreCase))
            return "Logout";
        if (path.Contains("/register", StringComparison.OrdinalIgnoreCase))
            return "Register";
        if (path.Contains("/refreshToken", StringComparison.OrdinalIgnoreCase))
            return "RefreshToken";

        return $"{method} {path}";
    }

    /// <summary>
    /// 获取资源类型和ID
    /// </summary>
    private (string? ResourceType, string? ResourceId) GetResource(HttpContext context)
    {
        var path = context.Request.Path.Value ?? string.Empty;

        if (path.Contains("/users"))
            return ("User", GetIdFromPath(path));
        if (path.Contains("/roles"))
            return ("Role", GetIdFromPath(path));
        if (path.Contains("/menus"))
            return ("Menu", GetIdFromPath(path));

        return (null, null);
    }

    /// <summary>
    /// 从路径中提取ID
    /// </summary>
    private string? GetIdFromPath(string path)
    {
        var segments = path.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var lastSegment = segments.LastOrDefault();

        // 如果最后一段是GUID或数字，则认为是ID
        if (Guid.TryParse(lastSegment, out _) || int.TryParse(lastSegment, out _))
        {
            return lastSegment;
        }

        return null;
    }

    /// <summary>
    /// 获取客户端IP地址
    /// </summary>
    private string? GetClientIp(HttpContext context)
    {
        // 首先检查 X-Forwarded-For 头（代理/负载均衡场景）
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',').FirstOrDefault()?.Trim();
        }

        // 检查 X-Real-IP 头
        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }

        // 最后使用连接的远程IP
        return context.Connection.RemoteIpAddress?.ToString();
    }
}
