using System.Net.Http.Headers;
using System.Text;
using Collector.Core.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Collector.Core.ApiClient;

/// <summary>
/// Admin.Api 客户端实现
/// </summary>
public class AdminApiClient : IAdminApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AdminApiClient> _logger;

    public AdminApiClient(HttpClient httpClient, ILogger<AdminApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public void SetToken(string token)
    {
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<ApiResponse<NodeInfo>> RegisterNodeAsync(NodeRegisterRequest request)
    {
        try
        {
            _logger.LogInformation("注册节点: {NodeId}", request.NodeId);

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"/api/edge-nodes/{request.NodeId}/register", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogDebug("注册响应: {StatusCode}, {Content}", response.StatusCode, responseContent);

            var result = JsonConvert.DeserializeObject<ApiResponse<NodeInfo>>(responseContent);

            if (result?.IsSuccess == true)
            {
                _logger.LogInformation("节点注册成功: {NodeId}", request.NodeId);
            }
            else
            {
                _logger.LogWarning("节点注册失败: {Code} - {Msg}", result?.Code, result?.Msg);
            }

            return result ?? new ApiResponse<NodeInfo> { Code = "9999", Msg = "反序列化失败" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "注册节点异常");
            return new ApiResponse<NodeInfo> { Code = "9999", Msg = ex.Message };
        }
    }

    public async Task<ApiResponse<object>> SendHeartbeatAsync(string nodeId, HeartbeatRequest request)
    {
        try
        {
            _logger.LogDebug("发送心跳: {NodeId}", nodeId);

            var json = JsonConvert.SerializeObject(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PutAsync($"/api/edge-nodes/{nodeId}/heartbeat", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<ApiResponse<object>>(responseContent);

            if (result?.IsSuccess != true)
            {
                _logger.LogWarning("心跳发送失败: {Code} - {Msg}", result?.Code, result?.Msg);
            }

            return result ?? new ApiResponse<object> { Code = "500", Msg = "响应解析失败" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "心跳发送异常");
            return new ApiResponse<object>
            {
                Code = "500",
                Msg = $"心跳发送失败: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<NodeConfig>> GetNodeConfigAsync(string nodeId)
    {
        try
        {
            _logger.LogInformation("获取节点配置: {NodeId}", nodeId);

            var response = await _httpClient.GetAsync($"/api/edge-nodes/{nodeId}/config");
            var responseContent = await response.Content.ReadAsStringAsync();

            _logger.LogDebug("配置响应: {StatusCode}, {Content}", response.StatusCode, responseContent);

            // 先反序列化为服务端 DTO（ProtocolConfig 为 string），再转换为 Core 模型
            var serverResult = JsonConvert.DeserializeObject<ApiResponse<ServerNodeConfigResponse>>(responseContent);

            ApiResponse<NodeConfig> result = null;
            if (serverResult != null)
            {
                result = new ApiResponse<NodeConfig>
                {
                    Code = serverResult.Code,
                    Msg = serverResult.Msg,
                    Data = serverResult.Data != null
                        ? ApiModelConverter.ToNodeConfig(serverResult.Data)
                        : null
                };
            }

            if (result?.IsSuccess == true)
            {
                _logger.LogInformation("获取配置成功: {NodeId}, Tasks: {TaskCount}, Devices: {DeviceCount}",
                    nodeId,
                    result.Data?.Tasks?.Count ?? 0,
                    result.Data?.Devices?.Count ?? 0);
            }
            else
            {
                _logger.LogWarning("获取配置失败: {Code} - {Msg}", result?.Code, result?.Msg);
            }

            return result ?? new ApiResponse<NodeConfig> { Code = "500", Msg = "响应解析失败" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "获取节点配置异常");
            return new ApiResponse<NodeConfig>
            {
                Code = "500",
                Msg = $"获取配置失败: {ex.Message}"
            };
        }
    }

    public async Task<ApiResponse<object>> ReportStatusAsync(string nodeId, NodeStatusReport status)
    {
        try
        {
            _logger.LogDebug("上报节点状态: {NodeId}", nodeId);

            var json = JsonConvert.SerializeObject(status);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync($"/api/edge-nodes/{nodeId}/status", content);
            var responseContent = await response.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<ApiResponse<object>>(responseContent);

            return result ?? new ApiResponse<object> { Code = "500", Msg = "响应解析失败" };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "上报状态异常");
            return new ApiResponse<object>
            {
                Code = "500",
                Msg = $"上报状态失败: {ex.Message}"
            };
        }
    }
}
