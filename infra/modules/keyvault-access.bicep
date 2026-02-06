// ──────────────────────────────────────────────────────────────────────────────
// Key Vault Access Policy — grants Secret Get/List to a Managed Identity
// Deployed into the PoShared resource group via a module scope
// Uses access policies (NOT RBAC) because kv-poshared has
// enableRbacAuthorization = false.
// ──────────────────────────────────────────────────────────────────────────────

@description('Name of the existing Key Vault')
param keyVaultName string

@description('Principal ID of the managed identity')
param principalId string

@description('Tenant ID for the access policy')
param tenantId string = subscription().tenantId

@description('Principal type (unused — kept for backwards compat)')
@allowed(['ServicePrincipal', 'User', 'Group'])
param principalType string = 'ServicePrincipal'

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

// Add an access policy for secret get + list
resource keyVaultAccessPolicy 'Microsoft.KeyVault/vaults/accessPolicies@2023-07-01' = {
  parent: keyVault
  name: 'add'
  properties: {
    accessPolicies: [
      {
        tenantId: tenantId
        objectId: principalId
        permissions: {
          secrets: [
            'get'
            'list'
          ]
        }
      }
    ]
  }
}
