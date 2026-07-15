@description('Azure Static Web App name')
param staticWebAppName string

// SWA is only available in a subset of regions; centralus has broad availability.
@description('Azure Region (SWA has limited region availability)')
param location string = 'centralus'

// ─────────────────────────────────────────────────────────────────────────────
// Azure Static Web Apps — Free tier is sufficient for Angular SPAs.
// The Angular build will be pushed via the GitHub Actions deployment token.
// ─────────────────────────────────────────────────────────────────────────────
resource staticWebApp 'Microsoft.Web/staticSites@2023-12-01' = {
  name: staticWebAppName
  location: location
  sku: {
    name: 'Free'
    tier: 'Free'
  }
  properties: {}
}

// ─── Outputs ─────────────────────────────────────────────────────────────────
output staticWebAppId string = staticWebApp.id
output staticWebAppDefaultHostname string = staticWebApp.properties.defaultHostname

@description('Deployment token — use as AZURE_STATIC_WEB_APPS_API_TOKEN in GitHub Secrets')
output deploymentToken string = listSecrets(staticWebApp.id, '2023-12-01').properties.apiKey
