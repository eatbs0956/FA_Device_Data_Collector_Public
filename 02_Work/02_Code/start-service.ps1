# start-service.ps1
# DevDCP Single Service Startup Script

param(
    [Parameter(Mandatory=$true)]
    [ValidateSet("Auth.Api", "Admin.Api", "Gateway.Api", "Processor.Worker", "Frontend")]
    [string]$ServiceName
)

$baseDir = "d:\00_QC-share\01_DevDCP\02_Work\02_Code\platform\center"
$frontendDir = "d:\00_QC-share\01_DevDCP\02_Work\02_Code\web\frontend"
$scriptDir = "d:\00_QC-share\01_DevDCP\02_Work\02_Code"

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

Write-Host "=================================" -ForegroundColor Green
Write-Host "Starting $ServiceName..." -ForegroundColor Green
Write-Host "=================================" -ForegroundColor Green

Write-Host "`nStep 1: Checking Docker infrastructure..." -ForegroundColor Cyan
& "$scriptDir\check-docker-services.ps1"
if ($LASTEXITCODE -ne 0) {
    Write-Host "Docker services not ready. Exiting..." -ForegroundColor Red
    exit 1
}

switch ($ServiceName) {
    "Auth.Api" {
        Write-Host "`nStep 2: Starting Auth.Api on port 60621..." -ForegroundColor Cyan
        Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$baseDir\Auth.Api'; dotnet run --urls http://localhost:60621"
        $healthy = Test-ServiceHealth -Url "http://localhost:60621/health" -ServiceName "Auth.Api"
        if ($healthy) {
            Write-Host "`nAuth.Api started successfully!" -ForegroundColor Green
            Write-Host "Swagger: http://localhost:60621/swagger" -ForegroundColor Cyan
        } else {
            Write-Host "Failed to start Auth.Api" -ForegroundColor Red
            exit 1
        }
    }
    
    "Admin.Api" {
        Write-Host "`nStep 2: Starting Admin.Api on port 60623..." -ForegroundColor Cyan
        Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$baseDir\Admin.Api'; dotnet run --urls http://localhost:60623"
        $healthy = Test-ServiceHealth -Url "http://localhost:60623/health" -ServiceName "Admin.Api"
        if ($healthy) {
            Write-Host "`nAdmin.Api started successfully!" -ForegroundColor Green
            Write-Host "Swagger: http://localhost:60623/swagger" -ForegroundColor Cyan
        } else {
            Write-Host "Failed to start Admin.Api" -ForegroundColor Red
            exit 1
        }
    }
    
    "Gateway.Api" {
        Write-Host "`nStep 2: Starting Gateway.Api on port 60620..." -ForegroundColor Cyan
        Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$baseDir\Gateway.Api'; dotnet run --urls http://localhost:60620"
        $healthy = Test-ServiceHealth -Url "http://localhost:60620/health" -ServiceName "Gateway.Api"
        if ($healthy) {
            Write-Host "`nGateway.Api started successfully!" -ForegroundColor Green
            Write-Host "Gateway URL: http://localhost:60620" -ForegroundColor Cyan
        } else {
            Write-Host "Failed to start Gateway.Api" -ForegroundColor Red
            exit 1
        }
    }
    
    "Processor.Worker" {
        Write-Host "`nStep 2: Starting Processor.Worker on port 60624..." -ForegroundColor Cyan
        Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$baseDir\Processor.Worker'; dotnet run --urls http://localhost:60624"
        $healthy = Test-ServiceHealth -Url "http://localhost:60624/health" -ServiceName "Processor.Worker"
        if ($healthy) {
            Write-Host "`nProcessor.Worker started successfully!" -ForegroundColor Green
            Write-Host "Health: http://localhost:60624/health" -ForegroundColor Cyan
        } else {
            Write-Host "Failed to start Processor.Worker" -ForegroundColor Red
            exit 1
        }
    }
    
    "Frontend" {
        if (Test-Path $frontendDir) {
            Write-Host "`nStep 2: Starting Frontend on port 3200..." -ForegroundColor Cyan
            Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$frontendDir'; pnpm dev"
            Start-Sleep -Seconds 5
            Write-Host "`nFrontend started successfully!" -ForegroundColor Green
            Write-Host "URL: http://localhost:3200" -ForegroundColor Cyan
        } else {
            Write-Host "Frontend directory not found: $frontendDir" -ForegroundColor Red
            exit 1
        }
    }
}
