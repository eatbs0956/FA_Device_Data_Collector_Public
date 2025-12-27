using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Shared.Domain.Entities;

/// <summary>
/// 菜单实体 - 系统菜单导航模型
/// </summary>
/// <remarks>
/// 继承自 BaseEntity，自动包含审计字段和多租户支持
/// </remarks>
public class Menu : BaseEntity
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
    [Required]
    [MaxLength(100)]
    public string MenuName { get; set; } = string.Empty;

    /// <summary>
    /// 路由名称 - 前端路由名称，最大长度100字符
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string RouteName { get; set; } = string.Empty;

    /// <summary>
    /// 路由路径 - 菜单对应的前端路由地址，最大长度200字符
    /// </summary>
    [Required]
    [MaxLength(200)]
    public string RoutePath { get; set; } = string.Empty;

    /// <summary>
    /// 组件路径 - 前端组件路径，最大长度200字符
    /// </summary>
    [Required]
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
    /// 固定标签页索引 - 标签页固定位置的顺序索引（null表示不固定）
    /// </summary>
    public int? FixedIndexInTab { get; set; }

    /// <summary>
    /// 查询参数 - 路由查询参数JSON
    /// </summary>
    [MaxLength(500)]
    public string? Query { get; set; }

    /// <summary>
    /// 按钮权限配置 - 菜单关联的按钮权限JSON数组
    /// </summary>
    /// <remarks>
    /// 存储格式: [{"code":"add","desc":"新增"},{"code":"edit","desc":"编辑"}]
    /// - code: 按钮权限编码，用于控制按钮显示和接口访问
    /// - desc: 按钮权限描述
    /// </remarks>
    [MaxLength(2000)]
    public string? Buttons { get; set; }

    /// <summary>
    /// 子菜单列表 - 树形结构的子节点集合（不映射到数据库）
    /// </summary>
    [NotMapped]
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public List<Menu>? Children { get; set; }
}

/// <summary>
/// 角色菜单关联实体 - 定义角色与菜单的多对多关系
/// </summary>
/// <remarks>
/// 用于实现基于角色的菜单访问控制(RBAC)
/// 不继承 BaseEntity，因为这是纯关联表
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
    /// 角色导航属性
    /// </summary>
    public Role? Role { get; set; }
    
    /// <summary>
    /// 菜单导航属性
    /// </summary>
    public Menu? Menu { get; set; }
}

/// <summary>
/// 角色按钮权限关联实体 - 定义角色与按钮权限的多对多关系
/// </summary>
/// <remarks>
/// 用于实现基于角色的按钮权限控制
/// ButtonCode 格式: "menu_id:button_code" 例如: "18:add", "18:edit"
/// 不继承 BaseEntity，因为这是纯关联表
/// </remarks>
public class RoleButton
{
    /// <summary>
    /// 角色ID - 外键关联到 Roles 表
    /// </summary>
    public Guid RoleId { get; set; }
    
    /// <summary>
    /// 按钮权限编码 - 格式: "menu_id:button_code"
    /// </summary>
    /// <example>
    /// "18:add" - 菜单18的新增按钮权限
    /// "18:edit" - 菜单18的编辑按钮权限
    /// "19:delete" - 菜单19的删除按钮权限
    /// </example>
    [Required]
    [MaxLength(200)]
    public string ButtonCode { get; set; } = string.Empty;
    
    /// <summary>
    /// 角色导航属性
    /// </summary>
    public Role? Role { get; set; }
}
