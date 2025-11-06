param(
 [string]$AppHostPath = "src/AppHost",
 [string]$E2EProject = "e2e/Web.Tests.Playwright/Web.Tests.Playwright.csproj",
 [string]$BaseUrl = "http://localhost:5057",
 [int]$WaitSeconds =120
)

$ErrorActionPreference = 'Stop'

Write-Host "Starting AppHost..." -ForegroundColor Cyan
$log = "apphost_output.log"
if (Test-Path $log) { Remove-Item $log -Force }
# Redirect stdout to a log for URL discovery
$proc = Start-Process -FilePath "dotnet" -ArgumentList @('run','--project', $AppHostPath) -RedirectStandardOutput $log -PassThru

try {
 $deadline = (Get-Date).AddSeconds($WaitSeconds)
 $ready = $false
 while (-not $ready -and (Get-Date) -lt $deadline) {
 try {
 $resp = Invoke-WebRequest -Uri $BaseUrl -UseBasicParsing -TimeoutSec5 -ErrorAction Stop
 if ($resp.StatusCode -ge200 -or $resp.StatusCode -eq404) { $ready = $true }
 } catch { Start-Sleep -Seconds1 }
 }
 if (-not $ready) { Write-Warning "Base URL $BaseUrl did not respond; tests may be skipped by the fixture." }

 Write-Host "Running Playwright E2E with BASE_URL=$BaseUrl" -ForegroundColor Cyan
 $env:BASE_URL = $BaseUrl
 dotnet test $E2EProject --nologo --verbosity minimal
}
finally {
 if ($proc -and -not $proc.HasExited) {
 Write-Host "Stopping AppHost (PID $($proc.Id))" -ForegroundColor Yellow
 Stop-Process -Id $proc.Id -Force -ErrorAction SilentlyContinue
 }
}

