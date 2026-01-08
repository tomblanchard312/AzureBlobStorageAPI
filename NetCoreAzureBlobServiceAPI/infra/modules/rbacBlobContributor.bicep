// Module for assigning Storage Blob Data Contributor RBAC role

param storageAccountName string
param principalId string

// Built-in role: Storage Blob Data Contributor
var roleDefinitionGuid = 'ba92f5b4-2d11-453d-a403-e96b0029c9fe'

resource ra 'Microsoft.Authorization/roleAssignments@2022-04-01' = {
  name: guid(storageAccountName, principalId, roleDefinitionGuid)
  properties: {
    roleDefinitionId: subscriptionResourceId('Microsoft.Authorization/roleDefinitions', roleDefinitionGuid)
    principalId: principalId
    principalType: 'ServicePrincipal'
  }
}

output roleAssignmentId string = ra.id
