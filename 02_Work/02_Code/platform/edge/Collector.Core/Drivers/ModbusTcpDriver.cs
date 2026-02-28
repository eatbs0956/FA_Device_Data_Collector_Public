using System.Net.Sockets;
using Collector.Core.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NModbus;

namespace Collector.Core.Drivers;

/// <summary>
/// Modbus TCP 驱动实现
/// </summary>
public class ModbusTcpDriver : DriverBase
{
    private TcpClient? _tcpClient;
    private IModbusMaster? _master;
    private readonly object _lockObject = new();
    private byte _slaveId = 1;

    public override string DriverName => "Modbus TCP Driver";
    public override string ProtocolType => "MODBUS_TCP";

    /// <summary>
    /// 创建 Modbus TCP 驱动（使用空日志记录器）
    /// </summary>
    public ModbusTcpDriver() : base(NullLogger<ModbusTcpDriver>.Instance)
    {
    }

    /// <summary>
    /// 创建 Modbus TCP 驱动
    /// </summary>
    /// <param name="logger">日志记录器</param>
    public ModbusTcpDriver(ILogger<ModbusTcpDriver> logger) : base(logger)
    {
    }

    public override async Task<bool> ConnectAsync(DeviceConfig config)
    {
        try
        {
            // 重连前先清理旧连接，避免资源泄漏
            await DisconnectAsync();

            ConnectionState = DeviceConnectionState.Connecting;
            _deviceConfig = config;

            var ipAddress = config.Connection.IpAddress;
            var port = config.Connection.Port > 0 ? config.Connection.Port : 502;

            // 从协议配置中获取从站ID（支持 slaveId 和 unitId 两种键名）
            if (config.ProtocolConfig.TryGetValue("slaveId", out var slaveIdObj))
            {
                _slaveId = Convert.ToByte(slaveIdObj);
            }
            else if (config.ProtocolConfig.TryGetValue("unitId", out var unitIdObj))
            {
                _slaveId = Convert.ToByte(unitIdObj);
            }

            _logger.LogInformation("连接 Modbus TCP 设备: {IpAddress}:{Port}, SlaveId: {SlaveId}",
                ipAddress, port, _slaveId);

            _tcpClient = new TcpClient();
            _tcpClient.ReceiveTimeout = config.Connection.ReadTimeout;
            _tcpClient.SendTimeout = config.Connection.WriteTimeout;

            // 使用异步连接并设置超时
            var connectTask = _tcpClient.ConnectAsync(ipAddress, port);
            var timeoutTask = Task.Delay(config.Connection.ConnectionTimeout);

            var completedTask = await Task.WhenAny(connectTask, timeoutTask);
            if (completedTask == timeoutTask)
            {
                throw new TimeoutException($"连接超时 ({config.Connection.ConnectionTimeout}ms)");
            }

            await connectTask; // 确保连接任务完成或抛出异常

            // 使用 NModbus 工厂创建主站
            var factory = new ModbusFactory();
            _master = factory.CreateMaster(_tcpClient);
            _master.Transport.ReadTimeout = config.Connection.ReadTimeout;
            _master.Transport.WriteTimeout = config.Connection.WriteTimeout;
            _master.Transport.Retries = config.Connection.RetryCount;

            ConnectionState = DeviceConnectionState.Connected;
            _logger.LogInformation("Modbus TCP 连接成功: {DeviceName}", config.DeviceName);
            return true;
        }
        catch (Exception ex)
        {
            OnError($"连接失败: {ex.Message}", ex);
            ConnectionState = DeviceConnectionState.Error;
            await DisconnectAsync();
            return false;
        }
    }

    public override Task DisconnectAsync()
    {
        lock (_lockObject)
        {
            try
            {
                _master?.Dispose();
                _master = null;

                _tcpClient?.Close();
                _tcpClient?.Dispose();
                _tcpClient = null;

                ConnectionState = DeviceConnectionState.Disconnected;
                _logger.LogInformation("Modbus TCP 已断开连接");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "断开连接时发生错误");
            }
        }
        return Task.CompletedTask;
    }

    public override async Task<List<TagReadResult>> ReadTagsAsync(IEnumerable<TagConfig> tags)
    {
        var results = new List<TagReadResult>();
        
        foreach (var tag in tags.Where(t => t.Enabled))
        {
            var result = await ReadTagAsync(tag);
            results.Add(result);
        }

        return results;
    }

    public override async Task<TagReadResult> ReadTagAsync(TagConfig tag)
    {
        var result = new TagReadResult
        {
            TagId = tag.TagId,
            TagName = tag.TagName,
            Timestamp = DateTimeOffset.UtcNow
        };

        if (_master == null || ConnectionState != DeviceConnectionState.Connected)
        {
            result.Success = false;
            result.Quality = DataQuality.Bad;
            result.ErrorMessage = "设备未连接";
            return result;
        }

        try
        {
            // 解析地址 - 现在返回值包含 explicitQuantity
            var (functionCode, address, length, explicitQuantity) = ParseModbusAddress(tag.TagAddress, tag.DataType);

            // 记录请求日志 (模拟 Modbus TCP 帧)
            var requestFrame = BuildModbusRequestFrame(functionCode, address, length);
            OnRawDataLog(DataDirection.TX, requestFrame, $"FC{functionCode:D2} Addr={address} Len={length}");
            _logger.LogDebug("[{DeviceName}] TX: {Frame} | FC{FunctionCode:D2} Addr={Address} Len={Length}",
                _deviceConfig?.DeviceName ?? "Unknown",
                FormatFrameHex(requestFrame),
                functionCode, address, length);

            object rawValue;
            ushort[]? registers = null;

            lock (_lockObject)
            {
                switch (functionCode)
                {
                    case 1: // 线圈
                        var coils = _master.ReadCoils(_slaveId, address, length);
                        rawValue = coils[0];
                        break;
                    case 2: // 离散输入
                        var inputs = _master.ReadInputs(_slaveId, address, length);
                        rawValue = inputs[0];
                        break;
                    case 3: // 保持寄存器
                        registers = _master.ReadHoldingRegisters(_slaveId, address, length);
                        // 如果指定了 quantity 且 > 1，返回所有寄存器值的数组
                        rawValue = (explicitQuantity > 1) 
                            ? ConvertRegistersToArray(registers, tag.DataType, explicitQuantity.Value) 
                            : ConvertRegistersToValue(registers, tag.DataType);
                        break;
                    case 4: // 输入寄存器
                        registers = _master.ReadInputRegisters(_slaveId, address, length);
                        // 如果指定了 quantity 且 > 1，返回所有寄存器值的数组
                        rawValue = (explicitQuantity > 1)
                            ? ConvertRegistersToArray(registers, tag.DataType, explicitQuantity.Value)
                            : ConvertRegistersToValue(registers, tag.DataType);
                        break;
                    default:
                        throw new NotSupportedException($"不支持的功能码: {functionCode}");
                }
            }

            // 记录响应日志
            var responseFrame = BuildModbusResponseFrame(functionCode, registers, rawValue);
            OnRawDataLog(DataDirection.RX, responseFrame, $"Value={rawValue}");

            // 格式化值的显示：数组类型展示所有元素
            var valueDisplay = FormatValueForLog(rawValue);
            _logger.LogDebug("[{DeviceName}] RX: {Frame} | {TagName} = {Value}",
                _deviceConfig?.DeviceName ?? "Unknown",
                FormatFrameHex(responseFrame),
                tag.TagName, valueDisplay);

            result.RawValue = rawValue;
            result.Value = ApplyTransformation(rawValue, tag);
            result.Success = true;
            result.Quality = DataQuality.Good;
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.Quality = DataQuality.Bad;
            result.ErrorMessage = ex.Message;
            _logger.LogWarning(ex, "读取标签 {TagName} 失败", tag.TagName);

            // 通信异常（连接断开、socket 错误）时标记连接状态，触发下次重连
            if (ex is System.IO.IOException or System.Net.Sockets.SocketException or InvalidOperationException)
            {
                ConnectionState = DeviceConnectionState.Error;
            }
        }

        return await Task.FromResult(result);
    }

    public override async Task<TagWriteResult> WriteTagAsync(TagConfig tag, object value)
    {
        var result = new TagWriteResult
        {
            TagId = tag.TagId,
            Timestamp = DateTimeOffset.UtcNow
        };

        if (_master == null || ConnectionState != DeviceConnectionState.Connected)
        {
            result.Success = false;
            result.ErrorMessage = "设备未连接";
            return result;
        }

        if (tag.AccessMode == AccessMode.ReadOnly)
        {
            result.Success = false;
            result.ErrorMessage = "标签为只读模式";
            return result;
        }

        try
        {
            var (functionCode, address, _, _) = ParseModbusAddress(tag.TagAddress, tag.DataType);

            lock (_lockObject)
            {
                switch (functionCode)
                {
                    case 1: // 写入线圈
                    case 5:
                        _master.WriteSingleCoil(_slaveId, address, Convert.ToBoolean(value));
                        break;
                    case 3: // 写入保持寄存器
                    case 6:
                        WriteHoldingRegister(address, value, tag.DataType);
                        break;
                    default:
                        throw new NotSupportedException($"不支持写入功能码: {functionCode}");
                }
            }

            result.Success = true;
            _logger.LogInformation("写入标签 {TagName}: {Value}", tag.TagName, value);
        }
        catch (Exception ex)
        {
            result.Success = false;
            result.ErrorMessage = ex.Message;
            _logger.LogWarning(ex, "写入标签 {TagName} 失败", tag.TagName);
        }

        return await Task.FromResult(result);
    }

    /// <summary>
    /// 解析 Modbus 地址
    /// 支持多种格式:
    /// 1. JSON 格式: {"functionCode":"03","address":0,"quantity":1}
    /// 2. 前缀格式: C0, DI0, HR0, IR0
    /// 3. 功能码格式: 3:0
    /// 4. Modicon 格式: 40001, 30001, 10001, 00001
    /// 5. 简单数字格式: 0 (默认为保持寄存器)
    /// </summary>
    private (int FunctionCode, ushort Address, ushort Length, ushort? ExplicitQuantity) ParseModbusAddress(string address, DataPointType dataType)
    {
        int functionCode;
        ushort registerAddress;
        ushort? explicitQuantity = null;

        address = address?.Trim() ?? string.Empty;

        // 尝试解析 JSON 格式
        if (address.StartsWith("{"))
        {
            try
            {
                var jsonResult = ParseJsonAddress(address);
                functionCode = jsonResult.FunctionCode;
                registerAddress = jsonResult.Address;
                explicitQuantity = jsonResult.Quantity;
                
                _logger.LogDebug("JSON 地址解析成功: FC={FunctionCode}, Addr={Address}, Qty={Quantity}", 
                    functionCode, registerAddress, explicitQuantity);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "JSON 地址解析失败: {Address}", address);
                throw new ArgumentException($"无法解析 JSON 地址: {address}", ex);
            }
        }
        else
        {
            // 非 JSON 格式，使用传统解析
            address = address.ToUpper();

            // 格式1: 带前缀 (C, DI, HR, IR)
            if (address.StartsWith("C") && !address.StartsWith("C:"))
            {
                functionCode = 1;
                registerAddress = ushort.Parse(address.Substring(1));
            }
            else if (address.StartsWith("DI"))
            {
                functionCode = 2;
                registerAddress = ushort.Parse(address.Substring(2));
            }
            else if (address.StartsWith("HR"))
            {
                functionCode = 3;
                registerAddress = ushort.Parse(address.Substring(2));
            }
            else if (address.StartsWith("IR"))
            {
                functionCode = 4;
                registerAddress = ushort.Parse(address.Substring(2));
            }
            // 格式2: 功能码:地址
            else if (address.Contains(":"))
            {
                var parts = address.Split(':');
                functionCode = int.Parse(parts[0]);
                registerAddress = ushort.Parse(parts[1]);
            }
            // 格式3: Modicon 格式 (0xxxx, 1xxxx, 3xxxx, 4xxxx)
            else if (address.Length >= 5 && char.IsDigit(address[0]))
            {
                var numericAddress = int.Parse(address);
                if (numericAddress >= 40001 && numericAddress <= 49999)
                {
                    functionCode = 3; // 保持寄存器
                    registerAddress = (ushort)(numericAddress - 40001);
                }
                else if (numericAddress >= 30001 && numericAddress <= 39999)
                {
                    functionCode = 4; // 输入寄存器
                    registerAddress = (ushort)(numericAddress - 30001);
                }
                else if (numericAddress >= 10001 && numericAddress <= 19999)
                {
                    functionCode = 2; // 离散输入
                    registerAddress = (ushort)(numericAddress - 10001);
                }
                else if (numericAddress >= 1 && numericAddress <= 9999)
                {
                    functionCode = 1; // 线圈
                    registerAddress = (ushort)(numericAddress - 1);
                }
                else
                {
                    throw new ArgumentException($"无法解析地址: {address}");
                }
            }
            else
            {
                // 默认为保持寄存器
                functionCode = 3;
                registerAddress = ushort.Parse(address);
            }
        }

        // 如果 JSON 中指定了 quantity，使用它；否则根据数据类型计算
        var length = explicitQuantity ?? GetRegisterCount(dataType);

        return (functionCode, registerAddress, length, explicitQuantity);
    }

    /// <summary>
    /// 解析 JSON 格式的地址
    /// 格式: {"functionCode":"03","address":0,"quantity":1}
    /// </summary>
    private (int FunctionCode, ushort Address, ushort? Quantity) ParseJsonAddress(string jsonAddress)
    {
        // 使用简单的 JSON 解析（避免依赖 Newtonsoft.Json）
        var json = jsonAddress.Trim();
        
        int functionCode = 3; // 默认保持寄存器
        ushort address = 0;
        ushort? quantity = null;

        // 提取 functionCode
        var fcMatch = System.Text.RegularExpressions.Regex.Match(json, @"""functionCode""\s*:\s*""?(\d+)""?");
        if (fcMatch.Success)
        {
            functionCode = int.Parse(fcMatch.Groups[1].Value);
        }

        // 提取 address
        var addrMatch = System.Text.RegularExpressions.Regex.Match(json, @"""address""\s*:\s*(\d+)");
        if (addrMatch.Success)
        {
            address = ushort.Parse(addrMatch.Groups[1].Value);
        }

        // 提取 quantity (可选)
        var qtyMatch = System.Text.RegularExpressions.Regex.Match(json, @"""quantity""\s*:\s*(\d+)");
        if (qtyMatch.Success)
        {
            quantity = ushort.Parse(qtyMatch.Groups[1].Value);
        }

        return (functionCode, address, quantity);
    }

    private ushort GetRegisterCount(DataPointType dataType)
    {
        return dataType switch
        {
            DataPointType.Boolean => 1,
            DataPointType.Int16 => 1,
            DataPointType.UInt16 => 1,
            DataPointType.Int32 => 2,
            DataPointType.UInt32 => 2,
            DataPointType.Int64 => 4,
            DataPointType.UInt64 => 4,
            DataPointType.Float => 2,
            DataPointType.Double => 4,
            DataPointType.String => 16, // 默认32字符
            _ => 1
        };
    }

    /// <summary>
    /// 将寄存器转换为数组
    /// 根据数据类型将多个寄存器分组转换
    /// 例如：quantity=5, dataType=Int16 时返回 5 个 Int16 值的数组
    ///       quantity=2, dataType=Int32 时返回 2 个 Int32 值的数组 (每个Int32占2个寄存器)
    /// </summary>
    private object ConvertRegistersToArray(ushort[] registers, DataPointType dataType, ushort quantity)
    {
        if (registers == null || registers.Length == 0)
            throw new InvalidOperationException("读取到的寄存器数据为空");

        // 根据数据类型和 quantity 计算每个值占用的寄存器数
        var registersPerValue = GetRegisterCount(dataType);
        
        // 验证寄存器数量是否足够
        if (registers.Length < registersPerValue * quantity)
        {
            _logger.LogWarning("寄存器数量不足: 期望 {Expected}, 实际 {Actual}", 
                registersPerValue * quantity, registers.Length);
        }

        var result = new object[quantity];

        return dataType switch
        {
            // 每个寄存器返回一个值（Boolean/Int16/UInt16）
            DataPointType.Boolean or DataPointType.Int16 or DataPointType.UInt16 =>
                ConvertRegistersToSimpleArray(registers, dataType, quantity),
            
            // 每个值占用2个寄存器（Int32/UInt32/Float）
            DataPointType.Int32 or DataPointType.UInt32 or DataPointType.Float =>
                ConvertRegistersPairArray(registers, dataType, quantity),
            
            // 每个值占用4个寄存器（Int64/UInt64/Double）
            DataPointType.Int64 or DataPointType.UInt64 or DataPointType.Double =>
                ConvertRegistersQuadArray(registers, dataType, quantity),
            
            // 字符串类型
            DataPointType.String =>
                ConvertRegistersToStringArray(registers, quantity),
            
            _ => ConvertRegistersToSimpleArray(registers, dataType, quantity)
        };
    }

    /// <summary>
    /// 将寄存器转换为简单类型数组（每个寄存器对应一个值）
    /// </summary>
    private object ConvertRegistersToSimpleArray(ushort[] registers, DataPointType dataType, ushort quantity)
    {
        var result = new object[quantity];
        for (int i = 0; i < quantity && i < registers.Length; i++)
        {
            result[i] = dataType switch
            {
                DataPointType.Boolean => registers[i] != 0,
                DataPointType.Int16 => (short)registers[i],
                DataPointType.UInt16 => registers[i],
                _ => registers[i]
            };
        }
        return result;
    }

    /// <summary>
    /// 将寄存器转换为成对类型数组（每2个寄存器对应一个值）
    /// </summary>
    private object ConvertRegistersToRegisterPairArray(ushort[] registers, DataPointType dataType, ushort quantity)
    {
        var result = new object[quantity];
        for (int i = 0; i < quantity && (i * 2 + 1) < registers.Length; i++)
        {
            var pairRegisters = new[] { registers[i * 2], registers[i * 2 + 1] };
            result[i] = dataType switch
            {
                DataPointType.Int32 => CombineRegistersToInt32(pairRegisters),
                DataPointType.UInt32 => CombineRegistersToUInt32(pairRegisters),
                DataPointType.Float => CombineRegistersToFloat32(pairRegisters),
                _ => CombineRegistersToInt32(pairRegisters)
            };
        }
        return result;
    }

    /// <summary>
    /// 将寄存器转换为成对类型数组（每2个寄存器对应一个值）- 辅助方法名称修正
    /// </summary>
    private object ConvertRegistersPairArray(ushort[] registers, DataPointType dataType, ushort quantity)
    {
        return ConvertRegistersToRegisterPairArray(registers, dataType, quantity);
    }

    /// <summary>
    /// 将寄存器转换为四元组类型数组（每4个寄存器对应一个值）
    /// </summary>
    private object ConvertRegistersQuadArray(ushort[] registers, DataPointType dataType, ushort quantity)
    {
        var result = new object[quantity];
        for (int i = 0; i < quantity && (i * 4 + 3) < registers.Length; i++)
        {
            var quadRegisters = new[] { registers[i * 4], registers[i * 4 + 1], registers[i * 4 + 2], registers[i * 4 + 3] };
            result[i] = dataType switch
            {
                DataPointType.Int64 => CombineRegistersToInt64(quadRegisters),
                DataPointType.UInt64 => CombineRegistersToUInt64(quadRegisters),
                DataPointType.Double => CombineRegistersToFloat64(quadRegisters),
                _ => CombineRegistersToInt64(quadRegisters)
            };
        }
        return result;
    }

    /// <summary>
    /// 将寄存器转换为字符串数组
    /// </summary>
    private object ConvertRegistersToStringArray(ushort[] registers, ushort quantity)
    {
        var result = new string[quantity];
        var registersPerString = Math.Max(1, registers.Length / quantity);
        
        for (int i = 0; i < quantity && (i * registersPerString) < registers.Length; i++)
        {
            var endIdx = Math.Min((i + 1) * registersPerString, registers.Length);
            var stringRegisters = new ushort[endIdx - (i * registersPerString)];
            Array.Copy(registers, i * registersPerString, stringRegisters, 0, stringRegisters.Length);
            result[i] = ConvertRegistersToString(stringRegisters);
        }
        return result;
    }

    private object ConvertRegistersToValue(ushort[] registers, DataPointType dataType)
    {
        if (registers == null || registers.Length == 0)
            throw new InvalidOperationException("读取到的寄存器数据为空");

        return dataType switch
        {
            DataPointType.Boolean => registers[0] != 0,
            DataPointType.Int16 => (short)registers[0],
            DataPointType.UInt16 => registers[0],
            DataPointType.Int32 => CombineRegistersToInt32(registers),
            DataPointType.UInt32 => CombineRegistersToUInt32(registers),
            DataPointType.Int64 => CombineRegistersToInt64(registers),
            DataPointType.UInt64 => CombineRegistersToUInt64(registers),
            DataPointType.Float => CombineRegistersToFloat32(registers),
            DataPointType.Double => CombineRegistersToFloat64(registers),
            DataPointType.String => ConvertRegistersToString(registers),
            _ => registers[0]
        };
    }

    private int CombineRegistersToInt32(ushort[] registers)
    {
        if (registers.Length < 2)
            throw new ArgumentException("需要至少2个寄存器来转换为Int32");
        
        var bytes = new byte[4];
        Buffer.BlockCopy(registers, 0, bytes, 0, 4);
        return BitConverter.ToInt32(bytes, 0);
    }

    private uint CombineRegistersToUInt32(ushort[] registers)
    {
        if (registers.Length < 2)
            throw new ArgumentException("需要至少2个寄存器来转换为UInt32");
        
        var bytes = new byte[4];
        Buffer.BlockCopy(registers, 0, bytes, 0, 4);
        return BitConverter.ToUInt32(bytes, 0);
    }

    private long CombineRegistersToInt64(ushort[] registers)
    {
        if (registers.Length < 4)
            throw new ArgumentException("需要至少4个寄存器来转换为Int64");
        
        var bytes = new byte[8];
        Buffer.BlockCopy(registers, 0, bytes, 0, 8);
        return BitConverter.ToInt64(bytes, 0);
    }

    private ulong CombineRegistersToUInt64(ushort[] registers)
    {
        if (registers.Length < 4)
            throw new ArgumentException("需要至少4个寄存器来转换为UInt64");
        
        var bytes = new byte[8];
        Buffer.BlockCopy(registers, 0, bytes, 0, 8);
        return BitConverter.ToUInt64(bytes, 0);
    }

    private float CombineRegistersToFloat32(ushort[] registers)
    {
        if (registers.Length < 2)
            throw new ArgumentException("需要至少2个寄存器来转换为Float32");
        
        var bytes = new byte[4];
        Buffer.BlockCopy(registers, 0, bytes, 0, 4);
        return BitConverter.ToSingle(bytes, 0);
    }

    private double CombineRegistersToFloat64(ushort[] registers)
    {
        if (registers.Length < 4)
            throw new ArgumentException("需要至少4个寄存器来转换为Float64");
        
        var bytes = new byte[8];
        Buffer.BlockCopy(registers, 0, bytes, 0, 8);
        return BitConverter.ToDouble(bytes, 0);
    }

    private string ConvertRegistersToString(ushort[] registers)
    {
        var bytes = new byte[registers.Length * 2];
        Buffer.BlockCopy(registers, 0, bytes, 0, bytes.Length);
        return System.Text.Encoding.ASCII.GetString(bytes).TrimEnd('\0');
    }

    private void WriteHoldingRegister(ushort address, object value, DataPointType dataType)
    {
        switch (dataType)
        {
            case DataPointType.Boolean:
            case DataPointType.Int16:
            case DataPointType.UInt16:
                _master!.WriteSingleRegister(_slaveId, address, Convert.ToUInt16(value));
                break;
            case DataPointType.Int32:
            case DataPointType.UInt32:
            case DataPointType.Float:
                var registers32 = ConvertValueToRegisters(value, dataType);
                _master!.WriteMultipleRegisters(_slaveId, address, registers32);
                break;
            case DataPointType.Int64:
            case DataPointType.UInt64:
            case DataPointType.Double:
                var registers64 = ConvertValueToRegisters(value, dataType);
                _master!.WriteMultipleRegisters(_slaveId, address, registers64);
                break;
            default:
                throw new NotSupportedException($"不支持写入数据类型: {dataType}");
        }
    }

    private ushort[] ConvertValueToRegisters(object value, DataPointType dataType)
    {
        byte[] bytes;
        
        switch (dataType)
        {
            case DataPointType.Int32:
                bytes = BitConverter.GetBytes(Convert.ToInt32(value));
                break;
            case DataPointType.UInt32:
                bytes = BitConverter.GetBytes(Convert.ToUInt32(value));
                break;
            case DataPointType.Float:
                bytes = BitConverter.GetBytes(Convert.ToSingle(value));
                break;
            case DataPointType.Int64:
                bytes = BitConverter.GetBytes(Convert.ToInt64(value));
                break;
            case DataPointType.UInt64:
                bytes = BitConverter.GetBytes(Convert.ToUInt64(value));
                break;
            case DataPointType.Double:
                bytes = BitConverter.GetBytes(Convert.ToDouble(value));
                break;
            default:
                throw new NotSupportedException($"不支持转换数据类型: {dataType}");
        }

        var registers = new ushort[bytes.Length / 2];
        Buffer.BlockCopy(bytes, 0, registers, 0, bytes.Length);
        return registers;
    }

    /// <summary>
    /// 构建 Modbus TCP 请求帧（用于日志记录）
    /// 格式: [事务ID 2字节][协议ID 2字节][长度 2字节][从站ID 1字节][功能码 1字节][起始地址 2字节][数量 2字节]
    /// </summary>
    private byte[] BuildModbusRequestFrame(int functionCode, ushort address, ushort length)
    {
        var frame = new byte[12];
        
        // 事务ID (模拟)
        frame[0] = 0x00;
        frame[1] = 0x01;
        
        // 协议ID (Modbus TCP = 0x0000)
        frame[2] = 0x00;
        frame[3] = 0x00;
        
        // 后续长度 (6 字节)
        frame[4] = 0x00;
        frame[5] = 0x06;
        
        // 从站ID
        frame[6] = _slaveId;
        
        // 功能码
        frame[7] = (byte)functionCode;
        
        // 起始地址 (高字节在前)
        frame[8] = (byte)(address >> 8);
        frame[9] = (byte)(address & 0xFF);
        
        // 寄存器数量 (高字节在前)
        frame[10] = (byte)(length >> 8);
        frame[11] = (byte)(length & 0xFF);
        
        return frame;
    }

    /// <summary>
    /// 构建 Modbus TCP 响应帧（用于日志记录）
    /// </summary>
    private byte[] BuildModbusResponseFrame(int functionCode, ushort[]? registers, object rawValue)
    {
        if (registers != null && registers.Length > 0)
        {
            // 寄存器响应: [事务ID][协议ID][长度][从站ID][功能码][字节数][数据...]
            var dataBytes = registers.Length * 2;
            var frame = new byte[9 + dataBytes];
            
            frame[0] = 0x00; frame[1] = 0x01; // 事务ID
            frame[2] = 0x00; frame[3] = 0x00; // 协议ID
            frame[4] = (byte)((3 + dataBytes) >> 8);
            frame[5] = (byte)((3 + dataBytes) & 0xFF); // 长度
            frame[6] = _slaveId;
            frame[7] = (byte)functionCode;
            frame[8] = (byte)dataBytes;
            
            // 复制寄存器数据
            for (int i = 0; i < registers.Length; i++)
            {
                frame[9 + i * 2] = (byte)(registers[i] >> 8);
                frame[10 + i * 2] = (byte)(registers[i] & 0xFF);
            }
            
            return frame;
        }
        else
        {
            // 线圈/离散输入响应 (简化)
            var boolValue = Convert.ToBoolean(rawValue);
            return new byte[]
            {
                0x00, 0x01, // 事务ID
                0x00, 0x00, // 协议ID
                0x00, 0x04, // 长度
                _slaveId,
                (byte)functionCode,
                0x01, // 字节数
                (byte)(boolValue ? 0x01 : 0x00)
            };
        }
    }

    /// <summary>
    /// 将字节数组格式化为十六进制字符串
    /// 例如: "00 01 00 00 00 06 05 03 00 00 00 05"
    /// </summary>
    private static string FormatFrameHex(byte[] frame)
    {
        if (frame == null || frame.Length == 0)
            return "(empty)";

        return string.Join(" ", frame.Select(b => b.ToString("X2")));
    }

    /// <summary>
    /// 格式化值用于日志显示
    /// 数组类型显示所有元素，如 [11, 12, 13, 14, 15]
    /// </summary>
    private static string FormatValueForLog(object value)
    {
        if (value is object[] array)
        {
            return "[" + string.Join(", ", array.Select(v => v?.ToString() ?? "null")) + "]";
        }

        return value?.ToString() ?? "null";
    }
}
