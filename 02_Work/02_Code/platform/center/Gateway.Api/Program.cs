using Microsoft.AspNetCore.HttpOverrides;
using Serilog;
using Serilog.Events;
using Gateway.Api.Middleware;

// ========== 配置 Serilog ==========
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Yarp", LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .Enrich.WithProperty("Service", "Gateway.Api")
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/gateway-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting Gateway.Api...");
    
    var builder = WebApplication.CreateBuilder(args);
    
    // 使用 Serilog
    builder.Host.UseSerilog();

    // 添加 YARP 反向代理服务
    builder.Services.AddReverseProxy()
        .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

    // 添加 CORS 支持
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    });

    // 添加健康检查
    builder.Services.AddHealthChecks();

    var app = builder.Build();

    // 配置转发头（用于获取真实客户端 IP）
    app.UseForwardedHeaders(new ForwardedHeadersOptions
    {
        ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
    });

    // 启用 CORS
    app.UseCors();
    
    // IP 限流中间件（1000 请求/分钟/IP）
    app.UseRateLimiting(options =>
    {
        options.WindowSeconds = 60;
        options.MaxRequests = 1000;
    });
    
    // 请求日志中间件
    app.UseRequestLogging();

    // 健康检查端点
    app.MapHealthChecks("/health");

    // 简单的根路径响应
    app.MapGet("/", () => Results.Ok(new 
    { 
        service = "DevDCP API Gateway",
        version = "1.0.0",
        timestamp = DateTime.UtcNow
    }));

    // 映射反向代理
    app.MapReverseProxy();
    
    Log.Information("Gateway.Api started successfully on {Urls}", string.Join(", ", app.Urls));

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Gateway.Api terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
