using Avalonia;
using Collector.Agent;
using Collector.Agent.Services;
using Serilog;

// 配置 Serilog（含自定义 InMemoryLogSink 供 LogView 消费）
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/collector-.log", rollingInterval: RollingInterval.Day)
    .WriteTo.Sink(InMemoryLogSink.Instance)
    .CreateLogger();

try
{
    Log.Information("启动 Collector.Agent...");
    
    BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);
}
catch (Exception ex)
{
    Log.Fatal(ex, "应用程序异常终止");
}
finally
{
    Log.CloseAndFlush();
}

static AppBuilder BuildAvaloniaApp()
    => AppBuilder.Configure<App>()
        .UsePlatformDetect()
        .WithInterFont()
        .LogToTrace();
