using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Collector.Agent.Legacy.Models;
using NLog;
using CoreModels = Collector.Core.Models;
using CoreDrivers = Collector.Core.Drivers;

namespace Collector.Agent.Legacy.Services
{
    /// <summary>
    /// 采集服务 - 封装 Collector.Core 提供的协议驱动功能
    /// </summary>
    public class CollectionService : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static CollectionService _instance;
        private static readonly object _lock = new object();

        private readonly Dictionary<string, CoreDrivers.IProtocolDriver> _drivers = new Dictionary<string, CoreDrivers.IProtocolDriver>();
        private CancellationTokenSource _collectionCts;
        private bool _isCollecting;
        private int _collectionIntervalMs = 1000;
        private bool _disposed;

        /// <summary>
        /// 当原始数据日志被记录时触发（用于 Debug 级别的协议数据）
        /// </summary>
        public event EventHandler<RawDataLogEventArgs> RawDataLogged;

        /// <summary>
        /// 当标签数据被采集到时触发
        /// </summary>
        public event EventHandler<TagDataCollectedEventArgs> TagDataCollected;

        /// <summary>
        /// 当采集发生错误时触发
        /// </summary>
        public event EventHandler<CollectionErrorEventArgs> CollectionError;

        /// <summary>
        /// 当设备连接状态变化时触发
        /// </summary>
        public event EventHandler<DeviceStateChangedEventArgs> DeviceStateChanged;

        /// <summary>
        /// 单例实例
        /// </summary>
        public static CollectionService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new CollectionService();
                        }
                    }
                }
                return _instance;
            }
        }

        /// <summary>
        /// 是否正在采集
        /// </summary>
        public bool IsCollecting => _isCollecting;

        /// <summary>
        /// 采集间隔（毫秒）
        /// </summary>
        public int CollectionIntervalMs
        {
            get => _collectionIntervalMs;
            set => _collectionIntervalMs = Math.Max(100, value);
        }

        private CollectionService()
        {
        }

        /// <summary>
        /// 创建协议驱动实例
        /// </summary>
        private CoreDrivers.IProtocolDriver CreateDriver(string protocol)
        {
            var protocolType = CoreModels.ApiModelConverter.MapProtocolType(protocol);
            
            switch (protocolType)
            {
                case "MODBUS_TCP":
                    return new CoreDrivers.ModbusTcpDriver();
                // 后续可添加其他协议支持:
                // case "OPC_UA":
                //     return new CoreDrivers.OpcUaDriver();
                // case "S7":
                //     return new CoreDrivers.S7Driver();
                default:
                    Logger.Warn("不支持的协议类型: {0}", protocol);
                    return null;
            }
        }

        /// <summary>
        /// 获取或创建设备的驱动
        /// </summary>
        private CoreDrivers.IProtocolDriver GetOrCreateDriver(DeviceConfig device)
        {
            if (_drivers.TryGetValue(device.DeviceId, out var existingDriver))
            {
                return existingDriver;
            }

            var driver = CreateDriver(device.Protocol);
            if (driver != null)
            {
                // 订阅驱动的原始数据日志事件
                driver.RawDataLogged += OnDriverRawDataLogged;
                _drivers[device.DeviceId] = driver;
            }
            return driver;
        }

        /// <summary>
        /// 处理驱动的原始数据日志事件
        /// </summary>
        private void OnDriverRawDataLogged(object sender, CoreDrivers.RawDataLogEventArgs e)
        {
            // 转发给订阅者
            RawDataLogged?.Invoke(this, new RawDataLogEventArgs
            {
                ProtocolType = e.ProtocolType,
                DeviceName = e.DeviceName,
                Direction = e.Direction == CoreDrivers.DataDirection.TX ? DataDirection.TX : DataDirection.RX,
                HexString = e.HexString,
                Description = e.Description,
                Timestamp = e.Timestamp
            });

            // 同时写入 NLog 的 Debug 日志
            Logger.Debug("[{0}] [{1}] {2}: {3}",
                e.ProtocolType,
                e.DeviceName,
                e.Direction,
                e.HexString);
        }

        /// <summary>
        /// 将 API 模型转换为 Core 模型
        /// </summary>
        private CoreModels.DeviceConfig ConvertToCore(DeviceConfig apiDevice)
        {
            var coreDevice = CoreModels.ApiModelConverter.CreateDeviceConfig(
                apiDevice.DeviceId,
                apiDevice.DeviceName,
                apiDevice.Protocol,
                apiDevice.IsEnabled,
                apiDevice.ProtocolConfigDict
            );

            // 如果 API 直接提供了 IP 和端口，优先使用
            if (!string.IsNullOrEmpty(apiDevice.IpAddress))
            {
                coreDevice.Connection.IpAddress = apiDevice.IpAddress;
            }
            if (apiDevice.Port.HasValue)
            {
                coreDevice.Connection.Port = apiDevice.Port.Value;
            }

            // 转换标签
            if (apiDevice.Tags != null)
            {
                coreDevice.Tags = apiDevice.Tags.Select(t => CoreModels.ApiModelConverter.CreateTagConfig(
                    t.TagId,
                    t.TagName,
                    t.Address,
                    t.DataType,
                    true
                )).ToList();
            }

            return coreDevice;
        }

        /// <summary>
        /// 测试设备连接
        /// </summary>
        public async Task<ConnectionTestResult> TestConnectionAsync(DeviceConfig device)
        {
            var result = new ConnectionTestResult();
            
            try
            {
                var driver = CreateDriver(device.Protocol);
                if (driver == null)
                {
                    result.Success = false;
                    result.Message = $"不支持的协议类型: {device.Protocol}";
                    return result;
                }

                // 订阅原始数据日志事件（临时）
                driver.RawDataLogged += OnDriverRawDataLogged;

                try
                {
                    var coreDevice = ConvertToCore(device);
                    var connected = await driver.ConnectAsync(coreDevice);
                    
                    if (connected)
                    {
                        result.Success = true;
                        result.Message = "连接成功";
                        
                        await driver.DisconnectAsync();
                    }
                    else
                    {
                        result.Success = false;
                        result.Message = "连接失败";
                    }
                }
                finally
                {
                    driver.RawDataLogged -= OnDriverRawDataLogged;
                    driver.Dispose();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "测试连接失败: {0}", device.DeviceName);
                result.Success = false;
                result.Message = $"连接错误: {ex.Message}";
            }

            return result;
        }

        /// <summary>
        /// 测试采集设备的标签数据
        /// </summary>
        public async Task<CollectionTestResult> TestCollectionAsync(DeviceConfig device)
        {
            var result = new CollectionTestResult
            {
                TagResults = new List<TagTestResult>()
            };
            var sw = System.Diagnostics.Stopwatch.StartNew();

            try
            {
                Logger.Info("开始测试采集，设备: {0}, 协议: {1}", device.DeviceName, device.Protocol);
                
                var driver = CreateDriver(device.Protocol);
                if (driver == null)
                {
                    result.Success = false;
                    result.Message = $"不支持的协议类型: {device.Protocol}";
                    Logger.Warn("不支持的协议类型: {0}", device.Protocol);
                    return result;
                }

                // 订阅原始数据日志事件（临时）
                driver.RawDataLogged += OnDriverRawDataLogged;
                Logger.Debug("已订阅 RawDataLogged 事件");

                try
                {
                    var coreDevice = ConvertToCore(device);
                    Logger.Debug("设备转换完成，标签数: {0}", coreDevice.Tags?.Count ?? 0);
                    
                    var connected = await driver.ConnectAsync(coreDevice);
                    
                    if (!connected)
                    {
                        result.Success = false;
                        result.Message = "连接设备失败";
                        Logger.Warn("连接设备失败: {0}", device.DeviceName);
                        return result;
                    }
                    
                    Logger.Info("设备连接成功: {0}", device.DeviceName);

                    // 读取所有标签
                    foreach (var tag in coreDevice.Tags)
                    {
                        var tagResult = new TagTestResult
                        {
                            TagName = tag.TagName,
                            Address = tag.TagAddress,
                            DataType = tag.DataType.ToString()
                        };

                        try
                        {
                            Logger.Debug("开始读取标签: {0} (Address: {1})", tag.TagName, tag.TagAddress);
                            var dataPoint = await driver.ReadTagAsync(tag);
                            
                            if (dataPoint != null)
                            {
                                tagResult.Success = true;
                                tagResult.Value = dataPoint.Value;
                                Logger.Info("标签读取成功: {0} = {1}", tag.TagName, dataPoint.Value);
                            }
                            else
                            {
                                tagResult.Success = false;
                                tagResult.Error = "读取返回空";
                                Logger.Warn("标签读取返回空: {0}", tag.TagName);
                            }
                        }
                        catch (Exception ex)
                        {
                            tagResult.Success = false;
                            tagResult.Error = ex.Message;
                            Logger.Error(ex, "标签读取失败: {0}", tag.TagName);
                        }

                        result.TagResults.Add(tagResult);
                    }

                    await driver.DisconnectAsync();

                    result.Success = result.TagResults.Any(r => r.Success);
                    result.Message = result.Success ? 
                        $"成功读取 {result.TagResults.Count(r => r.Success)}/{result.TagResults.Count} 个标签" :
                        "所有标签读取失败";
                    
                    Logger.Info("采集测试完成: {0}", result.Message);
                }
                finally
                {
                    driver.RawDataLogged -= OnDriverRawDataLogged;
                    driver.Dispose();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "测试采集失败: {0}", device.DeviceName);
                result.Success = false;
                result.Message = $"采集错误: {ex.Message}";
            }

            sw.Stop();
            result.ElapsedMs = sw.ElapsedMilliseconds;
            return result;
        }

        /// <summary>
        /// 启动采集
        /// </summary>
        public async Task StartAsync(IEnumerable<DeviceConfig> devices)
        {
            if (_isCollecting)
            {
                Logger.Warn("采集已在运行中");
                return;
            }

            _collectionCts = new CancellationTokenSource();
            _isCollecting = true;

            var enabledDevices = devices.Where(d => d.IsEnabled).ToList();
            Logger.Info("启动采集，设备数量: {0}", enabledDevices.Count);

            // 初始化所有设备的驱动
            foreach (var device in enabledDevices)
            {
                try
                {
                    var driver = GetOrCreateDriver(device);
                    if (driver == null) continue;

                    var coreDevice = ConvertToCore(device);
                    var connected = await driver.ConnectAsync(coreDevice);
                    
                    DeviceStateChanged?.Invoke(this, new DeviceStateChangedEventArgs
                    {
                        DeviceId = device.DeviceId,
                        DeviceName = device.DeviceName,
                        IsConnected = connected,
                        State = connected ? "Connected" : "Disconnected"
                    });

                    if (connected)
                    {
                        Logger.Info("设备连接成功: {0}", device.DeviceName);
                    }
                    else
                    {
                        Logger.Warn("设备连接失败: {0}", device.DeviceName);
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "初始化设备失败: {0}", device.DeviceName);
                    CollectionError?.Invoke(this, new CollectionErrorEventArgs
                    {
                        DeviceId = device.DeviceId,
                        DeviceName = device.DeviceName,
                        ErrorMessage = ex.Message
                    });
                }
            }

            // 启动采集循环
            _ = Task.Run(() => CollectionLoopAsync(enabledDevices, _collectionCts.Token));
        }

        /// <summary>
        /// 采集循环
        /// </summary>
        private async Task CollectionLoopAsync(List<DeviceConfig> devices, CancellationToken cancellationToken)
        {
            Logger.Info("采集循环已启动");

            while (!cancellationToken.IsCancellationRequested)
            {
                foreach (var device in devices)
                {
                    if (cancellationToken.IsCancellationRequested) break;

                    try
                    {
                        if (!_drivers.TryGetValue(device.DeviceId, out var driver))
                            continue;

                        if (!driver.IsConnected)
                        {
                            // 尝试重连
                            var coreDevice = ConvertToCore(device);
                            await driver.ConnectAsync(coreDevice);
                        }

                        if (!driver.IsConnected)
                            continue;

                        var coreDeviceForRead = ConvertToCore(device);
                        foreach (var tag in coreDeviceForRead.Tags)
                        {
                            if (cancellationToken.IsCancellationRequested) break;

                            try
                            {
                                var dataPoint = await driver.ReadTagAsync(tag);
                                if (dataPoint != null)
                                {
                                    // 找到对应的 API 标签并更新
                                    var apiTag = device.Tags?.FirstOrDefault(t => t.TagId == tag.TagId);
                                    if (apiTag != null)
                                    {
                                        apiTag.CurrentValue = dataPoint.Value;
                                        apiTag.LastUpdateTime = dataPoint.Timestamp.DateTime;
                                    }

                                    TagDataCollected?.Invoke(this, new TagDataCollectedEventArgs
                                    {
                                        DeviceId = device.DeviceId,
                                        DeviceName = device.DeviceName,
                                        TagId = tag.TagId,
                                        TagName = tag.TagName,
                                        Value = dataPoint.Value,
                                        Timestamp = dataPoint.Timestamp.DateTime,
                                        Quality = dataPoint.Quality.ToString()
                                    });
                                }
                            }
                            catch (Exception ex)
                            {
                                Logger.Warn(ex, "读取标签失败: {0}.{1}", device.DeviceName, tag.TagName);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex, "采集设备数据失败: {0}", device.DeviceName);
                        CollectionError?.Invoke(this, new CollectionErrorEventArgs
                        {
                            DeviceId = device.DeviceId,
                            DeviceName = device.DeviceName,
                            ErrorMessage = ex.Message
                        });
                    }
                }

                try
                {
                    await Task.Delay(_collectionIntervalMs, cancellationToken);
                }
                catch (TaskCanceledException)
                {
                    break;
                }
            }

            Logger.Info("采集循环已停止");
        }

        /// <summary>
        /// 停止采集
        /// </summary>
        public async Task StopAsync()
        {
            if (!_isCollecting)
            {
                return;
            }

            Logger.Info("正在停止采集...");
            
            _collectionCts?.Cancel();
            _isCollecting = false;

            // 断开所有驱动连接
            foreach (var kvp in _drivers)
            {
                try
                {
                    await kvp.Value.DisconnectAsync();
                    kvp.Value.Dispose();
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, "断开设备连接失败: {0}", kvp.Key);
                }
            }
            _drivers.Clear();

            Logger.Info("采集已停止");
        }

        /// <summary>
        /// 释放资源
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            _collectionCts?.Cancel();
            _collectionCts?.Dispose();

            foreach (var driver in _drivers.Values)
            {
                try
                {
                    driver.Dispose();
                }
                catch { }
            }
            _drivers.Clear();

            _disposed = true;
        }
    }

    #region Event Args

    /// <summary>
    /// 原始数据日志事件参数
    /// </summary>
    public class RawDataLogEventArgs : EventArgs
    {
        public string ProtocolType { get; set; }
        public string DeviceName { get; set; }
        public DataDirection Direction { get; set; }
        public string HexString { get; set; }
        public string Description { get; set; }
        public DateTimeOffset Timestamp { get; set; }
    }

    /// <summary>
    /// 数据方向枚举
    /// </summary>
    public enum DataDirection
    {
        TX,
        RX
    }

    /// <summary>
    /// 标签数据采集事件参数
    /// </summary>
    public class TagDataCollectedEventArgs : EventArgs
    {
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public string TagId { get; set; }
        public string TagName { get; set; }
        public object Value { get; set; }
        public DateTime Timestamp { get; set; }
        public string Quality { get; set; }
    }

    /// <summary>
    /// 采集错误事件参数
    /// </summary>
    public class CollectionErrorEventArgs : EventArgs
    {
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public string ErrorMessage { get; set; }
    }

    /// <summary>
    /// 设备状态变化事件参数
    /// </summary>
    public class DeviceStateChangedEventArgs : EventArgs
    {
        public string DeviceId { get; set; }
        public string DeviceName { get; set; }
        public bool IsConnected { get; set; }
        public string State { get; set; }
    }

    /// <summary>
    /// 连接测试结果
    /// </summary>
    public class ConnectionTestResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    #endregion
}
