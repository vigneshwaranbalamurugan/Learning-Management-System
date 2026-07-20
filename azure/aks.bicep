param aksName string
param location string = resourceGroup().location

@description('AKS node pool subnet resource ID (VNet integration)')
param aksSubnetId string

// ─────────────────────────────────────────────────────────────────────────────
// AKS Managed Cluster
//   • Azure CNI networking (required for VNet subnet assignment)
//   • OIDC Issuer + Workload Identity enabled (for Key Vault secret access)
//   • System-assigned identity on the cluster control plane
// ─────────────────────────────────────────────────────────────────────────────
resource aks 'Microsoft.ContainerService/managedClusters@2024-02-01' = {
  name: aksName
  location: location
  identity: {
    type: 'SystemAssigned'
  }

  properties: {
    dnsPrefix: aksName

    agentPoolProfiles: [
      {
        name: 'system'
        count: 1
        vmSize: 'Standard_D2as_v5'
        mode: 'System'
        osType: 'Linux'
        type: 'VirtualMachineScaleSets'
        vnetSubnetID: aksSubnetId
      }
      {
        name: 'ainodepool'
        count: 1
        vmSize: 'Standard_D4as_v5'
        mode: 'User'
        osType: 'Linux'
        type: 'VirtualMachineScaleSets'
        vnetSubnetID: aksSubnetId
        nodeLabels: {
          workload: 'ai-engine'
        }
      }
    ]

    // Azure CNI is required when specifying a VNet subnet for the node pool
    networkProfile: {
      networkPlugin: 'azure'
      serviceCidr: '10.1.0.0/16'
      dnsServiceIP: '10.1.0.10'
    }

    // OIDC Issuer — required for Workload Identity federation
    oidcIssuerProfile: {
      enabled: true
    }

    // Workload Identity — allows pods to get Azure AD tokens via projected service account tokens
    securityProfile: {
      workloadIdentity: {
        enabled: true
      }
    }
  }
}

// ─── Outputs ─────────────────────────────────────────────────────────────────
output aksId string = aks.id

@description('Kubelet identity object ID — used to grant AcrPull on ACR')
output kubeletIdentityObjectId string = aks.properties.identityProfile.kubeletidentity.objectId

@description('AKS OIDC Issuer URL — needed to create the federated credential on the UAMI')
output aksOidcIssuerUrl string = aks.properties.oidcIssuerProfile.issuerURL

@description('AKS control-plane system-assigned identity principal ID')
output aksPrincipalId string = aks.identity.principalId