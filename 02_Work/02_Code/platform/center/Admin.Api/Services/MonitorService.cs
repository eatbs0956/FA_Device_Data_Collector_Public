using Admin.Api.Domain.DTOs;
using Microsoft.EntityFrameworkCore;
using Shared.Domain.Data;
using Shared.Domain.Entities;
using Shared.Tsdb;
using Shared.Tsdb.Models;
using System.Text.Json;

namespace Admin.Api.Services;

/// <summary>
/// 监控数据服务实现
/// </summary>
public class MonitorService : IMonitorService
{
    private readonly UnifiedDbContext _dbContext;
    private readonly IInfluxDbService _influxDbService;
    private readonly ILogger<MonitorService> _logger;

    // 自动采样策略：时间范围 -> 采样间隔
    private static readonly Dictionary<TimeSpan, string> AutoSampleIntervals = new()
    {
        { TimeSpan.FromHours(1), "" },        // ≤1小时：原始数据
        { TimeSpan.FromHours(24), "1m" },     // ≤24小时：1分钟
        { TimeSpan.FromDays(7), "5m" },       // ≤7天：5分钟
        { TimeSpan.MaxValue, "1h" }           // >7天：1小时
    };

    public MonitorService(
        UnifiedDbContext dbContext,
        IInfluxDbService influxDbService,
        ILogger<MonitorService> logger)
    {
        _dbContext = dbContext;
        _influxDbService = influxDbService;
        _logger = logger;
    }

    #region 实时监控

    /// <inheritdoc />
    public async Task<List<DeviceMonitorData>> GetDevicesMonitorDataAsync(
        Guid? groupId = null,
        Guid? nodeId = null,
        string? connectionStatus = null)
    {
        // 1. 从数据库获取设备基础信息
        var query = _dbContext.Set<Device>()
            .Include(d => d.Group)
            .Include(d => d.EdgeNode)
            .Include(d => d.TagDefinitions)
            .Where(d => !d.DeletedFlag && d.Enabled);

        if (groupId.HasValue)
            query = query.Where(d => d.GroupId == groupId);

        if (nodeId.HasValue)
            query = query.Where(d => d.EdgeNodeId == nodeId);

        if (!string.IsNullOrWhiteSpace(connectionStatus))
            query = query.Where(d => d.ConnectionStatus == connectionStatus);

        var devices = await query.OrderBy(d => d.DeviceName).ToListAsync();

        if (devices.Count == 0)
            return [];

        // 2. 获取租户ID
        var tenantId = _dbContext.CurrentTenantId;

        // 3. 从 InfluxDB 获取最新数据
        var latestDataList = await _influxDbService.GetLatestDeviceDataAsync(tenantId);
        var latestDataDict = latestDataList.ToDictionary(d => d.Device, d => d);

        // 4. 组装结果
        var result = new List<DeviceMonitorData>();
        foreach (var device in devices)
        {
            var monitorData = new DeviceMonitorData
            {
                Id = device.Id,
                DeviceId = device.DeviceId,
                DeviceName = device.DeviceName,
                ConnectionStatus = device.ConnectionStatus,
                Enabled = device.Enabled,
                GroupName = device.Group?.Name,
                NodeName = device.EdgeNode?.NodeName,
                KeyTags = []
            };

            // 获取关键标签配置
            var keyTagNames = GetKeyTags(device);

            // 从 InfluxDB 数据中获取标签值
            if (latestDataDict.TryGetValue(device.DeviceId, out var latestData))
            {
                monitorData.LastUpdateTime = latestData.Timestamp;

                // 获取标签定义用于显示名和单位
                var tagDefinitions = device.TagDefinitions.ToDictionary(t => t.TagName);

                foreach (var tagName in keyTagNames)
                {
                    var tagValue = new TagValueItem
                    {
                        TagName = tagName,
                        UpdatedAt = latestData.Timestamp
                    };

                    if (tagDefinitions.TryGetValue(tagName, out var tagDef))
                    {
                        tagValue.DisplayName = tagDef.Description;
                        tagValue.Unit = tagDef.Unit;
                    }

                    if (latestData.Fields.TryGetValue(tagName, out var value))
                    {
                        tagValue.Value = value;
                    }

                    monitorData.KeyTags.Add(tagValue);
                }
            }

            result.Add(monitorData);
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<DeviceDetailData?> GetDeviceLatestDataAsync(Guid deviceId)
    {
        // 1. 获取设备信息
        var device = await _dbContext.Set<Device>()
            .Include(d => d.TagDefinitions)
            .FirstOrDefaultAsync(d => d.Id == deviceId && !d.DeletedFlag);

        if (device == null)
            return null;

        // 2. 获取租户ID
        var tenantId = _dbContext.CurrentTenantId;

        // 3. 从 InfluxDB 获取最新数据
        var latestDataList = await _influxDbService.GetLatestDeviceDataAsync(tenantId, device.DeviceId);
        var latestData = latestDataList.FirstOrDefault();

        // 4. 组装结果
        var result = new DeviceDetailData
        {
            Id = device.Id,
            DeviceId = device.DeviceId,
            DeviceName = device.DeviceName,
            ConnectionStatus = device.ConnectionStatus,
            DeviceType = device.DeviceType,
            ProtocolType = device.ProtocolType,
            Location = device.Location,
            LastConnectTime = device.LastConnectTime?.DateTime,
            AllTags = []
        };

        // 填充所有标签值
        foreach (var tagDef in device.TagDefinitions.OrderBy(t => t.TagName))
        {
            var tagValue = new TagValueItem
            {
                TagName = tagDef.TagName,
                DisplayName = tagDef.Description,
                Unit = tagDef.Unit
            };

            if (latestData != null)
            {
                tagValue.UpdatedAt = latestData.Timestamp;
                if (latestData.Fields.TryGetValue(tagDef.TagName, out var value))
                {
                    tagValue.Value = value;
                }
            }

            result.AllTags.Add(tagValue);
        }

        return result;
    }

    #endregion

    #region 历史数据

    /// <inheritdoc />
    public async Task<List<DeviceTreeNode>> GetDeviceTreeAsync()
    {
        // 获取所有分组
        var groups = await _dbContext.Set<DeviceGroup>()
            .Where(g => !g.DeletedFlag)
            .OrderBy(g => g.Name)
            .ToListAsync();

        // 获取所有设备（含标签定义）
        var devices = await _dbContext.Set<Device>()
            .Include(d => d.TagDefinitions)
            .Where(d => !d.DeletedFlag && d.Enabled)
            .OrderBy(d => d.DeviceName)
            .ToListAsync();

        var result = new List<DeviceTreeNode>();

        // 有分组的设备
        foreach (var group in groups)
        {
            var groupDevices = devices.Where(d => d.GroupId == group.Id).ToList();

            var groupNode = new DeviceTreeNode
            {
                Id = $"group_{group.Id}",
                Label = group.Name,
                Type = "group",
                IsLeaf = false,
                Children = groupDevices.Select(d => BuildDeviceNode(d)).ToList()
            };

            result.Add(groupNode);
        }

        // 未分组的设备
        var ungroupedDevices = devices.Where(d => d.GroupId == null).ToList();
        if (ungroupedDevices.Count > 0)
        {
            var ungroupedNode = new DeviceTreeNode
            {
                Id = "group_ungrouped",
                Label = "未分组设备",
                Type = "group",
                IsLeaf = false,
                Children = ungroupedDevices.Select(d => BuildDeviceNode(d)).ToList()
            };

            result.Add(ungroupedNode);
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<HistoryDataResult> GetDeviceHistoryAsync(Guid deviceId, HistoryQueryRequest request)
    {
        // 1. 获取设备信息
        var device = await _dbContext.Set<Device>()
            .FirstOrDefaultAsync(d => d.Id == deviceId && !d.DeletedFlag);

        if (device == null)
            throw new ArgumentException($"设备不存在: {deviceId}");

        // 2. 确定时间范围
        var endTime = request.End ?? DateTime.UtcNow;
        var startTime = request.Start ?? endTime.AddHours(-1); // 默认最近1小时

        // 3. 确定采样间隔
        var interval = request.Interval;
        if (string.IsNullOrWhiteSpace(interval))
        {
            interval = GetAutoSampleInterval(endTime - startTime);
        }

        // 4. 解析标签列表
        var tags = string.IsNullOrWhiteSpace(request.Tags)
            ? null
            : request.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList();

        // 5. 构建查询
        var tenantId = _dbContext.CurrentTenantId;
        var query = new DeviceDataQuery
        {
            Tenant = tenantId,
            Device = device.DeviceId,
            StartTime = startTime,
            EndTime = endTime,
            Fields = tags,
            AggregateWindow = interval,
            AggregateFn = request.AggregateFn,
            Limit = request.Limit
        };

        // 6. 执行查询
        var queryResults = await _influxDbService.QueryDeviceDataAsync(query);

        // 7. 转换结果为时间序列格式
        var result = new HistoryDataResult
        {
            DeviceId = device.DeviceId,
            Series = []
        };

        // 按时间戳分组
        var groupedByTime = queryResults
            .GroupBy(r => r.Timestamp)
            .OrderBy(g => g.Key);

        foreach (var group in groupedByTime)
        {
            var point = new TimeSeriesPoint
            {
                Timestamp = group.Key,
                Values = group.ToDictionary(r => r.Field, r => r.Value)
            };
            result.Series.Add(point);
        }

        return result;
    }

    #endregion

    #region 统计报表

    /// <inheritdoc />
    public async Task<List<StatisticsResult>> GetStatisticsByDevicesAsync(StatisticsQueryRequest request)
    {
        var tenantId = _dbContext.CurrentTenantId;
        var endTime = request.End ?? DateTime.UtcNow;
        var startTime = request.Start ?? endTime.AddDays(-7);

        // 获取设备列表
        var deviceQuery = _dbContext.Set<Device>()
            .Where(d => !d.DeletedFlag && d.Enabled);

        if (!string.IsNullOrWhiteSpace(request.DeviceIds))
        {
            var deviceIdList = request.DeviceIds.Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(id => Guid.Parse(id.Trim()))
                .ToList();
            deviceQuery = deviceQuery.Where(d => deviceIdList.Contains(d.Id));
        }

        var devices = await deviceQuery.ToListAsync();
        var results = new List<StatisticsResult>();

        foreach (var device in devices)
        {
            var stats = await GetDeviceStatisticsAsync(
                tenantId, device.DeviceId, device.DeviceName, startTime, endTime, request.Granularity);
            stats.Dimension = "device";
            results.Add(stats);
        }

        return results;
    }

    /// <inheritdoc />
    public async Task<List<StatisticsResult>> GetStatisticsByGroupsAsync(StatisticsQueryRequest request)
    {
        var tenantId = _dbContext.CurrentTenantId;
        var endTime = request.End ?? DateTime.UtcNow;
        var startTime = request.Start ?? endTime.AddDays(-7);

        // 获取分组及其设备
        var groupQuery = _dbContext.Set<DeviceGroup>()
            .Include(g => g.Devices.Where(d => !d.DeletedFlag && d.Enabled))
            .Where(g => !g.DeletedFlag);

        if (request.GroupId.HasValue)
            groupQuery = groupQuery.Where(g => g.Id == request.GroupId);

        var groups = await groupQuery.ToListAsync();
        var results = new List<StatisticsResult>();

        foreach (var group in groups)
        {
            var deviceIds = group.Devices.Select(d => d.DeviceId).ToList();
            if (deviceIds.Count == 0) continue;

            var stats = await GetAggregatedStatisticsAsync(
                tenantId, deviceIds, group.Id.ToString(), group.Name, startTime, endTime, request.Granularity);
            stats.Dimension = "group";
            results.Add(stats);
        }

        return results;
    }

    /// <inheritdoc />
    public async Task<List<StatisticsResult>> GetStatisticsByNodesAsync(StatisticsQueryRequest request)
    {
        var tenantId = _dbContext.CurrentTenantId;
        var endTime = request.End ?? DateTime.UtcNow;
        var startTime = request.Start ?? endTime.AddDays(-7);

        // 获取节点及其设备
        var nodeQuery = _dbContext.Set<EdgeNode>()
            .Include(n => n.Devices.Where(d => !d.DeletedFlag && d.Enabled))
            .Where(n => !n.DeletedFlag);

        if (request.NodeId.HasValue)
            nodeQuery = nodeQuery.Where(n => n.Id == request.NodeId);

        var nodes = await nodeQuery.ToListAsync();
        var results = new List<StatisticsResult>();

        foreach (var node in nodes)
        {
            var deviceIds = node.Devices.Select(d => d.DeviceId).ToList();
            if (deviceIds.Count == 0) continue;

            var stats = await GetAggregatedStatisticsAsync(
                tenantId, deviceIds, node.Id.ToString(), node.NodeName, startTime, endTime, request.Granularity);
            stats.Dimension = "node";
            results.Add(stats);
        }

        return results;
    }

    #endregion

    #region 仪表盘

    /// <inheritdoc />
    public async Task<DashboardSummary> GetDashboardSummaryAsync()
    {
        var tenantId = _dbContext.CurrentTenantId;
        var now = DateTime.UtcNow;
        var todayStart = now.Date;
        var yesterdayStart = todayStart.AddDays(-1);

        // 1. 设备统计
        var deviceStats = await _dbContext.Set<Device>()
            .Where(d => !d.DeletedFlag)
            .GroupBy(d => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Online = g.Count(d => d.ConnectionStatus == "Connected"),
                Offline = g.Count(d => d.ConnectionStatus == "Disconnected"),
                Error = g.Count(d => d.ConnectionStatus == "Error")
            })
            .FirstOrDefaultAsync();

        // 2. 节点统计
        var nodeStats = await _dbContext.Set<EdgeNode>()
            .Where(n => !n.DeletedFlag)
            .GroupBy(n => 1)
            .Select(g => new
            {
                Total = g.Count(),
                Online = g.Count(n => n.Status == "Online")
            })
            .FirstOrDefaultAsync();

        // 3. 分组统计
        var groupStats = await _dbContext.Set<DeviceGroup>()
            .Include(g => g.Devices.Where(d => !d.DeletedFlag))
            .Where(g => !g.DeletedFlag)
            .Select(g => new GroupDeviceCount
            {
                GroupId = g.Id,
                GroupName = g.Name,
                DeviceCount = g.Devices.Count,
                OnlineCount = g.Devices.Count(d => d.ConnectionStatus == "Connected")
            })
            .ToListAsync();

        // 4. 采集数据量统计（从 InfluxDB 获取）
        var todayDataPoints = await GetDataPointCountAsync(tenantId, todayStart, now);
        var yesterdayDataPoints = await GetDataPointCountAsync(tenantId, yesterdayStart, todayStart);

        // 5. 组装结果
        var summary = new DashboardSummary
        {
            TotalDevices = deviceStats?.Total ?? 0,
            OnlineDevices = deviceStats?.Online ?? 0,
            OfflineDevices = deviceStats?.Offline ?? 0,
            ErrorDevices = deviceStats?.Error ?? 0,
            OnlineRate = deviceStats?.Total > 0 
                ? Math.Round((double)(deviceStats.Online) / deviceStats.Total * 100, 2) 
                : 0,
            TotalNodes = nodeStats?.Total ?? 0,
            OnlineNodes = nodeStats?.Online ?? 0,
            TodayDataPoints = todayDataPoints,
            YesterdayDataPoints = yesterdayDataPoints,
            TodayAlerts = 0, // TODO: 从告警表获取
            UnhandledAlerts = 0, // TODO: 从告警表获取
            GroupStats = groupStats,
            RecentAlerts = [], // TODO: 从告警表获取
            CollectionTrend = await GetCollectionTrendAsync(tenantId)
        };

        return summary;
    }

    #endregion

    #region 私有方法

    /// <summary>
    /// 获取设备的关键标签列表
    /// </summary>
    private List<string> GetKeyTags(Device device)
    {
        // 尝试从 TagsConfig 中解析关键标签
        if (!string.IsNullOrWhiteSpace(device.TagsConfig) && device.TagsConfig != "[]")
        {
            try
            {
                var tagsConfig = JsonSerializer.Deserialize<JsonElement>(device.TagsConfig);
                
                // 查找 keyTags 配置
                if (tagsConfig.ValueKind == JsonValueKind.Object && 
                    tagsConfig.TryGetProperty("keyTags", out var keyTagsElement))
                {
                    var keyTags = keyTagsElement.EnumerateArray()
                        .Select(e => e.GetString())
                        .Where(s => !string.IsNullOrWhiteSpace(s))
                        .Cast<string>()
                        .ToList();

                    if (keyTags.Count > 0)
                        return keyTags;
                }
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(ex, "解析设备 {DeviceId} 的 TagsConfig 失败", device.DeviceId);
            }
        }

        // 没有配置关键标签，返回前5个标签定义
        return device.TagDefinitions
            .OrderBy(t => t.TagName)
            .Take(5)
            .Select(t => t.TagName)
            .ToList();
    }

    /// <summary>
    /// 构建设备节点（含标签子节点）
    /// </summary>
    private DeviceTreeNode BuildDeviceNode(Device device)
    {
        var deviceNode = new DeviceTreeNode
        {
            Id = $"device_{device.Id}",
            Label = device.DeviceName,
            Type = "device",
            IsLeaf = device.TagDefinitions.Count == 0,
            DeviceId = device.DeviceId,
            Status = device.ConnectionStatus,
            Children = device.TagDefinitions
                .OrderBy(t => t.TagName)
                .Select(t => new DeviceTreeNode
                {
                    Id = $"tag_{device.Id}_{t.TagName}",
                    Label = string.IsNullOrWhiteSpace(t.Description) ? t.TagName : $"{t.Description} ({t.TagName})",
                    Type = "tag",
                    IsLeaf = true,
                    DeviceId = device.DeviceId
                })
                .ToList()
        };

        return deviceNode;
    }

    /// <summary>
    /// 根据时间范围自动选择采样间隔
    /// </summary>
    private string GetAutoSampleInterval(TimeSpan duration)
    {
        foreach (var kv in AutoSampleIntervals)
        {
            if (duration <= kv.Key)
                return kv.Value;
        }
        return "1h";
    }

    /// <summary>
    /// 获取单个设备的统计数据
    /// </summary>
    private async Task<StatisticsResult> GetDeviceStatisticsAsync(
        string tenantId, string deviceId, string deviceName,
        DateTime startTime, DateTime endTime, string granularity)
    {
        var query = new DeviceDataQuery
        {
            Tenant = tenantId,
            Device = deviceId,
            StartTime = startTime,
            EndTime = endTime,
            AggregateWindow = granularity,
            AggregateFn = "mean"
        };

        var queryResults = await _influxDbService.QueryDeviceDataAsync(query);

        // 按时间窗口分组统计
        var items = queryResults
            .GroupBy(r => r.Timestamp)
            .Select(g => new StatisticsItem
            {
                PeriodStart = g.Key,
                PeriodEnd = g.Key.Add(ParseDuration(granularity)),
                DataPointCount = g.Count(),
                TagAggregations = g.GroupBy(r => r.Field)
                    .ToDictionary(
                        fg => fg.Key,
                        fg => new AggregatedValue
                        {
                            Avg = fg.Average(r => Convert.ToDouble(r.Value ?? 0)),
                            Count = fg.Count()
                        })
            })
            .OrderBy(i => i.PeriodStart)
            .ToList();

        return new StatisticsResult
        {
            DimensionId = deviceId,
            DimensionName = deviceName,
            Items = items
        };
    }

    /// <summary>
    /// 获取多设备聚合统计数据
    /// </summary>
    private async Task<StatisticsResult> GetAggregatedStatisticsAsync(
        string tenantId, List<string> deviceIds, string dimensionId, string dimensionName,
        DateTime startTime, DateTime endTime, string granularity)
    {
        var allResults = new List<DeviceDataResult>();

        foreach (var deviceId in deviceIds)
        {
            var query = new DeviceDataQuery
            {
                Tenant = tenantId,
                Device = deviceId,
                StartTime = startTime,
                EndTime = endTime,
                AggregateWindow = granularity,
                AggregateFn = "mean"
            };

            var results = await _influxDbService.QueryDeviceDataAsync(query);
            allResults.AddRange(results);
        }

        // 按时间窗口分组统计
        var items = allResults
            .GroupBy(r => r.Timestamp)
            .Select(g => new StatisticsItem
            {
                PeriodStart = g.Key,
                PeriodEnd = g.Key.Add(ParseDuration(granularity)),
                DataPointCount = g.Count(),
                OnlineDeviceCount = g.Select(r => r.Device).Distinct().Count()
            })
            .OrderBy(i => i.PeriodStart)
            .ToList();

        return new StatisticsResult
        {
            DimensionId = dimensionId,
            DimensionName = dimensionName,
            Items = items
        };
    }

    /// <summary>
    /// 获取数据点计数
    /// </summary>
    private async Task<long> GetDataPointCountAsync(string tenantId, DateTime startTime, DateTime endTime)
    {
        try
        {
            var query = new DeviceDataQuery
            {
                Tenant = tenantId,
                StartTime = startTime,
                EndTime = endTime,
                AggregateFn = "count"
            };

            var results = await _influxDbService.QueryDeviceDataAsync(query);
            return results.Sum(r => Convert.ToInt64(r.Value ?? 0));
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "获取数据点计数失败");
            return 0;
        }
    }

    /// <summary>
    /// 获取采集趋势数据（最近24小时）
    /// </summary>
    private async Task<List<TrendPoint>> GetCollectionTrendAsync(string tenantId)
    {
        try
        {
            var endTime = DateTime.UtcNow;
            var startTime = endTime.AddHours(-24);

            var query = new DeviceDataQuery
            {
                Tenant = tenantId,
                StartTime = startTime,
                EndTime = endTime,
                AggregateWindow = "1h",
                AggregateFn = "count"
            };

            var results = await _influxDbService.QueryDeviceDataAsync(query);

            return results
                .GroupBy(r => r.Timestamp)
                .Select(g => new TrendPoint
                {
                    Time = g.Key,
                    Value = g.Sum(r => Convert.ToInt64(r.Value ?? 0))
                })
                .OrderBy(p => p.Time)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "获取采集趋势数据失败");
            return [];
        }
    }

    /// <summary>
    /// 解析时间间隔字符串
    /// </summary>
    private TimeSpan ParseDuration(string duration)
    {
        if (string.IsNullOrWhiteSpace(duration))
            return TimeSpan.Zero;

        var value = int.Parse(duration.TrimEnd('m', 'h', 'd', 'w'));
        return duration.Last() switch
        {
            'm' => TimeSpan.FromMinutes(value),
            'h' => TimeSpan.FromHours(value),
            'd' => TimeSpan.FromDays(value),
            'w' => TimeSpan.FromDays(value * 7),
            _ => TimeSpan.FromHours(value)
        };
    }

    #endregion
}
