// Main deployment file that orchestrates all Azure resources for the .NET Web API infrastructureparam location string = resourceGroup().location
param location string = resourceGroup().location
param appName string
param storageAccountName string
param vnetName string = '${appName}-vnet'

param apiAppRegistrationName string
param consumerAppRegistrationName string

// 1) Storage
module storage 'modules/storageAccount.bicep' = {
  name: 'storage'
  params: {
    name: storageAccountName
    location: location
  }
}

// 2) VNet for Private Endpoint
module vnet 'modules/vnet.bicep' = {
  name: 'vnet'
  params: {
    vnetName: vnetName
    location: location
  }
}

// 3) Private DNS zone + link
module dns 'modules/privateDnsZoneBlob.bicep' = {
  name: 'dnsBlob'
  params: {
    vnetId: vnet.outputs.vnetId
  }
}

// 4) Private Endpoint for Blob
module pe 'modules/privateEndpointBlob.bicep' = {
  name: 'peBlob'
  params: {
    name: '${appName}-stg-blob-pe'
    location: location
    subnetId: vnet.outputs.peSubnetId
    storageAccountId: storage.outputs.id
    privateDnsZoneId: dns.outputs.privateDnsZoneId
  }
}

// 5) Azure AD API app registration
module aadApi 'modules/aadApiApp.bicep' = {
  name: 'aadApi'
  params: {
    apiAppName: apiAppRegistrationName
    scopeValue: 'Files.Manage'
  }
}

// 6) Consumer app registration
module aadConsumer 'modules/aadConsumerApp.bicep' = {
  name: 'aadConsumer'
  params: {
    consumerAppName: consumerAppRegistrationName
    apiAppClientId: aadApi.outputs.apiClientId
    apiScopeId: aadApi.outputs.scopeId
    isPublicClient: false
  }
}

// 7) Web App with Managed Identity and app settings
module app 'modules/appService.bicep' = {
  name: 'app'
  params: {
    appName: appName
    location: location
    appSettings: {
      'BlobStorage__AccountUri': storage.outputs.blobEndpoint
      'AzureAd__Instance': 'https://login.microsoftonline.com/'
      'AzureAd__TenantId': '<tenant-id>'
      'AzureAd__ClientId': aadApi.outputs.apiClientId
      'AzureAd__Audience': aadApi.outputs.apiIdentifierUri
    }
  }
}

// 8) RBAC: allow the Web App identity to access blobs
module rbac 'modules/rbacBlobContributor.bicep' = {
  name: 'rbac'
  params: {
    storageAccountId: storage.outputs.id
    principalId: app.outputs.principalId
  }
}

output webAppHostname string = app.outputs.defaultHostname
output apiAppClientId string = aadApi.outputs.apiClientId
output consumerAppClientId string = aadConsumer.outputs.consumerClientId
