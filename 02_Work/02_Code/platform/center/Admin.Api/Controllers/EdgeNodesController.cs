using Admin.Api.Domain.DTOs;
using Admin.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Entities;

namespace Admin.Api.Controllers;

/// <summary>
/// 边缘节点控制器 - 完整的边缘节点CRUD功能
/// </summary>
/// <remarks>
/// 采集节点管理画面的菜单ID为8，包含以下按钮权限：
/// - 8:select - 查询权限
/// - 8:add - 新增权限
/// - 8:edit - 编辑权限
/// - 8:delete - 删除权限
/// </remarks>
[ApiController]
[Route("api/edge-nodes")]
[Authorize]
public class EdgeNodesController : ControllerBase
{
    private readonly IEdgeNodeService _edgeNodeService;
    private readonly DbContext _dbContext;
    private readonly ILogger<EdgeNodesController> _logger;

    public EdgeNodesController(
        IEdgeNodeService edgeNodeService,
        DbContext dbContext,
        ILogger<EdgeNodesController> logger)
    {
        _edgeNodeService = edgeNodeService;
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// 获取边缘节点列表（分页）
    /// </summary>
    /// <param name="request">查询参数</param>
    /// <returns>边缘节点列表</returns>
    [HttpGet]
    [Authorize(Policy = "ButtonPermission:8:select")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEdgeNodes([FromQuery] EdgeNodeQueryRequest request)
    {
        try
        {
            var result = await _edgeNodeService.GetEdgeNodesAsync(request);

            return Ok(new
            {
                code = "0000",
                msg = "查询成功",
                data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "查询边缘节点列表失败");
            return StatusCode(500, new
            {
                code = "500",
                msg = "查询边缘节点列表失败",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// 获取边缘节点下拉列表（用于设备表单）
    /// </summary>
    /// <returns>简化的边缘节点列表</returns>
    [HttpGet("dropdown")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEdgeNodesDropdown()
    {
        try
        {
            var edgeNodes = await _dbContext.Set<EdgeNode>()
                .Where(e => !e.DeletedFlag)
                .OrderBy(e => e.NodeName)
                .Select(e => new
                {
                    id = e.Id.ToString(),
                    nodeName = e.NodeName,
                    nodeId = e.NodeId,
                    status = e.Status,
                    platform = e.Platform
                })
                .ToListAsync();

            return Ok(new
            {
                code = "0000",
                msg = "查询成功",
                data = edgeNodes
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "查询边缘节点下拉列表失败");
            return StatusCode(500, new
            {
                code = "500",
                msg = "查询边缘节点下拉列表失败",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// 获取边缘节点详情
    /// </summary>
    /// <param name="id">节点ID</param>
    /// <returns>节点详情</returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "ButtonPermission:8:select")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEdgeNode(Guid id)
    {
        try
        {
            var edgeNode = await _edgeNodeService.GetEdgeNodeByIdAsync(id);

            if (edgeNode == null)
            {
                return NotFound(new
                {
                    code = "404",
                    msg = "边缘节点不存在",
                    data = (object?)null
                });
            }

            return Ok(new
            {
                code = "0000",
                msg = "查询成功",
                data = edgeNode
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "查询边缘节点详情失败: {Id}", id);
            return StatusCode(500, new
            {
                code = "500",
                msg = "查询边缘节点详情失败",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// 创建边缘节点（手动添加）
    /// </summary>
    /// <param name="request">创建请求</param>
    /// <returns>新创建的节点ID</returns>
    [HttpPost]
    [Authorize(Policy = "ButtonPermission:8:add")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateEdgeNode([FromBody] CreateEdgeNodeRequest request)
    {
        try
        {
            var id = await _edgeNodeService.CreateEdgeNodeAsync(request);

            return Ok(new
            {
                code = "0000",
                msg = "创建成功",
                data = new { id = id.ToString() }
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new
            {
                code = "400",
                msg = ex.Message,
                data = (object?)null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建边缘节点失败");
            return StatusCode(500, new
            {
                code = "500",
                msg = "创建边缘节点失败",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// 更新边缘节点
    /// </summary>
    /// <param name="id">节点ID</param>
    /// <param name="request">更新请求</param>
    /// <returns>操作结果</returns>
    [HttpPut("{id}")]
    [Authorize(Policy = "ButtonPermission:8:edit")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateEdgeNode(Guid id, [FromBody] UpdateEdgeNodeRequest request)
    {
        try
        {
            await _edgeNodeService.UpdateEdgeNodeAsync(id, request);

            return Ok(new
            {
                code = "0000",
                msg = "更新成功",
                data = (object?)null
            });
        }
        catch (KeyNotFoundException ex)
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
            _logger.LogError(ex, "更新边缘节点失败: {Id}", id);
            return StatusCode(500, new
            {
                code = "500",
                msg = "更新边缘节点失败",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// 删除边缘节点
    /// </summary>
    /// <param name="id">节点ID</param>
    /// <returns>操作结果（包含解除关联的设备数量）</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "ButtonPermission:8:delete")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteEdgeNode(Guid id)
    {
        try
        {
            var deviceCount = await _edgeNodeService.DeleteEdgeNodeAsync(id);

            return Ok(new
            {
                code = "0000",
                msg = deviceCount > 0 
                    ? $"删除成功，已解除 {deviceCount} 个设备的节点关联" 
                    : "删除成功",
                data = new { deviceCount }
            });
        }
        catch (KeyNotFoundException ex)
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
            _logger.LogError(ex, "删除边缘节点失败: {Id}", id);
            return StatusCode(500, new
            {
                code = "500",
                msg = "删除边缘节点失败",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// 获取边缘节点关联的设备数量
    /// </summary>
    /// <param name="id">节点ID</param>
    /// <returns>设备数量</returns>
    [HttpGet("{id}/device-count")]
    [Authorize(Policy = "ButtonPermission:8:select")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDeviceCount(Guid id)
    {
        try
        {
            var count = await _edgeNodeService.GetDeviceCountAsync(id);

            return Ok(new
            {
                code = "0000",
                msg = "查询成功",
                data = new { count }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "查询设备数量失败: {Id}", id);
            return StatusCode(500, new
            {
                code = "500",
                msg = "查询设备数量失败",
                data = (object?)null
            });
        }
    }
}
