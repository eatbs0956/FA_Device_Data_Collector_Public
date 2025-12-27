# check-services.ps1
# DevDCP Service Status Check Script

Write-Host "=================================" -ForegroundColor Cyan
Write-Host "DevDCP Service Status Check" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan

function Test-Port {
    param([int]$Port, [string]$ServiceName)
    
    try {
        $tcpClient = New-Object System.Net.Sockets.TcpClient
        $tcpClient.Connect("localhost", $Port)
        $tcpClient.Close()
        Write-Host "[OK] $ServiceName".PadRight(25) -NoNewline -ForegroundColor Green
        Write-Host "Running (port $Port)" -ForegroundColor Gray
        return $true
    }
    catch {
        Write-Host "[X] $ServiceName".PadRight(25) -NoNewline -ForegroundColor Red
        Write-Host "Not running (port $Port)" -ForegroundColor Gray
        return $false
    }
}

Write-Host ""

# Check backend services
$gatewayRunning = Test-Port -Port 60620 -ServiceName "Gateway.Api"
$authRunning = Test-Port -Port 60621 -ServiceName "Auth.Api"
$adminRunning = Test-Port -Port 60623 -ServiceName "Admin.Api"

# Check frontend service
$frontendRunning = Test-Port -Port 9527 -ServiceName "Frontend"

# Summary
Write-Host "`n=================================" -ForegroundColor Cyan

$runningCount = 0
if ($gatewayRunning) { $runningCount++ }
if ($authRunning) { $runningCount++ }
if ($adminRunning) { $runningCount++ }
if ($frontendRunning) { $runningCount++ }

if ($runningCount -eq 4) {
    Write-Host "[OK] All services running ($runningCount/4)" -ForegroundColor Green
} elseif ($runningCount -gt 0) {
    Write-Host "[WARNING] Partial services running ($runningCount/4)" -ForegroundColor Yellow
} else {
    Write-Host "[X] All services stopped (0/4)" -ForegroundColor Red
}

Write-Host "=================================" -ForegroundColor Cyan

# Show process information
Write-Host "`nProcess Information:" -ForegroundColor Cyan
$dotnetProcesses = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue
$nodeProcesses = Get-Process -Name "node" -ErrorAction SilentlyContinue

if ($dotnetProcesses) {
    Write-Host "   dotnet processes: $($dotnetProcesses.Count)" -ForegroundColor Gray
}
if ($nodeProcesses) {
    Write-Host "   node processes: $($nodeProcesses.Count)" -ForegroundColor Gray
}

Write-Host "`nUsage Tips:" -ForegroundColor Cyan
Write-Host "   - Run .\start-all-services.ps1 to start all services" -ForegroundColor Gray
Write-Host "   - Run .\stop-all-services.ps1 to stop all services" -ForegroundColor Gray
Write-Host "   - Run .\start-service.ps1 -ServiceName [Gateway.Api|Auth.Api|Admin.Api|Frontend] to start a single service" -ForegroundColor Gray
