using System;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Collector.Agent.ViewModels;

namespace Collector.Agent;

/// <summary>
/// ViewLocator - 自动将 ViewModel 映射到对应的 View
/// </summary>
public class ViewLocator : IDataTemplate
{
    public Control Build(object? data)
    {
        if (data is null)
            return new TextBlock { Text = "ViewModel is null" };

        var vmFullName = data.GetType().FullName!;
        var viewName = vmFullName.Replace("ViewModel", "View");
        
        // 先尝试在当前程序集中查找
        var type = data.GetType().Assembly.GetType(viewName);
        
        // 回退到全局查找
        if (type == null)
            type = Type.GetType(viewName);

        if (type != null)
        {
            return (Control)Activator.CreateInstance(type)!;
        }

        Debug.WriteLine($"[ViewLocator] Not Found: VM={vmFullName}, expected View={viewName}");
        return new TextBlock { Text = $"Not Found: {viewName}" };
    }

    public bool Match(object? data)
    {
        return data is ViewModelBase;
    }
}
