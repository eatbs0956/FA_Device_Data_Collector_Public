using Collector.Core.Models;

namespace Collector.Agent.Services;

/// <summary>
/// 配置服务接口
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// 当前配置
    /// </summary>
    AppSettings CurrentSettings { get; }

    /// <summary>
    /// 加载配置
    /// </summary>
    Task<AppSettings> LoadSettingsAsync();

    /// <summary>
    /// 保存配置
    /// </summary>
    Task SaveSettingsAsync(AppSettings settings);

    /// <summary>
    /// 生成节点ID
    /// </summary>
    string GenerateNodeId();
}
