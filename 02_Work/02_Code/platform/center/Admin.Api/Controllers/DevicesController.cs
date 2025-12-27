using Admin.Api.Domain.DTOs;
using Admin.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Entities;

namespace Admin.Api.Controllers;

/// <summary>
/// 设备管理控制器 - 完整的设备CRUD和连接测试功能
/// </summary>
/// <remarks>
/// 设备列表画面的菜单ID为3，包含以下按钮权限：
/// - 3:select - 查询权限
/// - 3:add - 新增权限
/// - 3:edit - 编辑权限
/// - 3:delete - 删除权限
/// 
/// 权限验证流程：
/// 1. 前端携带JWT Token访问Admin.Api
/// 2. Admin.Api验证JWT签名（通过Auth.Api的JWKS公钥）
/// 3. 提取JWT中的用户ID（动态获取当前登录用户）
/// 4. CrossServiceButtonPermissionHandler连接Auth数据库查询：
///    a. 用户所属的角色（sys_user_role）
///    b. 角色绑定的按钮权限（sys_role_button）
/// 5. 验证用户是否拥有指定的按钮权限编码（如 3:select）
/// </remarks>
[ApiController]
[Route("api/devices")]
[Authorize]
public class DevicesController : ControllerBase
{
    private readonly IDeviceService _deviceService;
    private readonly DbContext _dbContext;
    private readonly ILogger<DevicesController> _logger;

    public DevicesController(
        IDeviceService deviceService,
        DbContext dbContext,
        ILogger<DevicesController> logger)
    {
        _deviceService = deviceService;
        _dbContext = dbContext;
        _logger = logger;
    }

    /// <summary>
    /// 获取设备列表（分页）
    /// </summary>
    /// <param name="request">查询条件</param>
    /// <returns>设备列表</returns>
    [HttpGet]
    [Authorize(Policy = "ButtonPermission:3:select")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDevices([FromQuery] DeviceQueryRequest request)
    {
        try
        {
            var result = await _deviceService.GetDevicesAsync(request);
            return Ok(new
            {
                code = "0000",
                msg = "查询成功",
                data = result
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "查询设备列表失败");
            return StatusCode(500, new
            {
                code = "500",
                msg = "查询设备列表失败",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// 获取设备列表（轻量级，用于标签管理左侧设备树）
    /// </summary>
    /// <param name="deviceName">设备名称（可选）</param>
    /// <param name="enabled">启用状态（可选）</param>
    /// <returns>简化的设备列表</returns>
    [HttpGet("for-tags")]
    [Authorize(Policy = "ButtonPermission:5:select")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDevicesForTags([FromQuery] string? deviceName, [FromQuery] bool? enabled)
    {
        try
        {
            var query = _dbContext.Set<Device>()
                .Where(d => !d.DeletedFlag)
                .AsQueryable();

            // 只选择必要的字段，不关联其他表
            if (!string.IsNullOrWhiteSpace(deviceName))
            {
                query = query.Where(d => EF.Functions.ILike(d.DeviceName, $"%{deviceName}%"));
            }

            if (enabled.HasValue)
            {
                query = query.Where(d => d.Enabled == enabled.Value);
            }

            var devices = await query
                .OrderBy(d => d.DeviceName)
                .Select(d => new
                {
                    d.Id,
                    d.DeviceId,
                    d.DeviceName,
                    d.ProtocolType,
                    d.Enabled,
                    TagCount = d.TagDefinitions != null ? d.TagDefinitions.Count : 0
                })
                .ToListAsync();

            return Ok(new
            {
                code = "0000",
                msg = "查询成功",
                data = new
                {
                    records = devices,
                    total = devices.Count
                }
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "查询设备列表失败");
            return StatusCode(500, new
            {
                code = "500",
                msg = "查询设备列表失败",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// 获取设备详情
    /// </summary>
    /// <param name="id">设备ID</param>
    /// <returns>设备详情</returns>
    [HttpGet("{id}")]
    [Authorize(Policy = "ButtonPermission:3:select")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetDevice(Guid id)
    {
        try
        {
            var device = await _deviceService.GetDeviceByIdAsync(id);
            if (device == null)
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
                data = device
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "查询设备详情失败, DeviceId: {DeviceId}", id);
            return StatusCode(500, new
            {
                code = "500",
                msg = "查询设备详情失败",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// 新增设备
    /// </summary>
    /// <param name="request">创建设备请求</param>
    /// <returns>创建的设备ID</returns>
    [HttpPost]
    [Authorize(Policy = "ButtonPermission:3:add")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateDevice([FromBody] CreateDeviceRequest request)
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

            var deviceId = await _deviceService.CreateDeviceAsync(request);
            
            _logger.LogInformation("设备创建成功: {DeviceId}", request.DeviceId);
            
            return Ok(new
            {
                code = "0000",
                msg = "创建成功",
                data = new { id = deviceId }
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "创建设备失败: {Message}", ex.Message);
            return BadRequest(new
            {
                code = "400",
                msg = ex.Message,
                data = (object?)null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "创建设备失败");
            return StatusCode(500, new
            {
                code = "500",
                msg = "创建设备失败",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// 更新设备
    /// </summary>
    /// <param name="id">设备ID</param>
    /// <param name="request">更新设备请求</param>
    /// <returns>无返回内容</returns>
    [HttpPut("{id}")]
    [Authorize(Policy = "ButtonPermission:3:edit")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateDevice(Guid id, [FromBody] UpdateDeviceRequest request)
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

            await _deviceService.UpdateDeviceAsync(id, request);
            
            _logger.LogInformation("设备更新成功: {DeviceId}", id);
            
            return Ok(new
            {
                code = "0000",
                msg = "更新成功",
                data = (object?)null
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "更新设备失败: {Message}", ex.Message);
            return NotFound(new
            {
                code = "404",
                msg = ex.Message,
                data = (object?)null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新设备失败, DeviceId: {DeviceId}", id);
            return StatusCode(500, new
            {
                code = "500",
                msg = "更新设备失败",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// 删除设备（软删除）
    /// </summary>
    /// <param name="id">设备ID</param>
    /// <returns>无返回内容</returns>
    [HttpDelete("{id}")]
    [Authorize(Policy = "ButtonPermission:3:delete")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteDevice(Guid id)
    {
        try
        {
            await _deviceService.DeleteDeviceAsync(id);
            
            _logger.LogInformation("设备删除成功: {DeviceId}", id);
            
            return Ok(new
            {
                code = "0000",
                msg = "删除成功",
                data = (object?)null
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "删除设备失败: {Message}", ex.Message);
            return NotFound(new
            {
                code = "404",
                msg = ex.Message,
                data = (object?)null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "删除设备失败, DeviceId: {DeviceId}", id);
            return StatusCode(500, new
            {
                code = "500",
                msg = "删除设备失败",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// 批量删除设备（软删除）
    /// </summary>
    /// <param name="ids">设备ID列表</param>
    /// <returns>无返回内容</returns>
    [HttpPost("batch-delete")]
    [Authorize(Policy = "ButtonPermission:3:delete")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public async Task<IActionResult> BatchDelete([FromBody] List<Guid> ids)
    {
        try
        {
            if (ids == null || ids.Count == 0)
            {
                return BadRequest(new
                {
                    code = "400",
                    msg = "设备ID列表不能为空",
                    data = (object?)null
                });
            }

            await _deviceService.BatchDeleteAsync(ids);
            
            _logger.LogInformation("批量删除设备成功, 数量: {Count}", ids.Count);
            
            return Ok(new
            {
                code = "0000",
                msg = $"成功删除 {ids.Count} 个设备",
                data = (object?)null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "批量删除设备失败");
            return StatusCode(500, new
            {
                code = "500",
                msg = "批量删除设备失败",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// 测试设备连接
    /// </summary>
    /// <param name="id">设备ID</param>
    /// <returns>连接测试结果</returns>
    [HttpPost("{id}/test-connection")]
    [Authorize(Policy = "ButtonPermission:3:edit")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> TestConnection(Guid id)
    {
        try
        {
            var result = await _deviceService.TestConnectionAsync(id);
            
            _logger.LogInformation("设备连接测试完成: {DeviceId}, 结果: {Success}", id, result.Success);
            
            return Ok(new
            {
                code = "0000",
                msg = result.Success ? "连接成功" : "连接失败",
                data = result
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "设备连接测试失败: {Message}", ex.Message);
            return NotFound(new
            {
                code = "404",
                msg = ex.Message,
                data = (object?)null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "设备连接测试失败, DeviceId: {DeviceId}", id);
            return StatusCode(500, new
            {
                code = "500",
                msg = "连接测试失败",
                data = (object?)null
            });
        }
    }

    /// <summary>
    /// 启用/禁用设备
    /// </summary>
    /// <param name="id">设备ID</param>
    /// <param name="enabled">是否启用</param>
    /// <returns>无返回内容</returns>
    [HttpPatch("{id}/enabled")]
    [Authorize(Policy = "ButtonPermission:3:edit")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(object), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ToggleEnabled(Guid id, [FromBody] bool enabled)
    {
        try
        {
            await _deviceService.ToggleEnabledAsync(id, enabled);
            
            _logger.LogInformation("设备状态切换成功: {DeviceId}, Enabled: {Enabled}", id, enabled);
            
            return Ok(new
            {
                code = "0000",
                msg = enabled ? "设备已启用" : "设备已禁用",
                data = (object?)null
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "切换设备状态失败: {Message}", ex.Message);
            return NotFound(new
            {
                code = "404",
                msg = ex.Message,
                data = (object?)null
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "切换设备状态失败, DeviceId: {DeviceId}", id);
            return StatusCode(500, new
            {
                code = "500",
                msg = "切换设备状态失败",
                data = (object?)null
            });
        }
    }
}
