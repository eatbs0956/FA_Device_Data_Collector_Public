using System;
using System.Collections.Generic;

namespace Collector.Agent.Legacy.Models
{
    /// <summary>
    /// 节点配置响应
    /// </summary>
    public class NodeConfigResponse
    {
        public NodeBasicInfo Node { get; set; }
        public List<DeviceConfig> Devices { get; set; }
        public List<TaskConfig> Tasks { get; set; }
        public long ConfigVersion { get; set; }
    }

    /// <summary>
    /// 节点基本信息
    /// </summary>
    public class NodeBasicInfo
    {
        [Newtonsoft.Json.JsonProperty("id")]
        public string Id { get; set; }
        
        [Newtonsoft.Json.JsonProperty("nodeId")]
        public string NodeId { get; set; }
        
        [Newtonsoft.Json.JsonProperty("nodeName")]
        public string NodeName { get; set; }
    }

    /// <summary>
    /// 设备配置
    /// </summary>
    public class DeviceConfig
    {
        [Newtonsoft.Json.JsonProperty("id")]
        public string DeviceId { get; set; }
        
        [Newtonsoft.Json.JsonProperty("name")]
        public string DeviceName { get; set; }
        
        [Newtonsoft.Json.JsonProperty("code")]
        public string DeviceCode { get; set; }
        
        [Newtonsoft.Json.JsonProperty("protocol")]
        public string Protocol { get; set; }
        
        [Newtonsoft.Json.JsonProperty("ipAddress")]
        public string IpAddress { get; set; }
        
        [Newtonsoft.Json.JsonProperty("port")]
        public int? Port { get; set; }
        
        [Newtonsoft.Json.JsonProperty("isEnabled")]
        public bool IsEnabled { get; set; }
        
        public string Status { get; set; }
        
        [Newtonsoft.Json.JsonProperty("tags")]
        public List<TagConfig> Tags { get; set; }
        
        /// <summary>
        /// 协议配置 JSON 字符串
        /// </summary>
        [Newtonsoft.Json.JsonProperty("protocolConfig")]
        public string ProtocolConfig { get; set; }
        
        /// <summary>
        /// 获取协议配置字典（延迟解析）
        /// </summary>
        [Newtonsoft.Json.JsonIgnore]
        public Dictionary<string, object> ProtocolConfigDict
        {
            get
            {
                if (string.IsNullOrEmpty(ProtocolConfig))
                    return new Dictionary<string, object>();
                    
                try
                {
                    return Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(ProtocolConfig);
                }
                catch
                {
                    return new Dictionary<string, object>();
                }
            }
        }
    }

    /// <summary>
    /// 任务配置
    /// </summary>
    public class TaskConfig
    {
        [Newtonsoft.Json.JsonProperty("id")]
        public string TaskId { get; set; }
        
        [Newtonsoft.Json.JsonProperty("name")]
        public string TaskName { get; set; }
        
        [Newtonsoft.Json.JsonProperty("code")]
        public string TaskCode { get; set; }
        
        [Newtonsoft.Json.JsonProperty("taskType")]
        public string TaskType { get; set; }
        
        [Newtonsoft.Json.JsonProperty("intervalMs")]
        public int? CollectIntervalMs { get; set; }
        
        [Newtonsoft.Json.JsonProperty("priority")]
        public int? Priority { get; set; }
        
        [Newtonsoft.Json.JsonProperty("isEnabled")]
        public bool IsEnabled { get; set; }
        
        public string Status { get; set; }
        
        [Newtonsoft.Json.JsonProperty("deviceIds")]
        public List<string> DeviceIds { get; set; }
    }

    /// <summary>
    /// 标签点配置
    /// </summary>
    public class TagConfig
    {
        [Newtonsoft.Json.JsonProperty("id")]
        public string TagId { get; set; }
        
        [Newtonsoft.Json.JsonProperty("name")]
        public string TagName { get; set; }
        
        [Newtonsoft.Json.JsonProperty("code")]
        public string TagCode { get; set; }
        
        [Newtonsoft.Json.JsonProperty("address")]
        public string Address { get; set; }
        
        [Newtonsoft.Json.JsonProperty("dataType")]
        public string DataType { get; set; }
        
        public object CurrentValue { get; set; }
        public DateTime? LastUpdateTime { get; set; }
    }

    /// <summary>
    /// 采集测试结果
    /// </summary>
    public class CollectionTestResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<TagTestResult> TagResults { get; set; }
        public long ElapsedMs { get; set; }
    }

    /// <summary>
    /// 标签点测试结果
    /// </summary>
    public class TagTestResult
    {
        public string TagName { get; set; }
        public string Address { get; set; }
        public object Value { get; set; }
        public string DataType { get; set; }
        public bool Success { get; set; }
        public string Error { get; set; }
    }
}


