using Collector.Core.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Collector.Agent.Services;

/// <summary>
/// 配置服务实现
/// </summary>
public class ConfigurationService : IConfigurationService
{
    private readonly ILogger<ConfigurationService> _logger;
    private readonly string _baseConfigPath;
    private readonly string _userConfigPath;
    private AppSettings _currentSettings;

    public AppSettings CurrentSettings => _currentSettings;

    public ConfigurationService(AppSettings settings, ILogger<ConfigurationService> logger)
    {
        _currentSettings = settings;
        _logger = logger;
        _baseConfigPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        _userConfigPath = Path.Combine(AppContext.BaseDirectory, "usersettings.json");
    }

    public Task<AppSettings> LoadSettingsAsync()
    {
        try
        {
            // 先加载基础配置
            if (File.Exists(_baseConfigPath))
            {
                var json = File.ReadAllText(_baseConfigPath);
                _currentSettings = JsonConvert.DeserializeObject<AppSettings>(json) ?? new AppSettings();
            }

            // 再用用户运行时配置覆盖（不会被编译覆盖）
            if (File.Exists(_userConfigPath))
            {
                var userJson = File.ReadAllText(_userConfigPath);
                JsonConvert.PopulateObject(userJson, _currentSettings);
                _logger.LogInformation("配置已加载: {Path}", _userConfigPath);
            }
            else
            {
                _logger.LogInformation("配置已加载: {Path}", _baseConfigPath);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "加载配置失败");
        }

        return Task.FromResult(_currentSettings);
    }

    public async Task SaveSettingsAsync(AppSettings settings)
    {
        try
        {
            // 始终保存到用户运行时配置文件（不覆盖源目录的 appsettings.json）
            var json = JsonConvert.SerializeObject(settings, Formatting.Indented);
            await File.WriteAllTextAsync(_userConfigPath, json);
            _currentSettings = settings;
            _logger.LogInformation("配置已保存: {Path}", _userConfigPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "保存配置失败");
            throw;
        }
    }

    public string GenerateNodeId()
    {
        // 生成格式: NODE-{时间戳}-{随机数}
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        var random = new Random().Next(1000, 9999);
        return $"NODE-{timestamp}-{random}";
    }
}
