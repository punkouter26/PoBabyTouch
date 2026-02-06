// ──────────────────────────────────────────────────────────────────────────────
// Key Vault Access Policy — grants Secret Get/List to a Managed Identity
// Deployed into the PoShared resource group via a module scope
// ──────────────────────────────────────────────────────────────────────────────

@description('Name of the existing Key Vault')
param keyVaultName string

@description('Principal ID of the managed identity')
param principalId string

@description('Principal type')
@allowed(['ServicePrincipal', 'User', 'Group'])
param principalType string = 'ServicePrincipal'

// Key Vault Secrets User role
var keyVaultSecretsUserRoleId = '4633458b-17de-408a-b874-0445c86b69e6'

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

resource keyVaultRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(keyVault.id, principalId, keyVaultSecretsUserRoleId)
  scope: keyVault
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', keyVaultSecretsUserRoleId)
    principalId: principalId
    principalType: principalType
  }
}
