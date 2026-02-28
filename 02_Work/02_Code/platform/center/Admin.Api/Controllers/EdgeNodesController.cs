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
    private readonly IServiceAccountValidator _serviceAccountValidator;
    private readonly DbContext _dbContext;
    private readonly ILogger<EdgeNodesController> _logger;

    public EdgeNodesController(
        IEdgeNodeService edgeNodeService,
        IServiceAccountValidator serviceAccountValidator,
        DbContext dbContext,
        ILogger<EdgeNodesController> logger)
    {
        _edgeNodeService = edgeNodeService;
        _serviceAccountValidator = serviceAccountValidator;
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

    // ============ Collector API 新增端点（采集器调用） ============

    /// <summary>
    /// 边缘节点注册（采集器启动时调用）
    /// </summary>
    /// <remarks>
    /// 若NodeId不存在则新建（auto类型）；
    /// 若NodeId已存在则更新系统信息并记录心跳。
    /// 如果携带JWT Token且为服务账号，会验证是否有权限操作该节点。
    /// </remarks>
    /// <param name="nodeId">节点标识</param>
    /// <param name="request">注册请求</param>
    /// <returns>注册响应</returns>
    [HttpPost("{nodeId}/register")]
    [AllowAnonymous] // 支持无认证和服务账号认证两种模式
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> RegisterEdgeNode(string nodeId, [FromBody] EdgeNodeRegisterRequest request)
    {
        try
        {
            // 如果请求携带了有效的认证信息，验证服务账号权限
            if (User.Identity?.IsAuthenticated == true)
            {
                var (isAllowed, errorMessage) = await _serviceAccountValidator.ValidateNodeAccessAsync(nodeId);
                if (!isAllowed)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new
                    {
                        code = "403",
                        msg = errorMessage ?? "无权限访问该节点",
                        data = (object?)null
                    });
                }
            }

            var result = await _edgeNodeService.RegisterEdgeNodeAsync(nodeId, request);

            _logger.LogInformation("边缘节点注册成功: NodeId={NodeId}, IsNew={IsNew}", 
                nodeId, result.IsNewNode);

            var response = new
            {
                code = "0000",
                msg = result.IsNewNode ? "节点注册成功" : "节点信息更新成功",
                data = result
            };

            return result.IsNewNode 
                ? StatusCode(StatusCodes.Status201Created, response) 
                : Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "边缘节点注册失败: {NodeId}", nodeId);
            return StatusCode(500, new
            {
                code = "500",
                msg = "节点注册失败",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// 节点心跳上报（采集器定期调用）
    /// </summary>
    /// <remarks>
    /// 如果携带JWT Token且为服务账号，会验证是否有权限操作该节点。
    /// </remarks>
    /// <param name="nodeId">节点标识</param>
    /// <param name="request">心跳请求（可选）</param>
    /// <returns>心跳响应</returns>
    [HttpPut("{nodeId}/heartbeat")]
    [AllowAnonymous] // 支持无认证和服务账号认证两种模式
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateHeartbeat(string nodeId, [FromBody] EdgeNodeHeartbeatRequest? request = null)
    {
        try
        {
            // 如果请求携带了有效的认证信息，验证服务账号权限
            if (User.Identity?.IsAuthenticated == true)
            {
                var (isAllowed, errorMessage) = await _serviceAccountValidator.ValidateNodeAccessAsync(nodeId);
                if (!isAllowed)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new
                    {
                        code = "403",
                        msg = errorMessage ?? "无权限访问该节点",
                        data = (object?)null
                    });
                }
            }

            var result = await _edgeNodeService.UpdateHeartbeatAsync(nodeId, request);

            if (!result.Success)
            {
                return NotFound(new
                {
                    code = "404",
                    msg = "节点不存在",
                    data = (object?)null
                });
            }

            return Ok(new
            {
                code = "0000",
                msg = "心跳更新成功",
                data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "心跳更新失败: {NodeId}", nodeId);
            return StatusCode(500, new
            {
                code = "500",
                msg = "心跳更新失败",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// 获取节点完整配置（采集器调用）
    /// </summary>
    /// <remarks>
    /// 返回分配给该节点的所有设备、标签和采集任务配置。
    /// 采集器启动后应调用此接口获取完整配置。
    /// 如果携带JWT Token且为服务账号，会验证是否有权限操作该节点。
    /// </remarks>
    /// <param name="nodeId">节点标识</param>
    /// <returns>节点配置</returns>
    [HttpGet("{nodeId}/config")]
    [AllowAnonymous] // 支持无认证和服务账号认证两种模式
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(object), StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetNodeConfig(string nodeId)
    {
        try
        {
            // 如果请求携带了有效的认证信息，验证服务账号权限
            if (User.Identity?.IsAuthenticated == true)
            {
                var (isAllowed, errorMessage) = await _serviceAccountValidator.ValidateNodeAccessAsync(nodeId);
                if (!isAllowed)
                {
                    return StatusCode(StatusCodes.Status403Forbidden, new
                    {
                        code = "403",
                        msg = errorMessage ?? "无权限访问该节点",
                        data = (object?)null
                    });
                }
            }

            var config = await _edgeNodeService.GetNodeConfigAsync(nodeId);

            if (config == null)
            {
                return NotFound(new
                {
                    code = "404",
                    msg = "节点不存在",
                    data = (object?)null
                });
            }

            return Ok(new
            {
                code = "0000",
                msg = "查询成功",
                data = config
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取节点配置失败: {NodeId}", nodeId);
            return StatusCode(500, new
            {
                code = "500",
                msg = "获取节点配置失败",
                data = (object?)null
            });
        }
    }

    // ============ 服务账号管理 API ============

    /// <summary>
    /// 绑定服务账号到边缘节点
    /// </summary>
    /// <remarks>
    /// 将服务账号绑定到指定的边缘节点，用于权限控制。
    /// 一个服务账号可以绑定多个边缘节点。
    /// 传入空的 ServiceUserId 可解除绑定。
    /// </remarks>
    /// <param name="id">边缘节点ID</param>
    /// <param name="request">绑定请求</param>
    /// <returns>操作结果</returns>
    [HttpPost("{id}/bind-service-account")]
    [Authorize(Policy = "ButtonPermission:8:edit")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> BindServiceAccount(Guid id, [FromBody] BindServiceAccountRequest request)
    {
        try
        {
            await _edgeNodeService.BindServiceAccountAsync(id, request.ServiceUserId);

            return Ok(new
            {
                code = "0000",
                msg = request.ServiceUserId.HasValue ? "绑定成功" : "解绑成功",
                data = (object?)null
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("绑定服务账号失败: {Message}", ex.Message);
            return BadRequest(new
            {
                code = "400",
                msg = ex.Message,
                data = (object?)null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "绑定服务账号失败: EdgeNodeId={Id}, ServiceUserId={ServiceUserId}", id, request.ServiceUserId);
            return StatusCode(500, new
            {
                code = "500",
                msg = "绑定服务账号失败",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// 获取服务账号绑定的边缘节点列表
    /// </summary>
    /// <remarks>
    /// 查询指定服务账号绑定的所有边缘节点。
    /// </remarks>
    /// <param name="serviceUserId">服务账号ID</param>
    /// <returns>边缘节点列表</returns>
    [HttpGet("by-service-account/{serviceUserId}")]
    [Authorize(Policy = "ButtonPermission:8:select")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetEdgeNodesByServiceAccount(Guid serviceUserId)
    {
        try
        {
            var edgeNodes = await _edgeNodeService.GetEdgeNodesByServiceUserAsync(serviceUserId);

            return Ok(new
            {
                code = "0000",
                msg = "查询成功",
                data = edgeNodes
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "查询服务账号绑定的边缘节点失败: {ServiceUserId}", serviceUserId);
            return StatusCode(500, new
            {
                code = "500",
                msg = "查询失败",
                data = (object?)null
            });
        }
    }
}
