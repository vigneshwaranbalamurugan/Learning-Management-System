@description('User Assigned Managed Identity name')
param uamiName string

@description('Azure Region')
param location string = resourceGroup().location

@description('AKS OIDC Issuer URL (for Workload Identity federation)')
param aksOidcIssuerUrl string

@description('Kubernetes namespace where the service account lives')
param k8sNamespace string = 'lms'

@description('Kubernetes service account name')
param k8sServiceAccountName string = 'lms-workload-sa'

// ─────────────────────────────────────────────────────────────────────────────
// User Assigned Managed Identity
// Pods annotated with this identity's clientId will be able to request
// Azure AD tokens WITHOUT a stored secret (Workload Identity / OIDC).
// ─────────────────────────────────────────────────────────────────────────────
resource uami 'Microsoft.ManagedIdentity/userAssignedIdentities@2023-01-31' = {
  name: uamiName
  location: location
}

// Federated credential: trusts tokens issued by the AKS OIDC endpoint
// for the specific Kubernetes service account.
resource federatedCredential 'Microsoft.ManagedIdentity/userAssignedIdentities/federatedIdentityCredentials@2023-01-31' = {
  parent: uami
  name: 'lms-k8s-federated'
  properties: {
    issuer: aksOidcIssuerUrl
    subject: 'system:serviceaccount:${k8sNamespace}:${k8sServiceAccountName}'
    audiences: ['api://AzureADTokenExchange']
  }
}

// ─── Outputs ─────────────────────────────────────────────────────────────────
output uamiId string = uami.id
output uamiClientId string = uami.properties.clientId
output uamiPrincipalId string = uami.properties.principalId
