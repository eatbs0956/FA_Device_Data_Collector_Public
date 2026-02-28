using Collector.Core.Models;
using Microsoft.Extensions.Logging;

namespace Collector.Core.Drivers;

/// <summary>
/// 协议驱动基类 - 提供通用实现
/// </summary>
public abstract class DriverBase : IProtocolDriver
{
    protected readonly ILogger _logger;
    protected DeviceConfig? _deviceConfig;
    private DeviceConnectionState _connectionState = DeviceConnectionState.Disconnected;

    public abstract string DriverName { get; }
    public abstract string ProtocolType { get; }

    public DeviceConnectionState ConnectionState
    {
        get => _connectionState;
        protected set
        {
            if (_connectionState != value)
            {
                _connectionState = value;
                ConnectionStateChanged?.Invoke(this, value);
            }
        }
    }

    /// <summary>
    /// 是否已连接（便捷属性）
    /// </summary>
    public bool IsConnected => _connectionState == DeviceConnectionState.Connected;

    public string? LastError { get; protected set; }

    public event EventHandler<DeviceConnectionState>? ConnectionStateChanged;
    public event EventHandler<DriverErrorEventArgs>? ErrorOccurred;
    public event EventHandler<RawDataLogEventArgs>? RawDataLogged;

    protected DriverBase(ILogger logger)
    {
        _logger = logger;
    }

    public abstract Task<bool> ConnectAsync(DeviceConfig config);
    public abstract Task DisconnectAsync();
    public abstract Task<List<TagReadResult>> ReadTagsAsync(IEnumerable<TagConfig> tags);
    public abstract Task<TagReadResult> ReadTagAsync(TagConfig tag);
    public abstract Task<TagWriteResult> WriteTagAsync(TagConfig tag, object value);

    public virtual async Task<ConnectionTestResult> TestConnectionAsync(DeviceConfig config)
    {
        var result = new ConnectionTestResult();
        var startTime = DateTime.UtcNow;

        try
        {
            var connected = await ConnectAsync(config);
            result.ResponseTimeMs = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;

            if (connected)
            {
                result.Success = true;
                result.Details["Status"] = "连接成功";
                await DisconnectAsync();
            }
            else
            {
                result.Success = false;
                result.ErrorMessage = LastError ?? "连接失败";
            }
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            result.ResponseTimeMs = (int)(DateTime.UtcNow - startTime).TotalMilliseconds;
        }

        return result;
    }

    protected void OnError(string message, Exception? exception = null)
    {
        LastError = message;
        _logger.LogError(exception, "{DriverName} 错误: {Message}", DriverName, message);
        ErrorOccurred?.Invoke(this, new DriverErrorEventArgs
        {
            ErrorMessage = message,
            Exception = exception
        });
    }

    /// <summary>
    /// 记录原始数据日志（TX/RX）
    /// </summary>
    /// <param name="direction">数据方向</param>
    /// <param name="data">原始字节数据</param>
    /// <param name="description">描述信息（可选）</param>
    protected void OnRawDataLog(DataDirection direction, byte[] data, string? description = null)
    {
        RawDataLogged?.Invoke(this, new RawDataLogEventArgs
        {
            ProtocolType = ProtocolType,
            DeviceName = _deviceConfig?.DeviceName ?? "Unknown",
            Direction = direction,
            RawData = data,
            Description = description,
            Timestamp = DateTimeOffset.UtcNow
        });
    }

    /// <summary>
    /// 应用数据转换（比例因子和偏移量）
    /// </summary>
    protected object ApplyTransformation(object rawValue, TagConfig tag)
    {
        if (rawValue is IConvertible convertible)
        {
            try
            {
                var numericValue = Convert.ToDecimal(convertible);
                var transformedValue = numericValue * tag.ScalingFactor + tag.Offset;
                
                // 根据目标数据类型转换
                return tag.DataType switch
                {
                    DataPointType.Int16 => (short)transformedValue,
                    DataPointType.UInt16 => (ushort)transformedValue,
                    DataPointType.Int32 => (int)transformedValue,
                    DataPointType.UInt32 => (uint)transformedValue,
                    DataPointType.Int64 => (long)transformedValue,
                    DataPointType.UInt64 => (ulong)transformedValue,
                    DataPointType.Float => (float)transformedValue,
                    DataPointType.Double => (double)transformedValue,
                    _ => transformedValue
                };
            }
            catch
            {
                return rawValue;
            }
        }
        return rawValue;
    }

    public virtual void Dispose()
    {
        try
        {
            DisconnectAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "{DriverName} 释放资源时发生错误", DriverName);
        }
    }
}
