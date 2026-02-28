using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Tsdb.Models;

namespace Shared.Tsdb;

/// <summary>
/// InfluxDB 服务实现
/// </summary>
public class InfluxDbService : IInfluxDbService
{
    private readonly InfluxDBClient _client;
    private readonly InfluxDbOptions _options;
    private readonly ILogger<InfluxDbService> _logger;

    public InfluxDbService(IOptions<InfluxDbOptions> options, ILogger<InfluxDbService> logger)
    {
        _options = options.Value;
        _logger = logger;
        _client = new InfluxDBClient(_options.Url, _options.Token);
    }

    #region 写入操作

    public async Task WriteDeviceDataAsync(DeviceDataPoint dataPoint, string? bucket = null)
    {
        var targetBucket = bucket ?? _options.DefaultBucket;
        var writeApi = _client.GetWriteApiAsync();

        var point = CreatePointFromDeviceData(dataPoint);
        await writeApi.WritePointAsync(point, targetBucket, _options.Org);

        _logger.LogDebug("写入设备数据: {Tenant}/{Device}, 字段数: {FieldCount}",
            dataPoint.Tenant, dataPoint.Device, dataPoint.Fields.Count);
    }

    public async Task WriteDeviceDataBatchAsync(IEnumerable<DeviceDataPoint> dataPoints, string? bucket = null)
    {
        var targetBucket = bucket ?? _options.DefaultBucket;
        var writeApi = _client.GetWriteApiAsync();

        var points = dataPoints.Select(CreatePointFromDeviceData).ToList();
        await writeApi.WritePointsAsync(points, targetBucket, _options.Org);

        _logger.LogDebug("批量写入设备数据: {Count} 条", points.Count);
    }

    public async Task WriteDeviceStatusAsync(DeviceStatusPoint statusPoint, string? bucket = null)
    {
        var targetBucket = bucket ?? _options.DefaultBucket;
        var writeApi = _client.GetWriteApiAsync();

        var point = PointData.Measurement(TsdbMeasurements.DeviceStatus)
            .Tag("tenant", statusPoint.Tenant)
            .Tag("device", statusPoint.Device)
            .Field("status", statusPoint.Status)
            .Field("message", statusPoint.Message ?? "")
            .Timestamp(statusPoint.Timestamp, WritePrecision.Ms);

        await writeApi.WritePointAsync(point, targetBucket, _options.Org);

        _logger.LogDebug("写入设备状态: {Tenant}/{Device} -> {Status}",
            statusPoint.Tenant, statusPoint.Device, statusPoint.Status);
    }

    public async Task WriteSystemMetricsAsync(SystemMetricsPoint metricsPoint)
    {
        var writeApi = _client.GetWriteApiAsync();

        var point = PointData.Measurement(TsdbMeasurements.SystemMetrics)
            .Tag("tenant", metricsPoint.Tenant)
            .Tag("node", metricsPoint.Node)
            .Field("cpu_usage", metricsPoint.CpuUsage)
            .Field("memory_usage", metricsPoint.MemoryUsage)
            .Field("disk_usage", metricsPoint.DiskUsage)
            .Field("network_io", metricsPoint.NetworkIo)
            .Timestamp(metricsPoint.Timestamp, WritePrecision.Ms);

        await writeApi.WritePointAsync(point, TsdbBuckets.Metrics, _options.Org);

        _logger.LogDebug("写入系统指标: {Tenant}/{Node}", metricsPoint.Tenant, metricsPoint.Node);
    }

    private static PointData CreatePointFromDeviceData(DeviceDataPoint dataPoint)
    {
        var point = PointData.Measurement(TsdbMeasurements.DeviceData)
            .Tag("tenant", dataPoint.Tenant)
            .Tag("device", dataPoint.Device)
            .Tag("source", dataPoint.Source)
            .Timestamp(dataPoint.Timestamp, WritePrecision.Ms);

        foreach (var field in dataPoint.Fields)
        {
            point = field.Value switch
            {
                int intVal => point.Field(field.Key, intVal),
                long longVal => point.Field(field.Key, longVal),
                float floatVal => point.Field(field.Key, floatVal),
                double doubleVal => point.Field(field.Key, doubleVal),
                bool boolVal => point.Field(field.Key, boolVal),
                string strVal => point.Field(field.Key, strVal),
                _ => point.Field(field.Key, field.Value?.ToString() ?? "")
            };
        }

        return point;
    }

    #endregion

    #region 查询操作

    public async Task<List<DeviceDataResult>> QueryDeviceDataAsync(DeviceDataQuery query, string? bucket = null)
    {
        var targetBucket = bucket ?? _options.DefaultBucket;
        var queryApi = _client.GetQueryApi();

        var fluxQuery = BuildDeviceDataQuery(query, targetBucket);
        _logger.LogDebug("执行 Flux 查询: {Query}", fluxQuery);

        var tables = await queryApi.QueryAsync(fluxQuery, _options.Org);
        var results = new List<DeviceDataResult>();

        foreach (var table in tables)
        {
            foreach (var record in table.Records)
            {
                results.Add(new DeviceDataResult
                {
                    Timestamp = record.GetTimeInDateTime() ?? DateTime.UtcNow,
                    Device = record.GetValueByKey("device")?.ToString() ?? "",
                    Field = record.GetField() ?? "",
                    Value = record.GetValue()
                });
            }
        }

        return results;
    }

    public async Task<List<DeviceLatestData>> GetLatestDeviceDataAsync(string tenant, string? device = null, string? bucket = null)
    {
        var targetBucket = bucket ?? _options.DefaultBucket;
        var queryApi = _client.GetQueryApi();

        var deviceFilter = string.IsNullOrEmpty(device)
            ? ""
            : $@"|> filter(fn: (r) => r[""device""] == ""{device}"")";

        var fluxQuery = $@"
from(bucket: ""{targetBucket}"")
  |> range(start: -1h)
  |> filter(fn: (r) => r[""_measurement""] == ""{TsdbMeasurements.DeviceData}"")
  |> filter(fn: (r) => r[""tenant""] == ""{tenant}"")
  {deviceFilter}
  |> last()
  |> pivot(rowKey: [""_time"", ""device""], columnKey: [""_field""], valueColumn: ""_value"")
";

        var tables = await queryApi.QueryAsync(fluxQuery, _options.Org);
        var results = new Dictionary<string, DeviceLatestData>();

        foreach (var table in tables)
        {
            foreach (var record in table.Records)
            {
                var deviceId = record.GetValueByKey("device")?.ToString() ?? "";
                if (string.IsNullOrEmpty(deviceId)) continue;

                if (!results.TryGetValue(deviceId, out var data))
                {
                    data = new DeviceLatestData
                    {
                        Device = deviceId,
                        Timestamp = record.GetTimeInDateTime() ?? DateTime.UtcNow
                    };
                    results[deviceId] = data;
                }

                // 提取所有字段值
                foreach (var col in record.Values)
                {
                    if (col.Key.StartsWith("_") || col.Key == "device" || col.Key == "tenant" || col.Key == "source")
                        continue;
                    data.Fields[col.Key] = col.Value;
                }
            }
        }

        return results.Values.ToList();
    }

    public async Task<List<DeviceStatusResult>> GetDeviceStatusAsync(string tenant, string? device = null)
    {
        var queryApi = _client.GetQueryApi();

        var deviceFilter = string.IsNullOrEmpty(device)
            ? ""
            : $@"|> filter(fn: (r) => r[""device""] == ""{device}"")";

        var fluxQuery = $@"
from(bucket: ""{_options.DefaultBucket}"")
  |> range(start: -24h)
  |> filter(fn: (r) => r[""_measurement""] == ""{TsdbMeasurements.DeviceStatus}"")
  |> filter(fn: (r) => r[""tenant""] == ""{tenant}"")
  {deviceFilter}
  |> last()
  |> pivot(rowKey: [""_time"", ""device""], columnKey: [""_field""], valueColumn: ""_value"")
";

        var tables = await queryApi.QueryAsync(fluxQuery, _options.Org);
        var results = new List<DeviceStatusResult>();

        foreach (var table in tables)
        {
            foreach (var record in table.Records)
            {
                results.Add(new DeviceStatusResult
                {
                    Device = record.GetValueByKey("device")?.ToString() ?? "",
                    Status = record.GetValueByKey("status")?.ToString() ?? "offline",
                    Message = record.GetValueByKey("message")?.ToString(),
                    UpdatedAt = record.GetTimeInDateTime() ?? DateTime.UtcNow
                });
            }
        }

        return results;
    }

    public async Task<List<Dictionary<string, object?>>> QueryRawAsync(string fluxQuery)
    {
        var queryApi = _client.GetQueryApi();
        var tables = await queryApi.QueryAsync(fluxQuery, _options.Org);
        var results = new List<Dictionary<string, object?>>();

        foreach (var table in tables)
        {
            foreach (var record in table.Records)
            {
                results.Add(record.Values.ToDictionary(x => x.Key, x => x.Value));
            }
        }

        return results;
    }

    private string BuildDeviceDataQuery(DeviceDataQuery query, string bucket)
    {
        var filters = new List<string>
        {
            $@"r[""_measurement""] == ""{TsdbMeasurements.DeviceData}""",
            $@"r[""tenant""] == ""{query.Tenant}"""
        };

        if (!string.IsNullOrEmpty(query.Device))
        {
            filters.Add($@"r[""device""] == ""{query.Device}""");
        }

        if (query.Fields?.Count > 0)
        {
            var fieldFilters = query.Fields.Select(f => $@"r[""_field""] == ""{f}""");
            filters.Add($"({string.Join(" or ", fieldFilters)})");
        }

        var filterClause = string.Join(" and ", filters);

        var aggregateClause = string.IsNullOrEmpty(query.AggregateWindow)
            ? ""
            : $@"|> aggregateWindow(every: {query.AggregateWindow}, fn: {query.AggregateFn}, createEmpty: false)";

        var limitClause = query.Limit.HasValue
            ? $"|> limit(n: {query.Limit.Value})"
            : "";

        return $@"
from(bucket: ""{bucket}"")
  |> range(start: {query.StartTime:yyyy-MM-ddTHH:mm:ssZ}, stop: {query.EndTime:yyyy-MM-ddTHH:mm:ssZ})
  |> filter(fn: (r) => {filterClause})
  {aggregateClause}
  {limitClause}
";
    }

    #endregion

    #region 管理操作

    public async Task<bool> PingAsync()
    {
        try
        {
            var health = await _client.PingAsync();
            return health;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "InfluxDB 连接检查失败");
            return false;
        }
    }

    public async Task EnsureBucketExistsAsync(string bucketName, int retentionDays)
    {
        var bucketsApi = _client.GetBucketsApi();
        var orgsApi = _client.GetOrganizationsApi();

        var org = (await orgsApi.FindOrganizationsAsync(org: _options.Org)).FirstOrDefault();
        if (org == null)
        {
            _logger.LogError("组织 {Org} 不存在", _options.Org);
            return;
        }

        var bucket = await bucketsApi.FindBucketByNameAsync(bucketName);
        if (bucket == null)
        {
            var retention = new BucketRetentionRules(
                BucketRetentionRules.TypeEnum.Expire,
                retentionDays * 24 * 60 * 60 // 转换为秒
            );

            await bucketsApi.CreateBucketAsync(bucketName, retention, org.Id);
            _logger.LogInformation("创建 Bucket: {BucketName}, 保留 {Days} 天", bucketName, retentionDays);
        }
    }

    #endregion

    public void Dispose()
    {
        _client.Dispose();
    }
}
