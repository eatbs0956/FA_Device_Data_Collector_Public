using Shared.Domain.Entities;

namespace Admin.Api.Services;

/// <summary>
/// 服务账号验证器接口
/// </summary>
/// <remarks>
/// 用于验证服务账号对边缘节点的操作权限
/// </remarks>
public interface IServiceAccountValidator
{
    /// <summary>
    /// 验证当前用户是否有权限操作指定的边缘节点
    /// </summary>
    /// <param name="nodeId">边缘节点的业务标识（node_id字段）</param>
    /// <returns>
    /// - 如果有权限，返回 (true, null)
    /// - 如果无权限，返回 (false, 错误消息)
    /// </returns>
    /// <remarks>
    /// 权限规则：
    /// 1. 如果当前用户是交互账号(user_type=user)，直接允许（由其他权限机制控制）
    /// 2. 如果当前用户是服务账号(user_type=service)：
    ///    - 如果节点已绑定该服务账号，允许
    ///    - 如果节点未绑定任何服务账号，允许（兼容模式）
    ///    - 如果节点绑定了其他服务账号，拒绝
    /// </remarks>
    Task<(bool IsAllowed, string? ErrorMessage)> ValidateNodeAccessAsync(string nodeId);

    /// <summary>
    /// 验证当前用户是否有权限操作指定的边缘节点（通过ID）
    /// </summary>
    /// <param name="edgeNodeId">边缘节点的主键ID</param>
    /// <returns>验证结果</returns>
    Task<(bool IsAllowed, string? ErrorMessage)> ValidateNodeAccessByIdAsync(Guid edgeNodeId);
}
