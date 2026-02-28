using Avalonia.Markup.Xaml;

namespace Collector.Agent.Localization;

/// <summary>
/// XAML 标记扩展 - 在 AXAML 中直接使用本地化 key
/// 用法: Text="{l:Localize Menu.Dashboard}"
///       Text="{l:Localize Common.Refresh, Prefix='🔄 '}"
/// </summary>
public class LocalizeExtension : MarkupExtension
{
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// 可选前缀（如 emoji 图标），拼接在翻译文本之前
    /// </summary>
    public string? Prefix { get; set; }

    /// <summary>
    /// 可选后缀，拼接在翻译文本之后
    /// </summary>
    public string? Suffix { get; set; }

    public LocalizeExtension() { }

    public LocalizeExtension(string key)
    {
        Key = key;
    }

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        var text = LocalizationManager.T(Key);
        if (!string.IsNullOrEmpty(Prefix))
            text = Prefix + text;
        if (!string.IsNullOrEmpty(Suffix))
            text = text + Suffix;
        return text;
    }
}
