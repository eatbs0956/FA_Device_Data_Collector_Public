using System.ComponentModel.DataAnnotations;

namespace Shared.Domain.Entities;

/// <summary>
/// 用户实体 - 系统用户基础信息模型
/// </summary>
/// <remarks>
/// 继承自 BaseEntity，自动包含审计字段和多租户支持
/// </remarks>
public class User : BaseEntity
{
    /// <summary>
    /// 用户标识 - 全局唯一标识符
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 用户名称 - 登录用户名，最大长度100字符
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 用户昵称 - 显示名称，最大长度100字符
    /// </summary>
    [MaxLength(100)]
    public string NickName { get; set; } = string.Empty;

    /// <summary>
    /// 用户性别 - 1:男 2:女
    /// </summary>
    public int? Gender { get; set; }

    /// <summary>
    /// 电子邮箱 - 用户邮箱地址，最大长度256字符
    /// </summary>
    [MaxLength(256)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 手机号码 - 用户联系电话，最大长度20字符
    /// </summary>
    [MaxLength(20)]
    public string Phone { get; set; } = string.Empty;

    /// <summary>
    /// 密码哈希 - 加密后的密码存储，最大长度512字符
    /// </summary>
    [Required]
    [MaxLength(512)]
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>
    /// 密码更新时间 - 最近一次密码修改的时间戳
    /// </summary>
    public DateTimeOffset PasswordUpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// 用户状态 - 1:启用 2:禁用
    /// </summary>
    public int Status { get; set; } = 1;

    /// <summary>
    /// 启用状态 - 用户账户是否处于激活状态（兼容性字段）
    /// </summary>
    public bool Enabled { get; set; } = true;

    /// <summary>
    /// 用户角色导航属性 - 用户关联的角色权限集合
    /// </summary>
    public List<UserRole> UserRoles { get; set; } = new();
}
