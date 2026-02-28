namespace Auth.Api.Contracts;

/// <summary>
/// 响应状态码 - 系统统一响应代码定义
/// </summary>
public static class Codes
{
    /// <summary>
    /// 成功状态码 - 操作成功的标准返回码
    /// </summary>
    public const string Success = "0000";
    
    /// <summary>
    /// 通用登出码 - 通用用户登出状态码
    /// </summary>
    public const string Logout = "8888"; // generic logout
    
    /// <summary>
    /// 模态登出码 - 模态窗口登出状态码
    /// </summary>
    public const string ModalLogout = "7777";
    
    /// <summary>
    /// 令牌过期码 - JWT令牌过期状态码
    /// </summary>
    public const string TokenExpired = "9999";
}

/// <summary>
/// 响应信封 - 统一API响应格式包装器
/// </summary>
/// <typeparam name="T">数据类型 - 响应数据的泛型类型</typeparam>
/// <param name="code">状态码 - 响应状态编码</param>
/// <param name="msg">消息内容 - 响应描述信息</param>
/// <param name="data">响应数据 - 实际返回的业务数据</param>
public record Envelope<T>(string code, string msg, T data)
{
    /// <summary>
    /// 成功响应 - 创建成功状态的响应信封
    /// </summary>
    /// <param name="data">响应数据 - 成功时返回的业务数据</param>
    /// <param name="msg">成功消息 - 成功状态描述，默认为"success"</param>
    /// <returns>成功响应信封</returns>
    public static Envelope<T> Ok(T data, string msg = "success") => new(Codes.Success, msg, data);
    
    /// <summary>
    /// 失败响应 - 创建失败状态的响应信封
    /// </summary>
    /// <param name="code">错误码 - 失败状态编码</param>
    /// <param name="msg">错误消息 - 失败状态描述</param>
    /// <returns>失败响应信封</returns>
    public static Envelope<T> Fail(string code, string msg) => new(code, msg, default!);
}

/// <summary>
/// 登录请求 - 用户登录凭据数据传输对象
/// </summary>
/// <param name="userName">用户名 - 登录用户名</param>
/// <param name="password">密码 - 登录密码明文</param>
public record LoginRequest(string userName, string password);

/// <summary>
/// 登录令牌 - 认证成功后返回的令牌数据传输对象
/// </summary>
/// <param name="token">访问令牌 - JWT访问令牌</param>
/// <param name="refreshToken">刷新令牌 - JWT刷新令牌</param>
public record LoginToken(string token, string refreshToken);

/// <summary>
/// 刷新请求 - 令牌刷新请求数据传输对象
/// </summary>
/// <param name="refreshToken">刷新令牌 - 用于获取新访问令牌的刷新令牌</param>
public record RefreshRequest(string refreshToken);

/// <summary>
/// 用户信息 - 用户基础信息和权限数据传输对象
/// </summary>
/// <param name="userId">用户标识 - 用户唯一标识符</param>
/// <param name="userName">用户名 - 用户登录名</param>
/// <param name="roles">角色列表 - 用户拥有的角色权限数组</param>
/// <param name="buttons">按钮权限 - 用户可访问的按钮权限数组</param>
public record UserInfo(string userId, string userName, string[] roles, string[] buttons);

/// <summary>
/// 菜单数据传输对象 - 用于菜单的创建和更新请求
/// </summary>
public record MenuDto
{
    /// <summary>
    /// 菜单ID - 菜单唯一标识符（更新时使用）
    /// </summary>
    public int? Id { get; set; }

    /// <summary>
    /// 菜单类型 - 1:目录 2:菜单
    /// </summary>
    public int MenuType { get; set; } = 2;

    /// <summary>
    /// 菜单名称 - 菜单显示名称
    /// </summary>
    public string MenuName { get; set; } = string.Empty;

    /// <summary>
    /// 路由名称 - 前端路由名称
    /// </summary>
    public string RouteName { get; set; } = string.Empty;

    /// <summary>
    /// 路由路径 - 前端路由路径
    /// </summary>
    public string RoutePath { get; set; } = string.Empty;

    /// <summary>
    /// 组件路径 - 前端组件路径
    /// </summary>
    public string Component { get; set; } = string.Empty;

    /// <summary>
    /// 国际化键 - i18n国际化键名
    /// </summary>
    public string? I18nKey { get; set; }

    /// <summary>
    /// 菜单图标 - 图标名称
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// 图标类型 - 1:Iconify图标 2:本地图标
    /// </summary>
    public string IconType { get; set; } = "1";

    /// <summary>
    /// 父级菜单ID - 上级菜单标识
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    /// 排序序号 - 显示顺序
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
    public string? Query { get; set; }

    /// <summary>
    /// 按钮权限配置 - 菜单关联的按钮权限JSON数组
    /// </summary>
    /// <remarks>
    /// 格式: [{"code":"add","desc":"新增"},{"code":"edit","desc":"编辑"}]
    /// </remarks>
    public string? Buttons { get; set; }
    
    /// <summary>
    /// 创建人ID - 审计字段
    /// </summary>
    public Guid? CreatedBy { get; set; }
    
    /// <summary>
    /// 更新人ID - 审计字段
    /// </summary>
    public Guid? UpdatedBy { get; set; }
    
    /// <summary>
    /// 创建时间 - 审计字段
    /// </summary>
    public DateTimeOffset? CreatedAt { get; set; }
    
    /// <summary>
    /// 更新时间 - 审计字段
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }
}

/// <summary>
/// 菜单响应对象 - 菜单详细信息响应
/// </summary>
public record MenuResponse
{
    /// <summary>
    /// 菜单ID - 菜单唯一标识符
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 菜单类型 - "1":目录 "2":菜单
    /// </summary>
    public string MenuType { get; set; } = "2";

    /// <summary>
    /// 菜单名称 - 菜单显示名称
    /// </summary>
    public string MenuName { get; set; } = string.Empty;

    /// <summary>
    /// 路由名称 - 前端路由名称
    /// </summary>
    public string RouteName { get; set; } = string.Empty;

    /// <summary>
    /// 路由路径 - 前端路由路径
    /// </summary>
    public string RoutePath { get; set; } = string.Empty;

    /// <summary>
    /// 组件路径 - 前端组件路径
    /// </summary>
    public string Component { get; set; } = string.Empty;

    /// <summary>
    /// 国际化键 - i18n国际化键名
    /// </summary>
    public string? I18nKey { get; set; }

    /// <summary>
    /// 菜单图标 - 图标名称
    /// </summary>
    public string Icon { get; set; } = string.Empty;

    /// <summary>
    /// 图标类型 - 1:Iconify图标 2:本地图标
    /// </summary>
    public string IconType { get; set; } = "1";

    /// <summary>
    /// 父级菜单ID - 上级菜单标识
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    /// 排序序号 - 显示顺序
    /// </summary>
    public int Order { get; set; }

    /// <summary>
    /// 菜单状态 - 1:启用 2:禁用
    /// </summary>
    public int Status { get; set; }

    /// <summary>
    /// 是否隐藏 - 是否在菜单中隐藏
    /// </summary>
    public bool HideInMenu { get; set; }

    /// <summary>
    /// 激活菜单键 - 当前激活的菜单键
    /// </summary>
    public string? ActiveMenu { get; set; }

    /// <summary>
    /// 多标签页 - 是否支持多标签页
    /// </summary>
    public bool MultiTab { get; set; }

    /// <summary>
    /// 固定标签页索引 - 标签页固定位置的顺序索引（null表示不固定）
    /// </summary>
    public int? FixedIndexInTab { get; set; }

    /// <summary>
    /// 查询参数 - 路由查询参数JSON
    /// </summary>
    public string? Query { get; set; }

    /// <summary>
    /// 按钮权限配置 - 菜单关联的按钮权限JSON数组
    /// </summary>
    /// <remarks>
    /// 格式: [{"code":"add","desc":"新增"},{"code":"edit","desc":"编辑"}]
    /// </remarks>
    public string? Buttons { get; set; }

    /// <summary>
    /// 创建人ID - 审计字段
    /// </summary>
    public Guid? CreatedBy { get; set; }
    
    /// <summary>
    /// 更新人ID - 审计字段
    /// </summary>
    public Guid? UpdatedBy { get; set; }

    /// <summary>
    /// 创建时间 - 记录创建时间
    /// </summary>
    public DateTimeOffset? CreatedAt { get; set; }

    /// <summary>
    /// 更新时间 - 记录更新时间
    /// </summary>
    public DateTimeOffset? UpdatedAt { get; set; }

    /// <summary>
    /// 子菜单列表 - 树形结构的子节点集合
    /// </summary>
    [System.Text.Json.Serialization.JsonIgnore(Condition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull)]
    public List<MenuResponse>? Children { get; set; }
}

/// <summary>
/// 菜单树响应对象 - 用于角色权限配置的菜单树结构
/// </summary>
public record MenuTreeDto
{
    /// <summary>
    /// 菜单ID - 菜单唯一标识符
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 菜单名称 - 显示在树形控件中的标签
    /// </summary>
    public string Label { get; set; } = string.Empty;

    /// <summary>
    /// 父级菜单ID - 父节点标识，0表示顶级节点
    /// </summary>
    public int PId { get; set; }

    /// <summary>
    /// 子菜单列表 - 下级菜单节点集合
    /// </summary>
    public List<MenuTreeDto>? Children { get; set; }
}

/// <summary>
/// 分页请求参数 - 通用分页查询参数
/// </summary>
public record PageRequest
{
    /// <summary>
    /// 当前页码 - 从1开始
    /// </summary>
    public int Current { get; set; } = 1;

    /// <summary>
    /// 每页大小 - 每页记录数
    /// </summary>
    public int Size { get; set; } = 10;
}

/// <summary>
/// 分页响应数据 - 通用分页响应格式
/// </summary>
/// <typeparam name="T">数据类型 - 分页数据的泛型类型</typeparam>
public record PageResponse<T>
{
    /// <summary>
    /// 记录列表 - 当前页的数据记录
    /// </summary>
    public List<T> Records { get; set; } = new();

    /// <summary>
    /// 当前页码 - 当前页数
    /// </summary>
    public int Current { get; set; }

    /// <summary>
    /// 每页大小 - 每页记录数
    /// </summary>
    public int Size { get; set; }

    /// <summary>
    /// 总记录数 - 数据总数
    /// </summary>
    public int Total { get; set; }
}

/// <summary>
/// 角色创建请求 - 创建新角色的请求数据
/// </summary>
public record RoleCreateRequest
{
    /// <summary>
    /// 角色名称 - 角色显示名称
    /// </summary>
    public string RoleName { get; set; } = string.Empty;

    /// <summary>
    /// 角色编码 - 角色唯一编码标识
    /// </summary>
    public string RoleCode { get; set; } = string.Empty;

    /// <summary>
    /// 角色描述 - 角色功能说明
    /// </summary>
    public string? RoleDesc { get; set; }

    /// <summary>
    /// 角色状态 - 1:启用 2:禁用
    /// </summary>
    public int? Status { get; set; }
}

/// <summary>
/// 角色更新请求 - 更新角色信息的请求数据
/// </summary>
public record RoleUpdateRequest
{
    /// <summary>
    /// 角色名称 - 角色显示名称
    /// </summary>
    public string RoleName { get; set; } = string.Empty;

    /// <summary>
    /// 角色编码 - 角色唯一编码标识
    /// </summary>
    public string RoleCode { get; set; } = string.Empty;

    /// <summary>
    /// 角色描述 - 角色功能说明
    /// </summary>
    public string? RoleDesc { get; set; }

    /// <summary>
    /// 角色状态 - 1:启用 2:禁用
    /// </summary>
    public int? Status { get; set; }
}

/// <summary>
/// 用户创建请求 - 创建新用户的请求数据
/// </summary>
public record UserCreateRequest
{
    /// <summary>
    /// 用户名 - 登录用户名
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 用户昵称 - 显示名称
    /// </summary>
    public string? NickName { get; set; }

    /// <summary>
    /// 用户性别 - 1:男 2:女
    /// </summary>
    public int? UserGender { get; set; }

    /// <summary>
    /// 用户手机 - 联系电话
    /// </summary>
    public string? UserPhone { get; set; }

    /// <summary>
    /// 用户邮箱 - 电子邮箱地址
    /// </summary>
    public string? UserEmail { get; set; }

    /// <summary>
    /// 用户状态 - 1:启用 2:禁用
    /// </summary>
    public int? Status { get; set; }

    /// <summary>
    /// 用户角色 - 角色编码数组
    /// </summary>
    public string[]? UserRoles { get; set; }
    
    /// <summary>
    /// 用户密码 - 登录密码（可选，不提供则使用默认密码）
    /// </summary>
    public string? Password { get; set; }
}

/// <summary>
/// 用户更新请求 - 更新用户信息的请求数据
/// </summary>
public record UserUpdateRequest
{
    /// <summary>
    /// 用户名 - 登录用户名
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 用户昵称 - 显示名称
    /// </summary>
    public string? NickName { get; set; }

    /// <summary>
    /// 用户类型 - user:交互账号, service:服务账号
    /// </summary>
    public string? UserType { get; set; }

    /// <summary>
    /// 用户性别 - 1:男 2:女
    /// </summary>
    public int? UserGender { get; set; }

    /// <summary>
    /// 用户手机 - 联系电话
    /// </summary>
    public string? UserPhone { get; set; }

    /// <summary>
    /// 用户邮箱 - 电子邮箱地址
    /// </summary>
    public string? UserEmail { get; set; }

    /// <summary>
    /// 用户状态 - 1:启用 2:禁用
    /// </summary>
    public int? Status { get; set; }

    /// <summary>
    /// 用户角色 - 角色编码数组
    /// </summary>
    public string[]? UserRoles { get; set; }
    
    /// <summary>
    /// 用户密码 - 登录密码（可选，留空则不修改密码）
    /// </summary>
    public string? Password { get; set; }
}


/// <summary>
/// 保存角色菜单权限请求 - 更新角色的菜单授权
/// </summary>
public record SaveRoleMenusRequest
{
    /// <summary>
    /// 菜单ID列表 - 授权给角色的菜单ID数组
    /// </summary>
    public List<int> MenuIds { get; set; } = new();
}

/// <summary>
/// 保存角色按钮权限请求 - 更新角色的按钮权限授权
/// </summary>
public record SaveRoleButtonsRequest
{
    /// <summary>
    /// 按钮权限编码列表 - 授权给角色的按钮权限数组
    /// </summary>
    /// <remarks>
    /// 格式: ["menu_id:button_code"]
    /// 示例: ["1001:add", "1001:edit", "1002:delete"]
    /// </remarks>
    public List<string> ButtonCodes { get; set; } = new();
}