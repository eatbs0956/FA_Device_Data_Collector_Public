using Admin.Api.Domain.DTOs;
using Admin.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Admin.Api.Controllers;

/// <summary>
/// 标签管理控制器 - 完整的标签CRUD和批量操作功能
/// </summary>
/// <remarks>
/// 设备标签画面的菜单ID为5，包含以下按钮权限：
/// - 5:select - 查询权限
/// - 5:add - 新增权限
/// - 5:edit - 编辑权限
/// - 5:delete - 删除权限
/// </remarks>
[ApiController]
[Route("api/tags")]
[Authorize]
public class TagsController : ControllerBase
{
    private readonly ITagService _tagService;
    private readonly ILogger<TagsController> _logger;

    public TagsController(
        ITagService tagService,
        ILogger<TagsController> logger)
    {
        _tagService = tagService;
        _logger = logger;
    }

    /// <summary>
    /// 获取标签列表（分页）
    /// </summary>
    /// <param name="request">查询条件</param>
    /// <returns>标签列表</returns>
    [HttpGet]
    [Authorize(Policy = "ButtonPermission:5:select")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTags([FromQuery] TagQueryRequest request)
    {
        try
        {
            var result = await _tagService.GetTagsAsync(request);
            return Ok(new
            {
                code = "0000",
                msg = "查询成功",
                data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "查询标签列表失败");
            return StatusCode(500, new
            {
                code = "500",
                msg = "查询标签列表失败",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// 获取标签详情
    /// </summary>
    /// <param name="id">标签ID</param>
    /// <returns>标签详情</returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "ButtonPermission:5:select")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTag(Guid id)
    {
        try
        {
            var tag = await _tagService.GetTagByIdAsync(id);
            if (tag == null)
            {
                return NotFound(new
                {
                    code = "404",
                    msg = "标签不存在",
                    data = (object?)null
                });
            }

            return Ok(new
            {
                code = "0000",
                msg = "查询成功",
                data = tag
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "查询标签详情失败, TagId: {TagId}", id);
            return StatusCode(500, new
            {
                code = "500",
                msg = "查询标签详情失败",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// 新增标签
    /// </summary>
    /// <param name="request">创建标签请求</param>
    /// <returns>创建的标签ID</returns>
    [HttpPost]
    [Authorize(Policy = "ButtonPermission:5:add")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateTag([FromBody] CreateTagRequest request)
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

            var tagId = await _tagService.CreateTagAsync(request);
            
            _logger.LogInformation("标签创建成功: {TagId}", request.TagId);
            
            return Ok(new
            {
                code = "0000",
                msg = "创建成功",
                data = new { id = tagId }
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "创建标签失败: {Message}", ex.Message);
            return BadRequest(new
            {
                code = "400",
                msg = ex.Message,
                data = (object?)null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建标签失败");
            return StatusCode(500, new
            {
                code = "500",
                msg = "创建标签失败",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// 更新标签
    /// </summary>
    /// <param name="id">标签ID</param>
    /// <param name="request">更新标签请求</param>
    /// <returns>操作结果</returns>
    [HttpPut("{id}")]
    [Authorize(Policy = "ButtonPermission:5:edit")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTag(Guid id, [FromBody] UpdateTagRequest request)
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

            await _tagService.UpdateTagAsync(id, request);
            
            _logger.LogInformation("标签更新成功: {TagId}", id);
            
            return Ok(new
            {
                code = "0000",
                msg = "更新成功",
                data = (object?)null
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "更新标签失败: {Message}", ex.Message);
            return BadRequest(new
            {
                code = "400",
                msg = ex.Message,
                data = (object?)null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新标签失败, TagId: {TagId}", id);
            return StatusCode(500, new
            {
                code = "500",
                msg = "更新标签失败",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// 删除标签（软删除）
    /// </summary>
    /// <param name="id">标签ID</param>
    /// <returns>操作结果</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "ButtonPermission:5:delete")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteTag(Guid id)
    {
        try
        {
            await _tagService.DeleteTagAsync(id);
            
            _logger.LogInformation("标签删除成功: {TagId}", id);
            
            return Ok(new
            {
                code = "0000",
                msg = "删除成功",
                data = (object?)null
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "删除标签失败: {Message}", ex.Message);
            return NotFound(new
            {
                code = "404",
                msg = ex.Message,
                data = (object?)null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除标签失败, TagId: {TagId}", id);
            return StatusCode(500, new
            {
                code = "500",
                msg = "删除标签失败",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// 批量删除标签
    /// </summary>
    /// <param name="ids">标签ID列表</param>
    /// <returns>操作结果</returns>
    [HttpPost("batch-delete")]
    [Authorize(Policy = "ButtonPermission:5:delete")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BatchDelete([FromBody] List<Guid> ids)
    {
        try
        {
            await _tagService.BatchDeleteAsync(ids);
            
            _logger.LogInformation("批量删除标签成功, 数量: {Count}", ids.Count);
            
            return Ok(new
            {
                code = "0000",
                msg = "批量删除成功",
                data = (object?)null
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                code = "400",
                msg = ex.Message,
                data = (object?)null
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
            _logger.LogError(ex, "批量删除标签失败");
            return StatusCode(500, new
            {
                code = "500",
                msg = "批量删除标签失败",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// 启用/禁用标签
    /// </summary>
    /// <param name="id">标签ID</param>
    /// <param name="request">启用状态</param>
    /// <returns>操作结果</returns>
    [HttpPut("{id}/toggle-enabled")]
    [Authorize(Policy = "ButtonPermission:5:edit")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleEnabled(Guid id, [FromBody] ToggleEnabledRequest request)
    {
        try
        {
            await _tagService.ToggleEnabledAsync(id, request.Enabled);
            
            var status = request.Enabled ? "启用" : "禁用";
            _logger.LogInformation("标签{Status}成功: {TagId}", status, id);
            
            return Ok(new
            {
                code = "0000",
                msg = $"{status}成功",
                data = (object?)null
            });
        }
        catch (InvalidOperationException ex)
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
            _logger.LogError(ex, "切换标签状态失败, TagId: {TagId}", id);
            return StatusCode(500, new
            {
                code = "500",
                msg = "操作失败",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// 批量启用标签
    /// </summary>
    /// <param name="ids">标签ID列表</param>
    /// <returns>操作结果</returns>
    [HttpPost("batch-enable")]
    [Authorize(Policy = "ButtonPermission:5:edit")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BatchEnable([FromBody] List<Guid> ids)
    {
        try
        {
            await _tagService.BatchToggleEnabledAsync(ids, true);
            
            _logger.LogInformation("批量启用标签成功, 数量: {Count}", ids.Count);
            
            return Ok(new
            {
                code = "0000",
                msg = "批量启用成功",
                data = (object?)null
            });
        }
        catch (ArgumentException ex)
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
            _logger.LogError(ex, "批量启用标签失败");
            return StatusCode(500, new
            {
                code = "500",
                msg = "批量启用失败",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// 批量禁用标签
    /// </summary>
    /// <param name="ids">标签ID列表</param>
    /// <returns>操作结果</returns>
    [HttpPost("batch-disable")]
    [Authorize(Policy = "ButtonPermission:5:edit")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> BatchDisable([FromBody] List<Guid> ids)
    {
        try
        {
            await _tagService.BatchToggleEnabledAsync(ids, false);
            
            _logger.LogInformation("批量禁用标签成功, 数量: {Count}", ids.Count);
            
            return Ok(new
            {
                code = "0000",
                msg = "批量禁用成功",
                data = (object?)null
            });
        }
        catch (ArgumentException ex)
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
            _logger.LogError(ex, "批量禁用标签失败");
            return StatusCode(500, new
            {
                code = "500",
                msg = "批量禁用失败",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// 导出设备的所有标签
    /// </summary>
    /// <param name="deviceId">设备ID</param>
    /// <returns>标签列表</returns>
    [HttpGet("export/{deviceId}")]
    [Authorize(Policy = "ButtonPermission:5:select")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> ExportTags(Guid deviceId)
    {
        try
        {
            var tags = await _tagService.GetTagsByDeviceIdAsync(deviceId);
            
            return Ok(new
            {
                code = "0000",
                msg = "导出成功",
                data = tags
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "导出标签失败, DeviceId: {DeviceId}", deviceId);
            return StatusCode(500, new
            {
                code = "500",
                msg = "导出标签失败",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// 批量导入标签
    /// </summary>
    /// <param name="deviceId">设备ID</param>
    /// <param name="tags">标签列表</param>
    /// <returns>导入数量</returns>
    [HttpPost("import/{deviceId}")]
    [Authorize(Policy = "ButtonPermission:5:add")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ImportTags(Guid deviceId, [FromBody] List<CreateTagRequest> tags)
    {
        try
        {
            var importCount = await _tagService.BatchImportAsync(deviceId, tags);
            
            _logger.LogInformation("批量导入标签成功, DeviceId: {DeviceId}, 数量: {Count}", deviceId, importCount);
            
            return Ok(new
            {
                code = "0000",
                msg = $"成功导入 {importCount} 个标签",
                data = new { importCount }
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                code = "400",
                msg = ex.Message,
                data = (object?)null
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
            _logger.LogError(ex, "批量导入标签失败, DeviceId: {DeviceId}", deviceId);
            return StatusCode(500, new
            {
                code = "500",
                msg = "批量导入标签失败",
                data = (object?)null
            });
        }
    }
}

/// <summary>
/// 启用/禁用请求
/// </summary>
public class ToggleEnabledRequest
{
    /// <summary>
    /// 是否启用
    /// </summary>
    public bool Enabled { get; set; }
}
