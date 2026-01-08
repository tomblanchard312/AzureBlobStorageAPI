// Module for assigning Storage Blob Data Contributor RBAC role
param storageAccountId string
param principalId string

// Built-in role: Storage Blob Data Contributor
var roleDefinitionGuid = 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'

resource ra 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccountId, principalId, roleDefinitionGuid)
  scope: resourceId(storageAccountId)
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleDefinitionGuid)
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}

// Helper: convert an id string to a resourceId for scope
// ARM scope wants the resource itself, so we use extension-scope trick:
resource storageExt 'Microsoft.Storage/storageAccounts@2023-01-01' existing = {
  scope: tenant()
  name: last(split(storageAccountId, '/'))
}

output roleAssignmentId string = ra.id
