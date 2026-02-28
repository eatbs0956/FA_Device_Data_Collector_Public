/// <summary>
/// Auth.Api 的入口程序，配置并启动基于 JWT 的认证与授权服务，包含用户、角色、菜单的管理接口。
/// </summary>
/// <remarks>
/// 主要功能：
/// 1. 配置数据库连接（PostgreSQL），自动迁移并初始化角色和管理员用户。
/// 2. 配置 JWT（RS256）认证，支持访问令牌与刷新令牌，容忍时钟偏差，支持角色授权策略。
/// 3. 提供健康检查接口（/health）、JWKS 公钥接口（/.well-known/jwks.json）。
/// 4. 提供认证相关接口：登录（/auth/login）、获取用户信息（/auth/getUserInfo）、刷新令牌（/auth/refreshToken）、注册（/auth/register）。
/// 5. 提供管理员接口：用户、角色、菜单的增删改查，以及用户角色、角色菜单的分配关系管理。
/// 6. 所有接口均采用统一 Envelope 响应结构，便于前端处理。
/// 7. 认证中间件需在路由映射前启用，确保安全性。
/// </remarks>
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Auth.Api.Authorization;
using Auth.Api.Contracts;
using Auth.Api.Middlewares;
using Auth.Api.Services;
using Auth.Api.Services.Abstractions;
using Shared.Domain.Data;
using Shared.Domain.Entities;
using Shared.Domain.Seeders;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using EFCore.NamingConventions;

// 创建 ASP.NET Core Web 应用程序构建器
var builder = WebApplication.CreateBuilder(args);

// Config - 配置参数
// PostgreSQL 数据库连接字符串 - 从环境变量获取或使用默认值
var pgConn = Environment.GetEnvironmentVariable("PG_CONN") ?? "Host=localhost;Username=devdcp;Password=devdcp;Database=devdcp";
// 访问令牌最短有效期（小时）- 确保即使接近午夜登录也有足够的使用时间
var minAccessHours = 2;
// 刷新令牌有效期（天）- 用于重新获取访问令牌的刷新令牌生存时间
var refreshDays = 7;
// 密码过期天数 - 用户密码强制过期的天数，0表示不过期
var passwordExpireDays = int.TryParse(Environment.GetEnvironmentVariable("PASSWORD_EXPIRE_DAYS"), out var d) ? d : 0; // 0=disabled

// 计算访问令牌有效期 - 到次日0点过期，但至少保证minAccessHours小时
TimeSpan CalculateAccessTokenExpiry()
{
	var now = DateTimeOffset.UtcNow;
	var nextMidnight = now.Date.AddDays(1); // 次日0点 UTC
	var timeUntilMidnight = nextMidnight - now.DateTime;
	
	// 如果距离次日0点不足最短有效期，则使用最短有效期
	if (timeUntilMidnight.TotalHours < minAccessHours)
	{
		return TimeSpan.FromHours(minAccessHours);
	}
	
	return timeUntilMidnight;
}

// Services - 服务注册
// 注册认证数据库上下文 - 配置 PostgreSQL 连接
builder.Services.AddDbContext<UnifiedDbContext>(opt => 
    opt.UseNpgsql(pgConn)
       .UseSnakeCaseNamingConvention()); 
// 注册 HttpContextAccessor - 用于在服务中访问当前HTTP上下文
builder.Services.AddHttpContextAccessor();
// 注册密码服务 - 处理密码哈希、验证和历史记录
builder.Services.AddScoped<IPasswordService, PasswordService>();
// 注册令牌服务 - 使用 DbContext，必须是作用域生命周期（非单例）
builder.Services.AddScoped<ITokenService, TokenService>();
// 注册菜单服务 - 处理菜单管理的CRUD操作
builder.Services.AddScoped<IMenuService, MenuService>();
// 注册角色服务 - 处理角色管理的CRUD操作
builder.Services.AddScoped<IRoleService, RoleService>();
// 注册用户服务 - 处理用户管理的CRUD操作
builder.Services.AddScoped<IUserService, UserService>();
// 兼容性注册 - 保留具体类型注册以确保现有代码兼容
builder.Services.AddScoped<PasswordService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<MenuService>();
builder.Services.AddScoped<RoleService>();
builder.Services.AddScoped<UserService>();
// 注册刷新令牌清理服务 - 后台定时清理过期和已撤销的刷新令牌
builder.Services.AddHostedService<RefreshTokenCleanupService>();
// 注册异步审计服务 - 后台批量处理审计日志写入
builder.Services.AddSingleton<AuditService>();
builder.Services.AddSingleton<IAuditService>(sp => sp.GetRequiredService<AuditService>());
builder.Services.AddHostedService(sp => sp.GetRequiredService<AuditService>());
// 注册端点 API 浏览器 - 支持 OpenAPI/Swagger 文档生成
builder.Services.AddEndpointsApiExplorer();

// 从 TokenService 中解析签名密钥（在作用域内）
using (var tmpSp = builder.Services.BuildServiceProvider())
using (var scope = tmpSp.CreateScope())
{
	// 临时令牌服务实例 - 用于确保 RSA 密钥已生成
	var tmpTokenSvc = scope.ServiceProvider.GetRequiredService<TokenService>();
	tmpTokenSvc.EnsureKey();
}
// 从 TokenService 的静态持有者中获取密钥实例
var signingKey = new TokenService(null!).GetSecurityKey();

// JWT 验证参数配置（RS256 算法）
var tokenValidationParameters = new TokenValidationParameters
{
	ValidateIssuer = true,
	ValidIssuer = "devdcp.auth",
	ValidateAudience = true,
	ValidAudience = "devdcp.portal",
	ValidateIssuerSigningKey = true,
	IssuerSigningKey = signingKey,
	ValidateLifetime = true,
	// 容忍 2 分钟的时钟偏差，避免偶发 NotYetValid/Expired 报错
	ClockSkew = TimeSpan.FromMinutes(2),
	RoleClaimType = "role"
};

// 避免 "sub" 被映射为 ClaimTypes.NameIdentifier
JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
	.AddJwtBearer(options =>
	{
		options.TokenValidationParameters = tokenValidationParameters;
		// 关闭映射，避免 "sub" 被转换为 NameIdentifier
        options.MapInboundClaims = false;
		// JWT Bearer 事件处理器配置
		options.Events = new JwtBearerEvents
		{
			// 消息接收处理 - 从 Authorization 头中提取 Bearer 令牌
			OnMessageReceived = ctx =>
			{
				// 授权头信息 - 获取请求头中的 Authorization 字段
				var auth = ctx.Request.Headers["Authorization"].FirstOrDefault();
				if (!string.IsNullOrEmpty(auth) && auth.StartsWith("Bearer "))
				{
					ctx.Token = auth["Bearer ".Length..];
				}
				return Task.CompletedTask;
			},
			// 令牌验证成功处理 - 记录验证通过的用户标识
			OnTokenValidated = ctx =>
			{
				// 用户标识 - 从令牌声明中提取用户 ID
				var sub = ctx.Principal?.FindFirstValue(JwtRegisteredClaimNames.Sub) ?? ctx.Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
				Console.WriteLine($"[JwtBearer] Token validated. sub={sub}");
				return Task.CompletedTask;
			},
			// 认证失败处理 - 记录认证失败原因但不写入响应
			OnAuthenticationFailed = ctx =>
			{
				// 仅记录日志，不在此处写响应，避免非授权端点(如 /auth/refreshToken)发生重复写入
				// 失败原因 - 构造详细的错误信息用于调试
				var reason = ctx.Exception?.GetType().Name + ": " + ctx.Exception?.Message;
				Console.WriteLine($"[JwtBearer] Authentication failed: {reason}");
				return Task.CompletedTask;
			},
			// 质询处理 - 当需要认证但令牌无效时的响应处理
			OnChallenge = ctx =>
			{
				ctx.HandleResponse();
				Console.WriteLine("[JwtBearer] Challenge triggered (no/invalid token)");
				ctx.Response.StatusCode = 200;
				return ctx.Response.WriteAsJsonAsync(Envelope<object>.Fail(Codes.TokenExpired, "Unauthorized"));
			}
		};
	});

builder.Services.AddAuthorization(options =>
{
	// ========== 传统角色授权策略（兼容旧代码） ==========
	options.AddPolicy("AdminOnly", policy => policy.RequireRole("R_ADMIN"));
	options.AddPolicy("UserOnly", policy => policy.RequireRole("R_USER"));
	options.AddPolicy("SuperOnly", policy => policy.RequireRole("R_SUPER"));
	options.AddPolicy("AdminOrSuper", policy => policy.RequireRole("R_ADMIN", "R_SUPER"));
	
	// 注意：按钮权限策略已改为动态生成，无需在此预定义
	// 使用 DynamicButtonPermissionPolicyProvider 自动处理 "ButtonPermission:{buttonCode}" 格式的策略
});

// 注册动态按钮权限策略提供器 - 自动生成按钮权限策略，无需手动定义
builder.Services.AddSingleton<IAuthorizationPolicyProvider, DynamicButtonPermissionPolicyProvider>();

// 注册按钮权限授权处理器
builder.Services.AddScoped<IAuthorizationHandler, ButtonPermissionHandler>();

// 注册友好授权结果处理器 - 返回友好的中文错误消息
builder.Services.AddSingleton<IAuthorizationMiddlewareResultHandler, FriendlyAuthorizationMiddlewareResultHandler>();

// ========== 配置 CORS ==========
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// 构建 Web 应用程序实例
var app = builder.Build();

// 确保数据库初始化 - 在应用启动时进行数据库迁移和种子数据创建
using (var scope = app.Services.CreateScope())
{
	// 数据库上下文实例 - 用于执行数据库操作
	var db = scope.ServiceProvider.GetRequiredService<UnifiedDbContext>();
	var pwd = scope.ServiceProvider.GetRequiredService<PasswordService>();

	db.Database.Migrate();
	
	// 使用 DataSeeder 自动初始化数据
	DataSeeder.SeedData(db, pwd.Hash);
}

// 启用 CORS
app.UseCors("AllowAll");

// 启动认证中间件
app.UseAuthentication();

// 启动授权中间件（使用自定义的FriendlyAuthorizationMiddlewareResultHandler处理授权失败）
app.UseAuthorization();

// 启动审计中间件 - 自动记录系统操作日志
app.UseMiddleware<AuditMiddleware>();

app.MapGet("/health", () => Results.Ok(new { ok = true }));

// 提供 JWKS 公钥端点，供前端或其他服务获取公钥以验证 JWT 签名
app.MapGet("/.well-known/jwks.json", (TokenService tokenSvc) => Results.Json(tokenSvc.GetJwks()));

// Auth endpoints matching frontend
app.MapPost("/api/auth/login", async (LoginRequest req, UnifiedDbContext db, PasswordService pwd, TokenService tokenSvc) =>
{
	var user = await db.Users.FirstOrDefaultAsync(x => x.UserName == req.userName && x.Enabled);
	if (user is null || !pwd.Verify(req.password, user.PasswordHash))
	{
		return Results.Json(Envelope<object>.Fail("1001", "用户名或密码错误"));
	}

	if (passwordExpireDays > 0 && user.PasswordUpdatedAt.AddDays(passwordExpireDays) < DateTimeOffset.UtcNow)
	{
		return Results.Json(Envelope<object>.Fail(Codes.ModalLogout, "密码已过期，请重置"));
	}

	var accessExpiry = CalculateAccessTokenExpiry();
	var pair = await tokenSvc.IssueAsync(user, accessExpiry, TimeSpan.FromDays(refreshDays));
	return Results.Json(Envelope<LoginToken>.Ok(new LoginToken(pair.Token, pair.RefreshToken)));
});

app.MapGet("/api/auth/getUserInfo", async (HttpContext http, UnifiedDbContext db) =>
{
    // 兼容读取 sub/NameIdentifier
    var uid = http.User.FindFirstValue(JwtRegisteredClaimNames.Sub)
              ?? http.User.FindFirstValue(ClaimTypes.NameIdentifier)
              ?? http.User.FindFirstValue("sub");
    if (string.IsNullOrEmpty(uid))
    {
        return Results.Json(Envelope<object>.Fail(Codes.TokenExpired, "未授权"));
    }
    var userId = Guid.Parse(uid);
    
    // 检查Token的JTI是否有效（Session验证）
    var jti = http.User.FindFirstValue(JwtRegisteredClaimNames.Jti);
    if (!string.IsNullOrEmpty(jti))
    {
        var session = await db.Sessions.FirstOrDefaultAsync(s => 
            s.AccessTokenJti == jti && s.UserId == userId);
        
        if (session == null || session.Revoked || session.ExpiresAt < DateTimeOffset.UtcNow)
        {
            return Results.Json(Envelope<object>.Fail(Codes.TokenExpired, "会话已失效"));
        }
    }
    
    var user = await db.Users.FirstOrDefaultAsync(x => x.Id == userId);
    if (user is null) return Results.Json(Envelope<object>.Fail(Codes.Logout, "用户不存在"));
    var roles = await db.UserRoles.Where(x => x.UserId == user.Id)
        .Join(db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Code).ToArrayAsync();
    
    // 查询用户可访问的菜单路由(通过角色-菜单关系)
    var menuRoutes = await db.UserRoles
        .Where(ur => ur.UserId == user.Id)
        .Join(db.RoleMenus, ur => ur.RoleId, rm => rm.RoleId, (ur, rm) => rm.MenuId)
        .Distinct()
        .Join(db.Menus, menuId => menuId, m => m.Id, (menuId, m) => m.RouteName)
        .ToArrayAsync();
    
    // 查询用户可访问的按钮权限(通过角色-按钮关系)
    var buttonCodes = await db.UserRoles
        .Where(ur => ur.UserId == user.Id)
        .Join(db.RoleButtons, ur => ur.RoleId, rb => rb.RoleId, (ur, rb) => rb.ButtonCode)
        .Distinct()
        .ToArrayAsync();
    
    var info = new UserInfo(user.Id.ToString(), user.UserName, roles, buttonCodes);
    return Results.Json(Envelope<UserInfo>.Ok(info));
}).RequireAuthorization();

// 刷新令牌 - 使用刷新令牌获取新的访问令牌和刷新令牌
app.MapPost("/api/auth/refreshToken", async (RefreshRequest req, UnifiedDbContext db, TokenService tokenSvc) =>
{
	var user = await tokenSvc.ValidateRefreshAsync(req.refreshToken);
	if (user is null)
	{
		return Results.Json(Envelope<object>.Fail(Codes.Logout, "刷新令牌无效"));
	}
	var accessExpiry = CalculateAccessTokenExpiry();
	var pair = await tokenSvc.IssueAsync(user, accessExpiry, TimeSpan.FromDays(refreshDays));
	return Results.Json(Envelope<LoginToken>.Ok(new LoginToken(pair.Token, pair.RefreshToken)));
});

// 注册新用户 - 简单注册接口，创建新用户但不分配角色
app.MapPost("/api/auth/register", async (LoginRequest req, UnifiedDbContext db, PasswordService pwd) =>
{
	if (!pwd.ValidatePolicy(req.password, out var msg))
		return Results.Json(Envelope<object>.Fail("1002", msg));
	if (await db.Users.AnyAsync(x => x.UserName == req.userName))
		return Results.Json(Envelope<object>.Fail("1003", "用户名已存在"));
	var u = new User { UserName = req.userName };
	u.PasswordHash = pwd.Hash(req.password);
	db.Users.Add(u);
	await db.SaveChangesAsync();
	return Results.Json(Envelope<object>.Ok(new { userId = u.Id }));
});

// ========== 用户管理接口 ==========
// 获取用户列表（分页） - GET /api/systemManage/getUserList
// 需要权限：用户管理-查询（18:select）
app.MapGet("/api/systemManage/getUserList", async (
	UserService userSvc,
	int current = 1,
	int size = 10,
	string? userName = null,
	string? nickName = null,
	string? userPhone = null,
	string? userEmail = null,
	int? userGender = null,
	int? status = null) =>
{
	var (items, total) = await userSvc.GetUserListAsync(
		current, size, userName, nickName, userPhone, userEmail, userGender, status);
	
	return Results.Json(Envelope<object>.Ok(new
	{
		records = items,
		current,
		size,
		total
	}));
}).RequireButtonPermission("18:select");

// 创建用户 - POST /api/admin/users
// 需要权限：用户管理-新增（18:add）
app.MapPost("/api/admin/addUsers", async (UserCreateRequest req, UserService userSvc) =>
{
	try
	{
		var user = await userSvc.CreateUserAsync(
			userName: req.UserName,
			nickName: req.NickName ?? "",
			userType: "user", // 默认为普通用户
			userGender: req.UserGender,
			userPhone: req.UserPhone ?? "",
			userEmail: req.UserEmail ?? "",
			status: req.Status ?? 1,
			userRoles: req.UserRoles,
			password: req.Password);

		return Results.Json(Envelope<object>.Ok(new { id = user.Id }, "用户创建成功"));
	}
	catch (ArgumentException ex)
	{
		return Results.Json(Envelope<object>.Fail("400", ex.Message));
	}
	catch (InvalidOperationException ex)
	{
		return Results.Json(Envelope<object>.Fail("400", ex.Message));
	}
}).RequireButtonPermission("18:add");

// 更新用户 - PUT /api/admin/users/{id}
// 需要权限：用户管理-编辑（18:edit）
app.MapPut("/api/admin/updateUsers/{id:guid}", async (Guid id, UserUpdateRequest req, UserService userSvc) =>
{
	try
	{
		var success = await userSvc.UpdateUserAsync(
			id: id,
			userName: req.UserName,
			nickName: req.NickName ?? "",
			userType: req.UserType, // 支持更新用户类型
			userGender: req.UserGender,
			userPhone: req.UserPhone ?? "",
			userEmail: req.UserEmail ?? "",
			status: req.Status,
			userRoles: req.UserRoles,
			password: req.Password);

		if (!success)
		{
			return Results.Json(Envelope<object>.Fail("404", "用户不存在"));
		}

		return Results.Json(Envelope<object>.Ok(new { id }, "用户更新成功"));
	}
	catch (InvalidOperationException ex)
	{
		return Results.Json(Envelope<object>.Fail("400", ex.Message));
	}
}).RequireButtonPermission("18:edit");

// 删除用户 - DELETE /api/admin/users/{id}
// 需要权限：用户管理-删除（18:delete）
app.MapDelete("/api/admin/deleteUsers/{id:guid}", async (Guid id, UserService userSvc) =>
{
	var success = await userSvc.DeleteUserAsync(id);
	if (!success)
	{
		return Results.Json(Envelope<object>.Fail("404", "用户不存在"));
	}

	return Results.Json(Envelope<object>.Ok(new { id }, "用户删除成功"));
}).RequireButtonPermission("18:delete");

// ========== 角色管理接口 ==========
// 获取所有角色列表 - GET /api/systemManage/getAllRoles
// 用于下拉选择，Admin 和 Super 都可访问
app.MapGet("/api/systemManage/getAllRoles", async (UnifiedDbContext db) =>
{
	// 查询所有未删除且启用的角色并返回
	var roles = await db.Roles
		.Where(x => !x.DeletedFlag && x.Status == 1) // 排除已删除的角色,只返回启用的角色
		.OrderBy(x => x.Name)
		.Select(x => new { roleId = x.Id, roleName = x.Name, roleCode = x.Code, roleDesc = x.Description })
		.ToListAsync();
	
	return Results.Json(Envelope<object>.Ok(roles));
}).RequireAuthorization("AdminOrSuper");

// 获取角色列表（分页） - GET /api/systemManage/getRoleList
// 需要权限：角色管理-查询（19:select）
app.MapGet("/api/systemManage/getRoleList", async (
	RoleService roleSvc,
	int current = 1,
	int size = 10,
	string? roleName = null,
	string? roleCode = null,
	int? status = null) =>
{
	var (items, total) = await roleSvc.GetRoleListAsync(current, size, roleName, roleCode, status);
	
	return Results.Json(Envelope<object>.Ok(new
	{
		records = items,
		current,
		size,
		total
	}));
}).RequireButtonPermission("19:select");

// 创建角色 - POST /api/admin/addRoles
// 需要权限：角色管理-新增（19:add）
app.MapPost("/api/admin/addRoles", async (RoleCreateRequest req, RoleService roleSvc) =>
{
	try
	{
		var role = await roleSvc.CreateRoleAsync(req.RoleName, req.RoleCode, req.RoleDesc ?? "", req.Status ?? 1);
		return Results.Json(Envelope<object>.Ok(new { id = role.Id }, "角色创建成功"));
	}
	catch (ArgumentException ex)
	{
		return Results.Json(Envelope<object>.Fail("400", ex.Message));
	}
	catch (InvalidOperationException ex)
	{
		return Results.Json(Envelope<object>.Fail("400", ex.Message));
	}
}).RequireButtonPermission("19:add");

// 更新角色 - PUT /api/admin/updateRoles/{id}
// 需要权限：角色管理-编辑（19:edit）
app.MapPut("/api/admin/updateRoles/{id:guid}", async (Guid id, RoleUpdateRequest req, RoleService roleSvc) =>
{
	try
	{
		var success = await roleSvc.UpdateRoleAsync(id, req.RoleName, req.RoleCode, req.RoleDesc ?? "", req.Status);
		if (!success)
		{
			return Results.Json(Envelope<object>.Fail("404", "角色不存在"));
		}
		return Results.Json(Envelope<object>.Ok(new { id }, "角色更新成功"));
	}
	catch (InvalidOperationException ex)
	{
		return Results.Json(Envelope<object>.Fail("400", ex.Message));
	}
}).RequireButtonPermission("19:edit");

// 删除角色 - DELETE /api/admin/deleteRoles/{id}
// 需要权限：角色管理-删除（19:delete）
app.MapDelete("/api/admin/deleteRoles/{id:guid}", async (Guid id, RoleService roleSvc) =>
{
	try
	{
		var success = await roleSvc.DeleteRoleAsync(id);
		if (!success)
		{
			return Results.Json(Envelope<object>.Fail("404", "角色不存在"));
		}
		return Results.Json(Envelope<object>.Ok(new { id }, "角色删除成功"));
	}
	catch (InvalidOperationException ex)
	{
		return Results.Json(Envelope<object>.Fail("400", ex.Message));
	}
}).RequireButtonPermission("19:delete");

// ========== 菜单管理接口 ==========
// 获取菜单列表（分页，树形结构） - GET /api/admin/getMenuList?current=1&size=10
// 需要权限：菜单管理-查询（20:select）
app.MapGet("/api/admin/getMenuList", async (MenuService menuSvc, int current = 1, int size = 10) =>
{
	// 获取分页数据（包含树形结构）
	var (items, total) = await menuSvc.GetMenuListAsync(current, size);
	
	// 转换为响应对象（递归转换包含子菜单）
	var records = items.Select(m => ConvertToMenuResponse(m)).ToList();

	var response = new PageResponse<MenuResponse>
	{
		Records = records,
		Current = current,
		Size = size,
		Total = total
	};

	return Results.Json(Envelope<PageResponse<MenuResponse>>.Ok(response));
}).RequireButtonPermission("20:select");

// 获取单个菜单详情 - GET /api/admin/getMenus/{id}
app.MapGet("/api/admin/getMenus/{id:int}", async (int id, MenuService menuSvc) =>
{
	var menu = await menuSvc.GetMenuByIdAsync(id);
	if (menu is null)
		return Results.Json(Envelope<MenuResponse>.Fail("404", "菜单不存在"));

	var response = new MenuResponse
	{
		Id = menu.Id,
		MenuType = menu.MenuType.ToString(),
		MenuName = menu.MenuName,
		RouteName = menu.RouteName,
		RoutePath = menu.RoutePath,
		Component = menu.Component,
		I18nKey = menu.I18nKey,
		Icon = menu.Icon,
		IconType = menu.IconType,
		ParentId = menu.ParentId,
		Order = menu.Order,
		Status = menu.Status,
		HideInMenu = menu.HideInMenu,
		ActiveMenu = menu.ActiveMenu,
		MultiTab = menu.MultiTab,
		FixedIndexInTab = menu.FixedIndexInTab,
		Query = menu.Query,
		Buttons = menu.Buttons,
		CreatedAt = menu.CreatedAt,
		UpdatedAt = menu.UpdatedAt
	};

	return Results.Json(Envelope<MenuResponse>.Ok(response));
}).RequireAuthorization("SuperOnly");

// 创建菜单 - POST /api/admin/addMenus
// 需要权限：菜单管理-新增（20:add）
app.MapPost("/api/admin/addMenus", async (MenuDto dto, MenuService menuSvc) =>
{
	try
	{
		Console.WriteLine($"[CreateMenu] Received request: MenuType={dto.MenuType}, MenuName={dto.MenuName}, RouteName={dto.RouteName}");
		
		var menu = new Menu
		{
			MenuType = dto.MenuType,
			MenuName = dto.MenuName,
			RouteName = dto.RouteName,
			RoutePath = dto.RoutePath,
			Component = dto.Component,
			I18nKey = dto.I18nKey,
			Icon = dto.Icon,
			IconType = dto.IconType,
			ParentId = dto.ParentId,
			Order = dto.Order,
			Status = dto.Status,
			HideInMenu = dto.HideInMenu,
			ActiveMenu = dto.ActiveMenu,
			MultiTab = dto.MultiTab,
			FixedIndexInTab = dto.FixedIndexInTab,
			Query = dto.Query,
			Buttons = dto.Buttons
		};

		var created = await menuSvc.CreateMenuAsync(menu);

		Console.WriteLine($"[CreateMenu] Menu created successfully with Id={created.Id}");
		return Results.Json(Envelope<object>.Ok(new { id = created.Id }, "菜单创建成功"));
	}
	catch (Exception ex)
	{
		Console.WriteLine($"[CreateMenu] Error: {ex.GetType().Name} - {ex.Message}");
		Console.WriteLine($"[CreateMenu] StackTrace: {ex.StackTrace}");
		if (ex.InnerException != null)
		{
			Console.WriteLine($"[CreateMenu] InnerException: {ex.InnerException.Message}");
		}
		return Results.Json(Envelope<object>.Fail("5000", $"创建菜单失败: {ex.Message}"));
	}
}).RequireButtonPermission("20:add");

// 更新菜单 - PUT /api/admin/updateMenus/{id}
// 需要权限：菜单管理-编辑（20:edit）
app.MapPut("/api/admin/updateMenus/{id:int}", async (int id, MenuDto dto, MenuService menuSvc) =>
{
	var menu = new Menu
	{
		MenuType = dto.MenuType,
		MenuName = dto.MenuName,
		RouteName = dto.RouteName,
		RoutePath = dto.RoutePath,
		Component = dto.Component,
		I18nKey = dto.I18nKey,
		Icon = dto.Icon,
		IconType = dto.IconType,
		ParentId = dto.ParentId,
		Order = dto.Order,
		Status = dto.Status,
		HideInMenu = dto.HideInMenu,
		ActiveMenu = dto.ActiveMenu,
		MultiTab = dto.MultiTab,
		FixedIndexInTab = dto.FixedIndexInTab,
		Query = dto.Query,
		Buttons = dto.Buttons
	};

	var success = await menuSvc.UpdateMenuAsync(id, menu);
	if (!success)
		return Results.Json(Envelope<object>.Fail("404", "菜单不存在"));

	return Results.Json(Envelope<object>.Ok(new { id }, "菜单更新成功"));
}).RequireButtonPermission("20:edit");

// 删除菜单 - DELETE /api/admin/deleteMenus/{id}
// 需要权限：菜单管理-删除（20:delete）
app.MapDelete("/api/admin/deleteMenus/{id:int}", async (int id, MenuService menuSvc) =>
{
	try
	{
		var success = await menuSvc.DeleteMenuAsync(id);
		if (!success)
			return Results.Json(Envelope<object>.Fail("404", "菜单不存在"));

		return Results.Json(Envelope<object>.Ok(new { id }, "菜单删除成功"));
	}
	catch (InvalidOperationException ex)
	{
		return Results.Json(Envelope<object>.Fail("400", ex.Message));
	}
}).RequireButtonPermission("20:delete");

// 批量删除菜单 - DELETE /api/admin/deleteMenus/batch
// 需要权限：菜单管理-删除（20:delete）
app.MapDelete("/api/admin/deleteMenus/batch", async ([FromBody] List<int> ids, MenuService menuSvc) =>
{
	var count = await menuSvc.BatchDeleteMenusAsync(ids);
	return Results.Json(Envelope<object>.Ok(new { deletedCount = count }, $"成功删除 {count} 个菜单"));
}).RequireButtonPermission("20:delete");

// 获取所有页面列表 - GET /api/systemManage/getAllPages
app.MapGet("/api/systemManage/getAllPages", async (MenuService menuSvc) =>
{
	var pages = await menuSvc.GetAllPagesAsync();
	return Results.Json(Envelope<List<string>>.Ok(pages));
}).RequireAuthorization("SuperOnly");

// 获取菜单树 - GET /api/systemManage/getMenuTree
app.MapGet("/api/systemManage/getMenuTree", async (MenuService menuSvc) =>
{
	var menuTree = await menuSvc.GetMenuTreeAsync();
	return Results.Json(Envelope<List<MenuTreeDto>>.Ok(menuTree));
}).RequireAuthorization("SuperOnly");

// 获取角色的菜单权限 - GET /api/systemManage/getRoleMenus/{roleId}
app.MapGet("/api/systemManage/getRoleMenus/{roleId:guid}", async (Guid roleId, MenuService menuSvc) =>
{
	var menuIds = await menuSvc.GetRoleMenusAsync(roleId);
	return Results.Json(Envelope<List<int>>.Ok(menuIds));
}).RequireAuthorization("SuperOnly");

// 保存角色的菜单权限 - POST /api/systemManage/saveRoleMenus/{roleId}
app.MapPost("/api/systemManage/saveRoleMenus/{roleId:guid}", async (Guid roleId, SaveRoleMenusRequest req, MenuService menuSvc) =>
{
	var affectedRows = await menuSvc.SaveRoleMenusAsync(roleId, req.MenuIds);
	return Results.Json(Envelope<object>.Ok(new { affectedRows }, "保存成功"));
}).RequireAuthorization("SuperOnly");

// 获取角色的按钮权限 - GET /api/systemManage/getRoleButtons/{roleId}
app.MapGet("/api/systemManage/getRoleButtons/{roleId:guid}", async (Guid roleId, UnifiedDbContext db) =>
{
	var buttonCodes = await db.RoleButtons
		.Where(x => x.RoleId == roleId)
		.Select(x => x.ButtonCode)
		.ToListAsync();
	return Results.Json(Envelope<List<string>>.Ok(buttonCodes));
}).RequireAuthorization("SuperOnly");

// 保存角色的按钮权限 - POST /api/systemManage/saveRoleButtons/{roleId}
app.MapPost("/api/systemManage/saveRoleButtons/{roleId:guid}", async (Guid roleId, SaveRoleButtonsRequest req, UnifiedDbContext db) =>
{
	// 检查角色是否存在
	var roleExists = await db.Roles.AnyAsync(r => r.Id == roleId && !r.DeletedFlag);
	if (!roleExists)
	{
		return Results.Json(Envelope<object>.Fail("404", "角色不存在"));
	}

	// 删除旧的按钮权限
	var oldButtons = await db.RoleButtons.Where(x => x.RoleId == roleId).ToListAsync();
	db.RoleButtons.RemoveRange(oldButtons);

	// 添加新的按钮权限
	var newButtons = req.ButtonCodes.Select(code => new RoleButton
	{
		RoleId = roleId,
		ButtonCode = code
	}).ToList();
	await db.RoleButtons.AddRangeAsync(newButtons);

	var affectedRows = await db.SaveChangesAsync();
	return Results.Json(Envelope<object>.Ok(new { affectedRows }, "保存成功"));
}).RequireAuthorization("SuperOnly");

// ========== 用户角色和角色菜单关联管理 ==========
// 获取用户的角色列表 - GET /api/admin/users/{id}/roles
app.MapGet("/api/admin/users/{id:guid}/roles", async (Guid id, UnifiedDbContext db) =>
{
	var roleIds = await db.UserRoles.Where(x => x.UserId == id).Select(x => x.RoleId).ToArrayAsync();
	return Results.Json(Envelope<Guid[]>.Ok(roleIds));
}).RequireAuthorization("AdminOrSuper");

// 为用户分配角色 - POST /api/admin/users/{id}/roles
app.MapPost("/api/admin/users/{id:guid}/roles", async (Guid id, Guid[] roleIds, UnifiedDbContext db) =>
{
	var exists = await db.Users.AnyAsync(x => x.Id == id);
	if (!exists) return Results.Json(Envelope<object>.Fail("404", "用户不存在"));
	var all = await db.Roles.Where(r => !r.DeletedFlag && roleIds.Contains(r.Id)).Select(r => r.Id).ToListAsync();
	var urs = await db.UserRoles.Where(x => x.UserId == id).ToListAsync();
	db.UserRoles.RemoveRange(urs);
	foreach (var rid in all) db.UserRoles.Add(new UserRole { UserId = id, RoleId = rid });
	await db.SaveChangesAsync();
	return Results.Json(Envelope<object>.Ok(new { userId = id }));
}).RequireAuthorization("AdminOrSuper");

// ========== 系统维护接口 ==========

// 重置菜单 ID 序列 - POST /api/admin/maintenance/reset-menu-sequence (临时端点)
app.MapPost("/api/admin/maintenance/reset-menu-sequence", async (UnifiedDbContext db) =>
{
	try
	{
		var maxId = await db.Menus.MaxAsync(m => (int?)m.Id) ?? 0;
		await db.Database.ExecuteSqlRawAsync("SELECT setval(pg_get_serial_sequence('\"Menus\"', 'Id'), {0}, true);", maxId);
		Console.WriteLine($"[Maintenance] Reset Menus Id sequence to {maxId}");
		return Results.Json(Envelope<object>.Ok(new { maxId }, $"序列已重置到 {maxId}"));
	}
	catch (Exception ex)
	{
		Console.WriteLine($"[Maintenance] Error resetting sequence: {ex.Message}");
		return Results.Json(Envelope<object>.Fail("5000", $"重置序列失败: {ex.Message}"));
	}
}).RequireAuthorization("SuperOnly");

// 获取刷新令牌统计信息 - GET /api/admin/maintenance/refresh-tokens/stats
app.MapGet("/api/admin/maintenance/refresh-tokens/stats", async (UnifiedDbContext db) =>
{
	var now = DateTimeOffset.UtcNow;
	var total = await db.RefreshTokens.CountAsync();
	var active = await db.RefreshTokens.CountAsync(x => !x.Revoked);
	var revoked = await db.RefreshTokens.CountAsync(x => x.Revoked);
	var valid = await db.RefreshTokens.CountAsync(x => x.ExpiresAt > now);
	var expired = await db.RefreshTokens.CountAsync(x => x.ExpiresAt <= now);
	
	var stats = new
	{
		total,
		active,
		revoked,
		valid,
		expired,
		cleanupCandidates = await db.RefreshTokens
			.CountAsync(x => x.ExpiresAt <= now && x.Revoked)
	};
	
	return Results.Json(Envelope<object>.Ok(stats, "刷新令牌统计信息"));
}).RequireAuthorization("SuperOnly");

// 手动清理过期刷新令牌 - POST /api/admin/maintenance/refresh-tokens/cleanup
app.MapPost("/api/admin/maintenance/refresh-tokens/cleanup", async (UnifiedDbContext db) =>
{
	var now = DateTimeOffset.UtcNow;
	var cutoffDate = now.AddDays(-30); // 保留30天内的记录

	// 删除已过期且已撤销的令牌
	var tokensToDelete = await db.RefreshTokens
		.Where(x => x.ExpiresAt <= now &&
				   (x.Revoked || x.CreatedAt <= cutoffDate))
		.ToListAsync();

	if (tokensToDelete.Any())
	{
		db.RefreshTokens.RemoveRange(tokensToDelete);
		await db.SaveChangesAsync();

		return Results.Json(Envelope<object>.Ok(
			new 
			{ 
				deletedCount = tokensToDelete.Count,
				revokedCount = tokensToDelete.Count(x => x.Revoked),
				oldCount = tokensToDelete.Count(x => x.CreatedAt <= cutoffDate)
			}, 
			$"成功清理 {tokensToDelete.Count} 条过期令牌"));
	}

	return Results.Json(Envelope<object>.Ok(
		new { deletedCount = 0 }, 
		"没有需要清理的过期令牌"));
}).RequireAuthorization("SuperOnly");

app.Run();

/// <summary>
/// 辅助函数：将菜单实体递归转换为菜单响应对象（包含子菜单）
/// </summary>
/// <param name="menu">菜单实体</param>
/// <returns>菜单响应对象</returns>
static MenuResponse ConvertToMenuResponse(Menu menu)
{
	var response = new MenuResponse
	{
		Id = menu.Id,
		MenuType = menu.MenuType.ToString(),
		MenuName = menu.MenuName,
		RouteName = menu.RouteName,
		RoutePath = menu.RoutePath,
		Component = menu.Component,
		I18nKey = menu.I18nKey,
		Icon = menu.Icon,
		IconType = menu.IconType,
		ParentId = menu.ParentId,
		Order = menu.Order,
		Status = menu.Status,
		HideInMenu = menu.HideInMenu,
		ActiveMenu = menu.ActiveMenu,
		MultiTab = menu.MultiTab,
		FixedIndexInTab = menu.FixedIndexInTab,
		Query = menu.Query,
		Buttons = menu.Buttons,
		CreatedAt = menu.CreatedAt,
		UpdatedAt = menu.UpdatedAt
	};

	// 如果有子菜单，递归转换
	if (menu.Children != null && menu.Children.Any())
	{
		response.Children = menu.Children.Select(ConvertToMenuResponse).ToList();
	}

	return response;
}
