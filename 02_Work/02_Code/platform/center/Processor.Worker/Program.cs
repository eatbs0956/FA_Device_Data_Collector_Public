using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Processor.Worker;
using Processor.Worker.Services;
using Shared.Tsdb;
using Shared.Realtime;
using Shared.Domain.Data;

var builder = WebApplication.CreateBuilder(args);

// 配置日志
builder.Logging.AddConsole();

// 加载配置
builder.Configuration
    .AddJsonFile("appsettings.json", optional: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// 配置选项
builder.Services.Configure<RabbitMqOptions>(builder.Configuration.GetSection(RabbitMqOptions.SectionName));
builder.Services.Configure<RedisOptions>(builder.Configuration.GetSection(RedisOptions.SectionName));
builder.Services.Configure<RealtimeOptions>(builder.Configuration.GetSection(RealtimeOptions.SectionName));

// 添加数据库上下文（用于读取标签配置）
builder.Services.AddDbContext<UnifiedDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// 添加 InfluxDB 服务
builder.Services.AddInfluxDb(builder.Configuration);

// 添加 Realtime 服务（用于发布实时数据到前端）
builder.Services.AddRealtime(builder.Configuration);

// 添加健康检查
builder.Services.AddHealthChecks();

// 添加后台服务
builder.Services.AddHostedService<DataProcessorService>();

var app = builder.Build();

// 健康检查端点
app.MapHealthChecks("/health");

// 根路径响应
app.MapGet("/", () => Results.Ok(new { service = "Processor.Worker", status = "running" }));

await app.RunAsync();
