using Collector.Core.Models;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Runtime.InteropServices;

namespace Collector.Agent.Services;

/// <summary>
/// 通知服务实现 - 使用 SignalR 接收服务端推送
/// </summary>
public class NotificationService : INotificationService, IAsyncDisposable
{
    private readonly ILogger<NotificationService> _logger;
    private readonly AppSettings _settings;
    private HubConnection? _hubConnection;
    private bool _isConnected;

    public bool IsConnected => _isConnected;

    public event EventHandler<ConfigChangeNotification>? ConfigChanged;
    public event EventHandler<bool>? ConnectionStateChanged;

    public NotificationService(ILogger<NotificationService> logger, AppSettings settings)
    {
        _logger = logger;
        _settings = settings;
    }

    public async Task ConnectAsync(string hubUrl, string accessToken)
    {
        await ConnectAsync(hubUrl, accessToken, null);
    }

    /// <summary>
    /// 支持传递 nodeId 的重载，便于自动注册节点
    /// </summary>
    public async Task ConnectAsync(string hubUrl, string accessToken, string? nodeId)
    {
        try
        {
            _logger.LogInformation("连接 SignalR Hub: {Url}", hubUrl);

            _hubConnection = new HubConnectionBuilder()
                .WithUrl(hubUrl, options =>
                {
                    options.AccessTokenProvider = () => Task.FromResult<string?>(accessToken);
                })
                .WithAutomaticReconnect(new[] { TimeSpan.Zero, TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(10), TimeSpan.FromSeconds(30) })
                .Build();

            // 注册事件处理
            _hubConnection.On<ConfigChangeNotification>("ConfigChanged", notification =>
            {
                _logger.LogInformation("收到配置变更通知: {ChangeType}", notification.ChangeType);
                ConfigChanged?.Invoke(this, notification);
            });

            // 监听注册成功事件
            _hubConnection.On<object>("Registered", data =>
            {
                _logger.LogInformation("节点注册成功: {Data}", data);
            });

            _hubConnection.Reconnecting += error =>
            {
                _logger.LogWarning(error, "SignalR 正在重连...");
                _isConnected = false;
                ConnectionStateChanged?.Invoke(this, false);
                return Task.CompletedTask;
            };

            _hubConnection.Reconnected += connectionId =>
            {
                _logger.LogInformation("SignalR 重连成功: {ConnectionId}", connectionId);
                _isConnected = true;
                ConnectionStateChanged?.Invoke(this, true);
                return Task.CompletedTask;
            };

            _hubConnection.Closed += error =>
            {
                _logger.LogWarning(error, "SignalR 连接已关闭");
                _isConnected = false;
                ConnectionStateChanged?.Invoke(this, false);
                return Task.CompletedTask;
            };

            await _hubConnection.StartAsync();
            _isConnected = true;
            ConnectionStateChanged?.Invoke(this, true);
            
            _logger.LogInformation("SignalR 连接成功");

            // 连接成功后自动注册节点
            if (!string.IsNullOrWhiteSpace(nodeId))
            {
                try
                {
                    var registrationRequest = new EdgeNodeRegisterRequest
                    {
                        NodeName = _settings.NodeName ?? Environment.MachineName,
                        Platform = $".NET {Environment.Version}",
                        Version = "1.0.0",
                        OsInfo = RuntimeInformation.OSDescription,
                        IpAddress = GetLocalIpAddress(),
                        InstallPath = AppContext.BaseDirectory
                    };

                    await _hubConnection.InvokeAsync("RegisterNode", nodeId, registrationRequest);
                    _logger.LogInformation("已调用 RegisterNode: {NodeId}, Name={NodeName}", nodeId, registrationRequest.NodeName);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "调用 RegisterNode 失败");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "SignalR 连接失败");
            _isConnected = false;
            ConnectionStateChanged?.Invoke(this, false);
            throw;
        }
    }

    private string? GetLocalIpAddress()
    {
        try
        {
            // 按优先级排列的网卡类型：以太网 > 无线网
            var preferredTypes = new[]
            {
                NetworkInterfaceType.Ethernet,
                NetworkInterfaceType.Wireless80211
            };

            // 需要排除的关键字（WSL、Hyper-V、VPN、回环等虚拟网卡）
            var excludeKeywords = new[]
            {
                "vEthernet", "Hyper-V", "VMware", "VirtualBox",
                "Loopback", "Pseudo", "Tunnel", "TAP", "VPN", "Tailscale"
            };

            foreach (var type in preferredTypes)
            {
                var ip = NetworkInterface.GetAllNetworkInterfaces()
                    .Where(nic =>
                        nic.NetworkInterfaceType == type &&
                        nic.OperationalStatus == OperationalStatus.Up &&
                        !excludeKeywords.Any(kw =>
                            nic.Description.Contains(kw, StringComparison.OrdinalIgnoreCase) ||
                            nic.Name.Contains(kw, StringComparison.OrdinalIgnoreCase)))
                    .SelectMany(nic => nic.GetIPProperties().UnicastAddresses)
                    .Where(addr =>
                        addr.Address.AddressFamily == AddressFamily.InterNetwork &&
                        !IPAddress.IsLoopback(addr.Address))
                    .Select(addr => addr.Address.ToString())
                    .FirstOrDefault();

                if (ip != null)
                    return ip;
            }

            _logger.LogWarning("未找到以太网或无线网卡的有效 IP，回退到 DNS 解析");

            // 兜底：DNS 解析（原有逻辑）
            var host = Dns.GetHostEntry(Dns.GetHostName());
            return host.AddressList
                .FirstOrDefault(ip =>
                    ip.AddressFamily == AddressFamily.InterNetwork &&
                    !IPAddress.IsLoopback(ip))
                ?.ToString();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "获取本地IP地址失败");
        }
        return null;
    }

    public async Task ReportStatusAsync(string nodeId, CollectorStatusReport report)
    {
        if (_hubConnection == null || !_isConnected)
        {
            _logger.LogWarning("无法发送状态报告：SignalR 未连接");
            return;
        }

        try
        {
            await _hubConnection.InvokeAsync("ReportStatus", nodeId, report);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "发送状态报告失败: {NodeId}", nodeId);
        }
    }

    public async Task DisconnectAsync()
    {
        if (_hubConnection != null)
        {
            await _hubConnection.StopAsync();
            await _hubConnection.DisposeAsync();
            _hubConnection = null;
        }

        _isConnected = false;
        ConnectionStateChanged?.Invoke(this, false);
        _logger.LogInformation("SignalR 已断开连接");
    }

    public async ValueTask DisposeAsync()
    {
        await DisconnectAsync();
    }
}
