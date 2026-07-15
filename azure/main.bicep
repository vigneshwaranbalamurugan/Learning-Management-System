// ═══════════════════════════════════════════════════════════════════════════════
// LMS Infrastructure — main.bicep  (Contributor-safe, no role assignments)
//
// ACR REMOVED — Docker Hub is used for container images instead.
// No container registry is provisioned here.
//
// Resources deployed:
//   VNet → AKS → UAMI (Workload Identity) → Postgres → Redis → Key Vault → Blob → SWA
//
// Deploy from local machine (az login with your own account):
//   az deployment group create \
//     --resource-group <rg> \
//     --template-file main.bicep \
//     --parameters postgresPassword='<password>'
// ═══════════════════════════════════════════════════════════════════════════════

@secure()
@description('PostgreSQL administrator password')
param postgresPassword string

@description('Azure region for all resources (SWA uses its own fixed region)')
param location string = 'centralindia'

@description('Storage account name — must be globally unique, 3-24 lowercase alphanumeric')
param storageAccountName string = 'lms${uniqueString(resourceGroup().id)}'

// Key Vault names are GLOBALLY unique across all Azure tenants.
// Use a uniqueString suffix to avoid VaultAlreadyExists conflict.
param keyVaultName string = 'lmskv${uniqueString(resourceGroup().id)}'

// Redis hostnames are GLOBALLY unique DNS names (name.redis.cache.windows.net).
// Use a uniqueString suffix to avoid name collision with other tenants.
param redisName string = 'lmsredis${uniqueString(resourceGroup().id)}'

// ─── VNet ─────────────────────────────────────────────────────────────────────
module vnet './vnet.bicep' = {
  name: 'vnet'
  params: {
    vnetName: 'lms-vnet'
    location: location
  }
}

// ─── AKS Cluster (VNet-integrated) ───────────────────────────────────────────
module aks './aks.bicep' = {
  name: 'aks'
  params: {
    aksName: 'lms-aks-dev'
    location: location
    aksSubnetId: vnet.outputs.aksSubnetId
  }
}

// ─── Workload Identity (UAMI + federated credential) ─────────────────────────
// Lets K8s pods authenticate to Azure AD (Key Vault) without any stored secret.
// Contributor CAN create UAMI resources — no role assignment needed.
module uami './uami.bicep' = {
  name: 'uami'
  params: {
    uamiName: 'lms-workload-identity'
    location: location
    aksOidcIssuerUrl: aks.outputs.aksOidcIssuerUrl
    k8sNamespace: 'lms'
    k8sServiceAccountName: 'lms-workload-sa'
  }
}

// ─── PostgreSQL Flexible Server (VNet-restricted, private only) ───────────────
module postgres './postgres.bicep' = {
  name: 'postgres'
  params: {
    serverName: 'lms-postgres-dev'
    adminPassword: postgresPassword
    location: location
    postgresSubnetId: vnet.outputs.postgresSubnetId
    privateDnsZoneId: vnet.outputs.privateDnsZoneId
  }
}

// ─── Redis Cache (eastus — Basic C0 available there, not in centralindia) ────
// Cross-region connection from AKS (centralindia) → Redis (eastus) works fine
// over Azure's private backbone. Latency is acceptable for a cache layer.
module redis './redis.bicep' = {
  name: 'redis'
  params: {
    redisName: redisName         // unique per RG — avoids global DNS collision
    // location intentionally omitted — redis.bicep defaults to eastus
  }
}

// ─── Key Vault (access policies — no RBAC role assignment required) ───────────
module kv './keyvault.bicep' = {
  name: 'keyvault'
  params: {
    keyVaultName: keyVaultName   // unique per resource group, avoids global collision
    location: location
    workloadIdentityObjectId: uami.outputs.uamiPrincipalId
  }
}

// ─── Blob Storage ─────────────────────────────────────────────────────────────
module blob './blob.bicep' = {
  name: 'blob'
  params: {
    storageAccountName: storageAccountName
    location: location
  }
}

// ─── Azure Static Web Apps ────────────────────────────────────────────────────
module swa './staticwebapp.bicep' = {
  name: 'staticwebapp'
  params: {
    staticWebAppName: 'lms-swa-dev'
  }
}

// ─── Outputs ──────────────────────────────────────────────────────────────────
output postgresHost string = postgres.outputs.postgresHost
output redisHost string = redis.outputs.redisHost
output keyVaultName string = kv.outputs.keyVaultName
output keyVaultUri string = kv.outputs.keyVaultUri
output storageAccountName string = blob.outputs.storageAccountName
output staticWebAppHostname string = swa.outputs.staticWebAppDefaultHostname

@description('Save as GitHub Secret: AZURE_STATIC_WEB_APPS_API_TOKEN')
output swaDeploymentToken string = swa.outputs.deploymentToken

@description('Save as GitHub Secret: UAMI_CLIENT_ID')
output uamiClientId string = uami.outputs.uamiClientId

@description('Informational — used internally in keyvault access policy')
output uamiPrincipalId string = uami.outputs.uamiPrincipalId