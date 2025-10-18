targetScope = 'subscription'

@description('Primary location for all resources')
param location string = 'eastus'

@description('Name of the resource group (same as solution name)')
param resourceGroupName string = 'PoBabyTouch'

@description('Environment name (dev, prod, etc.)')
param environmentName string = 'dev'

// Create resource group
resource rg 'Microsoft.Resources/resourceGroups@2024-03-01' = {
  name: resourceGroupName
  location: location
  tags: {
    environment: environmentName
    project: 'PoBabyTouch'
    'azd-env-name': environmentName
  }
}

// Deploy resources into the resource group
module resources 'main.bicep' = {
  name: 'resources'
  scope: rg
  params: {
    location: location
    environmentName: environmentName
  }
}

// Output important values
output APPLICATIONINSIGHTS_CONNECTION_STRING string = resources.outputs.APPLICATIONINSIGHTS_CONNECTION_STRING
output AZURE_LOCATION string = location
output AZURE_RESOURCE_GROUP_NAME string = rg.name
