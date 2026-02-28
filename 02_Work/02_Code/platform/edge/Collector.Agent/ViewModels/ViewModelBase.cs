using CommunityToolkit.Mvvm.ComponentModel;

namespace Collector.Agent.ViewModels;

/// <summary>
/// ViewModel 基类
/// </summary>
public abstract partial class ViewModelBase : ObservableObject
{
    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _busyMessage;

    protected void SetBusy(bool isBusy, string? message = null)
    {
        IsBusy = isBusy;
        BusyMessage = message;
    }
}
