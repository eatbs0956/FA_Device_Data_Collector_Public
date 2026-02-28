using Shared.Tsdb.Models;

namespace Shared.Tsdb;

/// <summary>
/// InfluxDB 服务接口
/// </summary>
public interface IInfluxDbService : IDisposable
{
    #region 写入操作

    /// <summary>
    /// 写入设备数据点
    /// </summary>
    /// <param name="dataPoint">数据点</param>
    /// <param name="bucket">目标 bucket（默认为 collected）</param>
    Task WriteDeviceDataAsync(DeviceDataPoint dataPoint, string? bucket = null);

    /// <summary>
    /// 批量写入设备数据点
    /// </summary>
    /// <param name="dataPoints">数据点列表</param>
    /// <param name="bucket">目标 bucket（默认为 collected）</param>
    Task WriteDeviceDataBatchAsync(IEnumerable<DeviceDataPoint> dataPoints, string? bucket = null);

    /// <summary>
    /// 写入设备状态
    /// </summary>
    /// <param name="statusPoint">状态数据点</param>
    /// <param name="bucket">目标 bucket（默认为 collected）</param>
    Task WriteDeviceStatusAsync(DeviceStatusPoint statusPoint, string? bucket = null);

    /// <summary>
    /// 写入系统指标
    /// </summary>
    /// <param name="metricsPoint">指标数据点</param>
    Task WriteSystemMetricsAsync(SystemMetricsPoint metricsPoint);

    #endregion

    #region 查询操作

    /// <summary>
    /// 查询设备数据
    /// </summary>
    /// <param name="query">查询参数</param>
    /// <param name="bucket">源 bucket（默认为 collected）</param>
    /// <returns>数据结果列表</returns>
    Task<List<DeviceDataResult>> QueryDeviceDataAsync(DeviceDataQuery query, string? bucket = null);

    /// <summary>
    /// 获取设备最新数据
    /// </summary>
    /// <param name="tenant">租户ID</param>
    /// <param name="device">设备ID（可选）</param>
    /// <param name="bucket">源 bucket（默认为 collected）</param>
    /// <returns>最新数据列表</returns>
    Task<List<DeviceLatestData>> GetLatestDeviceDataAsync(string tenant, string? device = null, string? bucket = null);

    /// <summary>
    /// 获取设备状态
    /// </summary>
    /// <param name="tenant">租户ID</param>
    /// <param name="device">设备ID（可选）</param>
    /// <returns>设备状态列表</returns>
    Task<List<DeviceStatusResult>> GetDeviceStatusAsync(string tenant, string? device = null);

    /// <summary>
    /// 执行自定义 Flux 查询
    /// </summary>
    /// <param name="fluxQuery">Flux 查询语句</param>
    /// <returns>查询结果</returns>
    Task<List<Dictionary<string, object?>>> QueryRawAsync(string fluxQuery);

    #endregion

    #region 管理操作

    /// <summary>
    /// 检查连接状态
    /// </summary>
    /// <returns>是否连接成功</returns>
    Task<bool> PingAsync();

    /// <summary>
    /// 确保 Bucket 存在
    /// </summary>
    /// <param name="bucketName">bucket 名称</param>
    /// <param name="retentionDays">保留天数</param>
    Task EnsureBucketExistsAsync(string bucketName, int retentionDays);

    #endregion
}
