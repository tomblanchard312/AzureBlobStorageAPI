@description('Location for all resources.')
param location string = resourceGroup().location

@description('Name of the storage account.')
param storageAccountName string = 'mystorageaccount${uniqueString(resourceGroup().id)}'

@description('Name of the App Service plan.')
param appServicePlanName string = 'myappserviceplan'

@description('Name of the App Service.')
param appServiceName string = 'mywebapi'

@description('SKU for the App Service plan.')
param appServicePlanSku string = 'F1'

resource storageAccount 'Microsoft.Storage/storageAccounts@2023-01-01' = {
  name: storageAccountName
  location: location
  sku: {
    name: 'Standard_LRS'
  }
  kind: 'StorageV2'
  properties: {
    allowBlobPublicAccess: false
    minimumTlsVersion: 'TLS1_2'
    supportsHttpsTrafficOnly: true
  }
}

resource appServicePlan 'Microsoft.Web/serverfarms@2022-03-01' = {
  name: appServicePlanName
  location: location
  sku: {
    name: appServicePlanSku
  }
}

resource appService 'Microsoft.Web/sites@2022-03-01' = {
  name: appServiceName
  location: location
  identity: {
    type: 'SystemAssigned'
  }
  properties: {
    serverFarmId: appServicePlan.id
    httpsOnly: true
  }
}

resource storageRoleAssignment 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccount.id, appService.id, 'Storage Blob Data Contributor')
  scope: storageAccount
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', 'ba92f5b4-2d11-453d-a403-e96b0029c9fe') // Storage Blob Data Contributor
    principalId: appService.identity.principalId
    principalType: 'ServicePrincipal'
  }
}

output storageAccountUri string = 'https://${storageAccount.name}.blob.core.windows.net/'
output appServiceUri string = 'https://${appService.name}.azurewebsites.net/'
output managedIdentityPrincipalId string = appService.identity.principalId</content>
<parameter name="filePath">main.bicep