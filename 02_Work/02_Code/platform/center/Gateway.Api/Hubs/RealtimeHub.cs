using Microsoft.AspNetCore.SignalR;

namespace Gateway.Api.Hubs;

/// <summary>
/// 实时数据推送 Hub
/// </summary>
public class RealtimeHub : Hub
{
    private readonly ILogger<RealtimeHub> _logger;

    public RealtimeHub(ILogger<RealtimeHub> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// 连接建立时
    /// </summary>
    public override async Task OnConnectedAsync()
    {
        var tenantId = Context.GetHttpContext()?.Request.Query["tenant"].ToString();
        
        if (!string.IsNullOrEmpty(tenantId))
        {
            // 将连接加入租户组
            await Groups.AddToGroupAsync(Context.ConnectionId, $"tenant:{tenantId}");
            _logger.LogInformation("客户端连接: {ConnectionId}, 租户: {TenantId}", 
                Context.ConnectionId, tenantId);
        }
        else
        {
            _logger.LogInformation("客户端连接: {ConnectionId}", Context.ConnectionId);
        }

        await base.OnConnectedAsync();
    }

    /// <summary>
    /// 连接断开时
    /// </summary>
    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        _logger.LogInformation("客户端断开: {ConnectionId}, 异常: {Exception}", 
            Context.ConnectionId, exception?.Message);
        
        await base.OnDisconnectedAsync(exception);
    }

    /// <summary>
    /// 订阅设备状态
    /// </summary>
    /// <param name="tenant">租户ID</param>
    /// <param name="device">设备ID（可选）</param>
    public async Task SubscribeDeviceStatus(string tenant, string? device = null)
    {
        var group = device == null 
            ? $"status:{tenant}" 
            : $"status:{tenant}:{device}";
            
        await Groups.AddToGroupAsync(Context.ConnectionId, group);
        _logger.LogDebug("客户端 {ConnectionId} 订阅设备状态: {Group}", Context.ConnectionId, group);
    }

    /// <summary>
    /// 取消订阅设备状态
    /// </summary>
    public async Task UnsubscribeDeviceStatus(string tenant, string? device = null)
    {
        var group = device == null 
            ? $"status:{tenant}" 
            : $"status:{tenant}:{device}";
            
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
        _logger.LogDebug("客户端 {ConnectionId} 取消订阅: {Group}", Context.ConnectionId, group);
    }

    /// <summary>
    /// 订阅告警通知
    /// </summary>
    public async Task SubscribeAlerts(string tenant)
    {
        var group = $"alert:{tenant}";
        await Groups.AddToGroupAsync(Context.ConnectionId, group);
        _logger.LogDebug("客户端 {ConnectionId} 订阅告警: {Group}", Context.ConnectionId, group);
    }

    /// <summary>
    /// 取消订阅告警
    /// </summary>
    public async Task UnsubscribeAlerts(string tenant)
    {
        var group = $"alert:{tenant}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
    }

    /// <summary>
    /// 订阅任务状态
    /// </summary>
    public async Task SubscribeTaskStatus(string tenant)
    {
        var group = $"task:{tenant}";
        await Groups.AddToGroupAsync(Context.ConnectionId, group);
        _logger.LogDebug("客户端 {ConnectionId} 订阅任务状态: {Group}", Context.ConnectionId, group);
    }

    /// <summary>
    /// 取消订阅任务状态
    /// </summary>
    public async Task UnsubscribeTaskStatus(string tenant)
    {
        var group = $"task:{tenant}";
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
    }

    /// <summary>
    /// 订阅设备实时数据
    /// </summary>
    /// <param name="tenant">租户ID</param>
    /// <param name="device">设备ID（可选，不传则订阅该租户所有设备）</param>
    public async Task SubscribeDeviceData(string tenant, string? device = null)
    {
        var group = device == null 
            ? $"data:{tenant}" 
            : $"data:{tenant}:{device}";
            
        await Groups.AddToGroupAsync(Context.ConnectionId, group);
        _logger.LogDebug("客户端 {ConnectionId} 订阅设备数据: {Group}", Context.ConnectionId, group);
    }

    /// <summary>
    /// 取消订阅设备实时数据
    /// </summary>
    public async Task UnsubscribeDeviceData(string tenant, string? device = null)
    {
        var group = device == null 
            ? $"data:{tenant}" 
            : $"data:{tenant}:{device}";
            
        await Groups.RemoveFromGroupAsync(Context.ConnectionId, group);
        _logger.LogDebug("客户端 {ConnectionId} 取消订阅设备数据: {Group}", Context.ConnectionId, group);
    }
}
