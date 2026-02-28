using Collector.Core.ApiClient;
using Collector.Core.Engine;
using Microsoft.Extensions.Logging;

namespace Collector.Agent.Services;

/// <summary>
/// 配置拉取服务 - 封装从服务端拉取配置并加载到引擎的逻辑。
/// 供 MainWindowViewModel 和各子 ViewModel 的刷新按钮共享使用。
/// </summary>
public class ConfigPullService
{
    private readonly IAdminApiClient _adminApiClient;
    private readonly ICollectionEngine _collectionEngine;
    private readonly IConfigurationService _configService;
    private readonly ILogger<ConfigPullService> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public ConfigPullService(
        IAdminApiClient adminApiClient,
        ICollectionEngine collectionEngine,
        IConfigurationService configService,
        ILogger<ConfigPullService> logger)
    {
        _adminApiClient = adminApiClient;
        _collectionEngine = collectionEngine;
        _configService = configService;
        _logger = logger;
    }

    /// <summary>
    /// 从服务端拉取最新配置并加载到引擎。
    /// 内部有互斥锁，可安全地从多处并发调用。
    /// </summary>
    /// <returns>是否成功拉取并加载配置</returns>
    public async Task<bool> PullAndLoadConfigAsync()
    {
        // 采集运行中禁止刷新配置
        if (_collectionEngine.State == EngineState.Running)
        {
            _logger.LogWarning("采集运行中，禁止刷新配置");
            return false;
        }

        if (!await _lock.WaitAsync(TimeSpan.FromSeconds(1)))
        {
            _logger.LogWarning("配置拉取操作正在进行中，跳过重复请求");
            return false;
        }

        try
        {
            // 双重检查
            if (_collectionEngine.State == EngineState.Running)
            {
                _logger.LogWarning("采集运行中，禁止刷新配置");
                return false;
            }

            var nodeId = _configService.CurrentSettings.NodeId;
            _logger.LogInformation("正在拉取服务端配置: NodeId={NodeId}", nodeId);

            var result = await _adminApiClient.GetNodeConfigAsync(nodeId);

            if (result.IsSuccess && result.Data != null)
            {
                _logger.LogInformation("成功拉取到配置: Tasks={TaskCount}, Devices={DeviceCount}",
                    result.Data.Tasks.Count, result.Data.Devices.Count);
                await _collectionEngine.LoadConfigAsync(result.Data);
                return true;
            }
            else
            {
                _logger.LogWarning("拉取配置失败: {Msg}, Code={Code}", result.Msg, result.Code);
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "拉取配置异常");
            return false;
        }
        finally
        {
            _lock.Release();
        }
    }
}
