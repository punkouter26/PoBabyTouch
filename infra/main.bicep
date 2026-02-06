// ──────────────────────────────────────────────────────────────────────────────
// PoBabyTouch – Infrastructure as Code (Bicep)
//
// PoBabyTouch RG:  Storage Account (Tables), Static Web App (React)
// PoShared RG:     App Service (API) on shared Linux plan, Key Vault access
// ──────────────────────────────────────────────────────────────────────────────

targetScope = 'resourceGroup'

// ── Parameters ───────────────────────────────────────────────────────────────

@description('Primary location for all resources')
param location string = resourceGroup().location

@description('Environment name used for resource naming')
@allowed(['dev', 'prod'])
param environmentName string = 'prod'

@description('The PoShared resource group name')
param sharedResourceGroup string = 'PoShared'

@description('Shared Linux App Service Plan name in PoShared')
param sharedAppServicePlanName string = 'asp-poshared-linux'

@description('Shared Application Insights name in PoShared')
param sharedAppInsightsName string = 'poappideinsights8f9c9a4e'

@description('Shared Key Vault name in PoShared')
param sharedKeyVaultName string = 'kv-poshared'

@description('Shared Log Analytics workspace name in PoShared')
param sharedLogAnalyticsName string = 'PoShared-LogAnalytics'

@description('Location of the shared App Service Plan (must match)')
param sharedAppServicePlanLocation string = 'westus2'

@description('Static Web App SKU')
@allowed(['Free', 'Standard'])
param swaSku string = 'Free'

// ── Naming Convention ────────────────────────────────────────────────────────

var appName = 'pobabytouch'
var suffix = environmentName == 'prod' ? '' : '-${environmentName}'
var storageAccountName = 'st${appName}${take(uniqueString(resourceGroup().id), 4)}'
var appServiceName = 'app-${appName}-api${suffix}'
var swaName = 'swa-${appName}${suffix}'

// ── References to PoShared Resources ─────────────────────────────────────────

resource sharedAppInsights 'Microsoft.Insights/components@2020-02-02' existing = {
  name: sharedAppInsightsName
  scope: resourceGroup(sharedResourceGroup)
}

resource sharedLogAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' existing = {
  name: sharedLogAnalyticsName
  scope: resourceGroup(sharedResourceGroup)
}

resource sharedKeyVault 'Microsoft.KeyVault/vaults@2024-04-01' existing = {
  name: sharedKeyVaultName
  scope: resourceGroup(sharedResourceGroup)
}

// ── Storage Account (Azure Table Storage) — in PoBabyTouch RG ───────────────

resource storageAccount 'Microsoft.Storage/storageAccounts@2024-01-01' = {
  name: storageAccountName
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
    allowBlobPublicAccess: false
    accessTier: 'Hot'
  }
}

resource tableService 'Microsoft.Storage/storageAccounts/tableServices@2024-01-01' = {
  parent: storageAccount
  name: 'default'
}

resource highScoresTable 'Microsoft.Storage/storageAccounts/tableServices/tables@2024-01-01' = {
  parent: tableService
  name: 'PoBabyTouchHighScores'
}

resource gameStatsTable 'Microsoft.Storage/storageAccounts/tableServices/tables@2024-01-01' = {
  parent: tableService
  name: 'PoBabyTouchGcGameStats'
}

// ── Static Web App (React Client) — in PoBabyTouch RG ───────────────────────

resource staticWebApp 'Microsoft.Web/staticSites@2024-04-01' = {
  name: swaName
  location: location
  sku: {
    name: swaSku
    tier: swaSku
  }
  properties: {
    buildProperties: {
      appLocation: 'src/client'
      outputLocation: 'dist'
    }
  }
}

// ── App Service (API) — deployed into PoShared RG ────────────────────────────

var storageConnectionString = 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=core.windows.net'

// Store the storage connection string in Key Vault with app-name prefix (non-shared secret)
module storageSecret 'modules/keyvault-secret.bicep' = {
  name: 'secret-${appName}-AzureTableStorage'
  scope: resourceGroup(sharedResourceGroup)
  params: {
    keyVaultName: sharedKeyVaultName
    secretName: 'PoBabyTouch-AzureTableStorage'
    secretValue: storageConnectionString
  }
}

module apiAppService 'modules/appservice.bicep' = {
  name: 'api-${appServiceName}'
  scope: resourceGroup(sharedResourceGroup)
  params: {
    location: sharedAppServicePlanLocation
    appServiceName: appServiceName
    appServicePlanName: sharedAppServicePlanName
    appInsightsConnectionString: sharedAppInsights.properties.ConnectionString
    appInsightsInstrumentationKey: sharedAppInsights.properties.InstrumentationKey
    keyVaultName: sharedKeyVaultName
    keyVaultUri: 'https://${sharedKeyVaultName}${environment().suffixes.keyvaultDns}'
    swaHostname: staticWebApp.properties.defaultHostname
    logAnalyticsWorkspaceId: sharedLogAnalytics.id
  }
  dependsOn: [
    storageSecret
  ]
}

// ── Key Vault Access for App Service (Managed Identity) ──────────────────────

module keyVaultAccess 'modules/keyvault-access.bicep' = {
  name: 'keyVaultAccess-${appServiceName}'
  scope: resourceGroup(sharedResourceGroup)
  params: {
    keyVaultName: sharedKeyVaultName
    principalId: apiAppService.outputs.appServicePrincipalId
    principalType: 'ServicePrincipal'
  }
}

// ── Storage Account Role Assignment (Managed Identity → Table Data) ──────────

var storageTableDataContributorRoleId = '0a9a7e1f-b9d0-4cc4-a60d-0319b160aaa3'

resource storageRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccount.id, appServiceName, storageTableDataContributorRoleId)
  scope: storageAccount
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', storageTableDataContributorRoleId)
    principalId: apiAppService.outputs.appServicePrincipalId
    principalType: 'ServicePrincipal'
  }
}

// ── Outputs ──────────────────────────────────────────────────────────────────

output resourceGroupName string = resourceGroup().name
output storageAccountName string = storageAccount.name
output appServiceName string = appServiceName
output appServiceHostname string = apiAppService.outputs.appServiceHostname
output appServiceUrl string = apiAppService.outputs.appServiceUrl
output staticWebAppName string = staticWebApp.name
output staticWebAppHostname string = staticWebApp.properties.defaultHostname
output staticWebAppUrl string = 'https://${staticWebApp.properties.defaultHostname}'
