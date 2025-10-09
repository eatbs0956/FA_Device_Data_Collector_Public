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
using Auth.Api.Contracts;
using Auth.Api.Models;
using Auth.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

// 创建 ASP.NET Core Web 应用程序构建器
var builder = WebApplication.CreateBuilder(args);

// Config - 配置参数
// PostgreSQL 数据库连接字符串 - 从环境变量获取或使用默认值
var pgConn = Environment.GetEnvironmentVariable("PG_CONN") ?? "Host=localhost;Username=devdcp;Password=devdcp;Database=devdcp";
// 访问令牌有效期（分钟）- JWT 访问令牌的生存时间
var accessMinutes = 15; // per decision
// 刷新令牌有效期（天）- 用于重新获取访问令牌的刷新令牌生存时间
var refreshDays = 7;
// 密码过期天数 - 用户密码强制过期的天数，0表示不过期
var passwordExpireDays = int.TryParse(Environment.GetEnvironmentVariable("PASSWORD_EXPIRE_DAYS"), out var d) ? d : 0; // 0=disabled

// Services - 服务注册
// 注册认证数据库上下文 - 配置 PostgreSQL 连接
builder.Services.AddDbContext<AuthDbContext>(opt => opt.UseNpgsql(pgConn)); 
// 注册密码服务 - 处理密码哈希、验证和历史记录
builder.Services.AddScoped<PasswordService>();
// 注册令牌服务 - 使用 DbContext，必须是作用域生命周期（非单例）
builder.Services.AddScoped<TokenService>();
// 注册菜单服务 - 处理菜单管理的CRUD操作
builder.Services.AddScoped<MenuService>();
// 注册角色服务 - 处理角色管理的CRUD操作
builder.Services.AddScoped<RoleService>();
// 注册用户服务 - 处理用户管理的CRUD操作
builder.Services.AddScoped<UserService>();
// 注册刷新令牌清理服务 - 后台定时清理过期和已撤销的刷新令牌
builder.Services.AddHostedService<RefreshTokenCleanupService>();
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
	options.AddPolicy("AdminOnly", policy => policy.RequireRole("R_ADMIN"));
	options.AddPolicy("UserOnly", policy => policy.RequireRole("R_USER"));
	options.AddPolicy("SuperOnly", policy => policy.RequireRole("R_SUPER"));
	// 允许管理员或超级管理员访问的策略
	options.AddPolicy("AdminOrSuper", policy => policy.RequireRole("R_ADMIN", "R_SUPER"));
});

// 构建 Web 应用程序实例
var app = builder.Build();

// 确保数据库初始化 - 在应用启动时进行数据库迁移和种子数据创建
using (var scope = app.Services.CreateScope())
{
	// 数据库上下文实例 - 用于执行数据库操作
	var db = scope.ServiceProvider.GetRequiredService<AuthDbContext>();

	db.Database.Migrate();
	// 如果角色表为空，创建默认角色
	if (!db.Roles.Any())
	{
		db.Roles.AddRange(
			new Role 
			{ 
				Name = "Super", 
				Code = "R_SUPER", 
				Status = 1, 
				Description = "超级管理员" 
			},
			new Role 
			{ 
				Name = "Admin", 
				Code = "R_ADMIN", 
				Status = 1, 
				Description = "管理员" 
			},
			new Role 
			{ 
				Name = "User", 
				Code = "R_USER", 
				Status = 1, 
				Description = "普通用户" 
			}
		);
		db.SaveChanges();
	}
	
	// 如果用户表为空，创建默认用户
	if (!db.Users.Any())
	{
		// 密码服务实例 - 用于哈希密码
		var pwd = scope.ServiceProvider.GetRequiredService<PasswordService>();
		
		// 获取角色
		var superRole = db.Roles.First(x => x.Code == "R_SUPER");
		var adminRole = db.Roles.First(x => x.Code == "R_ADMIN");
		var userRole = db.Roles.First(x => x.Code == "R_USER");
		
		// 创建默认密码 - 所有用户默认密码为 "ChangeMe@123"
		var defaultPassword = "Admin@123";
		var defaultPasswordHash = pwd.Hash(defaultPassword);
		
		// 1. 创建超级管理员用户 super
		var superUser = new User 
		{ 
			UserName = "super",
			NickName = "超级管理员",
			Gender = 1, // 1:男
			Phone = "",
			Email = "",
			Status = 1, // 1:启用
			Enabled = true,
			PasswordHash = defaultPasswordHash,
			PasswordUpdatedAt = DateTimeOffset.UtcNow,
			CreatedAt = DateTimeOffset.UtcNow,
			UpdatedAt = DateTimeOffset.UtcNow
		};
		db.Users.Add(superUser);
		db.SaveChanges();
		db.UserRoles.Add(new UserRole { UserId = superUser.Id, RoleId = superRole.Id });
		
		// 2. 创建管理员用户 admin
		var adminUser = new User 
		{ 
			UserName = "admin",
			NickName = "管理员",
			Gender = 1, // 1:男
			Phone = "",
			Email = "",
			Status = 1, // 1:启用
			Enabled = true,
			PasswordHash = defaultPasswordHash,
			PasswordUpdatedAt = DateTimeOffset.UtcNow,
			CreatedAt = DateTimeOffset.UtcNow,
			UpdatedAt = DateTimeOffset.UtcNow
		};
		db.Users.Add(adminUser);
		db.SaveChanges();
		db.UserRoles.Add(new UserRole { UserId = adminUser.Id, RoleId = adminRole.Id });
		
		// 3. 创建普通用户 user
		var normalUser = new User 
		{ 
			UserName = "user",
			NickName = "用户",
			Gender = 1, // 1:男
			Phone = "",
			Email = "",
			Status = 1, // 1:启用
			Enabled = true,
			PasswordHash = defaultPasswordHash,
			PasswordUpdatedAt = DateTimeOffset.UtcNow,
			CreatedAt = DateTimeOffset.UtcNow,
			UpdatedAt = DateTimeOffset.UtcNow
		};
		db.Users.Add(normalUser);
		db.SaveChanges();
		db.UserRoles.Add(new UserRole { UserId = normalUser.Id, RoleId = userRole.Id });
		
		// 保存所有更改
		db.SaveChanges();
	}
	else
	{
		// 修复已存在但数据不完整的用户
		var pwd = scope.ServiceProvider.GetRequiredService<PasswordService>();
		var defaultPassword = "Admin@123";
		var defaultPasswordHash = pwd.Hash(defaultPassword);
		
		var usersToFix = db.Users.Where(u => 
			(u.UserName == "super" || u.UserName == "admin" || u.UserName == "user") &&
			(u.NickName == "" || u.Gender == null || u.Status == 0 || u.PasswordHash.StartsWith("$2a$11$placeholder"))
		).ToList();
		
		if (usersToFix.Any())
		{
			foreach (var user in usersToFix)
			{
				var needsUpdate = false;
				
				// 更新昵称
				if (user.NickName == "")
				{
					user.NickName = user.UserName switch
					{
						"super" => "超级管理员",
						"admin" => "管理员",
						"user" => "用户",
						_ => user.UserName
					};
					needsUpdate = true;
				}
				
				// 更新性别
				if (user.Gender == null)
				{
					user.Gender = 1; // 默认男性
					needsUpdate = true;
				}
				
				// 更新状态
				if (user.Status == 0)
				{
					user.Status = 1; // 启用
					needsUpdate = true;
				}
				
				// 更新密码
				if (user.PasswordHash.StartsWith("$2a$11$placeholder"))
				{
					user.PasswordHash = defaultPasswordHash;
					user.PasswordUpdatedAt = DateTimeOffset.UtcNow;
					needsUpdate = true;
				}
				
				// 更新时间戳
				if (user.CreatedAt == DateTimeOffset.MinValue)
				{
					user.CreatedAt = DateTimeOffset.UtcNow;
					needsUpdate = true;
				}
				
				if (user.UpdatedAt == DateTimeOffset.MinValue)
				{
					user.UpdatedAt = DateTimeOffset.UtcNow;
					needsUpdate = true;
				}
				
				if (needsUpdate)
				{
					Console.WriteLine($"[Init] 修复用户 '{user.UserName}' 的数据");
				}
			}
			
			db.SaveChanges();
		}
	}

	// 初始化默认菜单 - 如果菜单表为空，创建默认菜单
	if (!db.Menus.Any())
	{
		Console.WriteLine("[Init] Creating default menus...");
		
		// 首页菜单 - 系统默认首页路由
		var homeMenu = new Menu
		{
			Id = 1,                                 // 手动指定ID
			MenuType = 2,                           // 菜单类型 - 2表示菜单（1为目录，2为菜单）
			MenuName = "首页",                      // 菜单名称 - 显示在导航中的名称
			RouteName = "home",                     // 路由名称 - Vue Router 中的路由名称
			RoutePath = "/home",                    // 路由路径 - URL 访问路径
			Component = "view.home",                // 页面组件 - 对应的 Vue 组件路径
			I18nKey = "route.home",                 // 国际化key - 用于多语言支持
			Icon = "mdi:monitor-dashboard",         // 图标 - Iconify 图标标识
			IconType = "1",                         // 图标类型 - 1为iconify图标，2为本地图标
			Order = 1,                              // 排序 - 在菜单中的显示顺序
			Status = 1,                             // 菜单状态 - 1为启用，2为禁用
			HideInMenu = false,                     // 隐藏菜单 - false表示在导航菜单中显示
			MultiTab = false,                       // 支持多页签 - false表示不支持
			FixedIndexInTab = false,                // 固定在页签中 - false表示不固定
			ActiveMenu = null,                      // 高亮的菜单 - null表示无特殊高亮
			Query = null,                           // 路由参数 - null表示无默认参数
			ParentId = null,                        // 父级菜单 - null表示顶级菜单
			CreatedAt = DateTimeOffset.UtcNow,      // 创建时间 - 当前UTC时间
			UpdatedAt = DateTimeOffset.UtcNow       // 更新时间 - 当前UTC时间
		};

		// 设备管理目录 - 顶级目录菜单
		var deviceMenu = new Menu
		{
			Id = 2,                                 // 手动指定ID
			MenuType = 1,                           // 菜单类型 - 1表示目录（1为目录，2为菜单）
			MenuName = "设备管理",                  // 菜单名称 - 显示在导航中的名称
			RouteName = "device",                   // 路由名称 - Vue Router 中的路由名称
			RoutePath = "/device",                  // 路由路径 - URL 访问路径
			Component = "view.device",              // 页面组件 - 对应的 Vue 组件路径
			I18nKey = "route.device",               // 国际化key - 用于多语言支持
			Icon = "tabler:devices-cog",                // 图标 - Iconify 图标标识
			IconType = "1",                         // 图标类型 - 1为iconify图标，2为本地图标
			Order = 2,                              // 排序 - 在菜单中的显示顺序
			Status = 1,                             // 菜单状态 - 1为启用，2为禁用
			HideInMenu = false,                     // 隐藏菜单 - false表示在导航菜单中显示
			MultiTab = false,                       // 支持多页签 - false表示不支持
			FixedIndexInTab = false,                // 固定在页签中 - false表示不固定
			ActiveMenu = null,                      // 高亮的菜单 - null表示无特殊高亮
			Query = null,                           // 路由参数 - null表示无默认参数
			ParentId = null,                        // 父级菜单 - null表示顶级菜单
			CreatedAt = DateTimeOffset.UtcNow,      // 创建时间 - 当前UTC时间
			UpdatedAt = DateTimeOffset.UtcNow       // 更新时间 - 当前UTC时间
		};

		// 设备列表子菜单 - 设备管理下的设备列表菜单
		var deviceListMenu = new Menu
		{
			Id = 3,                                 // 手动指定ID
			MenuType = 2,                           // 菜单类型 - 2表示菜单
			MenuName = "设备列表",                  // 菜单名称
			RouteName = "device_list",              // 路由名称
			RoutePath = "/device/list",             // 路由路径
			Component = "view.device_list",         // 页面组件
			I18nKey = "route.device_list",          // 国际化key
			Icon = "ri:list-settings-line",      // 图标 - 列表图标
			IconType = "1",                         // 图标类型 - iconify图标
			Order = 1,                              // 排序 - 第1个子菜单
			Status = 1,                             // 菜单状态 - 启用
			HideInMenu = false,                     // 隐藏菜单 - 否
			MultiTab = false,                       // 支持多页签 - 否
			FixedIndexInTab = false,                // 固定在页签中 - 否
			ActiveMenu = null,                      // 高亮的菜单
			Query = null,                           // 路由参数
			ParentId = 2,                          // 父级菜单 - 设备管理(ID=2)
			CreatedAt = DateTimeOffset.UtcNow,
			UpdatedAt = DateTimeOffset.UtcNow
		};

		// 设备标签子菜单 - 设备管理下的设备标签菜单
		var deviceTagMenu = new Menu
		{
			Id = 4,                                 // 手动指定ID
			MenuType = 2,                           // 菜单类型 - 2表示菜单
			MenuName = "设备标签",                  // 菜单名称
			RouteName = "device_label",               // 路由名称
			RoutePath = "/device/label",              // 路由路径
			Component = "view.device_label",          // 页面组件
			I18nKey = "route.device_label",           // 国际化key
			Icon = "mdi:tag-multiple-outline",      // 图标 - 标签图标
			IconType = "1",                         // 图标类型 - iconify图标
			Order = 2,                              // 排序 - 第2个子菜单
			Status = 1,                             // 菜单状态 - 启用
			HideInMenu = false,                     // 隐藏菜单 - 否
			MultiTab = false,                       // 支持多页签 - 否
			FixedIndexInTab = false,                // 固定在页签中 - 否
			ActiveMenu = null,                      // 高亮的菜单
			Query = null,                           // 路由参数
			ParentId = 2,                          // 父级菜单 - 设备管理(ID=2)
			CreatedAt = DateTimeOffset.UtcNow,
			UpdatedAt = DateTimeOffset.UtcNow
		};

		// 设备协议子菜单 - 设备管理下的设备协议菜单
		var deviceProtocolMenu = new Menu
		{
			Id = 5,                                 // 手动指定ID
			MenuType = 2,                           // 菜单类型 - 2表示菜单
			MenuName = "设备协议",                  // 菜单名称
			RouteName = "device_protocol",          // 路由名称
			RoutePath = "/device/protocol",         // 路由路径
			Component = "view.device_protocol",     // 页面组件
			I18nKey = "route.device_protocol",      // 国际化key
			Icon = "simple-icons:handshake-protocol",                  // 图标 - 协议图标
			IconType = "1",                         // 图标类型 - iconify图标
			Order = 3,                              // 排序 - 第3个子菜单
			Status = 1,                             // 菜单状态 - 启用
			HideInMenu = false,                     // 隐藏菜单 - 否
			MultiTab = false,                       // 支持多页签 - 否
			FixedIndexInTab = false,                // 固定在页签中 - 否
			ActiveMenu = null,                      // 高亮的菜单
			Query = null,                           // 路由参数
			ParentId = 2,                          // 父级菜单 - 设备管理(ID=2)
			CreatedAt = DateTimeOffset.UtcNow,
			UpdatedAt = DateTimeOffset.UtcNow

		};

		// 采集管理目录 - 顶级目录菜单
		var collectMenu = new Menu
		{
			Id = 6,                                 // 手动指定ID
			MenuType = 1,                           // 菜单类型 - 1表示目录（1为目录，2为菜单）
			MenuName = "采集管理",                  // 菜单名称 - 显示在导航中的名称
			RouteName = "collection",                  // 路由名称 - Vue Router 中的路由名称
			RoutePath = "/collection",                 // 路由路径 - URL 访问路径
			Component = "view.collection",             // 页面组件 - 对应的 Vue 组件路径
			I18nKey = "route.collection",              // 国际化key - 用于多语言支持
			Icon = "carbon:partition-collection",           // 图标 - Iconify 图标标识
			IconType = "1",                         // 图标类型 - 1为iconify图标，2为本地图标
			Order = 3,                              // 排序 - 在菜单中的显示顺序
			Status = 1,                             // 菜单状态 - 1为启用，2为禁用
			HideInMenu = false,                     // 隐藏菜单 - false表示在导航菜单中显示
			MultiTab = false,                       // 支持多页签 - false表示不支持
			FixedIndexInTab = false,                // 固定在页签中 - false表示不固定
			ActiveMenu = null,                      // 高亮的菜单 - null表示无特殊高亮
			Query = null,                           // 路由参数 - null表示无默认参数
			ParentId = null,                        // 父级菜单 - null表示顶级菜单
			CreatedAt = DateTimeOffset.UtcNow,      // 创建时间 - 当前UTC时间
			UpdatedAt = DateTimeOffset.UtcNow       // 更新时间 - 当前UTC时间

		};

		// 采集任务子菜单 - 采集管理下的采集任务菜单
		var collectTaskMenu = new Menu
		{
			Id = 7,                                 // 手动指定ID
			MenuType = 2,                           // 菜单类型 - 2表示菜单
			MenuName = "采集任务",                  // 菜单名称
			RouteName = "collection_task",             // 路由名称
			RoutePath = "/collection/task",            // 路由路径
			Component = "view.collection_task",        // 页面组件
			I18nKey = "route.collection_task",         // 国际化key
			Icon = "carbon:task-settings",             // 图标 - 任务图标
			IconType = "1",                         // 图标类型 - iconify图标
			Order = 1,                              // 排序 - 第1个子菜单
			Status = 1,                             // 菜单状态 - 启用
			HideInMenu = false,                     // 隐藏菜单 - 否
			MultiTab = false,                       // 支持多页签 - 否
			FixedIndexInTab = false,                // 固定在页签中 - 否
			ActiveMenu = null,                      // 高亮的菜单
			Query = null,                           // 路由参数
			ParentId = 6,                          // 父级菜单 - 采集管理(ID=6)
			CreatedAt = DateTimeOffset.UtcNow,
			UpdatedAt = DateTimeOffset.UtcNow
		};

		// 采集节点子菜单 - 采集管理下的采集节点菜单
		var collectNodeMenu = new Menu
		{
			Id = 8,                                 // 手动指定ID
			MenuType = 2,                           // 菜单类型 - 2表示菜单
			MenuName = "采集节点",                  // 菜单名称
			RouteName = "collection_node",             // 路由名称
			RoutePath = "/collection/node",            // 路由路径
			Component = "view.collection_node",        // 页面组件
			I18nKey = "route.collection_node",         // 国际化key
			Icon = "carbon:kubernetes-worker-node",            // 图标 - 节点图标
			IconType = "1",                         // 图标类型 - iconify图标
			Order = 2,                              // 排序 - 第2个子菜单
			Status = 1,                             // 菜单状态 - 启用
			HideInMenu = false,                     // 隐藏菜单 - 否
			MultiTab = false,                       // 支持多页签 - 否
			FixedIndexInTab = false,                // 固定在页签中 - 否
			ActiveMenu = null,                      // 高亮的菜单
			Query = null,                           // 路由参数
			ParentId = 6,                          // 父级菜单 - 采集管理(ID=6)
			CreatedAt = DateTimeOffset.UtcNow,
			UpdatedAt = DateTimeOffset.UtcNow
		};

		// 监控管理目录 - 顶级目录菜单
		var monitorMenu = new Menu
		{
			Id = 9,                                 // 手动指定ID
			MenuType = 1,                           // 菜单类型 - 1表示目录（1为目录，2为菜单）
			MenuName = "监控管理",                  // 菜单名称 - 显示在导航中的名称
			RouteName = "monitor",                  // 路由名称 - Vue Router 中的路由名称
			RoutePath = "/monitor",                 // 路由路径 - URL 访问路径
			Component = "view.monitor",             // 页面组件 - 对应的 Vue 组件路径
			I18nKey = "route.monitor",              // 国际化key - 用于多语言支持
			Icon = "carbon:cloud-monitoring",             // 图标 - Iconify 图标标识
			IconType = "1",                         // 图标类型 - 1为iconify图标，2为本地图标
			Order = 4,                              // 排序 - 在菜单中的显示顺序
			Status = 1,                             // 菜单状态 - 1为启用，2为禁用
			HideInMenu = false,                     // 隐藏菜单 - false表示在导航菜单中显示
			MultiTab = false,                       // 支持多页签 - false表示不支持
			FixedIndexInTab = false,                // 固定在页签中 - false表示不固定
			ActiveMenu = null,                      // 高亮的菜单 - null表示无特殊高亮
			Query = null,                           // 路由参数 - null表示无默认参数
			ParentId = null,                        // 父级菜单 - null表示顶级菜单
			CreatedAt = DateTimeOffset.UtcNow,      // 创建时间 - 当前UTC时间
			UpdatedAt = DateTimeOffset.UtcNow       // 更新时间 - 当前UTC时间
		};

		// 实时监控子菜单 - 监控管理下的实时监控菜单
		var realTimeMonitorMenu = new Menu
		{
			Id = 10,                                // 手动指定ID
			MenuType = 2,                           // 菜单类型 - 2表示菜单
			MenuName = "实时监控",                  // 菜单名称
			RouteName = "monitor_realtime",         // 路由名称
			RoutePath = "/monitor/realtime",        // 路由路径
			Component = "view.monitor_realtime",    // 页面组件
			I18nKey = "route.monitor_realtime",     // 国际化key
			Icon = "solar:monitor-camera-broken",               // 图标 - 监控图标
			IconType = "1",                         // 图标类型 - iconify图标
			Order = 1,                              // 排序 - 第1个子菜单
			Status = 1,                             // 菜单状态 - 启用
			HideInMenu = false,                     // 隐藏菜单 - 否
			MultiTab = false,                       // 支持多页签 - 否
			FixedIndexInTab = false,                // 固定在页签中 - 否
			ActiveMenu = null,                      // 高亮的菜单
			Query = null,                           // 路由参数
			ParentId = 9,                          // 父级菜单 - 监控管理(ID=9)
			CreatedAt = DateTimeOffset.UtcNow,
			UpdatedAt = DateTimeOffset.UtcNow
		};

		// 历史数据监控子菜单 - 监控管理下的历史数据监控菜单
		var historyDataMonitorMenu = new Menu
		{
			Id = 11,                                // 手动指定ID
			MenuType = 2,                           // 菜单类型 - 2表示菜单
			MenuName = "历史数据",                  // 菜单名称
			RouteName = "monitor_historical",          // 路由名称
			RoutePath = "/monitor/historical",         // 路由路径
			Component = "view.monitor_historical",     // 页面组件
			I18nKey = "route.monitor_historical",      // 国际化key
			Icon = "iconoir:database-monitor",                   // 图标 - 历史图标
			IconType = "1",                         // 图标类型 - iconify图标
			Order = 2,                              // 排序 - 第2个子菜单
			Status = 1,                             // 菜单状态 - 启用
			HideInMenu = false,                     // 隐藏菜单 - 否
			MultiTab = false,                       // 支持多页签 - 否
			FixedIndexInTab = false,                // 固定在页签中 - 否
			ActiveMenu = null,                      // 高亮的菜单
			Query = null,                           // 路由参数
			ParentId = 9,                          // 父级菜单 - 监控管理(ID=9)
			CreatedAt = DateTimeOffset.UtcNow,
			UpdatedAt = DateTimeOffset.UtcNow
		};

		// 统计报表子菜单 - 监控管理下的统计报表菜单
		var statsReportMenu = new Menu
		{
			Id = 12,                                // 手动指定ID
			MenuType = 2,                           // 菜单类型 - 2表示菜单
			MenuName = "统计报表",                  // 菜单名称
			RouteName = "monitor_statistics",            // 路由名称
			RoutePath = "/monitor/statistics",           // 路由路径
			Component = "view.monitor_statistics",       // 页面组件
			I18nKey = "route.monitor_statistics",        // 国际化key
			Icon = "mdi:chart-box-outline",         // 图标 - 报表图标
			IconType = "1",                         // 图标类型 - iconify图标
			Order = 3,                              // 排序 - 第3个子菜单
			Status = 1,                             // 菜单状态 - 启用
			HideInMenu = false,                     // 隐藏菜单 - 否
			MultiTab = false,                       // 支持多页签 - 否
			FixedIndexInTab = false,                // 固定在页签中 - 否
			ActiveMenu = null,                      // 高亮的菜单
			Query = null,                           // 路由参数
			ParentId = 9,                          // 父级菜单 - 监控管理(ID=9)
			CreatedAt = DateTimeOffset.UtcNow,
			UpdatedAt = DateTimeOffset.UtcNow
		};

		//告警管理目录 - 顶级目录菜单
		var alertMenu = new Menu
		{
			Id = 13,                                // 手动指定ID
			MenuType = 1,                           // 菜单类型 - 1表示目录（1为目录，2为菜单）
			MenuName = "告警管理",                  // 菜单名称 - 显示在导航中的名称
			RouteName = "alarm",                    // 路由名称 - Vue Router 中的路由名称
			RoutePath = "/alarm",                   // 路由路径 - URL 访问路径
			Component = "view.alarm",               // 页面组件 - 对应的 Vue 组件路径
			I18nKey = "route.alarm",                // 国际化key - 用于多语言支持
			Icon = "lets-icons:alarm-light",       // 图标 - Iconify 图标标识
			IconType = "1",                         // 图标类型 - 1为iconify图标，2为本地图标
			Order = 5,                              // 排序 - 在菜单中的显示顺序
			Status = 1,                             // 菜单状态 - 1为启用，2为禁用
			HideInMenu = false,                     // 隐藏菜单 - false表示在导航菜单中显示
			MultiTab = false,                       // 支持多页签 - false表示不支持
			FixedIndexInTab = false,                // 固定在页签中 - false表示不固定
			ActiveMenu = null,                      // 高亮的菜单 - null表示无特殊高亮
			Query = null,                           // 路由参数 - null表示无默认参数
			ParentId = null,                        // 父级菜单 - null表示顶级菜单
			CreatedAt = DateTimeOffset.UtcNow,      // 创建时间 - 当前UTC时间
			UpdatedAt = DateTimeOffset.UtcNow       // 更新时间 - 当前UTC时间
		};

		// 实时告警子菜单 - 告警管理下的实时告警菜单
		var realTimeAlertMenu = new Menu
		{
			Id = 14,                                // 手动指定ID
			MenuType = 2,                           // 菜单类型 - 2表示菜单
			MenuName = "实时告警",                  // 菜单名称
			RouteName = "alarm_realtime",           // 路由名称
			RoutePath = "/alarm/realtime",          // 路由路径
			Component = "view.alarm_realtime",      // 页面组件
			I18nKey = "route.alarm_realtime",       // 国际化key
			Icon = "material-symbols-light:alarm-outline",       // 图标 - 实时告警图标
			IconType = "1",                         // 图标类型 - iconify图标
			Order = 1,                              // 排序 - 第1个子菜单
			Status = 1,                             // 菜单状态 - 启用
			HideInMenu = false,                     // 隐藏菜单 - 否
			MultiTab = false,                       // 支持多页签 - 否
			FixedIndexInTab = false,                // 固定在页签中 - 否
			ActiveMenu = null,                      // 高亮的菜单
			Query = null,                           // 路由参数
			ParentId = 13,                         // 父级菜单 - 告警管理(ID=13)
			CreatedAt = DateTimeOffset.UtcNow,
			UpdatedAt = DateTimeOffset.UtcNow
		};

		// 告警规则子菜单 - 告警管理下的告警规则菜单
		var alertRuleMenu = new Menu
		{
			Id = 15,                                // 手动指定ID
			MenuType = 2,                           // 菜单类型 - 2表示菜单
			MenuName = "告警规则",                  // 菜单名称
			RouteName = "alarm_rule",               // 路由名称
			RoutePath = "/alarm/rule",              // 路由路径
			Component = "view.alarm_rule",          // 页面组件
			I18nKey = "route.alarm_rule",           // 国际化key
			Icon = "bi:file-earmark-ruled",         // 图标 - 告警规则图标
			IconType = "1",                         // 图标类型 - iconify图标
			Order = 2,                              // 排序 - 第2个子菜单
			Status = 1,                             // 菜单状态 - 启用
			HideInMenu = false,                     // 隐藏菜单 - 否
			MultiTab = false,                       // 支持多页签 - 否
			FixedIndexInTab = false,                // 固定在页签中 - 否
			ActiveMenu = null,                      // 高亮的菜单
			Query = null,                           // 路由参数
			ParentId = 13,                         // 父级菜单 - 告警管理(ID=13)
			CreatedAt = DateTimeOffset.UtcNow,
			UpdatedAt = DateTimeOffset.UtcNow
		};

		// 告警历史子菜单 - 告警管理下的告警历史菜单
		var alertHistoryMenu = new Menu
		{
			Id = 16,                                // 手动指定ID
			MenuType = 2,                           // 菜单类型 - 2表示菜单
			MenuName = "告警历史",                  // 菜单名称
			RouteName = "alarm_history",            // 路由名称
			RoutePath = "/alarm/history",           // 路由路径
			Component = "view.alarm_history",       // 页面组件
			I18nKey = "route.alarm_history",        // 国际化key
			Icon = "material-symbols-light:deployed-code-history-outline-sharp",                   // 图标 - 历史图标
			IconType = "1",                         // 图标类型 - iconify图标
			Order = 3,                              // 排序 - 第3个子菜单
			Status = 1,                             // 菜单状态 - 启用
			HideInMenu = false,                     // 隐藏菜单 - 否
			MultiTab = false,                       // 支持多页签 - 否
			FixedIndexInTab = false,                // 固定在页签中 - 否
			ActiveMenu = null,                      // 高亮的菜单
			Query = null,                           // 路由参数
			ParentId = 13,                         // 父级菜单 - 告警管理(ID=13)
			CreatedAt = DateTimeOffset.UtcNow,
			UpdatedAt = DateTimeOffset.UtcNow
		};

		// 系统管理目录 - 顶级目录菜单
		var manageMenu = new Menu
		{
			Id = 17,                                             // 手动指定ID
			MenuType = 1,                                       // 菜单类型 - 1表示目录（1为目录，2为菜单）
			MenuName = "系统管理",                              // 菜单名称 - 显示在导航中的名称
			RouteName = "manage",                               // 路由名称 - Vue Router 中的路由名称
			RoutePath = "/manage",                              // 路由路径 - URL 访问路径
			Component = "layout.base$view.manage",              // 页面组件 - 布局(base) + 页面组件(manage)
			I18nKey = "route.manage",                           // 国际化key - 用于多语言支持
			Icon = "carbon:cloud-service-management",           // 图标 - Iconify 图标标识
			IconType = "1",                                     // 图标类型 - 1为iconify图标，2为本地图标
			Order = 99,                                         // 排序 - 在菜单中的显示顺序
			Status = 1,                                         // 菜单状态 - 1为启用，2为禁用
			HideInMenu = false,                                 // 隐藏菜单 - false表示在导航菜单中显示
			MultiTab = false,                                   // 支持多页签 - false表示不支持
			FixedIndexInTab = false,                            // 固定在页签中 - false表示不固定
			ActiveMenu = null,                                  // 高亮的菜单 - null表示无特殊高亮
			Query = null,                                       // 路由参数 - null表示无默认参数
			ParentId = null,                                    // 父级菜单 - null表示顶级菜单
			CreatedAt = DateTimeOffset.UtcNow,                  // 创建时间 - 当前UTC时间
			UpdatedAt = DateTimeOffset.UtcNow                   // 更新时间 - 当前UTC时间
		};
		
		// 用户管理子菜单 - 系统管理下的用户管理菜单
		var userManageMenu = new Menu
		{
			Id = 18,                                             // 手动指定ID
			MenuType = 2,                                       // 菜单类型 - 2表示菜单
			MenuName = "用户管理",                              // 菜单名称
			RouteName = "manage_user",                          // 路由名称
			RoutePath = "/manage/user",                         // 路由路径
			Component = "view.manage_user",                     // 页面组件
			I18nKey = "route.manage_user",                      // 国际化key
			Icon = "ic:round-manage-accounts",                  // 图标 - 用户管理图标
			IconType = "1",                                     // 图标类型 - iconify图标
			Order = 1,                                          // 排序 - 第1个子菜单
			Status = 1,                                         // 菜单状态 - 启用
			HideInMenu = false,                                 // 隐藏菜单 - 否
			MultiTab = false,                                   // 支持多页签 - 否
			FixedIndexInTab = false,                            // 固定在页签中 - 否
			ActiveMenu = null,                                  // 高亮的菜单
			Query = null,                                       // 路由参数
			ParentId = 17,                                       // 父级菜单 - 系统管理(ID=2)
			CreatedAt = DateTimeOffset.UtcNow,
			UpdatedAt = DateTimeOffset.UtcNow
		};
		
		// 角色管理子菜单 - 系统管理下的角色管理菜单
		var roleManageMenu = new Menu
		{
			Id = 19,                                             // 手动指定ID
			MenuType = 2,                                       // 菜单类型 - 2表示菜单
			MenuName = "角色管理",                              // 菜单名称
			RouteName = "manage_role",                          // 路由名称
			RoutePath = "/manage/role",                         // 路由路径
			Component = "view.manage_role",                     // 页面组件
			I18nKey = "route.manage_role",                      // 国际化key
			Icon = "carbon:user-role",                          // 图标 - 角色管理图标
			IconType = "1",                                     // 图标类型 - iconify图标
			Order = 2,                                          // 排序 - 第2个子菜单
			Status = 1,                                         // 菜单状态 - 启用
			HideInMenu = false,                                 // 隐藏菜单 - 否
			MultiTab = false,                                   // 支持多页签 - 否
			FixedIndexInTab = false,                            // 固定在页签中 - 否
			ActiveMenu = null,                                  // 高亮的菜单
			Query = null,                                       // 路由参数
			ParentId = 17,                                       // 父级菜单 - 系统管理(ID=2)
			CreatedAt = DateTimeOffset.UtcNow,
			UpdatedAt = DateTimeOffset.UtcNow
		};
		
		// 菜单管理子菜单 - 系统管理下的菜单管理菜单
		var menuManageMenu = new Menu
		{
			Id = 20,                                             // 手动指定ID
			MenuType = 2,                                       // 菜单类型 - 2表示菜单
			MenuName = "菜单管理",                              // 菜单名称
			RouteName = "manage_menu",                          // 路由名称
			RoutePath = "/manage/menu",                         // 路由路径
			Component = "view.manage_menu",                     // 页面组件
			I18nKey = "route.manage_menu",                      // 国际化key
			Icon = "material-symbols:route",                    // 图标 - 菜单路由图标
			IconType = "1",                                     // 图标类型 - iconify图标
			Order = 3,                                          // 排序 - 第3个子菜单
			Status = 1,                                         // 菜单状态 - 启用
			HideInMenu = false,                                 // 隐藏菜单 - 否
			MultiTab = false,                                   // 支持多页签 - 否
			FixedIndexInTab = false,                            // 固定在页签中 - 否
			ActiveMenu = null,                                  // 高亮的菜单
			Query = null,                                       // 路由参数
			ParentId = 17,                                       // 父级菜单 - 系统管理(ID=2)
			CreatedAt = DateTimeOffset.UtcNow,
			UpdatedAt = DateTimeOffset.UtcNow
		};
	
		// 一次性保存所有菜单(所有ID都已手动指定,无需分批保存)
		db.Menus.AddRange(homeMenu,
			deviceMenu, deviceListMenu, deviceTagMenu, deviceProtocolMenu,
			collectMenu, collectTaskMenu, collectNodeMenu,
			monitorMenu, realTimeMonitorMenu, historyDataMonitorMenu, statsReportMenu,
			alertMenu, realTimeAlertMenu, alertRuleMenu, alertHistoryMenu,
			manageMenu, userManageMenu, roleManageMenu, menuManageMenu);
		db.SaveChanges();
	
		// 初始化默认角色菜单权限 - 如果角色菜单关联表为空,创建默认的权限配置
		if (!db.RoleMenus.Any())
		{
			// 获取角色
			var userRole = db.Roles.First(x => x.Code == "R_USER");
			var adminRole = db.Roles.First(x => x.Code == "R_ADMIN");
			var superRole = db.Roles.First(x => x.Code == "R_SUPER");
			
			// R_USER: 全部业务菜单(除系统管理)
			db.RoleMenus.AddRange(
				new RoleMenu { RoleId = userRole.Id, MenuId = 1 },
				new RoleMenu { RoleId = userRole.Id, MenuId = 2 },
				new RoleMenu { RoleId = userRole.Id, MenuId = 3 },
				new RoleMenu { RoleId = userRole.Id, MenuId = 4 },
				new RoleMenu { RoleId = userRole.Id, MenuId = 5 },
				new RoleMenu { RoleId = userRole.Id, MenuId = 6 },
				new RoleMenu { RoleId = userRole.Id, MenuId = 7 },
				new RoleMenu { RoleId = userRole.Id, MenuId = 8 },
				new RoleMenu { RoleId = userRole.Id, MenuId = 9 },
				new RoleMenu { RoleId = userRole.Id, MenuId = 10 },
				new RoleMenu { RoleId = userRole.Id, MenuId = 11 },
				new RoleMenu { RoleId = userRole.Id, MenuId = 12 },
				new RoleMenu { RoleId = userRole.Id, MenuId = 13 },
				new RoleMenu { RoleId = userRole.Id, MenuId = 14 },
				new RoleMenu { RoleId = userRole.Id, MenuId = 15 },
				new RoleMenu { RoleId = userRole.Id, MenuId = 16 }
				);
		
			// R_ADMIN: 业务菜单 + 系统管理-用户管理
			db.RoleMenus.AddRange(
				new RoleMenu { RoleId = adminRole.Id, MenuId = 1 },
				new RoleMenu { RoleId = adminRole.Id, MenuId = 2 },
				new RoleMenu { RoleId = adminRole.Id, MenuId = 3 },
				new RoleMenu { RoleId = adminRole.Id, MenuId = 4 },
				new RoleMenu { RoleId = adminRole.Id, MenuId = 5 },
				new RoleMenu { RoleId = adminRole.Id, MenuId = 6 },
				new RoleMenu { RoleId = adminRole.Id, MenuId = 7 },
				new RoleMenu { RoleId = adminRole.Id, MenuId = 8 },
				new RoleMenu { RoleId = adminRole.Id, MenuId = 9 },
				new RoleMenu { RoleId = adminRole.Id, MenuId = 10 },
				new RoleMenu { RoleId = adminRole.Id, MenuId = 11 },
				new RoleMenu { RoleId = adminRole.Id, MenuId = 12 },
				new RoleMenu { RoleId = adminRole.Id, MenuId = 13 },
				new RoleMenu { RoleId = adminRole.Id, MenuId = 14 },
				new RoleMenu { RoleId = adminRole.Id, MenuId = 15 },
				new RoleMenu { RoleId = adminRole.Id, MenuId = 16 },
				new RoleMenu { RoleId = adminRole.Id, MenuId = 17 },
				new RoleMenu { RoleId = adminRole.Id, MenuId = 18 }
			);
			
			// R_SUPER: 全部菜单
			db.RoleMenus.AddRange(
				new RoleMenu { RoleId = superRole.Id, MenuId = 1 },
				new RoleMenu { RoleId = superRole.Id, MenuId = 2 },
				new RoleMenu { RoleId = superRole.Id, MenuId = 3 },
				new RoleMenu { RoleId = superRole.Id, MenuId = 4 },
				new RoleMenu { RoleId = superRole.Id, MenuId = 5 },
				new RoleMenu { RoleId = superRole.Id, MenuId = 6 },
				new RoleMenu { RoleId = superRole.Id, MenuId = 7 },
				new RoleMenu { RoleId = superRole.Id, MenuId = 8 },
				new RoleMenu { RoleId = superRole.Id, MenuId = 9 },
				new RoleMenu { RoleId = superRole.Id, MenuId = 10 },
				new RoleMenu { RoleId = superRole.Id, MenuId = 11 },
				new RoleMenu { RoleId = superRole.Id, MenuId = 12 },
				new RoleMenu { RoleId = superRole.Id, MenuId = 13 },
				new RoleMenu { RoleId = superRole.Id, MenuId = 14 },
				new RoleMenu { RoleId = superRole.Id, MenuId = 15 },
				new RoleMenu { RoleId = superRole.Id, MenuId = 16 },
				new RoleMenu { RoleId = superRole.Id, MenuId = 17 },
				new RoleMenu { RoleId = superRole.Id, MenuId = 18 },
				new RoleMenu { RoleId = superRole.Id, MenuId = 19 },
				new RoleMenu { RoleId = superRole.Id, MenuId = 20 }
			);
			
			db.SaveChanges();
		}
	}
}

// 启动认证和授权中间件 - 确保在路由映射前启用
app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new { ok = true }));

// 提供 JWKS 公钥端点，供前端或其他服务获取公钥以验证 JWT 签名
app.MapGet("/.well-known/jwks.json", (TokenService tokenSvc) => Results.Json(tokenSvc.GetJwks()));

// Auth endpoints matching frontend
app.MapPost("/auth/login", async (LoginRequest req, AuthDbContext db, PasswordService pwd, TokenService tokenSvc) =>
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

	var pair = await tokenSvc.IssueAsync(user, TimeSpan.FromMinutes(accessMinutes), TimeSpan.FromDays(refreshDays));
	return Results.Json(Envelope<LoginToken>.Ok(new LoginToken(pair.Token, pair.RefreshToken)));
});

app.MapGet("/auth/getUserInfo", async (HttpContext http, AuthDbContext db) =>
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
    
    var info = new UserInfo(user.Id.ToString(), user.UserName, roles, menuRoutes);
    return Results.Json(Envelope<UserInfo>.Ok(info));
}).RequireAuthorization();

// 刷新令牌 - 使用刷新令牌获取新的访问令牌和刷新令牌
app.MapPost("/auth/refreshToken", async (RefreshRequest req, AuthDbContext db, TokenService tokenSvc) =>
{
	var user = await tokenSvc.ValidateRefreshAsync(req.refreshToken);
	if (user is null)
	{
		return Results.Json(Envelope<object>.Fail(Codes.Logout, "刷新令牌无效"));
	}
	var pair = await tokenSvc.IssueAsync(user, TimeSpan.FromMinutes(accessMinutes), TimeSpan.FromDays(refreshDays));
	return Results.Json(Envelope<LoginToken>.Ok(new LoginToken(pair.Token, pair.RefreshToken)));
});

// 注册新用户 - 简单注册接口，创建新用户但不分配角色
app.MapPost("/auth/register", async (LoginRequest req, AuthDbContext db, PasswordService pwd) =>
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
// 获取用户列表（分页） - GET /systemManage/getUserList
app.MapGet("/systemManage/getUserList", async (
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
}).RequireAuthorization("AdminOrSuper");

// 创建用户 - POST /admin/users
app.MapPost("/admin/users", async (UserCreateRequest req, UserService userSvc) =>
{
	try
	{
		var user = await userSvc.CreateUserAsync(
			req.UserName,
			req.NickName ?? "",
			req.UserGender,
			req.UserPhone ?? "",
			req.UserEmail ?? "",
			req.Status ?? 1,
			req.UserRoles,
			req.Password);

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
}).RequireAuthorization("AdminOrSuper");

// 更新用户 - PUT /admin/users/{id}
app.MapPut("/admin/users/{id:guid}", async (Guid id, UserUpdateRequest req, UserService userSvc) =>
{
	try
	{
		var success = await userSvc.UpdateUserAsync(
			id,
			req.UserName,
			req.NickName ?? "",
			req.UserGender,
			req.UserPhone ?? "",
			req.UserEmail ?? "",
			req.Status,
			req.UserRoles,
			req.Password);

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
}).RequireAuthorization("AdminOrSuper");

// 删除用户 - DELETE /admin/users/{id}
app.MapDelete("/admin/users/{id:guid}", async (Guid id, UserService userSvc) =>
{
	var success = await userSvc.DeleteUserAsync(id);
	if (!success)
	{
		return Results.Json(Envelope<object>.Fail("404", "用户不存在"));
	}

	return Results.Json(Envelope<object>.Ok(new { id }, "用户删除成功"));
}).RequireAuthorization("AdminOrSuper");

// ========== 角色管理接口 ==========
// 获取所有角色列表 - GET /systemManage/getAllRoles
app.MapGet("/systemManage/getAllRoles", async (AuthDbContext db) =>
{
	// 查询所有角色并返回
	var roles = await db.Roles
		.Where(x => x.Status == 1) // 只返回启用的角色
		.OrderBy(x => x.Name)
		.Select(x => new { roleId = x.Id, roleName = x.Name, roleCode = x.Code, roleDesc = x.Description })
		.ToListAsync();
	
	return Results.Json(Envelope<object>.Ok(roles));
}).RequireAuthorization("SuperOnly");

// 获取角色列表（分页） - GET /systemManage/getRoleList
app.MapGet("/systemManage/getRoleList", async (
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
}).RequireAuthorization("SuperOnly");

// 创建角色 - POST /admin/roles
app.MapPost("/admin/roles", async (RoleCreateRequest req, RoleService roleSvc) =>
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
}).RequireAuthorization("SuperOnly");

// 更新角色 - PUT /admin/roles/{id}
app.MapPut("/admin/roles/{id:guid}", async (Guid id, RoleUpdateRequest req, RoleService roleSvc) =>
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
}).RequireAuthorization("SuperOnly");

// 删除角色 - DELETE /admin/roles/{id}
app.MapDelete("/admin/roles/{id:guid}", async (Guid id, RoleService roleSvc) =>
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
}).RequireAuthorization("SuperOnly");

// ========== 菜单管理接口 ==========
// 获取菜单列表（分页，树形结构） - GET /admin/getMenuList?current=1&size=10
app.MapGet("/admin/getMenuList", async (MenuService menuSvc, int current = 1, int size = 10) =>
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
}).RequireAuthorization("SuperOnly");

// 获取单个菜单详情 - GET /admin/menus/{id}
app.MapGet("/admin/menus/{id:int}", async (int id, MenuService menuSvc) =>
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
		CreatedAt = menu.CreatedAt,
		UpdatedAt = menu.UpdatedAt
	};

	return Results.Json(Envelope<MenuResponse>.Ok(response));
}).RequireAuthorization("SuperOnly");

// 创建菜单 - POST /admin/menus
app.MapPost("/admin/menus", async (MenuDto dto, MenuService menuSvc) =>
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
			Query = dto.Query
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
}).RequireAuthorization("SuperOnly");

// 更新菜单 - PUT /admin/menus/{id}
app.MapPut("/admin/menus/{id:int}", async (int id, MenuDto dto, MenuService menuSvc) =>
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
		Query = dto.Query
	};

	var success = await menuSvc.UpdateMenuAsync(id, menu);
	if (!success)
		return Results.Json(Envelope<object>.Fail("404", "菜单不存在"));

	return Results.Json(Envelope<object>.Ok(new { id }, "菜单更新成功"));
}).RequireAuthorization("SuperOnly");

// 删除菜单 - DELETE /admin/menus/{id}
app.MapDelete("/admin/menus/{id:int}", async (int id, MenuService menuSvc) =>
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
}).RequireAuthorization("SuperOnly");

// 批量删除菜单 - DELETE /admin/menus/batch
app.MapDelete("/admin/menus/batch", async ([FromBody] List<int> ids, MenuService menuSvc) =>
{
	var count = await menuSvc.BatchDeleteMenusAsync(ids);
	return Results.Json(Envelope<object>.Ok(new { deletedCount = count }, $"成功删除 {count} 个菜单"));
}).RequireAuthorization("SuperOnly");

// 获取所有页面列表 - GET /systemManage/getAllPages
app.MapGet("/systemManage/getAllPages", async (MenuService menuSvc) =>
{
	var pages = await menuSvc.GetAllPagesAsync();
	return Results.Json(Envelope<List<string>>.Ok(pages));
}).RequireAuthorization("SuperOnly");

// 获取菜单树 - GET /systemManage/getMenuTree
app.MapGet("/systemManage/getMenuTree", async (MenuService menuSvc) =>
{
	var menuTree = await menuSvc.GetMenuTreeAsync();
	return Results.Json(Envelope<List<MenuTreeDto>>.Ok(menuTree));
}).RequireAuthorization("SuperOnly");

// 获取角色的菜单权限 - GET /systemManage/getRoleMenus/{roleId}
app.MapGet("/systemManage/getRoleMenus/{roleId:guid}", async (Guid roleId, MenuService menuSvc) =>
{
	var menuIds = await menuSvc.GetRoleMenusAsync(roleId);
	return Results.Json(Envelope<List<int>>.Ok(menuIds));
}).RequireAuthorization("SuperOnly");

// 保存角色的菜单权限 - POST /systemManage/saveRoleMenus/{roleId}
app.MapPost("/systemManage/saveRoleMenus/{roleId:guid}", async (Guid roleId, SaveRoleMenusRequest req, MenuService menuSvc) =>
{
	var affectedRows = await menuSvc.SaveRoleMenusAsync(roleId, req.MenuIds);
	return Results.Json(Envelope<object>.Ok(new { affectedRows }, "保存成功"));
}).RequireAuthorization("SuperOnly");

// ========== 用户角色和角色菜单关联管理 ==========
// 获取用户的角色列表 - GET /admin/users/{id}/roles
app.MapGet("/admin/users/{id:guid}/roles", async (Guid id, AuthDbContext db) =>
{
	var roleIds = await db.UserRoles.Where(x => x.UserId == id).Select(x => x.RoleId).ToArrayAsync();
	return Results.Json(Envelope<Guid[]>.Ok(roleIds));
}).RequireAuthorization("AdminOrSuper");

// 为用户分配角色 - POST /admin/users/{id}/roles
app.MapPost("/admin/users/{id:guid}/roles", async (Guid id, Guid[] roleIds, AuthDbContext db) =>
{
	var exists = await db.Users.AnyAsync(x => x.Id == id);
	if (!exists) return Results.Json(Envelope<object>.Fail("404", "用户不存在"));
	var all = await db.Roles.Where(r => roleIds.Contains(r.Id)).Select(r => r.Id).ToListAsync();
	var urs = await db.UserRoles.Where(x => x.UserId == id).ToListAsync();
	db.UserRoles.RemoveRange(urs);
	foreach (var rid in all) db.UserRoles.Add(new UserRole { UserId = id, RoleId = rid });
	await db.SaveChangesAsync();
	return Results.Json(Envelope<object>.Ok(new { userId = id }));
}).RequireAuthorization("AdminOrSuper");

// ========== 系统维护接口 ==========

// 重置菜单 ID 序列 - POST /admin/maintenance/reset-menu-sequence (临时端点)
app.MapPost("/admin/maintenance/reset-menu-sequence", async (AuthDbContext db) =>
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

// 获取刷新令牌统计信息 - GET /admin/maintenance/refresh-tokens/stats
app.MapGet("/admin/maintenance/refresh-tokens/stats", async (AuthDbContext db) =>
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

// 手动清理过期刷新令牌 - POST /admin/maintenance/refresh-tokens/cleanup
app.MapPost("/admin/maintenance/refresh-tokens/cleanup", async (AuthDbContext db) =>
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
