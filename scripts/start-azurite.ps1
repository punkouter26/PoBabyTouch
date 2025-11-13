# Start Azurite for local Azure Storage emulation
# Run this script before running tests that require Azure Table Storage

Write-Host "Starting Azurite..." -ForegroundColor Cyan

# Check if Azurite is installed
if (-not (Get-Command azurite -ErrorAction SilentlyContinue)) {
    Write-Host "Azurite is not installed. Installing via npm..." -ForegroundColor Yellow
    npm install -g azurite
}

# Start Azurite in the background
# Data will be stored in the AzuriteData folder
Start-Process -NoNewWindow -FilePath "azurite" -ArgumentList "--location", "AzuriteData", "--silent"

Write-Host "Azurite started successfully!" -ForegroundColor Green
Write-Host "Table Storage endpoint: http://127.0.0.1:10002" -ForegroundColor Gray
Write-Host ""
Write-Host "To stop Azurite, run: Stop-Process -Name azurite" -ForegroundColor Gray
