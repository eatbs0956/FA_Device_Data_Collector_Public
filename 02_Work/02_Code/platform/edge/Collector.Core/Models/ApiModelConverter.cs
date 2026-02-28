using System.Collections.Generic;
using Newtonsoft.Json;

namespace Collector.Core.Models;

/// <summary>
/// API Model 转换器 - 将 API 返回的配置数据转换为 Core Model
/// </summary>
public static class ApiModelConverter
{
    /// <summary>
    /// 将服务端响应转换为 Core NodeConfig
    /// </summary>
    public static NodeConfig ToNodeConfig(ServerNodeConfigResponse serverResponse)
    {
        var nodeConfig = new NodeConfig
        {
            ConfigVersion = serverResponse.ConfigVersion,
            FetchedAt = DateTimeOffset.UtcNow,
            Node = new NodeInfo
            {
                Id = ParseGuidOrGenerate(serverResponse.Node?.Id),
                NodeId = serverResponse.Node?.NodeId ?? string.Empty,
                NodeName = serverResponse.Node?.NodeName ?? string.Empty,
            },
            Devices = new List<DeviceConfig>(),
            Tasks = new List<TaskConfig>()
        };

        // 转换设备列表
        if (serverResponse.Devices != null)
        {
            foreach (var serverDevice in serverResponse.Devices)
            {
                nodeConfig.Devices.Add(ToDeviceConfig(serverDevice));
            }
        }

        // 转换任务列表
        if (serverResponse.Tasks != null)
        {
            foreach (var serverTask in serverResponse.Tasks)
            {
                nodeConfig.Tasks.Add(ToTaskConfig(serverTask));
            }
        }

        return nodeConfig;
    }

    /// <summary>
    /// 将服务端设备 DTO 转换为 Core DeviceConfig
    /// </summary>
    public static DeviceConfig ToDeviceConfig(ServerDeviceConfigInfo serverDevice)
    {
        // 解析 protocolConfig JSON 字符串为 Dictionary
        var protocolConfigDict = ParseProtocolConfig(serverDevice.ProtocolConfig);

        var device = new DeviceConfig
        {
            Id = ParseGuidOrGenerate(serverDevice.Id),
            DeviceId = serverDevice.Code ?? string.Empty,
            DeviceName = serverDevice.Name ?? string.Empty,
            DeviceType = serverDevice.DeviceType ?? string.Empty,
            ProtocolType = MapProtocolType(serverDevice.Protocol),
            Enabled = serverDevice.IsEnabled,
            ProtocolConfig = protocolConfigDict,
            Tags = new List<TagConfig>()
        };

        // 构建连接配置：优先用服务端的 IpAddress/Port，其次从 protocolConfig 中提取
        device.Connection = new ConnectionConfig
        {
            IpAddress = serverDevice.IpAddress ?? string.Empty,
            Port = serverDevice.Port ?? 0
        };

        // 如果 IpAddress 为空，从 protocolConfig 提取
        if (string.IsNullOrEmpty(device.Connection.IpAddress) && protocolConfigDict.Count > 0)
        {
            device.Connection = ExtractConnectionConfig(protocolConfigDict, serverDevice.Protocol);
        }
        else if (device.Connection.Port == 0 && protocolConfigDict.Count > 0)
        {
            // IpAddress 有值但 Port 为 0，尝试从 protocolConfig 获取 port
            if (protocolConfigDict.TryGetValue("port", out var portObj) && int.TryParse(portObj?.ToString(), out var port))
            {
                device.Connection.Port = port;
            }
            else
            {
                device.Connection.Port = GetDefaultPort(serverDevice.Protocol);
            }
        }

        // 转换标签列表
        if (serverDevice.Tags != null)
        {
            foreach (var serverTag in serverDevice.Tags)
            {
                device.Tags.Add(ToTagConfig(serverTag));
            }
        }

        return device;
    }

    /// <summary>
    /// 将服务端标签 DTO 转换为 Core TagConfig
    /// </summary>
    public static TagConfig ToTagConfig(ServerTagConfigInfo serverTag)
    {
        return new TagConfig
        {
            Id = ParseGuidOrGenerate(serverTag.Id),
            TagId = serverTag.Code ?? string.Empty,
            TagName = serverTag.Name ?? string.Empty,
            TagAddress = serverTag.Address ?? string.Empty,
            DataType = ParseDataType(serverTag.DataType),
            Enabled = serverTag.IsEnabled,
            Unit = serverTag.Unit,
            ScalingFactor = serverTag.ScalingFactor.HasValue ? (decimal)serverTag.ScalingFactor.Value : 1.0m,
            Offset = serverTag.Offset.HasValue ? (decimal)serverTag.Offset.Value : 0m
        };
    }

    /// <summary>
    /// 将服务端任务 DTO 转换为 Core TaskConfig
    /// </summary>
    public static TaskConfig ToTaskConfig(ServerTaskConfigInfo serverTask)
    {
        var task = new TaskConfig
        {
            Id = ParseGuidOrGenerate(serverTask.Id),
            Name = serverTask.Name ?? string.Empty,
            Code = serverTask.Code,
            TaskType = serverTask.TaskType ?? "Periodic",
            CronExpression = serverTask.CronExpression,
            DefaultInterval = serverTask.IntervalMs,
            IsEnabled = serverTask.IsEnabled,
            DeviceIds = new List<Guid>()
        };

        // 转换设备 ID 列表（string -> Guid）
        if (serverTask.DeviceIds != null)
        {
            foreach (var deviceIdStr in serverTask.DeviceIds)
            {
                task.DeviceIds.Add(ParseGuidOrGenerate(deviceIdStr));
            }
        }

        return task;
    }

    /// <summary>
    /// 解析 protocolConfig JSON 字符串为 Dictionary
    /// </summary>
    public static Dictionary<string, object> ParseProtocolConfig(string? protocolConfigJson)
    {
        if (string.IsNullOrEmpty(protocolConfigJson))
            return new Dictionary<string, object>();

        try
        {
            var result = JsonConvert.DeserializeObject<Dictionary<string, object>>(protocolConfigJson);
            return result ?? new Dictionary<string, object>();
        }
        catch
        {
            return new Dictionary<string, object>();
        }
    }

    /// <summary>
    /// 从 API 响应创建设备配置（保留旧接口兼容）
    /// </summary>
    /// <param name="apiDevice">API 返回的设备数据（Dictionary 或 dynamic）</param>
    /// <returns>Core 设备配置</returns>
    public static DeviceConfig CreateDeviceConfig(
        string deviceId,
        string deviceName,
        string protocol,
        bool isEnabled,
        Dictionary<string, object>? protocolConfig,
        List<TagConfig>? tags = null)
    {
        var config = new DeviceConfig
        {
            Id = ParseGuidOrGenerate(deviceId),
            DeviceId = deviceId ?? string.Empty,
            DeviceName = deviceName ?? string.Empty,
            ProtocolType = MapProtocolType(protocol),
            Enabled = isEnabled,
            ProtocolConfig = protocolConfig ?? new Dictionary<string, object>(),
            Tags = tags ?? new List<TagConfig>()
        };

        // 从 protocolConfig 中提取连接配置
        if (protocolConfig != null)
        {
            config.Connection = ExtractConnectionConfig(protocolConfig, protocol);
        }

        return config;
    }

    /// <summary>
    /// 从 API 响应创建标签配置
    /// </summary>
    public static TagConfig CreateTagConfig(
        string tagId,
        string tagName,
        string address,
        string dataType,
        bool isEnabled = true)
    {
        return new TagConfig
        {
            Id = ParseGuidOrGenerate(tagId),
            TagId = tagId ?? string.Empty,
            TagName = tagName ?? string.Empty,
            TagAddress = address ?? string.Empty,
            DataType = ParseDataType(dataType),
            Enabled = isEnabled
        };
    }

    /// <summary>
    /// 映射协议类型字符串到标准格式
    /// </summary>
    public static string MapProtocolType(string? protocol)
    {
        if (string.IsNullOrEmpty(protocol))
            return "UNKNOWN";

        return protocol.ToUpperInvariant() switch
        {
            "1" or "MODBUS_TCP" or "MODBUSTCP" => "MODBUS_TCP",
            "2" or "MODBUS_RTU" or "MODBUSRTU" => "MODBUS_RTU",
            "3" or "OPC_UA" or "OPCUA" => "OPC_UA",
            "4" or "OPC_DA" or "OPCDA" => "OPC_DA",
            "5" or "S7" or "SIEMENS_S7" => "S7",
            "6" or "MC" or "MITSUBISHI_MC" => "MC",
            "7" or "MQTT" => "MQTT",
            _ => protocol.ToUpperInvariant()
        };
    }

    /// <summary>
    /// 解析数据类型字符串
    /// </summary>
    public static DataPointType ParseDataType(string? dataType)
    {
        if (string.IsNullOrEmpty(dataType))
            return DataPointType.Float;

        return dataType.ToUpperInvariant() switch
        {
            "BOOLEAN" or "BOOL" => DataPointType.Boolean,
            "INT16" or "SHORT" => DataPointType.Int16,
            "UINT16" or "USHORT" or "WORD" => DataPointType.UInt16,
            "INT32" or "INT" or "INTEGER" => DataPointType.Int32,
            "UINT32" or "UINT" or "DWORD" => DataPointType.UInt32,
            "INT64" or "LONG" => DataPointType.Int64,
            "UINT64" or "ULONG" => DataPointType.UInt64,
            "FLOAT" or "SINGLE" or "REAL" => DataPointType.Float,
            "DOUBLE" or "LREAL" => DataPointType.Double,
            "STRING" or "TEXT" => DataPointType.String,
            _ => DataPointType.Float
        };
    }

    /// <summary>
    /// 从协议配置中提取连接配置
    /// </summary>
    private static ConnectionConfig ExtractConnectionConfig(Dictionary<string, object> protocolConfig, string? protocol)
    {
        var connection = new ConnectionConfig();

        // 提取 IP 地址
        if (protocolConfig.TryGetValue("ip", out var ip))
        {
            connection.IpAddress = ip?.ToString() ?? string.Empty;
        }
        else if (protocolConfig.TryGetValue("host", out var host))
        {
            connection.IpAddress = host?.ToString() ?? string.Empty;
        }
        else if (protocolConfig.TryGetValue("serverUrl", out var serverUrl))
        {
            // OPC UA 等使用 serverUrl
            connection.IpAddress = serverUrl?.ToString() ?? string.Empty;
        }

        // 提取端口
        if (protocolConfig.TryGetValue("port", out var port))
        {
            if (int.TryParse(port?.ToString(), out var portValue))
            {
                connection.Port = portValue;
            }
        }
        else
        {
            // 默认端口
            connection.Port = GetDefaultPort(protocol);
        }

        // 提取超时设置
        if (protocolConfig.TryGetValue("connectionTimeout", out var connTimeout))
        {
            if (int.TryParse(connTimeout?.ToString(), out var timeoutValue))
            {
                connection.ConnectionTimeout = timeoutValue;
            }
        }

        if (protocolConfig.TryGetValue("readTimeout", out var readTimeout))
        {
            if (int.TryParse(readTimeout?.ToString(), out var timeoutValue))
            {
                connection.ReadTimeout = timeoutValue;
            }
        }

        if (protocolConfig.TryGetValue("writeTimeout", out var writeTimeout))
        {
            if (int.TryParse(writeTimeout?.ToString(), out var timeoutValue))
            {
                connection.WriteTimeout = timeoutValue;
            }
        }

        return connection;
    }

    /// <summary>
    /// 获取协议默认端口
    /// </summary>
    private static int GetDefaultPort(string? protocol)
    {
        if (string.IsNullOrEmpty(protocol))
            return 502;

        return protocol.ToUpperInvariant() switch
        {
            "MODBUS_TCP" or "1" => 502,
            "OPC_UA" or "3" => 4840,
            "S7" or "5" => 102,
            "MQTT" or "7" => 1883,
            _ => 502
        };
    }

    /// <summary>
    /// 解析 GUID，如果失败则生成新的
    /// </summary>
    private static Guid ParseGuidOrGenerate(string? id)
    {
        if (string.IsNullOrEmpty(id))
            return Guid.NewGuid();

        if (Guid.TryParse(id, out var guid))
            return guid;

        // 如果不是有效的 GUID，使用字符串的哈希生成一个确定性的 GUID
        return GenerateDeterministicGuid(id);
    }

    /// <summary>
    /// 根据字符串生成确定性的 GUID
    /// </summary>
    private static Guid GenerateDeterministicGuid(string input)
    {
        using var md5 = System.Security.Cryptography.MD5.Create();
        var hash = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
        return new Guid(hash);
    }
}
