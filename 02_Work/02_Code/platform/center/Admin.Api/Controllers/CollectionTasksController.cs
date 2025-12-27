using Admin.Api.Domain.DTOs;
using Admin.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Api.Controllers;

/// <summary>
/// 采集任务控制器 - 完整的采集任务CRUD功能
/// </summary>
/// <remarks>
/// 采集任务管理画面的菜单ID为7，包含以下按钮权限：
/// - 7:select - 查询权限
/// - 7:add - 新增权限
/// - 7:edit - 编辑权限
/// - 7:delete - 删除权限
/// </remarks>
[ApiController]
[Route("api/collection-tasks")]
[Authorize]
public class CollectionTasksController : ControllerBase
{
    private readonly ICollectionTaskService _taskService;
    private readonly ILogger<CollectionTasksController> _logger;

    public CollectionTasksController(
        ICollectionTaskService taskService,
        ILogger<CollectionTasksController> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    /// <summary>
    /// 获取采集任务列表（分页）
    /// </summary>
    /// <param name="request">查询参数</param>
    /// <returns>采集任务列表</returns>
    [HttpGet]
    [Authorize(Policy = "ButtonPermission:7:select")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTasks([FromQuery] CollectionTaskQueryRequest request)
    {
        try
        {
            var result = await _taskService.GetTasksAsync(request);

            return Ok(new
            {
                code = "0000",
                msg = "查询成功",
                data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "查询采集任务列表失败");
            return StatusCode(500, new
            {
                code = "500",
                msg = "查询采集任务列表失败",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// 获取采集任务详情
    /// </summary>
    /// <param name="id">任务ID</param>
    /// <returns>任务详情</returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "ButtonPermission:7:select")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTask(Guid id)
    {
        try
        {
            var task = await _taskService.GetTaskByIdAsync(id);

            if (task == null)
            {
                return NotFound(new
                {
                    code = "404",
                    msg = "采集任务不存在",
                    data = (object?)null
                });
            }

            return Ok(new
            {
                code = "0000",
                msg = "查询成功",
                data = task
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "查询采集任务详情失败: {Id}", id);
            return StatusCode(500, new
            {
                code = "500",
                msg = "查询采集任务详情失败",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// 创建采集任务
    /// </summary>
    /// <param name="request">创建请求</param>
    /// <returns>创建的任务ID</returns>
    [HttpPost]
    [Authorize(Policy = "ButtonPermission:7:add")]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateTask([FromBody] CreateCollectionTaskRequest request)
    {
        try
        {
            var id = await _taskService.CreateTaskAsync(request);

            return Created($"/api/collection-tasks/{id}", new
            {
                code = "0000",
                msg = "创建成功",
                data = new { id = id.ToString() }
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "创建采集任务失败: 业务校验错误");
            return BadRequest(new
            {
                code = "400",
                msg = ex.Message,
                data = (object?)null
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "创建采集任务失败: 参数错误");
            return BadRequest(new
            {
                code = "400",
                msg = ex.Message,
                data = (object?)null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建采集任务失败");
            return StatusCode(500, new
            {
                code = "500",
                msg = "创建采集任务失败",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// 更新采集任务
    /// </summary>
    /// <param name="id">任务ID</param>
    /// <param name="request">更新请求</param>
    /// <returns>更新结果</returns>
    [HttpPut("{id}")]
    [Authorize(Policy = "ButtonPermission:7:edit")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> UpdateTask(Guid id, [FromBody] UpdateCollectionTaskRequest request)
    {
        try
        {
            await _taskService.UpdateTaskAsync(id, request);

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
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "更新采集任务失败: 业务校验错误");
            return BadRequest(new
            {
                code = "400",
                msg = ex.Message,
                data = (object?)null
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "更新采集任务失败: 参数错误");
            return BadRequest(new
            {
                code = "400",
                msg = ex.Message,
                data = (object?)null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新采集任务失败: {Id}", id);
            return StatusCode(500, new
            {
                code = "500",
                msg = "更新采集任务失败",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// 删除采集任务
    /// </summary>
    /// <param name="id">任务ID</param>
    /// <returns>删除结果</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "ButtonPermission:7:delete")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteTask(Guid id)
    {
        try
        {
            await _taskService.DeleteTaskAsync(id);

            return Ok(new
            {
                code = "0000",
                msg = "删除成功",
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
            _logger.LogError(ex, "删除采集任务失败: {Id}", id);
            return StatusCode(500, new
            {
                code = "500",
                msg = "删除采集任务失败",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// 变更任务状态
    /// </summary>
    /// <param name="id">任务ID</param>
    /// <param name="request">状态变更请求</param>
    /// <returns>变更结果</returns>
    [HttpPut("{id}/status")]
    [Authorize(Policy = "ButtonPermission:7:edit")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] TaskStatusChangeRequest request)
    {
        try
        {
            await _taskService.ChangeStatusAsync(id, request.Status);

            return Ok(new
            {
                code = "0000",
                msg = "状态变更成功",
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
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "变更任务状态失败: 业务校验错误");
            return BadRequest(new
            {
                code = "400",
                msg = ex.Message,
                data = (object?)null
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "变更任务状态失败: 参数错误");
            return BadRequest(new
            {
                code = "400",
                msg = ex.Message,
                data = (object?)null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "变更任务状态失败: {Id}", id);
            return StatusCode(500, new
            {
                code = "500",
                msg = "变更任务状态失败",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// 获取可用设备列表（用于任务关联）
    /// </summary>
    /// <returns>可用设备列表</returns>
    [HttpGet("available-devices")]
    [Authorize(Policy = "ButtonPermission:7:select")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAvailableDevices()
    {
        try
        {
            var devices = await _taskService.GetAvailableDevicesAsync();

            return Ok(new
            {
                code = "0000",
                msg = "查询成功",
                data = devices
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "查询可用设备列表失败");
            return StatusCode(500, new
            {
                code = "500",
                msg = "查询可用设备列表失败",
                data = (object?)null
            });
        }
    }
}
