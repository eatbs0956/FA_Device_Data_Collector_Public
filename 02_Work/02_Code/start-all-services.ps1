# start-all-services.ps1
# DevDCP Microservices Startup Script

$ErrorActionPreference = "Stop"
$baseDir = "d:\00_QC-share\01_DevDCP\02_Work\02_Code\platform\center"
$frontendDir = "d:\00_QC-share\01_DevDCP\02_Work\02_Code\web\frontend"
$scriptDir = "d:\00_QC-share\01_DevDCP\02_Work\02_Code"

Write-Host "=================================" -ForegroundColor Green
Write-Host "Starting DevDCP Microservices..." -ForegroundColor Green
Write-Host "=================================" -ForegroundColor Green

function Test-ServiceHealth {
    param(
        [string]$Url,
        [string]$ServiceName,
        [int]$MaxRetries = 30,
        [int]$RetryIntervalSeconds = 2
    )
    
    Write-Host "Waiting for $ServiceName..." -ForegroundColor Yellow
    
    for ($i = 1; $i -le $MaxRetries; $i++) {
        try {
            $response = Invoke-WebRequest -Uri $Url -Method Get -TimeoutSec 2 -ErrorAction SilentlyContinue
            if ($response.StatusCode -eq 200) {
                Write-Host "[OK] $ServiceName started ($i/$MaxRetries)" -ForegroundColor Green
                return $true
            }
        } catch { }
        
        Write-Host "  [$i/$MaxRetries] Waiting ${RetryIntervalSeconds}s..." -ForegroundColor Gray
        Start-Sleep -Seconds $RetryIntervalSeconds
    }
    
    Write-Host "[ERROR] Timeout after $MaxRetries retries" -ForegroundColor Red
    return $false
}

Write-Host "`nStep 1: Checking Docker infrastructure..." -ForegroundColor Cyan
& "$scriptDir\check-docker-services.ps1" -AutoStart -StartAll
if ($LASTEXITCODE -ne 0) {
    Write-Host "Docker services not ready. Exiting..." -ForegroundColor Red
    exit 1
}

# Double-check that all required services are running
Write-Host "`nStep 1b: Verifying Docker services are fully started..." -ForegroundColor Cyan
$maxRetries = 5
$retryCount = 0
$allServicesReady = $false

while ($retryCount -lt $maxRetries -and -not $allServicesReady) {
    $retryCount++
    Write-Host "  Verification attempt $retryCount/$maxRetries..." -ForegroundColor Gray
    
    $pgRunning = docker ps --filter "name=devdcp-postgres" --format "{{.Names}}" 2>$null
    $rabbitRunning = docker ps --filter "name=devdcp-rabbitmq" --format "{{.Names}}" 2>$null
    $influxRunning = docker ps --filter "name=devdcp-influx" --format "{{.Names}}" 2>$null
    $redisRunning = docker ps --filter "name=devdcp-redis" --format "{{.Names}}" 2>$null
    
    if ($pgRunning -eq "devdcp-postgres" -and $rabbitRunning -eq "devdcp-rabbitmq" -and $influxRunning -eq "devdcp-influx" -and $redisRunning -eq "devdcp-redis") {
        $allServicesReady = $true
        Write-Host "[OK] All Docker services verified as running" -ForegroundColor Green
    } else {
        Write-Host "  PostgreSQL: $(if ($pgRunning -eq 'devdcp-postgres') { '[OK]' } else { '[NOT RUNNING]' })" -ForegroundColor Gray
        Write-Host "  RabbitMQ: $(if ($rabbitRunning -eq 'devdcp-rabbitmq') { '[OK]' } else { '[NOT RUNNING]' })" -ForegroundColor Gray
        Write-Host "  InfluxDB: $(if ($influxRunning -eq 'devdcp-influx') { '[OK]' } else { '[NOT RUNNING]' })" -ForegroundColor Gray
        Write-Host "  Redis: $(if ($redisRunning -eq 'devdcp-redis') { '[OK]' } else { '[NOT RUNNING]' })" -ForegroundColor Gray
        
        if ($retryCount -lt $maxRetries) {
            Write-Host "  Waiting 3 seconds before retry..." -ForegroundColor Yellow
            Start-Sleep -Seconds 3
            
            # Try to start any stopped containers
            if ($pgRunning -ne "devdcp-postgres") { docker start devdcp-postgres 2>$null | Out-Null }
            if ($rabbitRunning -ne "devdcp-rabbitmq") { docker start devdcp-rabbitmq 2>$null | Out-Null }
            if ($influxRunning -ne "devdcp-influx") { docker start devdcp-influx 2>$null | Out-Null }
            if ($redisRunning -ne "devdcp-redis") { docker start devdcp-redis 2>$null | Out-Null }
        }
    }
}

if (-not $allServicesReady) {
    Write-Host "`n[ERROR] Some Docker services failed to start after $maxRetries attempts" -ForegroundColor Red
    Write-Host "`nManual troubleshooting:" -ForegroundColor Yellow
    Write-Host "  1. Check Docker Desktop is running: docker ps" -ForegroundColor White
    Write-Host "  2. Try manual start: cd infra && docker-compose -f docker-compose.dev.yml up -d" -ForegroundColor White
    Write-Host "  3. View logs: docker-compose -f docker-compose.dev.yml logs -f" -ForegroundColor White
    exit 1
}

Write-Host "`nStep 2: Starting Auth.Api on port 60621..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$baseDir\Auth.Api'; dotnet run --urls http://localhost:60621"
$authHealthy = Test-ServiceHealth -Url "http://localhost:60621/health" -ServiceName "Auth.Api"
if (-not $authHealthy) {
    Write-Host "Failed to start Auth.Api" -ForegroundColor Red
    exit 1
}

Write-Host "`nStep 3: Starting Admin.Api on port 60623..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$baseDir\Admin.Api'; dotnet run --urls http://localhost:60623"
$adminHealthy = Test-ServiceHealth -Url "http://localhost:60623/health" -ServiceName "Admin.Api"
if (-not $adminHealthy) {
    Write-Host "Failed to start Admin.Api" -ForegroundColor Red
    exit 1
}

Write-Host "`nStep 4: Starting Gateway.Api on port 60620..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$baseDir\Gateway.Api'; dotnet run --urls http://localhost:60620"
$gatewayHealthy = Test-ServiceHealth -Url "http://localhost:60620/health" -ServiceName "Gateway.Api"
if (-not $gatewayHealthy) {
    Write-Host "Failed to start Gateway.Api" -ForegroundColor Red
    exit 1
}

Write-Host "`nStep 5: Starting Processor.Worker on port 60624..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$baseDir\Processor.Worker'; dotnet run --urls http://localhost:60624"
$processorHealthy = Test-ServiceHealth -Url "http://localhost:60624/health" -ServiceName "Processor.Worker"
if (-not $processorHealthy) {
    Write-Host "Failed to start Processor.Worker" -ForegroundColor Red
    exit 1
}

if (Test-Path $frontendDir) {
    Write-Host "`nStep 6: Starting Frontend on port 9527..." -ForegroundColor Cyan
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$frontendDir'; pnpm dev"
    Start-Sleep -Seconds 5
    Write-Host "[OK] Frontend started" -ForegroundColor Green
} else {
    Write-Host "`nStep 6: Frontend directory not found, skipping..." -ForegroundColor Yellow
}

Write-Host "`n=================================" -ForegroundColor Green
Write-Host "All services started successfully!" -ForegroundColor Green
Write-Host "=================================" -ForegroundColor Green
Write-Host "`nService URLs:" -ForegroundColor Cyan
Write-Host "  - Gateway.Api:       http://localhost:60620 (API Gateway + SignalR)" -ForegroundColor White
Write-Host "  - Auth.Api:          http://localhost:60621/swagger" -ForegroundColor White
Write-Host "  - Admin.Api:         http://localhost:60623/swagger" -ForegroundColor White
Write-Host "  - Processor.Worker:  http://localhost:60624/health (Data Processing)" -ForegroundColor White
if (Test-Path $frontendDir) {
    Write-Host "  - Frontend:          http://localhost:9527" -ForegroundColor White
}
Write-Host "`nNote: Frontend requests go through Gateway.Api (60620)" -ForegroundColor Yellow
Write-Host "Press Ctrl+C to stop all services.`n" -ForegroundColor Yellow
