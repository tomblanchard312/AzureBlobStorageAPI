// Main deployment file that orchestrates all Azure resources for the .NET Web API infrastructure
param location string = resourceGroup().location
param appName string
param storageAccountName string
param vnetName string = '${appName}-vnet'

// Entra ID app registrations are created outside of Bicep and passed in as parameters
param apiClientId string
param apiAudience string

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

// 5) Entra ID app registrations are created outside Bicep. Expect `apiClientId` and `apiAudience` parameters.

// 7) Web App with Managed Identity and app settings
module app 'modules/appService.bicep' = {
  name: 'app'
  params: {
    appName: appName
    location: location
    appSettings: {
      BlobStorage__AccountUri: storage.outputs.blobEndpoint
      AzureAd__Instance: environment().authentication.loginEndpoint
      AzureAd__ClientId: apiClientId
      AzureAd__Audience: apiAudience
    }
  }
}

// 8) RBAC: allow the Web App identity to access blobs
module rbac 'modules/rbacBlobContributor.bicep' = {
  name: 'rbac'
  params: {
    storageAccountName: storage.outputs.nameOut
    principalId: app.outputs.principalId
  }
}

output webAppHostname string = app.outputs.defaultHostname
