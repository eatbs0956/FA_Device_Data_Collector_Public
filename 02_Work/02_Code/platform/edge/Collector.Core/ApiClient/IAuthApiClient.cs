using Collector.Core.Models;

namespace Collector.Core.ApiClient;

/// <summary>
/// Auth.Api 客户端接口
/// </summary>
public interface IAuthApiClient
{
    /// <summary>
    /// 用户登录
    /// </summary>
    /// <param name="request">登录请求</param>
    /// <returns>登录响应</returns>
    Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request);

    /// <summary>
    /// 刷新令牌
    /// </summary>
    /// <param name="refreshToken">刷新令牌</param>
    /// <returns>新的登录响应</returns>
    Task<ApiResponse<LoginResponse>> RefreshTokenAsync(string refreshToken);

    /// <summary>
    /// 登出
    /// </summary>
    Task<ApiResponse<object>> LogoutAsync();

    /// <summary>
    /// 获取当前令牌
    /// </summary>
    string? CurrentToken { get; }

    /// <summary>
    /// 是否已登录
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// 当前用户信息
    /// </summary>
    UserInfo? CurrentUser { get; }

    /// <summary>
    /// 令牌过期事件
    /// </summary>
    event EventHandler? TokenExpired;
}
