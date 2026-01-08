// Module for provisioning Azure App Service with Managed Identityparam appName string
param location string = resourceGroup().location
param skuName string = 'P1v3'
param skuTier string = 'PremiumV3'

@description('Optional app settings to merge in.')
param appSettings object = {}

resource plan 'Microsoft.Web/serverfarms@2023-01-01' = {
  name: '${appName}-plan'
  location: location
  sku: {
    name: skuName
    tier: skuTier
  }
}

resource site 'Microsoft.Web/sites@2023-01-01' = {
  name: appName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: plan.id
    httpsOnly: true
  }
}

resource cfg 'Microsoft.Web/sites/config@2023-01-01' = {
  parent: site
  name: 'appsettings'
  properties: appSettings
}

output webAppId string = site.id
output principalId string = site.identity.principalId
output defaultHostname string = site.properties.defaultHostName
