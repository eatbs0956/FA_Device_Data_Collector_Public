# check-docker-services.ps1
# DevDCP Docker Infrastructure Service Check Script
# Usage: Check if PostgreSQL, RabbitMQ, InfluxDB, Redis are running in Docker

$ErrorActionPreference = "Stop"

Write-Host "=================================" -ForegroundColor Cyan
Write-Host "DevDCP Docker Service Status Check" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan

# Check if Docker is running
function Test-DockerRunning {
    try {
        docker ps > $null 2>&1
        return $true
    } catch {
        return $false
    }
}

# Check if container is running
function Test-ContainerRunning {
    param([string]$ContainerName)
    
    $container = docker ps --filter "name=$ContainerName" --format "{{.Names}}" 2>$null
    return $container -eq $ContainerName
}

# Check container health status
function Get-ContainerHealth {
    param([string]$ContainerName)
    
    $health = docker inspect --format='{{.State.Health.Status}}' $ContainerName 2>$null
    if ($LASTEXITCODE -ne 0) {
        return "no-healthcheck"
    }
    return $health
}

# Check PostgreSQL connection
function Test-PostgresConnection {
    try {
        $result = docker exec devdcp-postgres pg_isready -U devdcp 2>&1
        return $result -like "*accepting connections*"
    } catch {
        return $false
    }
}

# ========== Start Checking ==========

# 1. Check if Docker Desktop is running
Write-Host "`n[1/5] Checking Docker Desktop..." -ForegroundColor Yellow
if (-not (Test-DockerRunning)) {
    Write-Host "[ERROR] Docker Desktop is not running" -ForegroundColor Red
    Write-Host "`nSolution:" -ForegroundColor Yellow
    Write-Host "   1. Start Docker Desktop" -ForegroundColor White
    Write-Host "   2. Wait for Docker Desktop to fully start (icon not spinning)" -ForegroundColor White
    Write-Host "   3. Run this script again" -ForegroundColor White
    exit 1
}
Write-Host "[OK] Docker Desktop is running" -ForegroundColor Green

# 2. Check PostgreSQL container
Write-Host "`n[2/5] Checking PostgreSQL container..." -ForegroundColor Yellow
if (-not (Test-ContainerRunning "devdcp-postgres")) {
    Write-Host "[ERROR] PostgreSQL container is not running" -ForegroundColor Red
    Write-Host "`nSolution:" -ForegroundColor Yellow
    Write-Host "   cd d:\00_QC-share\01_DevDCP\02_Work\02_Code\infra" -ForegroundColor White
    Write-Host "   docker-compose -f docker-compose.dev.yml up -d postgres" -ForegroundColor White
    exit 1
}

$pgHealth = Get-ContainerHealth "devdcp-postgres"
if ($pgHealth -eq "healthy") {
    Write-Host "[OK] PostgreSQL container is running (healthy)" -ForegroundColor Green
} elseif ($pgHealth -eq "no-healthcheck") {
    # Test connection directly
    if (Test-PostgresConnection) {
        Write-Host "[OK] PostgreSQL container is running (connection OK)" -ForegroundColor Green
    } else {
        Write-Host "[WARNING] PostgreSQL container is running but connection failed" -ForegroundColor Yellow
        exit 1
    }
} else {
    Write-Host "[WARNING] PostgreSQL container is running but health status: $pgHealth" -ForegroundColor Yellow
    exit 1
}

# 3. Check RabbitMQ container (optional)
Write-Host "`n[3/5] Checking RabbitMQ container (optional)..." -ForegroundColor Yellow
if (Test-ContainerRunning "devdcp-rabbitmq") {
    $rabbitHealth = Get-ContainerHealth "devdcp-rabbitmq"
    if ($rabbitHealth -eq "healthy") {
        Write-Host "[OK] RabbitMQ container is running (healthy)" -ForegroundColor Green
    } else {
        Write-Host "[WARNING] RabbitMQ container is running but health status: $rabbitHealth" -ForegroundColor Yellow
    }
} else {
    Write-Host "[WARNING] RabbitMQ container is not running (some features may be unavailable)" -ForegroundColor Yellow
}

# 4. Check InfluxDB container (optional)
Write-Host "`n[4/5] Checking InfluxDB container (optional)..." -ForegroundColor Yellow
if (Test-ContainerRunning "devdcp-influx") {
    $influxHealth = Get-ContainerHealth "devdcp-influx"
    if ($influxHealth -eq "healthy") {
        Write-Host "[OK] InfluxDB container is running (healthy)" -ForegroundColor Green
    } else {
        Write-Host "[WARNING] InfluxDB container is running but health status: $influxHealth" -ForegroundColor Yellow
    }
} else {
    Write-Host "[WARNING] InfluxDB container is not running (time-series data features unavailable)" -ForegroundColor Yellow
}

# 5. Check Redis container (optional)
Write-Host "`n[5/5] Checking Redis container (optional)..." -ForegroundColor Yellow
if (Test-ContainerRunning "devdcp-redis") {
    $redisHealth = Get-ContainerHealth "devdcp-redis"
    if ($redisHealth -eq "healthy") {
        Write-Host "[OK] Redis container is running (healthy)" -ForegroundColor Green
    } else {
        Write-Host "[WARNING] Redis container is running but health status: $redisHealth" -ForegroundColor Yellow
    }
} else {
    Write-Host "[WARNING] Redis container is not running (cache features unavailable)" -ForegroundColor Yellow
}

# ========== Check Results Summary ==========
Write-Host "`n=================================" -ForegroundColor Green
Write-Host "Core services check completed" -ForegroundColor Green
Write-Host "=================================" -ForegroundColor Green

Write-Host "`nRequired services:" -ForegroundColor Cyan
Write-Host "   [OK] Docker Desktop" -ForegroundColor Green
Write-Host "   [OK] PostgreSQL (devdcp-postgres)" -ForegroundColor Green

Write-Host "`nOptional services (based on feature requirements):" -ForegroundColor Cyan
if (Test-ContainerRunning "devdcp-rabbitmq") {
    Write-Host "   [OK] RabbitMQ (devdcp-rabbitmq)" -ForegroundColor Green
} else {
    Write-Host "   [WARNING] RabbitMQ (not running)" -ForegroundColor Yellow
}
if (Test-ContainerRunning "devdcp-influx") {
    Write-Host "   [OK] InfluxDB (devdcp-influx)" -ForegroundColor Green
} else {
    Write-Host "   [WARNING] InfluxDB (not running)" -ForegroundColor Yellow
}
if (Test-ContainerRunning "devdcp-redis") {
    Write-Host "   [OK] Redis (devdcp-redis)" -ForegroundColor Green
} else {
    Write-Host "   [WARNING] Redis (not running)" -ForegroundColor Yellow
}

Write-Host "`nTips:" -ForegroundColor Yellow
Write-Host "   - You can run .\start-all-services.ps1 to start microservices" -ForegroundColor White
Write-Host "   - PostgreSQL database: devdcp (user: devdcp, password: devdcp)" -ForegroundColor White
Write-Host "`n" -NoNewline

exit 0
