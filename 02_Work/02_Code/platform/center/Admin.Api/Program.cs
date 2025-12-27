using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Shared.Domain.Data;
using SharedAuth.Extensions;
using Admin.Api.Services;
using Admin.Api.Middlewares;
using Admin.Api.Authorization;
using Serilog;
using Serilog.Events;

// ========== Serilog 配置 ==========
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore.Database.Command", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithThreadId()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .WriteTo.File(
        path: "logs/admin-api-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        fileSizeLimitBytes: 104857600, // 100MB
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj} {Properties:j}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting Admin.Api...");

    var builder = WebApplication.CreateBuilder(args);

    // 使用 Serilog 作为日志提供程序
    builder.Host.UseSerilog();

    // 添加 HttpContextAccessor - 用于审计字段自动填充
    builder.Services.AddHttpContextAccessor();

// 添加服务到容器
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 配置 PostgreSQL 数据库上下文
builder.Services.AddDbContext<UnifiedDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DeviceDatabase"),
        b => b.MigrationsAssembly("SharedAuth.Library"))
           .UseSnakeCaseNamingConvention()
           .EnableSensitiveDataLogging(builder.Environment.IsDevelopment()) // 开发环境显示参数值
           .LogTo(Log.Information, LogLevel.Information) // 输出 SQL 到 Serilog
);

// 注册 DbContext 接口（为 DeviceService 提供依赖注入）
builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<UnifiedDbContext>());

// 注册业务服务
builder.Services.AddScoped<IDeviceService, DeviceService>();
builder.Services.AddScoped<IDeviceGroupService, DeviceGroupService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<IEdgeNodeService, EdgeNodeService>();
builder.Services.AddScoped<ICollectionTaskService, CollectionTaskService>();

// ========== JWT认证配置 ==========
// 从环境变量或配置文件获取Auth服务的JWKS地址
var authServiceUrl = Environment.GetEnvironmentVariable("AUTH_SERVICE_URL") ?? "http://localhost:60621";
var jwksUrl = $"{authServiceUrl}/.well-known/jwks.json";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false; // 开发环境可设为false
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = "devdcp.auth",
            ValidateAudience = true,
            ValidAudience = "devdcp.portal",
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5),
            ValidateIssuerSigningKey = true
        };
        
        // 直接从 JWKS 端点获取公钥（不使用 OpenID Discovery）
        var httpRetriever = new HttpDocumentRetriever { RequireHttps = false }; // 开发环境允许 HTTP
        options.ConfigurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
            jwksUrl,
            new JwksRetriever(),
            httpRetriever);
        
        // 启用详细的认证日志
        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var authHeader = context.Request.Headers["Authorization"].ToString();
                var preview = authHeader.Length > 50 ? authHeader.Substring(0, 50) : authHeader;
                Log.Error("JWT认证失败: {Exception}, Token: {Token}", 
                    context.Exception.Message, preview);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                Log.Information("JWT认证成功: User={User}, Claims={Claims}",
                    context.Principal?.Identity?.Name ?? "Unknown",
                    string.Join(", ", context.Principal?.Claims.Select(c => $"{c.Type}={c.Value}") ?? Array.Empty<string>()));
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                Log.Warning("JWT认证挑战: Error={Error}, ErrorDescription={ErrorDescription}",
                    context.Error ?? "None",
                    context.ErrorDescription ?? "None");
                return Task.CompletedTask;
            }
        };
    });

// ========== 跨服务按钮权限授权配置 ==========
// 获取Auth数据库连接字符串（与Auth.Api使用相同的数据库）
var authDbConnection = Environment.GetEnvironmentVariable("PG_CONN") ?? 
    "Host=localhost;Username=devdcp;Password=devdcp;Database=devdcp";

// 添加跨服务按钮权限支持
builder.Services.AddCrossServiceButtonPermission(authDbConnection);

// 配置授权策略
builder.Services.AddAuthorization();

// 配置 CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

    // 配置 HTTP 请求管道
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    // 全局异常处理中间件（必须在最前面）
    app.UseGlobalExceptionHandler();

    // 使用 Serilog 请求日志（记录 HTTP 请求）
    app.UseSerilogRequestLogging(options =>
    {
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
        options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
        {
            diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
            diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
            diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].ToString());
            
            // 添加用户信息
            if (httpContext.User.Identity?.IsAuthenticated == true)
            {
                diagnosticContext.Set("UserId", httpContext.User.FindFirst("sub")?.Value ?? "Unknown");
                diagnosticContext.Set("UserName", httpContext.User.FindFirst("name")?.Value ?? "Unknown");
            }
        };
    });

    // Health check endpoint
    app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "Admin.Api", timestamp = DateTime.UtcNow }));

    app.UseCors("AllowAll");
    app.UseHttpsRedirection();

    // ========== 认证授权中间件 ==========
    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    Log.Information("Admin.Api started successfully");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Admin.Api terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
