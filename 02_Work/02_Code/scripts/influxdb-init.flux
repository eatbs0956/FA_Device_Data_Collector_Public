// InfluxDB 初始化脚本
// 用于创建 Bucket 和聚合 Task
// 执行方式: 通过 InfluxDB UI 或 influx CLI 执行

// ============================================
// 1. 创建 Buckets
// ============================================

// 1.1 采集数据桶 (30天保留)
// influx bucket create --name collected --retention 30d --org devorg

// 1.2 聚合数据桶 (365天保留)
// influx bucket create --name aggregated --retention 365d --org devorg

// 1.3 系统指标桶 (90天保留)
// influx bucket create --name metrics --retention 90d --org devorg

// ============================================
// 2. 聚合 Tasks
// ============================================

// ------------------------------------------
// Task 1: 1分钟聚合
// ------------------------------------------
option task = {
  name: "aggregate_device_data_1m",
  every: 1m,
  offset: 10s
}

from(bucket: "collected")
  |> range(start: -task.every)
  |> filter(fn: (r) => r["_measurement"] == "device_data")
  |> aggregateWindow(every: 1m, fn: mean, createEmpty: false)
  |> set(key: "_measurement", value: "device_data_1m")
  |> to(bucket: "aggregated", org: "devorg")

// ------------------------------------------
// Task 2: 5分钟聚合
// ------------------------------------------
option task = {
  name: "aggregate_device_data_5m",
  every: 5m,
  offset: 30s
}

from(bucket: "collected")
  |> range(start: -task.every)
  |> filter(fn: (r) => r["_measurement"] == "device_data")
  |> aggregateWindow(every: 5m, fn: mean, createEmpty: false)
  |> set(key: "_measurement", value: "device_data_5m")
  |> to(bucket: "aggregated", org: "devorg")

// ------------------------------------------
// Task 3: 1小时聚合 (含最大最小值)
// ------------------------------------------
option task = {
  name: "aggregate_device_data_1h",
  every: 1h,
  offset: 5m
}

// 平均值
meanData = from(bucket: "collected")
  |> range(start: -task.every)
  |> filter(fn: (r) => r["_measurement"] == "device_data")
  |> aggregateWindow(every: 1h, fn: mean, createEmpty: false)
  |> set(key: "_measurement", value: "device_data_1h")

// 最大值
maxData = from(bucket: "collected")
  |> range(start: -task.every)
  |> filter(fn: (r) => r["_measurement"] == "device_data")
  |> aggregateWindow(every: 1h, fn: max, createEmpty: false)
  |> map(fn: (r) => ({r with _field: r._field + "_max"}))
  |> set(key: "_measurement", value: "device_data_1h")

// 最小值
minData = from(bucket: "collected")
  |> range(start: -task.every)
  |> filter(fn: (r) => r["_measurement"] == "device_data")
  |> aggregateWindow(every: 1h, fn: min, createEmpty: false)
  |> map(fn: (r) => ({r with _field: r._field + "_min"}))
  |> set(key: "_measurement", value: "device_data_1h")

union(tables: [meanData, maxData, minData])
  |> to(bucket: "aggregated", org: "devorg")
