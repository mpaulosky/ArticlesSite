<#
.SYNOPSIS
Run unit tests with code coverage and generate an HTML report using ReportGenerator.

This script installs ReportGenerator locally to `.tools` (if not already installed), runs `dotnet test` with the built-in Coverlet collector, finds the generated coverage file, and converts it to an HTML report.
#>

param(
	[string]$TestProject = "tests/Shared.Tests.Unit/Shared.Tests.Unit.csproj",
	[string]$ResultsDir = "TestResults/coverage",
	[string]$ReportDir = "coverage/shared"
)

Write-Host "Installing ReportGenerator to .tools (if missing)..." -ForegroundColor Cyan
$toolPath = [System.IO.Path]::GetFullPath([System.IO.Path]::Combine($PSScriptRoot, '..', '.tools'))
if (-not (Test-Path -Path $toolPath)) {
	dotnet tool install dotnet-reportgenerator-globaltool --tool-path (Join-Path $PSScriptRoot '..' '.tools') --version 5.*
}

Write-Host "Running tests with coverage collector..." -ForegroundColor Cyan
dotnet test $TestProject --no-build --collect:"XPlat Code Coverage" --results-directory $ResultsDir | Write-Host

# Find coverage file
$coverageFile = Get-ChildItem -Path $ResultsDir -Recurse -Filter "coverage.cobertura.xml" -ErrorAction SilentlyContinue | Select-Object -First 1
if (-not $coverageFile) {
	Write-Error "Coverage file not found in $ResultsDir. Ensure tests ran with --collect:'XPlat Code Coverage'."
	exit 1
}

# Generate HTML report
Write-Host "Generating HTML report in $ReportDir..." -ForegroundColor Cyan
$reportGen = [System.IO.Path]::GetFullPath([System.IO.Path]::Combine($PSScriptRoot, '..', '.tools', 'reportgenerator'))
& $reportGen -reports:"$($coverageFile.FullName)" -targetdir:"$ReportDir" -reporttypes:Html

Write-Host "Coverage report generated at: $ReportDir/index.htm" -ForegroundColor Green
