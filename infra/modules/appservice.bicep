// ──────────────────────────────────────────────────────────────────────────────
// PoBabyTouch – App Service module (deployed into PoShared resource group)
// Creates the API App Service on the shared Linux Free tier plan
// ──────────────────────────────────────────────────────────────────────────────

@description('Location for the App Service')
param location string

@description('App Service name')
param appServiceName string

@description('Shared Linux App Service Plan name')
param appServicePlanName string

@description('Application Insights connection string')
param appInsightsConnectionString string

@description('Application Insights instrumentation key')
param appInsightsInstrumentationKey string

@description('Key Vault name for secret references')
param keyVaultName string

@description('Key Vault URI')
param keyVaultUri string

@description('SWA default hostname for CORS')
param swaHostname string

@description('Log Analytics workspace ID for diagnostics')
param logAnalyticsWorkspaceId string

// ── App Service Plan (existing) ──────────────────────────────────────────────

resource appServicePlan 'Microsoft.Web/serverFarms@2024-04-01' existing = {
  name: appServicePlanName
}

// Reference existing Key Vault for secret URIs
resource keyVault 'Microsoft.KeyVault/vaults@2024-04-01' existing = {
  name: keyVaultName
}

// ── App Service (API – .NET 10) ──────────────────────────────────────────────

resource appService 'Microsoft.Web/sites@2024-04-01' = {
  name: appServiceName
  location: location // Must match the App Service Plan's location
  kind: 'app,linux'
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
    siteConfig: {
      linuxFxVersion: 'DOTNETCORE|10.0'
      alwaysOn: false // Free tier doesn't support alwaysOn
      ftpsState: 'Disabled'
      minTlsVersion: '1.2'
      healthCheckPath: '/health'
      appSettings: [
        { name: 'APPLICATIONINSIGHTS_CONNECTION_STRING', value: appInsightsConnectionString }
        { name: 'ApplicationInsights__InstrumentationKey', value: appInsightsInstrumentationKey }
        { name: 'ASPNETCORE_ENVIRONMENT', value: 'Production' }
        { name: 'ConnectionStrings__AzureTableStorage', value: '@Microsoft.KeyVault(VaultName=${keyVaultName};SecretName=PoBabyTouch-AzureTableStorage)' }
        { name: 'KeyVault__VaultUri', value: keyVaultUri }
        { name: 'AllowedOrigins__0', value: 'https://${swaHostname}' }
      ]
    }
  }
}

// ── Diagnostic Settings ──────────────────────────────────────────────────────

resource appServiceDiagnostics 'Microsoft.Insights/diagnosticSettings@2021-05-01-preview' = {
  name: '${appServiceName}-diagnostics'
  scope: appService
  properties: {
    workspaceId: logAnalyticsWorkspaceId
    logs: [
      { category: 'AppServiceHTTPLogs', enabled: true }
      { category: 'AppServiceConsoleLogs', enabled: true }
      { category: 'AppServiceAppLogs', enabled: true }
    ]
    metrics: [
      { category: 'AllMetrics', enabled: true }
    ]
  }
}

// ── Outputs ──────────────────────────────────────────────────────────────────

output appServicePrincipalId string = appService.identity.principalId
output appServiceHostname string = appService.properties.defaultHostName
output appServiceUrl string = 'https://${appService.properties.defaultHostName}'
output appServiceId string = appService.id
