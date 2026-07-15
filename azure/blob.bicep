@description('Storage Account Name (must be globally unique, 3-24 lowercase alphanumeric)')
param storageAccountName string

@description('Azure Region')
param location string = resourceGroup().location

@description('Storage Account SKU')
@allowed([
  'Standard_LRS'
  'Standard_GRS'
  'Standard_RAGRS'
  'Standard_ZRS'
  'Premium_LRS'
])
param storageSku string = 'Standard_LRS'

// ─────────────────────────────────────────────────────────────────────────────
// Azure Blob Storage for LMS media assets
//   lms-media  — private (videos, PDFs, assignment attachments)
//   lms-public — blob-level public access (course thumbnails, profile pictures)
// ─────────────────────────────────────────────────────────────────────────────
resource storageAccount 'Microsoft.Storage/storageAccounts@2023-05-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: storageSku
  }
  kind: 'StorageV2'
  properties: {
    accessTier: 'Hot'
    allowBlobPublicAccess: true   // required for lms-public container
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
  }
}

resource blobService 'Microsoft.Storage/storageAccounts/blobServices@2023-05-01' = {
  parent: storageAccount
  name: 'default'
}

// Private container — access via SAS tokens (videos, PDFs)
resource mediaContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  parent: blobService
  name: 'lms-media'
  properties: {
    publicAccess: 'None'
  }
}

// Public container — course thumbnails & profile pictures served directly
resource publicContainer 'Microsoft.Storage/storageAccounts/blobServices/containers@2023-05-01' = {
  parent: blobService
  name: 'lms-public'
  properties: {
    publicAccess: 'Blob'
  }
}

// ─── Outputs ─────────────────────────────────────────────────────────────────
output storageAccountId string = storageAccount.id
output storageAccountName string = storageAccount.name
output blobEndpoint string = storageAccount.properties.primaryEndpoints.blob