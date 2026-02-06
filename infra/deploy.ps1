#!/usr/bin/env pwsh
# ──────────────────────────────────────────────────────────────────────────────
# PoBabyTouch – Deploy infrastructure and apps to Azure
# Usage: ./infra/deploy.ps1 [-EnvironmentName prod]
# ──────────────────────────────────────────────────────────────────────────────

param(
    [ValidateSet('dev', 'prod')]
    [string]$EnvironmentName = 'prod',
    [switch]$InfraOnly,
    [switch]$SkipInfra
)

$ErrorActionPreference = 'Stop'
$RepoRoot = Split-Path $PSScriptRoot -Parent
$ResourceGroup = 'PoBabyTouch'
$Location = 'eastus2'

Write-Host "`n═══ PoBabyTouch Azure Deployment ═══" -ForegroundColor Cyan
Write-Host "Environment: $EnvironmentName" -ForegroundColor DarkGray
Write-Host "Resource Group: $ResourceGroup" -ForegroundColor DarkGray

# ── 1. Ensure Resource Group ─────────────────────────────────────────────────
if (-not $SkipInfra) {
    Write-Host "`n[1/5] Creating resource group..." -ForegroundColor Yellow
    az group create --name $ResourceGroup --location $Location --output none

    # ── 2. Deploy Bicep ──────────────────────────────────────────────────────
    Write-Host "[2/5] Deploying infrastructure (Bicep)..." -ForegroundColor Yellow
    $deployment = az deployment group create `
        --resource-group $ResourceGroup `
        --template-file "$PSScriptRoot/main.bicep" `
        --parameters "$PSScriptRoot/main.parameters.json" `
        --parameters environmentName=$EnvironmentName `
        --query "properties.outputs" `
        --output json | ConvertFrom-Json

    $appServiceName = $deployment.appServiceName.value
    $swaName = $deployment.staticWebAppName.value
    $storageAccountName = $deployment.storageAccountName.value
    $apiUrl = $deployment.appServiceUrl.value
    $swaUrl = $deployment.staticWebAppUrl.value

    Write-Host "  Storage Account: $storageAccountName" -ForegroundColor DarkGray
    Write-Host "  App Service:     $appServiceName ($apiUrl)" -ForegroundColor DarkGray
    Write-Host "  Static Web App:  $swaName ($swaUrl)" -ForegroundColor DarkGray
}
else {
    # Look up existing resources
    $suffix = if ($EnvironmentName -eq 'prod') { '' } else { "-$EnvironmentName" }
    $appServiceName = "app-pobabytouch-api$suffix"
    $swaName = "swa-pobabytouch$suffix"
    $apiUrl = "https://$(az webapp show --name $appServiceName --resource-group $ResourceGroup --query defaultHostName -o tsv)"
    Write-Host "`n[1-2/5] Skipping infrastructure deployment" -ForegroundColor DarkGray
}

if ($InfraOnly) {
    Write-Host "`n✅ Infrastructure deployed. Skipping app deployment." -ForegroundColor Green
    exit 0
}

# ── 3. Publish & Deploy API ──────────────────────────────────────────────────
Write-Host "`n[3/5] Publishing .NET API..." -ForegroundColor Yellow
$publishPath = Join-Path $RepoRoot "publish/api"
dotnet publish "$RepoRoot/src/PoBabyTouchGc.Api/PoBabyTouchGc.Api.csproj" `
    --configuration Release `
    --output $publishPath

Write-Host "[3/5] Deploying API to App Service ($appServiceName)..." -ForegroundColor Yellow
Push-Location $publishPath
Compress-Archive -Path * -DestinationPath "$publishPath/deploy.zip" -Force
az webapp deploy --resource-group $ResourceGroup --name $appServiceName --src-path "$publishPath/deploy.zip" --type zip
Pop-Location

# ── 4. Build & Deploy React Client ──────────────────────────────────────────
Write-Host "`n[4/5] Building React client..." -ForegroundColor Yellow
Push-Location "$RepoRoot/src/client"
$env:VITE_API_BASE_URL = $apiUrl
npm ci
npm run build
Pop-Location

Write-Host "[4/5] Deploying React to Static Web App ($swaName)..." -ForegroundColor Yellow
$swaToken = az staticwebapp secrets list --name $swaName --resource-group $ResourceGroup --query "properties.apiKey" -o tsv

# Use SWA CLI for deployment
npx --yes @azure/static-web-apps-cli deploy `
    "$RepoRoot/src/client/dist" `
    --deployment-token $swaToken `
    --env default

# ── 5. Verify ────────────────────────────────────────────────────────────────
Write-Host "`n[5/5] Verifying deployment..." -ForegroundColor Yellow

$healthResponse = try { Invoke-RestMethod -Uri "$apiUrl/health" -TimeoutSec 30 } catch { $_ }
if ($healthResponse -eq 'Healthy') {
    Write-Host "  API Health: ✅ Healthy" -ForegroundColor Green
}
else {
    Write-Host "  API Health: ⚠️  $healthResponse" -ForegroundColor Red
}

Write-Host "`n═══ Deployment Complete ═══" -ForegroundColor Cyan
Write-Host "  API:    $apiUrl" -ForegroundColor White
Write-Host "  Client: https://$(az staticwebapp show --name $swaName --resource-group $ResourceGroup --query defaultHostname -o tsv)" -ForegroundColor White
Write-Host ""
