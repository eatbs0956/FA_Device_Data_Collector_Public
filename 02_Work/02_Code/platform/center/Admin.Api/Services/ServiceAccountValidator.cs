using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Entities;

namespace Admin.Api.Services;

/// <summary>
/// 服务账号验证器实现
/// </summary>
public class ServiceAccountValidator : IServiceAccountValidator
{
    private readonly DbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<ServiceAccountValidator> _logger;

    public ServiceAccountValidator(
        DbContext dbContext,
        IHttpContextAccessor httpContextAccessor,
        ILogger<ServiceAccountValidator> logger)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    /// <summary>
    /// 获取当前用户ID
    /// </summary>
    private Guid? GetCurrentUserId()
    {
        // 尝试从 sub claim 获取（标准JWT格式）
        var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst("sub")?.Value;
        
        // 如果 sub 不存在，尝试从 NameIdentifier 获取（ASP.NET Identity 格式）
        if (string.IsNullOrEmpty(userIdClaim))
        {
            userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }
        
        return Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    /// <summary>
    /// 获取当前用户类型
    /// </summary>
    private string? GetCurrentUserType()
    {
        // 从 JWT token 中获取 user_type claim
        return _httpContextAccessor.HttpContext?.User.FindFirst("user_type")?.Value;
    }

    public async Task<(bool IsAllowed, string? ErrorMessage)> ValidateNodeAccessAsync(string nodeId)
    {
        var currentUserId = GetCurrentUserId();
        
        // 未认证用户 - 拒绝（如果启用了服务账号验证）
        if (currentUserId == null)
        {
            _logger.LogWarning("未认证用户尝试访问节点: {NodeId}", nodeId);
            return (false, "未认证");
        }

        var userType = GetCurrentUserType();
        
        // 交互账号 - 由其他权限机制控制（如菜单/按钮权限）
        if (userType != "service")
        {
            return (true, null);
        }

        // 服务账号 - 检查是否有权限操作该节点
        var edgeNode = await _dbContext.Set<EdgeNode>()
            .FirstOrDefaultAsync(e => e.NodeId == nodeId && !e.DeletedFlag);

        if (edgeNode == null)
        {
            // 节点不存在 - 对于注册场景，允许创建；其他场景由业务层处理
            return (true, null);
        }

        return ValidateNodeAccessInternal(edgeNode, currentUserId.Value);
    }

    public async Task<(bool IsAllowed, string? ErrorMessage)> ValidateNodeAccessByIdAsync(Guid edgeNodeId)
    {
        var currentUserId = GetCurrentUserId();
        
        if (currentUserId == null)
        {
            _logger.LogWarning("未认证用户尝试访问节点: {EdgeNodeId}", edgeNodeId);
            return (false, "未认证");
        }

        var userType = GetCurrentUserType();
        
        // 交互账号 - 由其他权限机制控制
        if (userType != "service")
        {
            return (true, null);
        }

        var edgeNode = await _dbContext.Set<EdgeNode>()
            .FirstOrDefaultAsync(e => e.Id == edgeNodeId && !e.DeletedFlag);

        if (edgeNode == null)
        {
            return (false, "边缘节点不存在");
        }

        return ValidateNodeAccessInternal(edgeNode, currentUserId.Value);
    }

    /// <summary>
    /// 内部验证逻辑
    /// </summary>
    private (bool IsAllowed, string? ErrorMessage) ValidateNodeAccessInternal(EdgeNode edgeNode, Guid currentUserId)
    {
        // 如果节点未绑定任何服务账号，允许访问（兼容模式）
        if (!edgeNode.ServiceUserId.HasValue)
        {
            _logger.LogDebug("节点 {NodeId} 未绑定服务账号，允许服务账号 {UserId} 访问（兼容模式）",
                edgeNode.NodeId, currentUserId);
            return (true, null);
        }

        // 如果节点绑定的是当前服务账号，允许访问
        if (edgeNode.ServiceUserId.Value == currentUserId)
        {
            _logger.LogDebug("服务账号 {UserId} 有权限访问节点 {NodeId}",
                currentUserId, edgeNode.NodeId);
            return (true, null);
        }

        // 节点绑定的是其他服务账号，拒绝访问
        _logger.LogWarning("服务账号 {UserId} 无权访问节点 {NodeId}，该节点绑定到服务账号 {BoundUserId}",
            currentUserId, edgeNode.NodeId, edgeNode.ServiceUserId.Value);
        return (false, $"无权访问节点 {edgeNode.NodeId}，该节点已绑定到其他服务账号");
    }
}
