@description('Primary location for all resources')
param location string = resourceGroup().location

@description('Environment name')
param environmentName string

// Common naming convention - all resources use the same name as the resource group
var baseName = 'PoBabyTouch'

// Storage Account for Table Storage (required by the application)
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: toLower(baseName) // Storage account names must be lowercase
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS' // Cheapest option
  }
  properties: {
    minimumTlsVersion: 'TLS1_2'
    allowBlobPublicAccess: false
    supportsHttpsTrafficOnly: true
  }
  tags: {
    environment: environmentName
    project: baseName
  }
}

// Application Insights (required for logging/monitoring)
resource appInsights 'Microsoft.Insights/components@2020-02-02' = {
  name: baseName
  location: location
  kind: 'web'
  properties: {
    Application_Type: 'web'
    Flow_Type: 'Redfield'
    Request_Source: 'IbizaAIExtension'
    RetentionInDays: 30 // Minimum retention to keep costs low
    IngestionMode: 'ApplicationInsights'
    publicNetworkAccessForIngestion: 'Enabled'
    publicNetworkAccessForQuery: 'Enabled'
  }
  tags: {
    environment: environmentName
    project: baseName
  }
}

// Log Analytics Workspace (free tier)
resource logAnalytics 'Microsoft.OperationalInsights/workspaces@2023-09-01' = {
  name: baseName
  location: location
  properties: {
    sku: {
      name: 'PerGB2018' // Pay-as-you-go, very low cost for small apps
    }
    retentionInDays: 30 // Minimum retention
    features: {
      enableLogAccessUsingOnlyResourcePermissions: true
    }
  }
  tags: {
    environment: environmentName
    project: baseName
  }
}

// Reference existing shared App Service Plan (F1 tier)
resource existingAppServicePlan 'Microsoft.Web/serverfarms@2023-12-01' existing = {
  name: 'PoShared'
  scope: resourceGroup('PoShared')
}

// App Service (Web App) - Using existing shared F1 plan
resource appService 'Microsoft.Web/sites@2023-12-01' = {
  name: baseName
  location: 'eastus2' // Must match the existing plan location
  kind: 'app'
  properties: {
    serverFarmId: existingAppServicePlan.id
    httpsOnly: true
    siteConfig: {
      netFrameworkVersion: 'v9.0'
      use32BitWorkerProcess: true // Required for F1 tier
      alwaysOn: false // Must be false for F1 tier
      metadata: [
        {
          name: 'CURRENT_STACK'
          value: 'dotnet'
        }
      ]
      appSettings: [
        {
          name: 'APPLICATIONINSIGHTS_CONNECTION_STRING'
          value: appInsights.properties.ConnectionString
        }
        {
          name: 'ApplicationInsightsAgent_EXTENSION_VERSION'
          value: '~3'
        }
        {
          name: 'ASPNETCORE_ENVIRONMENT'
          value: environmentName == 'prod' ? 'Production' : 'Development'
        }
        {
          name: 'WEBSITE_RUN_FROM_PACKAGE'
          value: '1'
        }
      ]
      connectionStrings: [
        {
          name: 'AzureTableStorage'
          connectionString: 'DefaultEndpointsProtocol=https;AccountName=${storageAccount.name};AccountKey=${storageAccount.listKeys().keys[0].value};EndpointSuffix=core.windows.net'
          type: 'Custom'
        }
      ]
    }
  }
  tags: {
    environment: environmentName
    project: baseName
    'azd-service-name': 'api'
  }
}

// Outputs for use by the application and other scripts
output APPLICATIONINSIGHTS_CONNECTION_STRING string = appInsights.properties.ConnectionString
output STORAGE_ACCOUNT_NAME string = storageAccount.name
output APP_SERVICE_NAME string = appService.name
output APP_SERVICE_URL string = 'https://${appService.properties.defaultHostName}'
output RESOURCE_GROUP_NAME string = resourceGroup().name
