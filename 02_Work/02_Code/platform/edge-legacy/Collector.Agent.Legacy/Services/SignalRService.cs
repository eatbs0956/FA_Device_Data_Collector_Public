using System;
using System.Threading.Tasks;
using Microsoft.AspNet.SignalR.Client;
using NLog;

namespace Collector.Agent.Legacy.Services
{
    /// <summary>
    /// SignalR 服务 - 用于接收服务端推送的配置变更通知
    /// </summary>
    /// <remarks>
    /// 使用 Microsoft.AspNet.SignalR.Client（非 ASP.NET Core 版本）
    /// 支持 .NET Framework 4.5+
    /// </remarks>
    public class SignalRService : IDisposable
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        
        private HubConnection _connection;
        private IHubProxy _hubProxy;
        private string _nodeId;
        private bool _disposed;
        private bool _isReconnecting;

        /// <summary>
        /// 配置变更事件
        /// </summary>
        public event EventHandler<ConfigChangedEventArgs> OnConfigChanged;

        /// <summary>
        /// 连接状态变更事件
        /// </summary>
        public event EventHandler<bool> OnConnectionStateChanged;

        /// <summary>
        /// 是否已连接
        /// </summary>
        public bool IsConnected => _connection?.State == ConnectionState.Connected;

        /// <summary>
        /// 连接到 SignalR Hub
        /// </summary>
        public async Task ConnectAsync(string gatewayUrl, string nodeId, string accessToken)
        {
            _nodeId = nodeId;

            try
            {
                // 构建 Hub URL（假设 Admin.Api 在 Gateway 后面）
                var hubUrl = $"{gatewayUrl.TrimEnd('/')}/hub/collector";
                
                _connection = new HubConnection(hubUrl);
                
                // 添加访问令牌
                if (!string.IsNullOrEmpty(accessToken))
                {
                    _connection.Headers.Add("Authorization", $"Bearer {accessToken}");
                }

                // 创建 Hub 代理
                _hubProxy = _connection.CreateHubProxy("CollectorHub");

                // 注册服务端回调
                RegisterCallbacks();

                // 注册连接事件
                _connection.StateChanged += Connection_StateChanged;
                _connection.Error += Connection_Error;
                _connection.Reconnecting += Connection_Reconnecting;
                _connection.Reconnected += Connection_Reconnected;
                _connection.Closed += Connection_Closed;

                // 开始连接
                Logger.Info("正在连接 SignalR Hub: {0}", hubUrl);
                await _connection.Start();
                
                Logger.Info("SignalR 连接成功");

                // 注册节点
                await RegisterNodeAsync();
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "SignalR 连接失败");
                throw;
            }
        }

        /// <summary>
        /// 注册服务端回调方法
        /// </summary>
        private void RegisterCallbacks()
        {
            // 注册成功回调
            _hubProxy.On<dynamic>("Registered", data =>
            {
                Logger.Info("节点注册成功: {0}", data?.nodeId);
            });

            // 配置变更回调
            _hubProxy.On<dynamic>("ConfigChanged", notification =>
            {
                try
                {
                    var changeType = notification?.ChangeType?.ToString() ?? notification?.changeType?.ToString();
                    var entityType = notification?.EntityType?.ToString() ?? notification?.entityType?.ToString();
                    var entityId = notification?.EntityId?.ToString() ?? notification?.entityId?.ToString();
                    var message = notification?.Message?.ToString() ?? notification?.message?.ToString();

                    Logger.Info("收到配置变更通知: Type={0}, Entity={1}, Id={2}", 
                        changeType, entityType, entityId);

                    OnConfigChanged?.Invoke(this, new ConfigChangedEventArgs
                    {
                        ChangeType = changeType,
                        EntityType = entityType,
                        EntityId = entityId,
                        Message = message
                    });
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "处理配置变更通知失败");
                }
            });
        }

        /// <summary>
        /// 向服务端注册节点
        /// </summary>
        private async Task RegisterNodeAsync()
        {
            try
            {
                await _hubProxy.Invoke("RegisterNode", _nodeId);
                Logger.Info("SignalR 节点注册完成: {0}", _nodeId);
            }
            catch (Exception ex)
            {
                Logger.Error(ex, "SignalR 节点注册失败");
            }
        }

        /// <summary>
        /// 向服务端报告状态
        /// </summary>
        public async Task ReportStatusAsync(string status, int runningTaskCount, double cpuUsage, double memoryUsageMb)
        {
            if (!IsConnected) return;

            try
            {
                var report = new
                {
                    Status = status,
                    RunningTaskCount = runningTaskCount,
                    CpuUsage = cpuUsage,
                    MemoryUsageMb = memoryUsageMb,
                    LastCollectionTime = DateTime.UtcNow
                };

                await _hubProxy.Invoke("ReportStatus", _nodeId, report);
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "状态报告失败");
            }
        }

        /// <summary>
        /// 断开连接
        /// </summary>
        public async Task DisconnectAsync()
        {
            try
            {
                if (IsConnected)
                {
                    await _hubProxy.Invoke("UnregisterNode");
                }
                _connection?.Stop();
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "断开连接时出错");
            }
        }

        #region 连接事件处理

        private void Connection_StateChanged(StateChange obj)
        {
            Logger.Debug("SignalR 状态变更: {0} -> {1}", obj.OldState, obj.NewState);
            OnConnectionStateChanged?.Invoke(this, obj.NewState == ConnectionState.Connected);
        }

        private void Connection_Error(Exception ex)
        {
            Logger.Error(ex, "SignalR 连接错误");
        }

        private void Connection_Reconnecting()
        {
            _isReconnecting = true;
            Logger.Warn("SignalR 正在重连...");
        }

        private async void Connection_Reconnected()
        {
            _isReconnecting = false;
            Logger.Info("SignalR 重连成功");

            // 重新注册节点
            await RegisterNodeAsync();
        }

        private async void Connection_Closed()
        {
            if (_disposed) return;

            Logger.Warn("SignalR 连接已关闭");
            OnConnectionStateChanged?.Invoke(this, false);

            // 自动重连
            if (!_isReconnecting)
            {
                await TryReconnectAsync();
            }
        }

        /// <summary>
        /// 尝试重新连接
        /// </summary>
        private async Task TryReconnectAsync()
        {
            var retryCount = 0;
            var maxRetries = 10;
            var delay = 5000;

            while (!_disposed && retryCount < maxRetries)
            {
                try
                {
                    Logger.Info("尝试重连 SignalR ({0}/{1})...", retryCount + 1, maxRetries);
                    await _connection.Start();
                    
                    if (_connection.State == ConnectionState.Connected)
                    {
                        Logger.Info("SignalR 重连成功");
                        await RegisterNodeAsync();
                        return;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Warn(ex, "重连失败");
                }

                retryCount++;
                await Task.Delay(delay);
                delay = Math.Min(delay * 2, 60000); // 指数退避，最大60秒
            }

            Logger.Error("SignalR 重连失败，已达最大重试次数");
        }

        #endregion

        #region IDisposable

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
            {
                try
                {
                    _connection?.Stop();
                    _connection?.Dispose();
                }
                catch { }
            }

            _disposed = true;
        }

        #endregion
    }

    /// <summary>
    /// 配置变更事件参数
    /// </summary>
    public class ConfigChangedEventArgs : EventArgs
    {
        /// <summary>
        /// 变更类型：ConfigUpdated, RestartRequested, EmergencyStop, TaskStart, TaskStop
        /// </summary>
        public string ChangeType { get; set; }

        /// <summary>
        /// 实体类型：Device, Tag, Task, Node
        /// </summary>
        public string EntityType { get; set; }

        /// <summary>
        /// 实体ID
        /// </summary>
        public string EntityId { get; set; }

        /// <summary>
        /// 附加消息
        /// </summary>
        public string Message { get; set; }
    }
}
