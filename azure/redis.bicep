@description('Redis Cache Name')
param redisName string

@description('Location')
param location string = 'eastus'

// Subscription requires SKU nested inside properties (Azure Managed Redis format).
// API 2023-08-01 with sku inside properties — confirmed working.
resource redisCache 'Microsoft.Cache/redis@2023-08-01' = {
  name: redisName
  location: location
  properties: {
    sku: {
      name: 'Basic'
      family: 'C'
      capacity: 0   // C0 Basic — cheapest tier, fine for learning/dev
    }
    enableNonSslPort: false
    minimumTlsVersion: '1.2'
  }
}

output redisName string = redisCache.name
output redisHost string = redisCache.properties.hostName