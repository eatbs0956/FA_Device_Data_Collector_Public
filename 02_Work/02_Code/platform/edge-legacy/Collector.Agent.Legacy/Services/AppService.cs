using System;
using System.IO;
using Newtonsoft.Json;
using NLog;

namespace Collector.Agent.Legacy.Services
{
    /// <summary>
    /// 应用服务 - 仅管理本地设置（API 调用已迁移到 DI 容器中的 IAuthApiClient / IAdminApiClient）
    /// </summary>
    public class AppService
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static AppService _instance;
        private static readonly object _lock = new object();

        private readonly string _settingsFilePath;

        public string GatewayUrl { get; set; }
        public string CurrentNodeId { get; set; }
        public string CurrentNodeName { get; set; }

        /// <summary>
        /// 单例实例
        /// </summary>
        public static AppService Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                        {
                            _instance = new AppService();
                        }
                    }
                }
                return _instance;
            }
        }

        private AppService()
        {
            _settingsFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "CollectorAgentLegacy",
                "settings.json"
            );
        }

        /// <summary>
        /// 加载设置
        /// </summary>
        public LocalSettings LoadSettings()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    var json = File.ReadAllText(_settingsFilePath);
                    var settings = JsonConvert.DeserializeObject<LocalSettings>(json);
                    
                    GatewayUrl = settings.ApiGatewayUrl;
                    CurrentNodeId = settings.NodeId;
                    CurrentNodeName = settings.NodeName;
                    
                    return settings;
                }
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "加载设置失败");
            }
            return null;
        }

        /// <summary>
        /// 保存设置
        /// </summary>
        public void SaveSettings(string gatewayUrl, string nodeId, string nodeName, string username)
        {
            try
            {
                GatewayUrl = gatewayUrl;
                CurrentNodeId = nodeId;
                CurrentNodeName = nodeName;

                var settings = LoadSettings() ?? new LocalSettings();
                settings.ApiGatewayUrl = gatewayUrl;
                settings.NodeId = nodeId;
                settings.NodeName = nodeName;
                settings.Username = username;

                SaveSettingsToFile(settings);
                
                Logger.Info("设置已保存");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "保存设置失败");
                throw;
            }
        }

        /// <summary>
        /// 保存 RabbitMQ 设置
        /// </summary>
        public void SaveRabbitMqSettings(string host, int port, string user, string password, string exchange)
        {
            try
            {
                var settings = LoadSettings() ?? new LocalSettings();
                settings.RabbitMqHost = host;
                settings.RabbitMqPort = port;
                settings.RabbitMqUser = user;
                settings.RabbitMqPassword = password;
                settings.RabbitMqExchange = exchange;

                SaveSettingsToFile(settings);
                Logger.Info("RabbitMQ 设置已保存");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "保存 RabbitMQ 设置失败");
                throw;
            }
        }

        /// <summary>
        /// 保存语言设置
        /// </summary>
        public void SaveLanguageSetting(string language)
        {
            try
            {
                var settings = LoadSettings() ?? new LocalSettings();
                settings.Language = language;
                
                SaveSettingsToFile(settings);
                
                Logger.Info("语言设置已保存: {0}", language);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "保存语言设置失败");
                throw;
            }
        }
        
        /// <summary>
        /// 获取设置项
        /// </summary>
        public string GetSetting(string key, string defaultValue = "")
        {
            try
            {
                var settings = LoadSettings();
                if (settings?.CustomSettings != null && settings.CustomSettings.ContainsKey(key))
                {
                    return settings.CustomSettings[key];
                }
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "获取设置项失败: {0}", key);
            }
            return defaultValue;
        }
        
        /// <summary>
        /// 保存设置项
        /// </summary>
        public void SaveSetting(string key, string value)
        {
            try
            {
                var settings = LoadSettings() ?? new LocalSettings();
                if (settings.CustomSettings == null)
                {
                    settings.CustomSettings = new System.Collections.Generic.Dictionary<string, string>();
                }
                settings.CustomSettings[key] = value;
                
                SaveSettingsToFile(settings);
                
                Logger.Debug("设置项已保存: {0} = {1}", key, value);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "保存设置项失败: {0}", key);
                throw;
            }
        }

        /// <summary>
        /// 保存设置到文件
        /// </summary>
        private void SaveSettingsToFile(LocalSettings settings)
        {
            var directory = Path.GetDirectoryName(_settingsFilePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            File.WriteAllText(_settingsFilePath, json);
        }

        /// <summary>
        /// 获取本机IP地址
        /// </summary>
        public static string GetLocalIpAddress()
        {
            try
            {
                var host = System.Net.Dns.GetHostEntry(System.Net.Dns.GetHostName());
                foreach (var ip in host.AddressList)
                {
                    if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    {
                        return ip.ToString();
                    }
                }
            }
            catch { }
            return "127.0.0.1";
        }
    }

    /// <summary>
    /// 本地设置
    /// </summary>
    public class LocalSettings
    {
        public string ApiGatewayUrl { get; set; }
        public string NodeId { get; set; }
        public string NodeName { get; set; }
        public string Username { get; set; }
        public string Language { get; set; }
        public System.Collections.Generic.Dictionary<string, string> CustomSettings { get; set; }

        // RabbitMQ 配置
        public string RabbitMqHost { get; set; } = "localhost";
        public int RabbitMqPort { get; set; } = 5672;
        public string RabbitMqUser { get; set; } = "devdcp";
        public string RabbitMqPassword { get; set; } = "devdcp";
        public string RabbitMqExchange { get; set; } = "devdcp.collection";

        /// <summary>
        /// 将 LocalSettings 转换为 Core.AppSettings 供引擎使用
        /// </summary>
        public Collector.Core.Models.AppSettings ToAppSettings()
        {
            return new Collector.Core.Models.AppSettings
            {
                NodeId = NodeId ?? "",
                NodeName = NodeName ?? "",
                ApiGatewayUrl = ApiGatewayUrl ?? "http://localhost:60620",
                RabbitMqHost = RabbitMqHost ?? "localhost",
                RabbitMqPort = RabbitMqPort,
                RabbitMqUser = RabbitMqUser ?? "devdcp",
                RabbitMqPassword = RabbitMqPassword ?? "devdcp",
                RabbitMqExchange = RabbitMqExchange ?? "devdcp.collection",
                Language = Language ?? "zh-CN"
            };
        }
    }
}
