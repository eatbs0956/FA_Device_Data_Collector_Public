# InfluxDB 初始化脚本 (PowerShell)
# 用于创建 Bucket 和聚合 Task
# 使用方式: .\influxdb-init.ps1

param(
    [string]$InfluxUrl = "http://localhost:8086",
    [string]$Token = "devdcp-token",
    [string]$Org = "devorg"
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "InfluxDB 初始化脚本" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "URL: $InfluxUrl"
Write-Host "Org: $Org"
Write-Host ""

# 通用请求头
$headers = @{
    "Authorization" = "Token $Token"
    "Content-Type" = "application/json"
}

# ============================================
# 1. 创建 Buckets
# ============================================
function New-InfluxBucket {
    param(
        [string]$Name,
        [int]$RetentionDays
    )
    
    Write-Host "创建 Bucket: $Name (保留 $RetentionDays 天)..." -ForegroundColor Yellow
    
    # 检查是否存在
    $checkUrl = "$InfluxUrl/api/v2/buckets?name=$Name&org=$Org"
    try {
        $existing = Invoke-RestMethod -Uri $checkUrl -Headers $headers -Method Get
        if ($existing.buckets.Count -gt 0) {
            Write-Host "  Bucket '$Name' 已存在，跳过" -ForegroundColor Gray
            return
        }
    } catch {
        # 忽略检查错误，继续创建
    }
    
    # 获取组织 ID
    $orgUrl = "$InfluxUrl/api/v2/orgs?org=$Org"
    $orgResponse = Invoke-RestMethod -Uri $orgUrl -Headers $headers -Method Get
    $orgId = $orgResponse.orgs[0].id
    
    # 创建 Bucket
    $body = @{
        name = $Name
        orgID = $orgId
        retentionRules = @(
            @{
                type = "expire"
                everySeconds = $RetentionDays * 24 * 60 * 60
            }
        )
    } | ConvertTo-Json -Depth 3
    
    $createUrl = "$InfluxUrl/api/v2/buckets"
    try {
        Invoke-RestMethod -Uri $createUrl -Headers $headers -Method Post -Body $body | Out-Null
        Write-Host "  ✓ Bucket '$Name' 创建成功" -ForegroundColor Green
    } catch {
        Write-Host "  ✗ Bucket '$Name' 创建失败: $_" -ForegroundColor Red
    }
}

# 创建所有 Bucket
New-InfluxBucket -Name "collected" -RetentionDays 30
New-InfluxBucket -Name "aggregated" -RetentionDays 365
New-InfluxBucket -Name "metrics" -RetentionDays 90

Write-Host ""

# ============================================
# 2. 创建聚合 Tasks
# ============================================
function New-InfluxTask {
    param(
        [string]$Name,
        [string]$FluxScript
    )
    
    Write-Host "创建 Task: $Name..." -ForegroundColor Yellow
    
    # 检查是否存在
    $checkUrl = "$InfluxUrl/api/v2/tasks?name=$Name&org=$Org"
    try {
        $existing = Invoke-RestMethod -Uri $checkUrl -Headers $headers -Method Get
        if ($existing.tasks.Count -gt 0) {
            Write-Host "  Task '$Name' 已存在，跳过" -ForegroundColor Gray
            return
        }
    } catch {
        # 忽略检查错误
    }
    
    # 获取组织 ID
    $orgUrl = "$InfluxUrl/api/v2/orgs?org=$Org"
    $orgResponse = Invoke-RestMethod -Uri $orgUrl -Headers $headers -Method Get
    $orgId = $orgResponse.orgs[0].id
    
    $body = @{
        name = $Name
        orgID = $orgId
        flux = $FluxScript
        status = "active"
    } | ConvertTo-Json -Depth 3
    
    $createUrl = "$InfluxUrl/api/v2/tasks"
    try {
        Invoke-RestMethod -Uri $createUrl -Headers $headers -Method Post -Body $body | Out-Null
        Write-Host "  ✓ Task '$Name' 创建成功" -ForegroundColor Green
    } catch {
        Write-Host "  ✗ Task '$Name' 创建失败: $_" -ForegroundColor Red
    }
}

# Task 1: 1分钟聚合
$task1m = @"
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
"@

# Task 2: 5分钟聚合
$task5m = @"
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
"@

# Task 3: 1小时聚合
$task1h = @"
option task = {
  name: "aggregate_device_data_1h",
  every: 1h,
  offset: 5m
}

meanData = from(bucket: "collected")
  |> range(start: -task.every)
  |> filter(fn: (r) => r["_measurement"] == "device_data")
  |> aggregateWindow(every: 1h, fn: mean, createEmpty: false)
  |> set(key: "_measurement", value: "device_data_1h")

maxData = from(bucket: "collected")
  |> range(start: -task.every)
  |> filter(fn: (r) => r["_measurement"] == "device_data")
  |> aggregateWindow(every: 1h, fn: max, createEmpty: false)
  |> map(fn: (r) => ({r with _field: r._field + "_max"}))
  |> set(key: "_measurement", value: "device_data_1h")

minData = from(bucket: "collected")
  |> range(start: -task.every)
  |> filter(fn: (r) => r["_measurement"] == "device_data")
  |> aggregateWindow(every: 1h, fn: min, createEmpty: false)
  |> map(fn: (r) => ({r with _field: r._field + "_min"}))
  |> set(key: "_measurement", value: "device_data_1h")

union(tables: [meanData, maxData, minData])
  |> to(bucket: "aggregated", org: "devorg")
"@

New-InfluxTask -Name "aggregate_device_data_1m" -FluxScript $task1m
New-InfluxTask -Name "aggregate_device_data_5m" -FluxScript $task5m
New-InfluxTask -Name "aggregate_device_data_1h" -FluxScript $task1h

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "初始化完成!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
