using Collector.Core.Models;

namespace Collector.Core.Drivers;

/// <summary>
/// 协议驱动接口 - 所有协议驱动的基类接口
/// </summary>
public interface IProtocolDriver : IDisposable
{
    /// <summary>
    /// 驱动名称
    /// </summary>
    string DriverName { get; }

    /// <summary>
    /// 支持的协议类型
    /// </summary>
    string ProtocolType { get; }

    /// <summary>
    /// 连接状态
    /// </summary>
    DeviceConnectionState ConnectionState { get; }

    /// <summary>
    /// 是否已连接（便捷属性）
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// 最后错误信息
    /// </summary>
    string? LastError { get; }

    /// <summary>
    /// 连接到设备
    /// </summary>
    /// <param name="config">设备配置</param>
    /// <returns>是否连接成功</returns>
    Task<bool> ConnectAsync(DeviceConfig config);

    /// <summary>
    /// 断开连接
    /// </summary>
    Task DisconnectAsync();

    /// <summary>
    /// 测试连接
    /// </summary>
    /// <param name="config">设备配置</param>
    /// <returns>测试结果</returns>
    Task<ConnectionTestResult> TestConnectionAsync(DeviceConfig config);

    /// <summary>
    /// 读取标签值
    /// </summary>
    /// <param name="tags">标签列表</param>
    /// <returns>读取结果</returns>
    Task<List<TagReadResult>> ReadTagsAsync(IEnumerable<TagConfig> tags);

    /// <summary>
    /// 读取单个标签值
    /// </summary>
    /// <param name="tag">标签配置</param>
    /// <returns>读取结果</returns>
    Task<TagReadResult> ReadTagAsync(TagConfig tag);

    /// <summary>
    /// 写入标签值
    /// </summary>
    /// <param name="tag">标签配置</param>
    /// <param name="value">要写入的值</param>
    /// <returns>写入结果</returns>
    Task<TagWriteResult> WriteTagAsync(TagConfig tag, object value);

    /// <summary>
    /// 连接状态变更事件
    /// </summary>
    event EventHandler<DeviceConnectionState>? ConnectionStateChanged;

    /// <summary>
    /// 错误发生事件
    /// </summary>
    event EventHandler<DriverErrorEventArgs>? ErrorOccurred;

    /// <summary>
    /// 原始数据日志事件（用于 Debug 级别记录 TX/RX 帧数据）
    /// </summary>
    event EventHandler<RawDataLogEventArgs>? RawDataLogged;
}

/// <summary>
/// 连接测试结果
/// </summary>
public class ConnectionTestResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public int ResponseTimeMs { get; set; }
    public Dictionary<string, string> Details { get; set; } = new();
}

/// <summary>
/// 标签读取结果
/// </summary>
public class TagReadResult
{
    public string TagId { get; set; } = string.Empty;
    public string TagName { get; set; } = string.Empty;
    public bool Success { get; set; }
    public object? RawValue { get; set; }
    public object? Value { get; set; }
    public DataQuality Quality { get; set; } = DataQuality.Good;
    public string? ErrorMessage { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// 标签写入结果
/// </summary>
public class TagWriteResult
{
    public string TagId { get; set; } = string.Empty;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// 驱动错误事件参数
/// </summary>
public class DriverErrorEventArgs : EventArgs
{
    public string ErrorMessage { get; set; } = string.Empty;
    public Exception? Exception { get; set; }
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// 原始数据日志事件参数（用于记录协议通信的原始帧数据）
/// </summary>
public class RawDataLogEventArgs : EventArgs
{
    /// <summary>
    /// 协议类型（如 ModbusTCP, ModbusRTU, OPC_UA, S7 等）
    /// </summary>
    public string ProtocolType { get; set; } = string.Empty;

    /// <summary>
    /// 设备名称
    /// </summary>
    public string DeviceName { get; set; } = string.Empty;

    /// <summary>
    /// 数据方向（TX=发送, RX=接收）
    /// </summary>
    public DataDirection Direction { get; set; }

    /// <summary>
    /// 原始字节数据
    /// </summary>
    public byte[] RawData { get; set; } = Array.Empty<byte>();

    /// <summary>
    /// 十六进制字符串表示
    /// </summary>
    public string HexString => RawData.Length > 0 
        ? BitConverter.ToString(RawData).Replace("-", " ") 
        : string.Empty;

    /// <summary>
    /// 描述信息（可选，如功能码说明等）
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// 时间戳
    /// </summary>
    public DateTimeOffset Timestamp { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// 数据传输方向
/// </summary>
public enum DataDirection
{
    /// <summary>
    /// 发送（请求）
    /// </summary>
    TX,

    /// <summary>
    /// 接收（响应）
    /// </summary>
    RX
}
