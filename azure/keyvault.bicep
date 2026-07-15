param keyVaultName string
param location string = resourceGroup().location

@description('Object ID of the UAMI (Workload Identity) that will read secrets from this vault')
param workloadIdentityObjectId string

// ─────────────────────────────────────────────────────────────────────────────
// Azure Key Vault — Access Policies mode (NOT RBAC mode).
//
// WHY: Contributor role CANNOT create Microsoft.Authorization/roleAssignments.
// RBAC mode on Key Vault requires a role assignment (Key Vault Secrets User).
// Access policies are a property of the vault resource itself, which a
// Contributor CAN write — no role assignment needed.
// ─────────────────────────────────────────────────────────────────────────────
resource kv 'Microsoft.KeyVault/vaults@2023-07-01' = {
  name: keyVaultName
  location: location

  properties: {
    tenantId: subscription().tenantId

    sku: {
      family: 'A'
      name: 'standard'
    }

    // Access policy mode — Contributor can write this as part of the vault resource
    enableRbacAuthorization: false
    enableSoftDelete: true
    softDeleteRetentionInDays: 7

    // Grant the Workload Identity (UAMI) read-only access to secrets.
    // objectId = UAMI's object/principal ID.
    accessPolicies: [
      {
        tenantId: subscription().tenantId
        objectId: workloadIdentityObjectId
        permissions: {
          secrets: ['get', 'list']
        }
      }
    ]
  }
}

// ─── Outputs ─────────────────────────────────────────────────────────────────
output keyVaultId string = kv.id
output keyVaultName string = kv.name
output keyVaultUri string = kv.properties.vaultUri