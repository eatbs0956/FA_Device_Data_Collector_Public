using Admin.Api.Domain.DTOs;
using Admin.Api.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Entities;
using System.Collections.Concurrent;

namespace Admin.Api.Hubs;

/// <summary>
/// 采集器通信 SignalR Hub
/// </summary>
/// <remarks>
/// 用于服务端向采集器推送配置变更、控制命令等。
/// 采集器连接时使用 NodeId 加入对应的组。
/// </remarks>
public class CollectorHub : Hub
{
    private readonly ILogger<CollectorHub> _logger;
    private readonly IEdgeNodeService _edgeNodeService;
    private readonly DbContext _dbContext;

    // 存储连接ID和NodeId的映射关系
    private static readonly ConcurrentDictionary<string, string> _connectionToNode = new();
    private static readonly ConcurrentDictionary<string, HashSet<string>> _nodeToConnections = new();
    private static readonly object _lock = new();

    public CollectorHub(ILogger<CollectorHub> logger, IEdgeNodeService edgeNodeService, DbContext dbContext)
    {
        _logger = logger;
        _edgeNodeService = edgeNodeService;
        _dbContext = dbContext;
    }

    /// <summary>
    /// 采集器连接时调用，注册到对应的节点组并保存到数据库
    /// </summary>
    /// <param name="nodeId">节点标识</param>
    /// <param name="request">注册请求信息</param>
    public async Task RegisterNode(string nodeId, EdgeNodeRegisterRequest request)
    {
        var connectionId = Context.ConnectionId;
        _logger.LogInformation("接收到节点注册请求: NodeId={NodeId}, ConnectionId={ConnectionId}, Name={Name}", 
            nodeId, connectionId, request.NodeName);

        // 存储映射关系
        _connectionToNode[connectionId] = nodeId;
        
        lock (_lock)
        {
            if (!_nodeToConnections.TryGetValue(nodeId, out var connections))
            {
                connections = new HashSet<string>();
                _nodeToConnections[nodeId] = connections;
            }
            connections.Add(connectionId);
        }

        // 加入节点组
        await Groups.AddToGroupAsync(connectionId, $"node_{nodeId}");

        // 同步到数据库
        try
        {
            // 如果客户端没传IP，尝试从连接中获取
            if (string.IsNullOrEmpty(request.IpAddress))
            {
                request.IpAddress = Context.GetHttpContext()?.Connection.RemoteIpAddress?.ToString();
            }

            await _edgeNodeService.RegisterEdgeNodeAsync(nodeId, request);
            _logger.LogInformation("采集器已同步至数据库: NodeId={NodeId}, Name={NodeName}", nodeId, request.NodeName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "采集器同步到数据库失败: {NodeId}", nodeId);
        }

        _logger.LogInformation("采集器已连接并注册: NodeId={NodeId}, ConnectionId={ConnectionId}", 
            nodeId, connectionId);

        // 通知客户端注册成功
        await Clients.Caller.SendAsync("Registered", new { nodeId, connectionId, serverTime = DateTimeOffset.UtcNow });
    }

    /// <summary>
    /// 采集器取消注册
    /// </summary>
    public async Task UnregisterNode()
    {
        var connectionId = Context.ConnectionId;
        
        if (_connectionToNode.TryRemove(connectionId, out var nodeId))
        {
            lock (_lock)
            {
                if (_nodeToConnections.TryGetValue(nodeId, out var connections))
                {
                    connections.Remove(connectionId);
                    if (connections.Count == 0)
                    {
                        _nodeToConnections.TryRemove(nodeId, out _);
                    }
                }
            }

            await Groups.RemoveFromGroupAsync(connectionId, $"node_{nodeId}");
            
            _logger.LogInformation("采集器已取消注册: NodeId={NodeId}, ConnectionId={ConnectionId}", 
                nodeId, connectionId);
        }
    }

    /// <summary>
    /// 连接断开时清理
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        var connectionId = Context.ConnectionId;
        string? nodeId = null;
        var isLastConnection = false;

        if (_connectionToNode.TryRemove(connectionId, out nodeId))
        {
            lock (_lock)
            {
                if (_nodeToConnections.TryGetValue(nodeId, out var connections))
                {
                    connections.Remove(connectionId);
                    if (connections.Count == 0)
                    {
                        _nodeToConnections.TryRemove(nodeId, out _);
                        isLastConnection = true;
                    }
                }
            }

            _logger.LogInformation("采集器断开连接: NodeId={NodeId}, ConnectionId={ConnectionId}, IsLastConnection={IsLast}, Exception={Exception}", 
                nodeId, connectionId, isLastConnection, exception?.Message ?? "无");
        }

        // 当节点的最后一个连接断开时，将该节点下的设备和任务标记为离线/停止
        if (isLastConnection && nodeId != null)
        {
            await MarkNodeOfflineAsync(nodeId);
        }

        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// 节点离线时，更新该节点下所有设备为 Disconnected、所有运行中任务为 Stopped
    /// </summary>
    private async Task MarkNodeOfflineAsync(string nodeId)
    {
        try
        {
            // 查找节点对应的 EdgeNode 记录
            var edgeNode = await _dbContext.Set<EdgeNode>()
                .Where(e => e.NodeId == nodeId && !e.DeletedFlag)
                .FirstOrDefaultAsync();

            if (edgeNode == null)
            {
                _logger.LogWarning("节点离线处理：未找到 EdgeNode 记录: {NodeId}", nodeId);
                return;
            }

            // 1. 将该节点下所有 Connected 状态的设备标记为 Disconnected
            var devices = await _dbContext.Set<Device>()
                .Where(d => d.EdgeNodeId == edgeNode.Id && !d.DeletedFlag && d.ConnectionStatus == "Connected")
                .ToListAsync();

            foreach (var device in devices)
            {
                device.ConnectionStatus = "Disconnected";
                device.UpdatedAt = DateTimeOffset.UtcNow;
            }

            if (devices.Count > 0)
            {
                _logger.LogInformation("节点离线，已将 {Count} 个设备标记为 Disconnected: NodeId={NodeId}", 
                    devices.Count, nodeId);
            }

            // 2. 将通过这些设备关联的运行中任务标记为 Stopped
            var deviceIds = await _dbContext.Set<Device>()
                .Where(d => d.EdgeNodeId == edgeNode.Id && !d.DeletedFlag)
                .Select(d => d.Id)
                .ToListAsync();

            if (deviceIds.Count > 0)
            {
                var activeTasks = await _dbContext.Set<CollectionTask>()
                    .Include(t => t.TaskDevices)
                    .Where(t => !t.DeletedFlag && t.Status == "Active" 
                        && t.TaskDevices.Any(td => deviceIds.Contains(td.DeviceId)))
                    .ToListAsync();

                foreach (var task in activeTasks)
                {
                    task.Status = "Stopped";
                    task.UpdatedAt = DateTimeOffset.UtcNow;
                }

                if (activeTasks.Count > 0)
                {
                    _logger.LogInformation("节点离线，已将 {Count} 个任务标记为 Stopped: NodeId={NodeId}", 
                        activeTasks.Count, nodeId);
                }
            }

            // 3. 更新节点自身状态
            edgeNode.Status = "Offline";
            edgeNode.UpdatedAt = DateTimeOffset.UtcNow;

            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("节点离线处理完成: NodeId={NodeId}", nodeId);

            // 4. 广播离线事件给管理端
            await Clients.Group("admin").SendAsync("NodeOffline", new 
            { 
                nodeId, 
                deviceCount = devices.Count,
                timestamp = DateTimeOffset.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "节点离线处理失败: NodeId={NodeId}", nodeId);
        }
    }

    /// <summary>
    /// 采集器报告状态
    /// </summary>
    public async Task ReportStatus(string nodeId, CollectorStatusReport report)
    {
        _logger.LogDebug("收到采集器状态报告: NodeId={NodeId}, Status={Status}, Cpu={Cpu}%, Mem={Mem}MB", 
            nodeId, report.Status, report.CpuUsage, report.MemoryUsageMb);

        // 更新数据库心跳
        try
        {
            await _edgeNodeService.UpdateHeartbeatAsync(nodeId, new EdgeNodeHeartbeatRequest
            {
                CpuUsage = report.CpuUsage,
                MemoryUsageMb = report.MemoryUsageMb,
                DataPointsProcessed = report.DataPointsProcessed,
                LastCollectionTime = report.LastCollectionTime
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新采集器心跳时发生异常: {NodeId}", nodeId);
        }

        // 更新设备连接状态到数据库
        if (report.DeviceStatuses != null && report.DeviceStatuses.Count > 0)
        {
            try
            {
                var deviceIds = report.DeviceStatuses.Select(d => d.DeviceId).ToList();
                var devices = await _dbContext.Set<Device>()
                    .Where(d => deviceIds.Contains(d.DeviceId) && !d.DeletedFlag)
                    .ToListAsync();

                foreach (var deviceStatus in report.DeviceStatuses)
                {
                    var device = devices.FirstOrDefault(d => d.DeviceId == deviceStatus.DeviceId);
                    if (device != null)
                    {
                        device.ConnectionStatus = deviceStatus.ConnectionStatus;
                        device.ErrorCount = deviceStatus.ErrorCount;
                        if (deviceStatus.LastError != null)
                            device.LastError = deviceStatus.LastError;
                        if (deviceStatus.ConnectionStatus == "Connected")
                            device.LastConnectTime = DateTimeOffset.UtcNow;
                        device.UpdatedAt = DateTimeOffset.UtcNow;
                    }
                }

                await _dbContext.SaveChangesAsync();
                _logger.LogDebug("已更新 {Count} 个设备连接状态", devices.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新设备连接状态失败: {NodeId}", nodeId);
            }
        }

        // 更新任务运行状态到数据库
        if (report.TaskStatuses != null && report.TaskStatuses.Count > 0)
        {
            try
            {
                var taskCodes = report.TaskStatuses.Select(t => t.TaskCode).ToList();
                var tasks = await _dbContext.Set<CollectionTask>()
                    .Where(t => taskCodes.Contains(t.Code!) && !t.DeletedFlag)
                    .ToListAsync();

                foreach (var taskStatus in report.TaskStatuses)
                {
                    var task = tasks.FirstOrDefault(t => t.Code == taskStatus.TaskCode);
                    if (task != null)
                    {
                        // 映射采集器运行状态到数据库状态
                        // Running → Active, Stopped → Stopped, Error → Stopped
                        task.Status = taskStatus.Status switch
                        {
                            "Running" => "Active",
                            "Stopped" => "Stopped",
                            "Paused" => "Paused",
                            "Error" => "Stopped",
                            _ => task.Status
                        };
                        task.UpdatedAt = DateTimeOffset.UtcNow;
                    }
                }

                await _dbContext.SaveChangesAsync();
                _logger.LogDebug("已更新 {Count} 个任务运行状态", tasks.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "更新任务运行状态失败: {NodeId}", nodeId);
            }
        }

        // 广播给管理端
        await Clients.Group("admin").SendAsync("CollectorStatusUpdated", new { nodeId, report });
    }

    // ============ 静态方法供其他服务调用 ============

    /// <summary>
    /// 获取节点的连接数量
    /// </summary>
    public static int GetNodeConnectionCount(string nodeId)
    {
        if (_nodeToConnections.TryGetValue(nodeId, out var connections))
        {
            return connections.Count;
        }
        return 0;
    }

    /// <summary>
    /// 检查节点是否在线
    /// </summary>
    public static bool IsNodeOnline(string nodeId)
    {
        return GetNodeConnectionCount(nodeId) > 0;
    }

    /// <summary>
    /// 获取所有在线节点
    /// </summary>
    public static IEnumerable<string> GetOnlineNodes()
    {
        return _nodeToConnections.Keys.ToList();
    }
}

/// <summary>
/// 采集器状态报告
/// </summary>
public class CollectorStatusReport
{
    /// <summary>
    /// 状态：Running, Stopped, Error
    /// </summary>
    public string Status { get; set; } = "Running";

    /// <summary>
    /// 正在运行的任务数
    /// </summary>
    public int RunningTaskCount { get; set; }

    /// <summary>
    /// CPU使用率
    /// </summary>
    public double CpuUsage { get; set; }

    /// <summary>
    /// 内存使用MB
    /// </summary>
    public double MemoryUsageMb { get; set; }

    /// <summary>
    /// 已处理数据点数
    /// </summary>
    public long DataPointsProcessed { get; set; }

    /// <summary>
    /// 最后采集时间
    /// </summary>
    public DateTimeOffset? LastCollectionTime { get; set; }

    /// <summary>
    /// 错误信息
    /// </summary>
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// 设备运行时状态列表
    /// </summary>
    public List<DeviceStatusItem>? DeviceStatuses { get; set; }

    /// <summary>
    /// 任务运行时状态列表
    /// </summary>
    public List<TaskStatusItem>? TaskStatuses { get; set; }
}

/// <summary>
/// 设备状态上报项
/// </summary>
public class DeviceStatusItem
{
    /// <summary>
    /// 设备标识符（device_id 字段，非主键 Id）
    /// </summary>
    public string DeviceId { get; set; } = string.Empty;

    /// <summary>
    /// 连接状态：Connected, Disconnected, Connecting, Error
    /// </summary>
    public string ConnectionStatus { get; set; } = "Disconnected";

    /// <summary>
    /// 错误次数
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// 最后错误信息
    /// </summary>
    public string? LastError { get; set; }
}

/// <summary>
/// 任务状态上报项
/// </summary>
public class TaskStatusItem
{
    /// <summary>
    /// 任务编码（对应 collection_tasks.code）
    /// </summary>
    public string TaskCode { get; set; } = string.Empty;

    /// <summary>
    /// 运行状态：Active, Stopped, Paused, Error
    /// </summary>
    public string Status { get; set; } = "Stopped";

    /// <summary>
    /// 采集次数
    /// </summary>
    public long TotalCollectionCount { get; set; }

    /// <summary>
    /// 错误次数
    /// </summary>
    public long ErrorCount { get; set; }

    /// <summary>
    /// 最后错误信息
    /// </summary>
    public string? LastError { get; set; }
}

/// <summary>
/// CollectorHub 扩展方法，用于向特定节点发送消息
/// </summary>
public static class CollectorHubExtensions
{
    /// <summary>
    /// 向指定节点发送配置变更通知
    /// </summary>
    public static async Task NotifyConfigChange(this IHubContext<CollectorHub> hubContext, 
        string nodeId, ConfigChangeNotification notification)
    {
        await hubContext.Clients.Group($"node_{nodeId}")
            .SendAsync("ConfigChanged", notification);
    }

    /// <summary>
    /// 向指定节点发送重启请求
    /// </summary>
    public static async Task RequestRestart(this IHubContext<CollectorHub> hubContext, 
        string nodeId, string? message = null)
    {
        var notification = new ConfigChangeNotification
        {
            ChangeType = ConfigChangeType.RestartRequested,
            EntityType = "Node",
            EntityId = nodeId,
            Message = message ?? "管理员请求重启采集器"
        };

        await hubContext.Clients.Group($"node_{nodeId}")
            .SendAsync("ConfigChanged", notification);
    }

    /// <summary>
    /// 向指定节点发送紧急停止命令
    /// </summary>
    public static async Task EmergencyStop(this IHubContext<CollectorHub> hubContext, 
        string nodeId, string? message = null)
    {
        var notification = new ConfigChangeNotification
        {
            ChangeType = ConfigChangeType.EmergencyStop,
            EntityType = "Node",
            EntityId = nodeId,
            Message = message ?? "紧急停止采集"
        };

        await hubContext.Clients.Group($"node_{nodeId}")
            .SendAsync("ConfigChanged", notification);
    }

    /// <summary>
    /// 向所有节点广播消息
    /// </summary>
    public static async Task BroadcastToAllNodes(this IHubContext<CollectorHub> hubContext, 
        ConfigChangeNotification notification)
    {
        foreach (var nodeId in CollectorHub.GetOnlineNodes())
        {
            await hubContext.Clients.Group($"node_{nodeId}")
                .SendAsync("ConfigChanged", notification);
        }
    }

    /// <summary>
    /// 向指定节点发送任务控制命令
    /// </summary>
    public static async Task ControlTask(this IHubContext<CollectorHub> hubContext,
        string nodeId, string taskId, bool start)
    {
        var notification = new ConfigChangeNotification
        {
            ChangeType = start ? ConfigChangeType.TaskStart : ConfigChangeType.TaskStop,
            EntityType = "Task",
            EntityId = taskId,
            Message = start ? "启动任务" : "停止任务"
        };

        await hubContext.Clients.Group($"node_{nodeId}")
            .SendAsync("ConfigChanged", notification);
    }
}
