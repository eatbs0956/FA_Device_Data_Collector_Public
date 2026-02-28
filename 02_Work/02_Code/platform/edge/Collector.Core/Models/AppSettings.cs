namespace Collector.Core.Models;

/// <summary>
/// 应用程序配置 - 存储在本地配置文件
/// </summary>
public class AppSettings
{
    /// <summary>
    /// 节点标识符（安装时生成或手动配置，默认为机器名）
    /// </summary>
    public string NodeId { get; set; } = Environment.MachineName;

    /// <summary>
    /// 节点名称（默认为机器名）
    /// </summary>
    public string NodeName { get; set; } = Environment.MachineName;

    /// <summary>
    /// API 网关地址（统一入口，所有API请求都通过网关）
    /// </summary>
    public string ApiGatewayUrl { get; set; } = "http://localhost:60620";

    /// <summary>
    /// RabbitMQ 主机地址
    /// </summary>
    public string RabbitMqHost { get; set; } = "localhost";

    /// <summary>
    /// RabbitMQ 端口
    /// </summary>
    public int RabbitMqPort { get; set; } = 5672;

    /// <summary>
    /// RabbitMQ 用户名
    /// </summary>
    public string RabbitMqUser { get; set; } = "devdcp";

    /// <summary>
    /// RabbitMQ 密码
    /// </summary>
    public string RabbitMqPassword { get; set; } = "devdcp";

    /// <summary>
    /// RabbitMQ 交换机名称
    /// </summary>
    public string RabbitMqExchange { get; set; } = "devdcp.collection";

    /// <summary>
    /// 心跳间隔（秒）
    /// </summary>
    public int HeartbeatIntervalSeconds { get; set; } = 30;

    /// <summary>
    /// 日志级别
    /// </summary>
    public string LogLevel { get; set; } = "Information";

    /// <summary>
    /// 日志文件路径
    /// </summary>
    public string LogFilePath { get; set; } = "logs/collector.log";

    /// <summary>
    /// 语言设置 (zh-CN, en-US, etc.)，默认跟随系统语言
    /// </summary>
    public string Language { get; set; } = System.Globalization.CultureInfo.CurrentUICulture.Name;
}
