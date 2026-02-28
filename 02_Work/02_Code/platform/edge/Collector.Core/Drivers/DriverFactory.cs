using Collector.Core.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Collector.Core.Drivers;

/// <summary>
/// 驱动工厂 - 根据协议类型创建对应的驱动实例
/// </summary>
public interface IDriverFactory
{
    /// <summary>
    /// 创建驱动实例
    /// </summary>
    /// <param name="protocolType">协议类型</param>
    /// <returns>驱动实例</returns>
    IProtocolDriver CreateDriver(string protocolType);

    /// <summary>
    /// 获取支持的协议类型列表
    /// </summary>
    IEnumerable<string> SupportedProtocols { get; }

    /// <summary>
    /// 检查是否支持指定协议
    /// </summary>
    bool IsSupported(string protocolType);
}

/// <summary>
/// 驱动工厂实现
/// </summary>
public class DriverFactory : IDriverFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILoggerFactory _loggerFactory;
    private readonly Dictionary<string, Type> _driverTypes;

    public DriverFactory(IServiceProvider serviceProvider, ILoggerFactory loggerFactory)
    {
        _serviceProvider = serviceProvider;
        _loggerFactory = loggerFactory;
        
        // 注册支持的驱动类型
        _driverTypes = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase)
        {
            { "MODBUS_TCP", typeof(ModbusTcpDriver) },
            // 后续添加更多驱动
            // { "MODBUS_RTU", typeof(ModbusRtuDriver) },
            // { "OPC_UA", typeof(OpcUaDriver) },
            // { "MQTT", typeof(MqttDriver) },
            // { "S7", typeof(SiemensS7Driver) },
            // { "MC", typeof(MitsubishiMcDriver) },
        };
    }

    public IEnumerable<string> SupportedProtocols => _driverTypes.Keys;

    public bool IsSupported(string protocolType)
    {
        return _driverTypes.ContainsKey(protocolType);
    }

    public IProtocolDriver CreateDriver(string protocolType)
    {
        if (string.IsNullOrWhiteSpace(protocolType))
        {
            throw new ArgumentNullException(nameof(protocolType));
        }

        if (!_driverTypes.TryGetValue(protocolType, out var driverType))
        {
            throw new NotSupportedException($"不支持的协议类型: {protocolType}。支持的协议: {string.Join(", ", SupportedProtocols)}");
        }

        // 创建驱动实例
        var logger = _loggerFactory.CreateLogger(driverType);
        
        return protocolType.ToUpperInvariant() switch
        {
            "MODBUS_TCP" => new ModbusTcpDriver(_loggerFactory.CreateLogger<ModbusTcpDriver>()),
            // 后续添加更多驱动的创建逻辑
            _ => throw new NotSupportedException($"驱动类型未实现: {protocolType}")
        };
    }
}

/// <summary>
/// 驱动服务扩展
/// </summary>
public static class DriverServiceExtensions
{
    /// <summary>
    /// 添加驱动服务到依赖注入容器
    /// </summary>
    public static IServiceCollection AddDrivers(this IServiceCollection services)
    {
        services.AddSingleton<IDriverFactory, DriverFactory>();
        return services;
    }
}
