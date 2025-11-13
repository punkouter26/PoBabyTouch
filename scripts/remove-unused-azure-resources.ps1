# Remove unused Azure resources from PoBabyTouch resource group
# This script removes resources that are not referenced in the application code

$resourceGroup = "PoBabyTouch"

Write-Host "Removing unused Azure resources from resource group: $resourceGroup" -ForegroundColor Cyan

# Remove duplicate Log Analytics workspace (PoBabyTouch-logs)
# The application uses "PoBabyTouch" workspace, so PoBabyTouch-logs is redundant
Write-Host "`nRemoving redundant Log Analytics workspace: PoBabyTouch-logs" -ForegroundColor Yellow
az monitor log-analytics workspace delete `
    --resource-group $resourceGroup `
    --workspace-name "PoBabyTouch-logs" `
    --yes `
    --force

# Remove unused storage account (pobabytouchstorage)
# The application uses "pobabytouch" storage account, so pobabytouchstorage is not needed
Write-Host "`nRemoving unused storage account: pobabytouchstorage" -ForegroundColor Yellow
az storage account delete `
    --name "pobabytouchstorage" `
    --resource-group $resourceGroup `
    --yes

# Remove Failure Anomalies alert rules (can be recreated automatically by Application Insights if needed)
Write-Host "`nRemoving Failure Anomalies alert rules" -ForegroundColor Yellow
az monitor metrics alert delete `
    --name "Failure Anomalies - PoBabyTouch" `
    --resource-group $resourceGroup `
    --yes

az monitor metrics alert delete `
    --name "Failure Anomalies - PoBabyTouch-insights" `
    --resource-group $resourceGroup `
    --yes

Write-Host "`nCleanup complete!" -ForegroundColor Green
Write-Host "`nRemaining resources:" -ForegroundColor Cyan
az resource list --resource-group $resourceGroup --output table
