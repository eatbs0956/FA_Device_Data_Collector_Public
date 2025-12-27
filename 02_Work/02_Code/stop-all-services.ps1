# stop-all-services.ps1
# DevDCP Microservices Stop Script

Write-Host "=================================" -ForegroundColor Red
Write-Host "Stopping DevDCP Microservices..." -ForegroundColor Red
Write-Host "=================================" -ForegroundColor Red

# Stop all dotnet processes (Auth.Api, Device.Api, Gateway.Api, etc.)
Write-Host "`nStopping backend services (dotnet)..." -ForegroundColor Yellow
$dotnetProcesses = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue
if ($dotnetProcesses) {
    $dotnetProcesses | Stop-Process -Force
    Write-Host "  [OK] Stopped $($dotnetProcesses.Count) dotnet process(es)" -ForegroundColor Green
    Write-Host "       (Auth.Api, Device.Api, Gateway.Api, etc.)" -ForegroundColor Gray
} else {
    Write-Host "  [INFO] No dotnet processes running" -ForegroundColor Gray
}

# Stop all node processes (frontend service)
Write-Host "`nStopping frontend services (node)..." -ForegroundColor Yellow
$nodeProcesses = Get-Process -Name "node" -ErrorAction SilentlyContinue
if ($nodeProcesses) {
    $nodeProcesses | Stop-Process -Force
    Write-Host "  [OK] Stopped $($nodeProcesses.Count) node process(es)" -ForegroundColor Green
} else {
    Write-Host "  [INFO] No node processes running" -ForegroundColor Gray
}

# Stop all PowerShell windows that were started by start-all-services.ps1
Write-Host "`nClosing service PowerShell windows..." -ForegroundColor Yellow
$currentPID = $PID
$allPowerShellProcesses = Get-Process -Name "powershell" -ErrorAction SilentlyContinue | Where-Object { 
    $_.Id -ne $currentPID -and $_.MainWindowTitle -ne ""
}
if ($allPowerShellProcesses) {
    $allPowerShellProcesses | Stop-Process -Force
    Write-Host "  [OK] Closed $($allPowerShellProcesses.Count) PowerShell window(s)" -ForegroundColor Green
} else {
    Write-Host "  [INFO] No service PowerShell windows found" -ForegroundColor Gray
}

# Also check for pwsh (PowerShell Core) windows
$pwshProcesses = Get-Process -Name "pwsh" -ErrorAction SilentlyContinue | Where-Object { 
    $_.Id -ne $currentPID -and $_.MainWindowTitle -ne ""
}
if ($pwshProcesses) {
    $pwshProcesses | Stop-Process -Force
    Write-Host "  [OK] Closed $($pwshProcesses.Count) pwsh window(s)" -ForegroundColor Green
}

Write-Host "`n=================================" -ForegroundColor Green
Write-Host "All services stopped successfully!" -ForegroundColor Green
Write-Host "=================================" -ForegroundColor Green

# Pause briefly to show the message
Start-Sleep -Seconds 2
