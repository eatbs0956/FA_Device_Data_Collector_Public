using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Collector.Core.Engine;
using Collector.Core.Models;
using Microsoft.Extensions.Logging;

namespace Collector.Agent.ViewModels;

/// <summary>
/// 数据预览 ViewModel
/// </summary>
public partial class DataPreviewViewModel : ViewModelBase
{
    private readonly ICollectionEngine _collectionEngine;
    private readonly ILogger<DataPreviewViewModel> _logger;

    [ObservableProperty]
    private bool _isAutoRefresh = true;

    public ObservableCollection<DataPreviewDataPoint> RecentDataPoints { get; } = new();

    public DataPreviewViewModel(ICollectionEngine collectionEngine, ILogger<DataPreviewViewModel> logger)
    {
        _collectionEngine = collectionEngine;
        _logger = logger;

        _collectionEngine.DataCollected += OnDataCollected;
    }

    private void OnDataCollected(object? sender, CollectionData data)
    {
        if (!IsAutoRefresh) return;

        var deviceName = _collectionEngine.CurrentConfig?.Devices
            .FirstOrDefault(d => d.Id == data.DeviceId)?.DeviceName ?? data.DeviceCode;

        // 在 UI 线程更新
        Avalonia.Threading.Dispatcher.UIThread.Post(() =>
        {
            foreach (var point in data.DataPoints)
            {
                // 保持最近 200 条数据
                if (RecentDataPoints.Count >= 200)
                {
                    RecentDataPoints.RemoveAt(RecentDataPoints.Count - 1);
                }
                RecentDataPoints.Insert(0, new DataPreviewDataPoint
                {
                    Timestamp = data.Timestamp.LocalDateTime,
                    DeviceName = deviceName,
                    TagId = point.TagId,
                    TagName = point.TagName,
                    RawValue = LogViewModel.FormatTagValue(point.RawValue),
                    Value = LogViewModel.FormatTagValue(point.Value),
                    Unit = point.Unit ?? "",
                    Quality = data.Quality.ToString()
                });
            }
        });
    }

    [RelayCommand]
    private void ClearData()
    {
        RecentDataPoints.Clear();
    }

    [RelayCommand]
    private void ToggleAutoRefresh()
    {
        IsAutoRefresh = !IsAutoRefresh;
    }
}

/// <summary>
/// 数据预览数据点显示模型（将 object? 类型的值格式化为字符串）
/// </summary>
public class DataPreviewDataPoint
{
    public DateTime Timestamp { get; set; }
    public string DeviceName { get; set; } = string.Empty;
    public string TagId { get; set; } = string.Empty;
    public string TagName { get; set; } = string.Empty;
    public string RawValue { get; set; } = string.Empty;
    public string Value { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty;
    public string Quality { get; set; } = string.Empty;

    public string FormattedTime => Timestamp.ToString("HH:mm:ss.fff");
}
