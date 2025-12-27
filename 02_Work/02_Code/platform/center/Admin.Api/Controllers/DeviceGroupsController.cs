using Admin.Api.Domain.DTOs;
using Admin.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Api.Controllers;

/// <summary>
/// 设备分组控制器 - 设备分组树的CRUD操作
/// </summary>
/// <remarks>
/// 设备分组菜单ID 4，按钮权限：
/// - 4:select - 查询权限
/// - 4:add - 新增权限
/// - 4:edit - 编辑权限
/// - 4:delete - 删除权限
/// </remarks>
[ApiController]
[Route("api/device-groups")]
[Authorize]
public class DeviceGroupsController : ControllerBase
{
    private readonly IDeviceGroupService _deviceGroupService;
    private readonly ILogger<DeviceGroupsController> _logger;

    public DeviceGroupsController(
        IDeviceGroupService deviceGroupService,
        ILogger<DeviceGroupsController> logger)
    {
        _deviceGroupService = deviceGroupService;
        _logger = logger;
    }

    /// <summary>
    /// 分页查询设备分组列表
    /// </summary>
    /// <param name="request">查询参数</param>
    /// <returns>分页的分组列表</returns>
    [HttpGet]
    [Authorize(Policy = "ButtonPermission:4:select")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGroups([FromQuery] DeviceGroupQueryRequest request)
    {
        try
        {
            var result = await _deviceGroupService.GetGroupsAsync(request);
            return Ok(new
            {
                code = "0000",
                msg = "查询成功",
                data = new
                {
                    records = result.Records,
                    total = result.Total,
                    current = request.Current,
                    size = request.Size
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "查询设备分组列表失败");
            return StatusCode(500, new
            {
                code = "500",
                msg = "查询设备分组列表失败",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// 获取设备分组树（完整版，包含设备数量）
    /// </summary>
    /// <returns>分组树结构</returns>
    [HttpGet("tree")]
    [Authorize(Policy = "ButtonPermission:4:select")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGroupTree()
    {
        try
        {
            var result = await _deviceGroupService.GetGroupTreeAsync();
            return Ok(new
            {
                code = "0000",
                msg = "查询成功",
                data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "查询设备分组树失败");
            return StatusCode(500, new
            {
                code = "500",
                msg = "查询设备分组树失败",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// 获取设备分组树（简化版，用于下拉选择）
    /// </summary>
    /// <returns>简化的分组树结构</returns>
    [HttpGet("tree/simple")]
    [Authorize(Policy = "ButtonPermission:4:select")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGroupTreeSimple()
    {
        try
        {
            var result = await _deviceGroupService.GetGroupTreeSimpleAsync();
            return Ok(new
            {
                code = "0000",
                msg = "查询成功",
                data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "查询设备分组树失败");
            return StatusCode(500, new
            {
                code = "500",
                msg = "查询设备分组树失败",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// 获取分组详情
    /// </summary>
    /// <param name="id">分组ID</param>
    /// <returns>分组详情</returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "ButtonPermission:4:select")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetGroup(Guid id)
    {
        try
        {
            var group = await _deviceGroupService.GetGroupByIdAsync(id);
            if (group == null)
            {
                return NotFound(new
                {
                    code = "404",
                    msg = "分组不存在",
                    data = (object?)null
                });
            }

            return Ok(new
            {
                code = "0000",
                msg = "查询成功",
                data = group
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "查询分组详情失败, GroupId: {GroupId}", id);
            return StatusCode(500, new
            {
                code = "500",
                msg = "查询分组详情失败",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// 创建分组
    /// </summary>
    /// <param name="request">创建分组请求</param>
    /// <returns>创建的分组ID</returns>
    [HttpPost]
    [Authorize(Policy = "ButtonPermission:4:add")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateGroup([FromBody] CreateDeviceGroupRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    code = "400",
                    msg = "请求参数验证失败",
                    data = ModelState
                });
            }

            var groupId = await _deviceGroupService.CreateGroupAsync(request);

            _logger.LogInformation("分组创建成功: {GroupName}", request.Name);

            return Ok(new
            {
                code = "0000",
                msg = "创建成功",
                data = new { id = groupId }
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "创建分组失败: {Message}", ex.Message);
            return BadRequest(new
            {
                code = "400",
                msg = ex.Message,
                data = (object?)null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建分组失败");
            return StatusCode(500, new
            {
                code = "500",
                msg = "创建分组失败",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// 更新分组
    /// </summary>
    /// <param name="id">分组ID</param>
    /// <param name="request">更新分组请求</param>
    /// <returns>无返回内容</returns>
    [HttpPut("{id}")]
    [Authorize(Policy = "ButtonPermission:4:edit")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateGroup(Guid id, [FromBody] UpdateDeviceGroupRequest request)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    code = "400",
                    msg = "请求参数验证失败",
                    data = ModelState
                });
            }

            await _deviceGroupService.UpdateGroupAsync(id, request);

            _logger.LogInformation("分组更新成功: {GroupId}", id);

            return Ok(new
            {
                code = "0000",
                msg = "更新成功",
                data = (object?)null
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "更新分组失败: {Message}", ex.Message);
            
            if (ex.Message.Contains("不存在"))
            {
                return NotFound(new
                {
                    code = "404",
                    msg = ex.Message,
                    data = (object?)null
                });
            }

            return BadRequest(new
            {
                code = "400",
                msg = ex.Message,
                data = (object?)null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新分组失败, GroupId: {GroupId}", id);
            return StatusCode(500, new
            {
                code = "500",
                msg = "更新分组失败",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// 删除分组（级联删除子分组，设备的分组ID置空）
    /// </summary>
    /// <param name="id">分组ID</param>
    /// <returns>无返回内容</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "ButtonPermission:4:delete")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteGroup(Guid id)
    {
        try
        {
            await _deviceGroupService.DeleteGroupAsync(id);

            _logger.LogInformation("分组删除成功: {GroupId}", id);

            return Ok(new
            {
                code = "0000",
                msg = "删除成功",
                data = (object?)null
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "删除分组失败: {Message}", ex.Message);
            return NotFound(new
            {
                code = "404",
                msg = ex.Message,
                data = (object?)null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除分组失败, GroupId: {GroupId}", id);
            return StatusCode(500, new
            {
                code = "500",
                msg = "删除分组失败",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// 移动分组（更改父级或排序）
    /// </summary>
    /// <param name="id">分组ID</param>
    /// <param name="request">移动分组请求</param>
    /// <returns>无返回内容</returns>
    [HttpPut("{id}/move")]
    [Authorize(Policy = "ButtonPermission:4:edit")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MoveGroup(Guid id, [FromBody] MoveDeviceGroupRequest request)
    {
        try
        {
            await _deviceGroupService.MoveGroupAsync(id, request);

            _logger.LogInformation("分组移动成功: {GroupId}", id);

            return Ok(new
            {
                code = "0000",
                msg = "移动成功",
                data = (object?)null
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "移动分组失败: {Message}", ex.Message);

            if (ex.Message.Contains("不存在"))
            {
                return NotFound(new
                {
                    code = "404",
                    msg = ex.Message,
                    data = (object?)null
                });
            }

            return BadRequest(new
            {
                code = "400",
                msg = ex.Message,
                data = (object?)null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "移动分组失败, GroupId: {GroupId}", id);
            return StatusCode(500, new
            {
                code = "500",
                msg = "移动分组失败",
                data = (object?)null
            });
        }
    }
}
