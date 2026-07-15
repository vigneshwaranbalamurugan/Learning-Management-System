@description('PostgreSQL Flexible Server Name')
param serverName string

@description('Location')
param location string = resourceGroup().location

@description('Admin Username')
param adminUser string = 'postgresadmin'

@secure()
param adminPassword string

@description('Database Name')
param databaseName string = 'coursehubdb'

@description('Delegated subnet for PostgreSQL VNet integration')
param postgresSubnetId string

@description('Private DNS Zone ARM resource ID')
param privateDnsZoneId string

// ─────────────────────────────────────────────────────────────────────────────
// PostgreSQL Flexible Server — VNet-restricted (no public endpoint).
// Access is only possible from within the VNet (i.e., the AKS node pool).
// ─────────────────────────────────────────────────────────────────────────────
resource postgres 'Microsoft.DBforPostgreSQL/flexibleServers@2024-08-01' = {
  name: serverName
  location: location
  sku: {
    name: 'Standard_B1ms'
    tier: 'Burstable'
  }
  properties: {
    administratorLogin: adminUser
    administratorLoginPassword: adminPassword
    version: '16'
    storage: {
      storageSizeGB: 32
    }
    backup: {
      backupRetentionDays: 7
      geoRedundantBackup: 'Disabled'
    }
    network: {
      publicNetworkAccess: 'Disabled'
      delegatedSubnetResourceId: postgresSubnetId
      privateDnsZoneArmResourceId: privateDnsZoneId
    }
    highAvailability: {
      mode: 'Disabled'
    }
  }
}

resource database 'Microsoft.DBforPostgreSQL/flexibleServers/databases@2024-08-01' = {
  parent: postgres
  name: databaseName
}

// ─── Outputs ─────────────────────────────────────────────────────────────────
output serverName string = postgres.name
output postgresHost string = postgres.properties.fullyQualifiedDomainName
output databaseName string = database.name