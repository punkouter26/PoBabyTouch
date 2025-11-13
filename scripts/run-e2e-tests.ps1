#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Starts the API server and runs Playwright E2E tests
.DESCRIPTION
    This script starts the PoBabyTouchGc API server in the background,
    waits for it to be ready, runs the Playwright E2E tests, and then
    stops the server.
#>

param(
    [int]$Port = 5000,
    [int]$TimeoutSeconds = 30
)

$ErrorActionPreference = "Stop"

# Get paths
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$rootDir = Split-Path -Parent $scriptDir
$apiProject = Join-Path $rootDir "src\PoBabyTouchGc.Api\PoBabyTouchGc.Api.csproj"
$e2eDir = Join-Path $rootDir "tests\PoBabyTouchGc.E2E"

Write-Host "Starting PoBabyTouchGc API server..." -ForegroundColor Cyan

# Start the API server in background
$serverJob = Start-Job -ScriptBlock {
    param($projectPath, $port)
    Set-Location (Split-Path -Parent $projectPath)
    dotnet run --project $projectPath --urls "http://localhost:$port"
} -ArgumentList $apiProject, $Port

Write-Host "Waiting for server to be ready at http://localhost:$Port..." -ForegroundColor Yellow

# Wait for server to be ready
$ready = $false
$elapsed = 0
while (-not $ready -and $elapsed -lt $TimeoutSeconds) {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:$Port/api/health" -Method Get -TimeoutSec 2 -UseBasicParsing
        if ($response.StatusCode -eq 200) {
            $ready = $true
            Write-Host "Server is ready!" -ForegroundColor Green
        }
    }
    catch {
        Start-Sleep -Seconds 1
        $elapsed++
        Write-Host "." -NoNewline
    }
}

if (-not $ready) {
    Write-Host "`nServer failed to start within $TimeoutSeconds seconds" -ForegroundColor Red
    Stop-Job -Job $serverJob
    Remove-Job -Job $serverJob
    exit 1
}

Write-Host "`nRunning Playwright E2E tests..." -ForegroundColor Cyan

# Run the E2E tests
Set-Location $e2eDir
$testResult = 0
try {
    npx playwright test
    $testResult = $LASTEXITCODE
}
catch {
    Write-Host "Error running tests: $_" -ForegroundColor Red
    $testResult = 1
}
finally {
    # Stop the server
    Write-Host "`nStopping API server..." -ForegroundColor Cyan
    Stop-Job -Job $serverJob
    Remove-Job -Job $serverJob
    Write-Host "Server stopped." -ForegroundColor Green
}

# Return to root directory
Set-Location $rootDir

if ($testResult -eq 0) {
    Write-Host "`nAll E2E tests passed!" -ForegroundColor Green
} else {
    Write-Host "`nSome E2E tests failed. Exit code: $testResult" -ForegroundColor Red
}

exit $testResult
