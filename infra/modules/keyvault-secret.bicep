// ──────────────────────────────────────────────────────────────────────────────
// Key Vault Secret — stores a secret in the shared Key Vault
// Non-shared secrets MUST be prefixed with the app name (e.g., PoBabyTouch-*)
// Shared secrets (used across multiple apps) are NOT prefixed
// ──────────────────────────────────────────────────────────────────────────────

@description('Name of the existing Key Vault')
param keyVaultName string

@description('Name of the secret (use AppName-SecretName for non-shared secrets)')
param secretName string

@description('Value of the secret')
@secure()
param secretValue string

resource keyVault 'Microsoft.KeyVault/vaults@2023-07-01' existing = {
  name: keyVaultName
}

resource secret 'Microsoft.KeyVault/vaults/secrets@2023-07-01' = {
  parent: keyVault
  name: secretName
  properties: {
    value: secretValue
  }
}

output secretUri string = secret.properties.secretUri
output secretUriWithVersion string = secret.properties.secretUriWithVersion
