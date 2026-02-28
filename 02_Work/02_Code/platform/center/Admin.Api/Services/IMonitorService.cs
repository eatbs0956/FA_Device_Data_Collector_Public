using Admin.Api.Domain.DTOs;

namespace Admin.Api.Services;

/// <summary>
/// 监控数据服务接口
/// </summary>
public interface IMonitorService
{
    #region 实时监控

    /// <summary>
    /// 获取所有设备的监控数据（用于实时监控卡片）
    /// </summary>
    /// <param name="groupId">分组ID（可选）</param>
    /// <param name="nodeId">节点ID（可选）</param>
    /// <param name="connectionStatus">连接状态筛选（可选）</param>
    /// <returns>设备监控数据列表</returns>
    Task<List<DeviceMonitorData>> GetDevicesMonitorDataAsync(
        Guid? groupId = null,
        Guid? nodeId = null,
        string? connectionStatus = null);

    /// <summary>
    /// 获取单个设备的最新数据（用于弹窗详情）
    /// </summary>
    /// <param name="deviceId">设备ID</param>
    /// <returns>设备详细数据</returns>
    Task<DeviceDetailData?> GetDeviceLatestDataAsync(Guid deviceId);

    #endregion

    #region 历史数据

    /// <summary>
    /// 获取设备树（按分组组织）
    /// </summary>
    /// <returns>设备树结构</returns>
    Task<List<DeviceTreeNode>> GetDeviceTreeAsync();

    /// <summary>
    /// 查询设备历史数据
    /// </summary>
    /// <param name="deviceId">设备ID</param>
    /// <param name="request">查询参数</param>
    /// <returns>历史数据结果</returns>
    Task<HistoryDataResult> GetDeviceHistoryAsync(Guid deviceId, HistoryQueryRequest request);

    #endregion

    #region 统计报表

    /// <summary>
    /// 按设备统计
    /// </summary>
    /// <param name="request">查询参数</param>
    /// <returns>统计结果列表</returns>
    Task<List<StatisticsResult>> GetStatisticsByDevicesAsync(StatisticsQueryRequest request);

    /// <summary>
    /// 按分组统计
    /// </summary>
    /// <param name="request">查询参数</param>
    /// <returns>统计结果列表</returns>
    Task<List<StatisticsResult>> GetStatisticsByGroupsAsync(StatisticsQueryRequest request);

    /// <summary>
    /// 按节点统计
    /// </summary>
    /// <param name="request">查询参数</param>
    /// <returns>统计结果列表</returns>
    Task<List<StatisticsResult>> GetStatisticsByNodesAsync(StatisticsQueryRequest request);

    #endregion

    #region 仪表盘

    /// <summary>
    /// 获取仪表盘摘要数据
    /// </summary>
    /// <returns>仪表盘摘要</returns>
    Task<DashboardSummary> GetDashboardSummaryAsync();

    #endregion
}
