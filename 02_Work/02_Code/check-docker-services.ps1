# check-docker-services.ps1
# DevDCP Docker Infrastructure Service Check Script
# Usage: Check if PostgreSQL, RabbitMQ, InfluxDB, Redis are running in Docker
# Options:
#   -AutoStart    Automatically start services if not running (default: $true)
#   -StartAll     Start all services including optional ones (default: $false)

param(
    [switch]$AutoStart = $true,
    [switch]$StartAll = $false
)

$ErrorActionPreference = "Stop"
$InfraPath = "D:\00_QC-share\01_DevDCP\02_Work\02_Code\infra"
$ComposeFile = "docker-compose.dev.yml"

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

# Check if container exists (running or stopped)
function Test-ContainerExists {
    param([string]$ContainerName)
    
    $container = docker ps -a --filter "name=$ContainerName" --format "{{.Names}}" 2>$null
    return $container -eq $ContainerName
}

# Start a stopped container by name
function Start-StoppedContainer {
    param([string]$ContainerName)
    
    Write-Host "[AUTO-START] Starting stopped container: $ContainerName..." -ForegroundColor Cyan
    try {
        $output = docker start $ContainerName 2>&1
        Start-Sleep -Seconds 2
        
        if (Test-ContainerRunning $ContainerName) {
            Write-Host "[AUTO-START] Container $ContainerName started successfully" -ForegroundColor Green
            return $true
        } else {
            Write-Host "[ERROR] Failed to start container $ContainerName" -ForegroundColor Red
            return $false
        }
    } catch {
        Write-Host "[ERROR] Exception while starting $ContainerName`: $_" -ForegroundColor Red
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

# Start services using docker-compose
function Start-DockerServices {
    param([string[]]$Services)
    
    Write-Host "`n[AUTO-START] Starting services: $($Services -join ', ')..." -ForegroundColor Cyan
    Push-Location $InfraPath
    try {
        $ErrorActionPreference = "Continue"
        $output = docker-compose -f $ComposeFile up -d $Services 2>&1
        $exitCode = $LASTEXITCODE
        $ErrorActionPreference = "Stop"
        
        # Check if containers are actually running (ignore docker-compose warnings)
        Start-Sleep -Seconds 2
        $allRunning = $true
        foreach ($svc in $Services) {
            # Map service names to container names
            $containerNameMap = @{
                "postgres" = "devdcp-postgres"
                "rabbitmq" = "devdcp-rabbitmq"
                "influxdb" = "devdcp-influx"
                "redis" = "devdcp-redis"
            }
            $containerName = $containerNameMap[$svc]
            if (-not $containerName) { $containerName = "devdcp-$svc" }
            
            $running = docker ps --filter "name=$containerName" --format "{{.Names}}" 2>$null
            if ($running -ne $containerName) {
                $allRunning = $false
                break
            }
        }
        
        if ($allRunning) {
            Write-Host "[AUTO-START] Services started successfully" -ForegroundColor Green
            # Wait for services to initialize
            Write-Host "[AUTO-START] Waiting for services to initialize (8 seconds)..." -ForegroundColor Yellow
            Start-Sleep -Seconds 8
            return $true
        } else {
            Write-Host "[AUTO-START] Failed to start services" -ForegroundColor Red
            return $false
        }
    } finally {
        Pop-Location
    }
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
$pgNeedStart = $false
if (-not (Test-ContainerRunning "devdcp-postgres")) {
    Write-Host "[WARNING] PostgreSQL container is not running" -ForegroundColor Yellow
    $pgNeedStart = $true
    
    if ($AutoStart) {
        if (Start-DockerServices @("postgres")) {
            # Re-check after starting
            if (-not (Test-ContainerRunning "devdcp-postgres")) {
                Write-Host "[ERROR] PostgreSQL failed to start" -ForegroundColor Red
                Write-Host "`nManual Solution:" -ForegroundColor Yellow
                Write-Host "   cd $InfraPath" -ForegroundColor White
                Write-Host "   docker-compose -f $ComposeFile up -d postgres" -ForegroundColor White
                exit 1
            }
        } else {
            Write-Host "[ERROR] Could not auto-start PostgreSQL" -ForegroundColor Red
            exit 1
        }
    } else {
        Write-Host "`nSolution:" -ForegroundColor Yellow
        Write-Host "   cd $InfraPath" -ForegroundColor White
        Write-Host "   docker-compose -f $ComposeFile up -d postgres" -ForegroundColor White
        Write-Host "`nOr run: .\check-docker-services.ps1 -AutoStart" -ForegroundColor Yellow
        exit 1
    }
}

$pgHealth = Get-ContainerHealth "devdcp-postgres"
if ($pgHealth -eq "healthy") {
    Write-Host "[OK] PostgreSQL container is running (healthy)" -ForegroundColor Green
} elseif ($pgHealth -eq "starting") {
    Write-Host "[OK] PostgreSQL container is running (health check in progress)" -ForegroundColor Green
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
    Write-Host "[WARNING] RabbitMQ container is not running" -ForegroundColor Yellow
    
    # Try to restart if it exists but is stopped
    if (Test-ContainerExists "devdcp-rabbitmq") {
        if ($AutoStart -or $StartAll) {
            if (Start-StoppedContainer "devdcp-rabbitmq") {
                Write-Host "[OK] RabbitMQ container restarted successfully" -ForegroundColor Green
            } else {
                Write-Host "[ERROR] Failed to restart RabbitMQ container" -ForegroundColor Red
            }
        } else {
            Write-Host "[INFO] RabbitMQ container exists but is stopped. Run with -AutoStart or -StartAll to restart" -ForegroundColor Cyan
        }
    } else {
        # Container doesn't exist, will create it with docker-compose if -AutoStart or -StartAll
        if ($AutoStart -or $StartAll) {
            Write-Host "[AUTO-START] RabbitMQ container not found, creating and starting..." -ForegroundColor Cyan
            if (Start-DockerServices @("rabbitmq")) {
                Write-Host "[OK] RabbitMQ container created and started successfully" -ForegroundColor Green
            } else {
                Write-Host "[ERROR] Failed to create and start RabbitMQ container" -ForegroundColor Red
            }
        } else {
            Write-Host "[INFO] RabbitMQ container does not exist (optional service)" -ForegroundColor Cyan
        }
    }
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
    Write-Host "[WARNING] InfluxDB container is not running" -ForegroundColor Yellow
    
    # Try to restart if it exists but is stopped
    if (Test-ContainerExists "devdcp-influx") {
        if ($AutoStart -or $StartAll) {
            if (Start-StoppedContainer "devdcp-influx") {
                Write-Host "[OK] InfluxDB container restarted successfully" -ForegroundColor Green
            } else {
                Write-Host "[ERROR] Failed to restart InfluxDB container" -ForegroundColor Red
            }
        } else {
            Write-Host "[INFO] InfluxDB container exists but is stopped. Run with -AutoStart or -StartAll to restart" -ForegroundColor Cyan
        }
    } else {
        # Container doesn't exist, will create it with docker-compose if -AutoStart or -StartAll
        if ($AutoStart -or $StartAll) {
            Write-Host "[AUTO-START] InfluxDB container not found, creating and starting..." -ForegroundColor Cyan
            if (Start-DockerServices @("influxdb")) {
                Write-Host "[OK] InfluxDB container created and started successfully" -ForegroundColor Green
            } else {
                Write-Host "[ERROR] Failed to create and start InfluxDB container" -ForegroundColor Red
            }
        } else {
            Write-Host "[INFO] InfluxDB container does not exist (time-series data features unavailable)" -ForegroundColor Cyan
        }
    }
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
    Write-Host "[WARNING] Redis container is not running" -ForegroundColor Yellow
    
    # Try to restart if it exists but is stopped
    if (Test-ContainerExists "devdcp-redis") {
        if ($AutoStart -or $StartAll) {
            if (Start-StoppedContainer "devdcp-redis") {
                Write-Host "[OK] Redis container restarted successfully" -ForegroundColor Green
            } else {
                Write-Host "[ERROR] Failed to restart Redis container" -ForegroundColor Red
            }
        } else {
            Write-Host "[INFO] Redis container exists but is stopped. Run with -AutoStart or -StartAll to restart" -ForegroundColor Cyan
        }
    } else {
        # Container doesn't exist, will create it with docker-compose if -AutoStart or -StartAll
        if ($AutoStart -or $StartAll) {
            Write-Host "[AUTO-START] Redis container not found, creating and starting..." -ForegroundColor Cyan
            if (Start-DockerServices @("redis")) {
                Write-Host "[OK] Redis container created and started successfully" -ForegroundColor Green
            } else {
                Write-Host "[ERROR] Failed to create and start Redis container" -ForegroundColor Red
            }
        } else {
            Write-Host "[INFO] Redis container does not exist (cache features unavailable)" -ForegroundColor Cyan
        }
    }
}

# Auto-start all optional services if -StartAll flag is set (for those that may have failed individually)
$failedOptionalServices = @()
if (-not (Test-ContainerRunning "devdcp-rabbitmq")) { $failedOptionalServices += "rabbitmq" }
if (-not (Test-ContainerRunning "devdcp-influx")) { $failedOptionalServices += "influxdb" }
if (-not (Test-ContainerRunning "devdcp-redis")) { $failedOptionalServices += "redis" }

if ($failedOptionalServices.Count -gt 0 -and ($AutoStart -or $StartAll)) {
    Write-Host "`n[AUTO-START] Attempting to start remaining optional services: $($failedOptionalServices -join ', ')..." -ForegroundColor Cyan
    Start-DockerServices $failedOptionalServices
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
Write-Host "   - Run .\check-docker-services.ps1 -AutoStart to auto-start PostgreSQL" -ForegroundColor White
Write-Host "   - Run .\check-docker-services.ps1 -StartAll to start all services" -ForegroundColor White
Write-Host "   - Run .\start-all-services.ps1 to start microservices" -ForegroundColor White
Write-Host "   - PostgreSQL database: devdcp (user: devdcp, password: devdcp)" -ForegroundColor White
Write-Host "`n" -NoNewline

exit 0
