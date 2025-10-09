using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Auth.Api.Models;

/// <summary>
/// 用户实体 - 系统用户基础信息模型
/// </summary>
public class User
{
    /// <summary>
    /// 用户标识 - 全局唯一标识符
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// 用户名称 - 登录用户名，最大长度100字符
    /// </summary>
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
    /// 创建时间 - 记录创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// 更新时间 - 记录更新时间
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// 用户角色 - 用户关联的角色权限集合
    /// </summary>
    public List<UserRole> UserRoles { get; set; } = new();
}

/// <summary>
/// 角色实体 - 系统角色权限模型
/// </summary>
public class Role
{
    /// <summary>
    /// 角色标识 - 全局唯一标识符
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// 角色名称 - 角色显示名称，最大长度100字符
    /// </summary>
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// 角色编码 - 角色唯一编码标识，最大长度100字符
    /// </summary>
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
    
    /// <summary>
    /// 创建时间 - 记录创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// 更新时间 - 记录更新时间
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}

/// <summary>
/// 用户角色关联 - 用户与角色的多对多关系模型
/// </summary>
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
}

/// <summary>
/// 刷新令牌 - JWT刷新令牌存储实体
/// </summary>
public class RefreshToken
{
    /// <summary>
    /// 令牌标识 - 全局唯一标识符
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();
    
    /// <summary>
    /// 用户标识 - 令牌关联的用户ID
    /// </summary>
    public Guid UserId { get; set; }
    
    /// <summary>
    /// 令牌值 - 刷新令牌字符串，最大长度256字符
    /// </summary>
    [MaxLength(256)]
    public string Token { get; set; } = string.Empty;
    
    /// <summary>
    /// 过期时间 - 令牌失效的时间戳
    /// </summary>
    public DateTimeOffset ExpiresAt { get; set; }
    
    /// <summary>
    /// 创建时间 - 令牌创建的时间戳
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// 撤销状态 - 令牌是否已被撤销
    /// </summary>
    public bool Revoked { get; set; }
}

/// <summary>
/// 菜单实体 - 系统菜单导航模型
/// </summary>
public class Menu
{
    /// <summary>
    /// 菜单标识 - 手动管理的整型主键（非自增）
    /// </summary>
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; set; }

    /// <summary>
    /// 菜单类型 - 1:目录 2:菜单
    /// </summary>
    public int MenuType { get; set; } = 2;

    /// <summary>
    /// 菜单名称 - 菜单显示名称，最大长度100字符
    /// </summary>
    [MaxLength(100)]
    public string MenuName { get; set; } = string.Empty;

    /// <summary>
    /// 路由名称 - 前端路由名称，最大长度100字符
    /// </summary>
    [MaxLength(100)]
    public string RouteName { get; set; } = string.Empty;

    /// <summary>
    /// 路由路径 - 菜单对应的前端路由地址，最大长度200字符
    /// </summary>
    [MaxLength(200)]
    public string RoutePath { get; set; } = string.Empty;

    /// <summary>
    /// 组件路径 - 前端组件路径，最大长度200字符
    /// </summary>
    [MaxLength(200)]
    public string Component { get; set; } = string.Empty;

    /// <summary>
    /// 国际化键 - i18n国际化键名，最大长度100字符
    /// </summary>
    [MaxLength(100)]
    public string? I18nKey { get; set; }

    /// <summary>
    /// 菜单图标 - 菜单显示图标名称，最大长度100字符
    /// </summary>
    [MaxLength(100)]
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// 图标类型 - 1:Iconify图标 2:本地图标
    /// </summary>
    [MaxLength(10)]
    public string IconType { get; set; } = "1";

    /// <summary>
    /// 父级菜单 - 上级菜单ID，用于构建菜单层级结构
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    /// 排序序号 - 菜单在同级中的显示顺序
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// 菜单状态 - 1:启用 2:禁用
    /// </summary>
    public int Status { get; set; } = 1;

    /// <summary>
    /// 是否隐藏 - 是否在菜单中隐藏
    /// </summary>
    public bool HideInMenu { get; set; } = false;

    /// <summary>
    /// 激活菜单键 - 当前激活的菜单键
    /// </summary>
    [MaxLength(100)]
    public string? ActiveMenu { get; set; }

    /// <summary>
    /// 多标签页 - 是否支持多标签页
    /// </summary>
    public bool MultiTab { get; set; } = true;

    /// <summary>
    /// 固定标签页 - 标签页是否固定
    /// </summary>
    public bool FixedIndexInTab { get; set; } = false;

    /// <summary>
    /// 查询参数 - 路由查询参数JSON
    /// </summary>
    [MaxLength(500)]
    public string? Query { get; set; }

    /// <summary>
    /// 创建时间 - 记录创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// 更新时间 - 记录更新时间
    /// </summary>
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// 子菜单列表 - 树形结构的子节点集合（不映射到数据库）
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public List<Menu>? Children { get; set; }
}

/// <summary>
/// 角色菜单关联实体 - 定义角色与菜单的多对多关系
/// </summary>
/// <remarks>
/// 用于实现基于角色的菜单访问控制(RBAC)
/// - 一个角色可以关联多个菜单
/// - 一个菜单可以被多个角色访问
/// - 联合主键: (RoleId, MenuId)
/// </remarks>
public class RoleMenu
{
    /// <summary>
    /// 角色ID - 外键关联到 Roles 表
    /// </summary>
    public Guid RoleId { get; set; }
    
    /// <summary>
    /// 菜单ID - 外键关联到 Menus 表
    /// </summary>
    public int MenuId { get; set; }
    
    /// <summary>
    /// 角色导航属性 - EF Core 导航属性
    /// </summary>
    public Role? Role { get; set; }
    
    /// <summary>
    /// 菜单导航属性 - EF Core 导航属性
    /// </summary>
    public Menu? Menu { get; set; }
}
