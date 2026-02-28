using System;
using System.Globalization;
using System.Resources;
using System.Threading;
using NLog;

namespace Collector.Agent.Legacy.Services
{
    /// <summary>
    /// 多语言管理器
    /// </summary>
    public static class LanguageManager
    {
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();
        private static ResourceManager _resourceManager;
        private static CultureInfo _currentCulture;

        /// <summary>
        /// 语言设置选项
        /// </summary>
        public enum LanguageSetting
        {
            System,     // 跟随系统
            Chinese,    // 中文
            English     // 英文
        }

        /// <summary>
        /// 当前语言设置
        /// </summary>
        public static LanguageSetting CurrentSetting { get; private set; } = LanguageSetting.System;

        /// <summary>
        /// 初始化语言管理器
        /// </summary>
        /// <param name="setting">语言设置</param>
        public static void Initialize(LanguageSetting setting = LanguageSetting.System)
        {
            CurrentSetting = setting;
            
            _resourceManager = new ResourceManager(
                "Collector.Agent.Legacy.Resources.Strings",
                typeof(LanguageManager).Assembly);

            SetCulture(setting);
            
            Logger.Info("语言管理器已初始化: Setting={0}, Culture={1}", setting, _currentCulture?.Name ?? "default");
        }

        /// <summary>
        /// 设置语言
        /// </summary>
        public static void SetCulture(LanguageSetting setting)
        {
            CurrentSetting = setting;
            
            switch (setting)
            {
                case LanguageSetting.Chinese:
                    _currentCulture = new CultureInfo("zh-CN");
                    break;
                case LanguageSetting.English:
                    _currentCulture = new CultureInfo("en-US");
                    break;
                case LanguageSetting.System:
                default:
                    _currentCulture = CultureInfo.CurrentUICulture;
                    break;
            }

            // 设置线程文化
            Thread.CurrentThread.CurrentUICulture = _currentCulture;
            Thread.CurrentThread.CurrentCulture = _currentCulture;
        }

        /// <summary>
        /// 设置语言 (通过文化代码)
        /// </summary>
        /// <param name="cultureCode">文化代码，如 "zh-CN", "en-US"，空字符串表示跟随系统</param>
        public static void SetCulture(string cultureCode)
        {
            if (string.IsNullOrEmpty(cultureCode))
            {
                SetCulture(LanguageSetting.System);
            }
            else
            {
                SetCulture(ParseSetting(cultureCode));
            }
        }

        /// <summary>
        /// 获取当前语言代码
        /// </summary>
        /// <returns>当前语言代码，如 "zh-CN", "en-US"，跟随系统时返回空字符串</returns>
        public static string GetCurrentLanguage()
        {
            switch (CurrentSetting)
            {
                case LanguageSetting.Chinese:
                    return "zh-CN";
                case LanguageSetting.English:
                    return "en-US";
                default:
                    return "";
            }
        }

        /// <summary>
        /// 获取本地化字符串
        /// </summary>
        /// <param name="key">资源键</param>
        /// <returns>本地化字符串</returns>
        public static string GetString(string key)
        {
            if (_resourceManager == null)
            {
                Initialize();
            }

            try
            {
                var value = _resourceManager.GetString(key, _currentCulture);
                return value ?? key;
            }
            catch (Exception ex)
            {
                Logger.Warn(ex, "获取本地化字符串失败: {0}", key);
                return key;
            }
        }

        /// <summary>
        /// 获取格式化的本地化字符串
        /// </summary>
        /// <param name="key">资源键</param>
        /// <param name="args">格式化参数</param>
        /// <returns>格式化后的本地化字符串</returns>
        public static string GetString(string key, params object[] args)
        {
            var format = GetString(key);
            try
            {
                return string.Format(format, args);
            }
            catch
            {
                return format;
            }
        }

        /// <summary>
        /// 获取当前语言设置的显示名称
        /// </summary>
        public static string GetSettingDisplayName(LanguageSetting setting)
        {
            switch (setting)
            {
                case LanguageSetting.System:
                    return GetString("SettingsForm_Language_System");
                case LanguageSetting.Chinese:
                    return GetString("SettingsForm_Language_Chinese");
                case LanguageSetting.English:
                    return GetString("SettingsForm_Language_English");
                default:
                    return setting.ToString();
            }
        }

        /// <summary>
        /// 从配置字符串解析语言设置
        /// </summary>
        public static LanguageSetting ParseSetting(string value)
        {
            if (string.IsNullOrEmpty(value))
                return LanguageSetting.System;

            switch (value.ToLower())
            {
                case "chinese":
                case "zh-cn":
                case "zh":
                    return LanguageSetting.Chinese;
                case "english":
                case "en-us":
                case "en":
                    return LanguageSetting.English;
                default:
                    return LanguageSetting.System;
            }
        }

        /// <summary>
        /// 将语言设置转换为配置字符串
        /// </summary>
        public static string SettingToString(LanguageSetting setting)
        {
            switch (setting)
            {
                case LanguageSetting.Chinese:
                    return "Chinese";
                case LanguageSetting.English:
                    return "English";
                default:
                    return "System";
            }
        }
    }

    /// <summary>
    /// 本地化字符串快捷访问类
    /// </summary>
    public static class L
    {
        /// <summary>
        /// 获取本地化字符串
        /// </summary>
        public static string T(string key) => LanguageManager.GetString(key);

        /// <summary>
        /// 获取格式化的本地化字符串
        /// </summary>
        public static string T(string key, params object[] args) => LanguageManager.GetString(key, args);
    }
}
