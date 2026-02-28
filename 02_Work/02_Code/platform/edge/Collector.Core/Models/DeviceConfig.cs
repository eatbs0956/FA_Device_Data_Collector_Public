namespace Collector.Core.Models;

/// <summary>
/// 设备配置
/// </summary>
public class DeviceConfig
{
    /// <summary>
    /// 设备ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 设备标识符
    /// </summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// 设备名称
    /// </summary>
    public string DeviceName { get; set; } = string.Empty;

    /// <summary>
    /// 设备类型
    /// </summary>
    public string DeviceType { get; set; } = string.Empty;

    /// <summary>
    /// 协议类型 - MODBUS_TCP, MODBUS_RTU, OPC_UA, MQTT, S7, MC
    /// </summary>
    public string ProtocolType { get; set; } = string.Empty;

    /// <summary>
    /// 连接配置（JSON 对象）
    /// </summary>
    public ConnectionConfig Connection { get; set; } = new();

    /// <summary>
    /// 协议特定配置（JSON 对象）
    /// </summary>
    public Dictionary<string, object> ProtocolConfig { get; set; } = new();

    /// <summary>
    /// 设备标签列表
    /// </summary>
    public List<TagConfig> Tags { get; set; } = new();

    /// <summary>
    /// 启用状态
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 设备描述
    /// </summary>
    public string? Description { get; set; }
}

/// <summary>
/// 连接配置
/// </summary>
public class ConnectionConfig
{
    /// <summary>
    /// IP 地址
    /// </summary>
    public string IpAddress { get; set; } = string.Empty;

    /// <summary>
    /// 端口号
    /// </summary>
    public int Port { get; set; }

    /// <summary>
    /// 连接超时（毫秒）
    /// </summary>
    public int ConnectionTimeout { get; set; } = 5000;

    /// <summary>
    /// 读取超时（毫秒）
    /// </summary>
    public int ReadTimeout { get; set; } = 3000;

    /// <summary>
    /// 写入超时（毫秒）
    /// </summary>
    public int WriteTimeout { get; set; } = 3000;

    /// <summary>
    /// 重试次数
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// 重试间隔（毫秒）
    /// </summary>
    public int RetryInterval { get; set; } = 1000;
}

/// <summary>
/// 设备运行时状态
/// </summary>
public class DeviceRuntimeStatus : System.ComponentModel.INotifyPropertyChanged
{
    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string? name = null)
        => PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(name));

    public Guid DeviceId { get; set; }
    public string DeviceName { get; set; } = string.Empty;

    private DeviceConnectionState _state = DeviceConnectionState.Disconnected;
    public DeviceConnectionState State
    {
        get => _state;
        set { if (_state != value) { _state = value; OnPropertyChanged(); } }
    }

    private DateTimeOffset? _lastConnectTime;
    public DateTimeOffset? LastConnectTime
    {
        get => _lastConnectTime;
        set { if (_lastConnectTime != value) { _lastConnectTime = value; OnPropertyChanged(); } }
    }

    private DateTimeOffset? _lastReadTime;
    public DateTimeOffset? LastReadTime
    {
        get => _lastReadTime;
        set { if (_lastReadTime != value) { _lastReadTime = value; OnPropertyChanged(); } }
    }

    private int _errorCount;
    public int ErrorCount
    {
        get => _errorCount;
        set { if (_errorCount != value) { _errorCount = value; OnPropertyChanged(); } }
    }

    private string? _lastError;
    public string? LastError
    {
        get => _lastError;
        set { if (_lastError != value) { _lastError = value; OnPropertyChanged(); } }
    }
}

/// <summary>
/// 设备连接状态枚举
/// </summary>
public enum DeviceConnectionState
{
    Disconnected,
    Connecting,
    Connected,
    Error
}
