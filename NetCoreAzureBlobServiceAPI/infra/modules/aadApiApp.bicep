// Module for provisioning Azure AD App Registration for the APIparam apiAppName string

@description('Scope value exposed by this API, used by client apps.')
param scopeValue string = 'Files.Manage'

var identifierUri = 'api://${apiAppName}'
var scopeId = guid(apiAppName, scopeValue)

resource apiApp 'Microsoft.Graph/applications@1.0' = {
  displayName: apiAppName
  identifierUris: [
    identifierUri
  ]
  api: {
    oauth2PermissionScopes: [
      {
        id: scopeId
        value: scopeValue
        type: 'User'
        isEnabled: true
        adminConsentDisplayName: 'Manage files'
        adminConsentDescription: 'Manage files in the Blob API'
        userConsentDisplayName: 'Manage files'
        userConsentDescription: 'Manage your files'
      }
    ]
  }
}

output apiClientId string = apiApp.appId
output apiIdentifierUri string = identifierUri
output scopeId string = string(scopeId)
