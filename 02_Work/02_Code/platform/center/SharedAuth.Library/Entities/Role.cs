using System.ComponentModel.DataAnnotations;

namespace Shared.Domain.Entities;

/// <summary>
/// 角色实体 - 系统角色权限模型
/// </summary>
/// <remarks>
/// 继承自 BaseEntity，自动包含审计字段和多租户支持
/// </remarks>
public class Role : BaseEntity
{
    /// <summary>
    /// 角色标识 - 全局唯一标识符
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// 角色名称 - 角色显示名称，最大长度100字符
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 角色编码 - 角色唯一编码标识，最大长度100字符
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// 角色描述 - 角色功能说明，最大长度500字符
    /// </summary>
    [MaxLength(500)]
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// 角色状态 - 1:启用 2:禁用
    /// </summary>
    public int Status { get; set; } = 1;
}

/// <summary>
/// 用户角色关联 - 用户与角色的多对多关系模型
/// </summary>
/// <remarks>
/// 不继承 BaseEntity，因为这是纯关联表，不需要审计字段
/// </remarks>
public class UserRole
{
    /// <summary>
    /// 用户标识 - 关联的用户ID
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// 角色标识 - 关联的角色ID
    /// </summary>
    public Guid RoleId { get; set; }
    
    /// <summary>
    /// 用户导航属性
    /// </summary>
    public User? User { get; set; }
    
    /// <summary>
    /// 角色导航属性
    /// </summary>
    public Role? Role { get; set; }
}
