using Serilog.Core;
using Serilog.Events;

namespace Collector.Agent.Services;

/// <summary>
/// Serilog 自定义 Sink：将日志事件转发到内存，供 LogViewModel 消费显示
/// 使用单例模式，在 Serilog 配置和 DI 之间共享
/// </summary>
public class InMemoryLogSink : ILogEventSink
{
    private static readonly Lazy<InMemoryLogSink> _instance = new(() => new InMemoryLogSink());

    /// <summary>
    /// 全局单例（Serilog 配置在 DI 容器之前，必须用静态访问）
    /// </summary>
    public static InMemoryLogSink Instance => _instance.Value;

    /// <summary>
    /// 日志事件回调：每当有新日志写入时触发
    /// </summary>
    public event Action<LogEvent>? LogReceived;

    public void Emit(LogEvent logEvent)
    {
        LogReceived?.Invoke(logEvent);
    }
}
