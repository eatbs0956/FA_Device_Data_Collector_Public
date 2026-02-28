using System.Net.Http.Headers;
using System.Text;
using Collector.Core.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Collector.Core.ApiClient;

/// <summary>
/// Auth.Api 客户端实现
/// </summary>
public class AuthApiClient : IAuthApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AuthApiClient> _logger;
    private LoginResponse? _currentLoginResponse;
    private DateTime _tokenExpireTime;

    public event EventHandler? TokenExpired;

    public AuthApiClient(HttpClient httpClient, ILogger<AuthApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public string? CurrentToken => _currentLoginResponse?.AccessToken;

    public bool IsAuthenticated => !string.IsNullOrEmpty(CurrentToken) && DateTime.UtcNow < _tokenExpireTime;

    public UserInfo? CurrentUser => _currentLoginResponse?.User;

    public async Task<ApiResponse<LoginResponse>> LoginAsync(LoginRequest request)
    {
        try
        {
            _logger.LogInformation("尝试登录: {UserName}", request.UserName);

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/auth/login", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogDebug("登录响应: {StatusCode}, {Content}", response.StatusCode, responseContent);

            var result = JsonConvert.DeserializeObject<ApiResponse<LoginResponse>>(responseContent);

            if (result?.IsSuccess == true && result.Data != null)
            {
                _currentLoginResponse = result.Data;
                _tokenExpireTime = DateTime.UtcNow.AddSeconds(result.Data.ExpiresIn - 60); // 提前60秒过期
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", result.Data.AccessToken);
                
                _logger.LogInformation("登录成功: {UserName}", request.UserName);
            }
            else
            {
                _logger.LogWarning("登录失败: {Code} - {Msg}", result?.Code, result?.Msg);
            }

            return result ?? new ApiResponse<LoginResponse> { Code = "500", Msg = "响应解析失败" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "登录异常");
            return new ApiResponse<LoginResponse>
            {
                Code = "500",
                Msg = $"登录失败: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<LoginResponse>> RefreshTokenAsync(string refreshToken)
    {
        try
        {
            _logger.LogInformation("刷新令牌");

            var request = new { refreshToken };
            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/auth/refresh", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<ApiResponse<LoginResponse>>(responseContent);

            if (result?.IsSuccess == true && result.Data != null)
            {
                _currentLoginResponse = result.Data;
                _tokenExpireTime = DateTime.UtcNow.AddSeconds(result.Data.ExpiresIn - 60);
                _httpClient.DefaultRequestHeaders.Authorization = 
                    new AuthenticationHeaderValue("Bearer", result.Data.AccessToken);
                
                _logger.LogInformation("令牌刷新成功");
            }
            else
            {
                _logger.LogWarning("令牌刷新失败: {Code} - {Msg}", result?.Code, result?.Msg);
                OnTokenExpired();
            }

            return result ?? new ApiResponse<LoginResponse> { Code = "500", Msg = "响应解析失败" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "刷新令牌异常");
            OnTokenExpired();
            return new ApiResponse<LoginResponse>
            {
                Code = "500",
                Msg = $"刷新令牌失败: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<object>> LogoutAsync()
    {
        try
        {
            _logger.LogInformation("用户登出");

            var response = await _httpClient.PostAsync("/api/auth/logout", null);
            var responseContent = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<ApiResponse<object>>(responseContent);

            // 清除本地状态
            _currentLoginResponse = null;
            _tokenExpireTime = DateTime.MinValue;
            _httpClient.DefaultRequestHeaders.Authorization = null;

            return result ?? new ApiResponse<object> { Code = "0000", Msg = "登出成功" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "登出异常");
            
            // 即使异常也清除本地状态
            _currentLoginResponse = null;
            _tokenExpireTime = DateTime.MinValue;
            _httpClient.DefaultRequestHeaders.Authorization = null;

            return new ApiResponse<object>
            {
                Code = "500",
                Msg = $"登出失败: {ex.Message}"
            };
        }
    }

    private void OnTokenExpired()
    {
        TokenExpired?.Invoke(this, EventArgs.Empty);
    }
}
