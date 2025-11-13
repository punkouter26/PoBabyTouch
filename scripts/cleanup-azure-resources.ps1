# Script to clean up unused Azure resources in PoBabyTouch resource group
# Run this script to remove duplicate/unused services and reduce costs

$resourceGroup = "PoBabyTouch"

Write-Host "🧹 Cleaning up unused Azure resources..." -ForegroundColor Cyan

# Delete duplicate Log Analytics workspace
Write-Host "`n1. Deleting duplicate Log Analytics workspace: PoBabyTouch-logs" -ForegroundColor Yellow
az monitor log-analytics workspace delete `
    --resource-group $resourceGroup `
    --workspace-name "PoBabyTouch-logs" `
    --yes `
    --force

# Delete duplicate Storage account
Write-Host "`n2. Deleting duplicate Storage account: pobabytouchstorage" -ForegroundColor Yellow
az storage account delete `
    --name "pobabytouchstorage" `
    --resource-group $resourceGroup `
    --yes

# Delete duplicate alert rule (if it exists)
Write-Host "`n3. Deleting duplicate alert rule: Failure Anomalies - PoBabyTouch-insights" -ForegroundColor Yellow
# Smart detector alert rules are managed by Azure and may auto-recreate
# This command may fail if the rule is system-managed
az monitor alert-rule delete `
    --resource-group $resourceGroup `
    --name "Failure Anomalies - PoBabyTouch-insights" `
    2>$null

Write-Host "`n✅ Cleanup complete!" -ForegroundColor Green
Write-Host "`nRemaining resources:" -ForegroundColor Cyan
Write-Host "  - App Service: PoBabyTouch (East US 2)" -ForegroundColor White
Write-Host "  - Storage Account: pobabytouch (East US)" -ForegroundColor White
Write-Host "  - Log Analytics: PoBabyTouch (East US)" -ForegroundColor White
Write-Host "  - Alert Rule: Failure Anomalies - PoBabyTouch (Global)" -ForegroundColor White

Write-Host "`n💰 Estimated monthly cost reduction: ~$2-3" -ForegroundColor Green
