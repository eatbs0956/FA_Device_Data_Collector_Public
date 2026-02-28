using Admin.Api.Domain.DTOs;
using Admin.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Api.Controllers;

/// <summary>
/// 监控数据控制器 - 提供实时监控、历史查询、统计报表、仪表盘数据接口
/// </summary>
/// <remarks>
/// 关联菜单：
/// - 实时监控 (MenuId: 10)
/// - 历史数据 (MenuId: 11)
/// - 统计报表 (MenuId: 12)
/// - 首页仪表盘 (MenuId: 1)
/// </remarks>
[ApiController]
[Route("api/monitor")]
[Authorize]
public class MonitorController : ControllerBase
{
    private readonly IMonitorService _monitorService;
    private readonly ILogger<MonitorController> _logger;

    public MonitorController(
        IMonitorService monitorService,
        ILogger<MonitorController> logger)
    {
        _monitorService = monitorService;
        _logger = logger;
    }

    #region 实时监控 (MenuId: 10)

    /// <summary>
    /// 获取所有设备的实时监控数据
    /// </summary>
    /// <param name="groupId">分组ID（可选筛选）</param>
    /// <param name="nodeId">节点ID（可选筛选）</param>
    /// <param name="connectionStatus">连接状态（可选筛选）</param>
    /// <returns>设备监控数据列表</returns>
    [HttpGet("devices/latest")]
    [Authorize(Policy = "ButtonPermission:10:select")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDevicesLatest(
        [FromQuery] Guid? groupId,
        [FromQuery] Guid? nodeId,
        [FromQuery] string? connectionStatus)
    {
        try
        {
            var result = await _monitorService.GetDevicesMonitorDataAsync(groupId, nodeId, connectionStatus);
            return Ok(new
            {
                code = "0000",
                msg = "查询成功",
                data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取设备监控数据失败");
            return StatusCode(500, new
            {
                code = "500",
                msg = "获取设备监控数据失败",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// 获取单个设备的最新详细数据
    /// </summary>
    /// <param name="id">设备ID（数据库主键）</param>
    /// <returns>设备详细数据</returns>
    [HttpGet("devices/{id}/latest")]
    [Authorize(Policy = "ButtonPermission:10:select")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeviceLatest(Guid id)
    {
        try
        {
            var result = await _monitorService.GetDeviceLatestDataAsync(id);
            if (result == null)
            {
                return NotFound(new
                {
                    code = "404",
                    msg = "设备不存在",
                    data = (object?)null
                });
            }

            return Ok(new
            {
                code = "0000",
                msg = "查询成功",
                data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取设备 {DeviceId} 的最新数据失败", id);
            return StatusCode(500, new
            {
                code = "500",
                msg = "获取设备详细数据失败",
                data = (object?)null
            });
        }
    }

    #endregion

    #region 历史数据 (MenuId: 11)

    /// <summary>
    /// 获取设备树（用于历史查询的设备选择）
    /// </summary>
    /// <returns>设备树结构</returns>
    [HttpGet("device-tree")]
    [Authorize(Policy = "ButtonPermission:11:select")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeviceTree()
    {
        try
        {
            var result = await _monitorService.GetDeviceTreeAsync();
            return Ok(new
            {
                code = "0000",
                msg = "查询成功",
                data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取设备树失败");
            return StatusCode(500, new
            {
                code = "500",
                msg = "获取设备树失败",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// 查询设备历史数据
    /// </summary>
    /// <param name="id">设备ID（数据库主键）</param>
    /// <param name="request">查询参数</param>
    /// <returns>历史数据</returns>
    [HttpGet("devices/{id}/history")]
    [Authorize(Policy = "ButtonPermission:11:select")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeviceHistory(Guid id, [FromQuery] HistoryQueryRequest request)
    {
        try
        {
            var result = await _monitorService.GetDeviceHistoryAsync(id, request);
            return Ok(new
            {
                code = "0000",
                msg = "查询成功",
                data = result
            });
        }
        catch (ArgumentException ex)
        {
            return NotFound(new
            {
                code = "404",
                msg = ex.Message,
                data = (object?)null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "查询设备 {DeviceId} 历史数据失败", id);
            return StatusCode(500, new
            {
                code = "500",
                msg = "查询历史数据失败",
                data = (object?)null
            });
        }
    }

    #endregion

    #region 统计报表 (MenuId: 12)

    /// <summary>
    /// 按设备统计
    /// </summary>
    /// <param name="request">查询参数</param>
    /// <returns>统计结果</returns>
    [HttpGet("statistics/devices")]
    [Authorize(Policy = "ButtonPermission:12:select")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatisticsByDevices([FromQuery] StatisticsQueryRequest request)
    {
        try
        {
            var result = await _monitorService.GetStatisticsByDevicesAsync(request);
            return Ok(new
            {
                code = "0000",
                msg = "查询成功",
                data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "按设备统计失败");
            return StatusCode(500, new
            {
                code = "500",
                msg = "统计查询失败",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// 按分组统计
    /// </summary>
    /// <param name="request">查询参数</param>
    /// <returns>统计结果</returns>
    [HttpGet("statistics/groups")]
    [Authorize(Policy = "ButtonPermission:12:select")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatisticsByGroups([FromQuery] StatisticsQueryRequest request)
    {
        try
        {
            var result = await _monitorService.GetStatisticsByGroupsAsync(request);
            return Ok(new
            {
                code = "0000",
                msg = "查询成功",
                data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "按分组统计失败");
            return StatusCode(500, new
            {
                code = "500",
                msg = "统计查询失败",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// 按节点统计
    /// </summary>
    /// <param name="request">查询参数</param>
    /// <returns>统计结果</returns>
    [HttpGet("statistics/nodes")]
    [Authorize(Policy = "ButtonPermission:12:select")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatisticsByNodes([FromQuery] StatisticsQueryRequest request)
    {
        try
        {
            var result = await _monitorService.GetStatisticsByNodesAsync(request);
            return Ok(new
            {
                code = "0000",
                msg = "查询成功",
                data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "按节点统计失败");
            return StatusCode(500, new
            {
                code = "500",
                msg = "统计查询失败",
                data = (object?)null
            });
        }
    }

    #endregion

    #region 仪表盘 (MenuId: 1 首页)

    /// <summary>
    /// 获取仪表盘摘要数据
    /// </summary>
    /// <returns>仪表盘摘要</returns>
    [HttpGet("dashboard/summary")]
    [Authorize(Policy = "ButtonPermission:1:select")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDashboardSummary()
    {
        try
        {
            var result = await _monitorService.GetDashboardSummaryAsync();
            return Ok(new
            {
                code = "0000",
                msg = "查询成功",
                data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取仪表盘数据失败");
            return StatusCode(500, new
            {
                code = "500",
                msg = "获取仪表盘数据失败",
                data = (object?)null
            });
        }
    }

    #endregion
}
