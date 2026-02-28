using System.Collections.ObjectModel;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Collector.Core.Drivers;
using Collector.Core.Engine;
using Collector.Core.Models;
using Microsoft.Extensions.Logging;

namespace Collector.Agent.ViewModels;

/// <summary>
/// 诊断工具 ViewModel
/// </summary>
public partial class DiagnosticsViewModel : ViewModelBase
{
    private readonly ICollectionEngine _collectionEngine;
    private readonly ILogger<DiagnosticsViewModel> _logger;

    [ObservableProperty]
    private DeviceRuntimeStatus? _selectedDevice;

    [ObservableProperty]
    private string? _readResult;

    public ObservableCollection<DeviceRuntimeStatus> Devices { get; } = new();
    public ObservableCollection<TagReadResultDisplay> ReadResults { get; } = new();

    public DiagnosticsViewModel(ICollectionEngine collectionEngine, ILogger<DiagnosticsViewModel> logger)
    {
        _collectionEngine = collectionEngine;
        _logger = logger;

        // 监听配置加载事件：引擎每次加载配置后自动刷新设备列表
        _collectionEngine.ConfigLoaded += (_, _) =>
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(RefreshDevices);
        };

        // 监听设备状态变更事件：新设备上线/离线时同步更新
        _collectionEngine.DeviceStatusChanged += (_, status) =>
        {
            Avalonia.Threading.Dispatcher.UIThread.Post(() =>
            {
                var existing = Devices.FirstOrDefault(d => d.DeviceId == status.DeviceId);
                if (existing == null)
                {
                    // 新设备：加入列表
                    Devices.Add(status);
                }
                else
                {
                    // 已有设备：更新状态
                    var index = Devices.IndexOf(existing);
                    Devices[index] = status;

                    // 保持选中项同步
                    if (SelectedDevice?.DeviceId == status.DeviceId)
                        SelectedDevice = status;
                }
            });
        };

        RefreshDevices();
    }

    // ─── 刷新设备列表 ───────────────────────────────────────────────────────

    [RelayCommand]
    private void RefreshDevices()
    {
        var currentSelectedId = SelectedDevice?.DeviceId;
        Devices.Clear();
        foreach (var status in _collectionEngine.DeviceStatuses)
        {
            Devices.Add(status);
        }
        // 恢复上次选中的设备
        SelectedDevice = currentSelectedId.HasValue
            ? Devices.FirstOrDefault(d => d.DeviceId == currentSelectedId.Value)
            : Devices.FirstOrDefault();
    }

    // ─── 单设备：测试连接 ────────────────────────────────────────────────────

    [RelayCommand]
    private async Task TestConnectionAsync()
    {
        var device = SelectedDevice;
        if (device == null)
        {
            ReadResult = "⚠️ 请先选择一个设备";
            return;
        }

        SetBusy(true, $"正在测试 [{device.DeviceName}] 连接...");

        try
        {
            var result = await _collectionEngine.TestDeviceConnectionAsync(device.DeviceId);
            ReadResult = result.Success
                ? $"✅ [{device.DeviceName}] 连接成功 (响应: {result.ResponseTimeMs}ms)"
                : $"❌ [{device.DeviceName}] 连接失败: {result.ErrorMessage}";
        }
        catch (Exception ex)
        {
            ReadResult = $"❌ [{device.DeviceName}] 测试异常: {ex.Message}";
        }
        finally
        {
            SetBusy(false);
        }
    }

    // ─── 单设备：读取所有标签 ─────────────────────────────────────────────────

    [RelayCommand]
    private async Task ReadAllTagsAsync()
    {
        var device = SelectedDevice;
        if (device == null)
        {
            ReadResult = "⚠️ 请先选择一个设备";
            return;
        }

        SetBusy(true, $"正在读取 [{device.DeviceName}] 标签...");
        ReadResults.Clear();

        try
        {
            var results = await _collectionEngine.ReadDeviceTagsAsync(device.DeviceId);
            foreach (var r in results)
                ReadResults.Add(new TagReadResultDisplay(r, device.DeviceName));

            ReadResult = $"✅ [{device.DeviceName}] 读取完成，共 {results.Count} 个标签";
        }
        catch (Exception ex)
        {
            ReadResult = $"❌ [{device.DeviceName}] 读取失败: {ex.Message}";
            _logger.LogError(ex, "读取标签失败: {DeviceName}", device.DeviceName);
        }
        finally
        {
            SetBusy(false);
        }
    }

    // ─── 全部设备：测试连接 ──────────────────────────────────────────────────

    [RelayCommand]
    private async Task TestAllConnectionsAsync()
    {
        if (Devices.Count == 0)
        {
            ReadResult = "⚠️ 暂无设备，请先加载配置";
            return;
        }

        SetBusy(true, $"正在测试全部 {Devices.Count} 个设备连接...");

        var sb = new StringBuilder();
        int successCount = 0, failCount = 0;

        try
        {
            foreach (var device in Devices.ToList())
            {
                try
                {
                    var result = await _collectionEngine.TestDeviceConnectionAsync(device.DeviceId);
                    if (result.Success)
                    {
                        sb.AppendLine($"✅ [{device.DeviceName}] 连接成功 (响应: {result.ResponseTimeMs}ms)");
                        successCount++;
                    }
                    else
                    {
                        sb.AppendLine($"❌ [{device.DeviceName}] 连接失败: {result.ErrorMessage}");
                        failCount++;
                    }
                }
                catch (Exception ex)
                {
                    sb.AppendLine($"❌ [{device.DeviceName}] 测试异常: {ex.Message}");
                    failCount++;
                }
            }

            sb.AppendLine();
            sb.Append($"合计: {successCount} 成功 / {failCount} 失败");
            ReadResult = sb.ToString().TrimEnd();
        }
        finally
        {
            SetBusy(false);
        }
    }

    // ─── 全部设备：读取所有标签 ──────────────────────────────────────────────

    [RelayCommand]
    private async Task ReadAllDevicesTagsAsync()
    {
        if (Devices.Count == 0)
        {
            ReadResult = "⚠️ 暂无设备，请先加载配置";
            return;
        }

        SetBusy(true, $"正在读取全部 {Devices.Count} 个设备标签...");
        ReadResults.Clear();

        int totalTags = 0, failCount = 0;

        try
        {
            foreach (var device in Devices.ToList())
            {
                try
                {
                    var results = await _collectionEngine.ReadDeviceTagsAsync(device.DeviceId);
                    foreach (var r in results)
                        ReadResults.Add(new TagReadResultDisplay(r, device.DeviceName));
                    totalTags += results.Count;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "读取设备标签失败: {DeviceName}", device.DeviceName);
                    failCount++;
                }
            }

            ReadResult = failCount == 0
                ? $"✅ 全部读取完成，共 {totalTags} 个标签"
                : $"⚠️ 读取完成，共 {totalTags} 个标签，{failCount} 个设备失败";
        }
        finally
        {
            SetBusy(false);
        }
    }

    // ─── 值格式化工具 ────────────────────────────────────────────────────────

    /// <summary>
    /// 智能格式化 object? 值：
    /// - null → "-"
    /// - byte[] → "0x01 02 03"（Hex）
    /// - 数值类型 → ToString()
    /// - IEnumerable → "[a, b, c]"
    /// - 其他 → ToString()
    /// </summary>
    internal static string FormatValue(object? value)
    {
        if (value == null) return "-";

        if (value is byte[] bytes)
            return bytes.Length == 0 ? "(空)" : "0x" + BitConverter.ToString(bytes).Replace("-", " ");

        if (value is bool b)
            return b ? "True" : "False";

        if (value is float f)
            return f.ToString("G6");

        if (value is double d)
            return d.ToString("G10");

        // 数值类型直接 ToString
        if (value is int or uint or short or ushort or long or ulong
            or sbyte or byte or decimal)
            return value.ToString()!;

        // 数组或集合（非 byte[]）→ 逗号分隔
        if (value is System.Collections.IEnumerable enumerable and not string)
        {
            var items = enumerable.Cast<object?>().Select(FormatValue);
            return "[" + string.Join(", ", items) + "]";
        }

        return value.ToString() ?? "-";
    }
}

/// <summary>
/// TagReadResult 的显示包装类：将 object? 值格式化为字符串，避免 DataGrid 显示类型名
/// </summary>
public class TagReadResultDisplay
{
    public string DeviceName { get; }
    public string TagId { get; }
    public string TagName { get; }
    public bool Success { get; }
    public string SuccessText { get; }
    public string RawValueText { get; }
    public string ValueText { get; }
    public string Quality { get; }
    public string? ErrorMessage { get; }
    public string Timestamp { get; }

    public TagReadResultDisplay(TagReadResult r, string deviceName = "")
    {
        DeviceName = deviceName;
        TagId = r.TagId;
        TagName = r.TagName;
        Success = r.Success;
        SuccessText = r.Success ? "✅" : "❌";
        RawValueText = DiagnosticsViewModel.FormatValue(r.RawValue);
        ValueText = DiagnosticsViewModel.FormatValue(r.Value);
        Quality = r.Quality.ToString();
        ErrorMessage = r.ErrorMessage;
        Timestamp = r.Timestamp.LocalDateTime.ToString("HH:mm:ss.fff");
    }
}
