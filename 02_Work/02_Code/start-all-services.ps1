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
& "$scriptDir\check-docker-services.ps1"
if ($LASTEXITCODE -ne 0) {
    Write-Host "Docker services not ready. Exiting..." -ForegroundColor Red
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

if (Test-Path $frontendDir) {
    Write-Host "`nStep 5: Starting Frontend on port 9527..." -ForegroundColor Cyan
    Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$frontendDir'; pnpm dev"
    Start-Sleep -Seconds 5
    Write-Host "[OK] Frontend started" -ForegroundColor Green
} else {
    Write-Host "`nStep 5: Frontend directory not found, skipping..." -ForegroundColor Yellow
}

Write-Host "`n=================================" -ForegroundColor Green
Write-Host "All services started successfully!" -ForegroundColor Green
Write-Host "=================================" -ForegroundColor Green
Write-Host "`nService URLs:" -ForegroundColor Cyan
Write-Host "  - Gateway.Api:  http://localhost:60620 (API Gateway)" -ForegroundColor White
Write-Host "  - Auth.Api:     http://localhost:60621/swagger" -ForegroundColor White
Write-Host "  - Admin.Api:    http://localhost:60623/swagger" -ForegroundColor White
if (Test-Path $frontendDir) {
    Write-Host "  - Frontend:     http://localhost:9527" -ForegroundColor White
}
Write-Host "`nNote: Frontend requests go through Gateway.Api (60620)" -ForegroundColor Yellow
Write-Host "Press Ctrl+C to stop all services.`n" -ForegroundColor Yellow
