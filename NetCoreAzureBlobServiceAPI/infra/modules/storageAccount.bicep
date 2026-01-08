// Module for provisioning Azure Storage Account with secure defaults
param name string
param location string = resourceGroup().location

resource stg 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: name
  location: location
  kind: 'StorageV2'
  sku: {
    name: 'Standard_LRS'
  }
  properties: {
    allowBlobPublicAccess: false
    minimumTlsVersion: 'TLS1_2'
  }
}

output id string = stg.id
output nameOut string = stg.name
output blobEndpoint string = stg.properties.primaryEndpoints.blob
