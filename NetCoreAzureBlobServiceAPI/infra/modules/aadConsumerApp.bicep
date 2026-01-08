// Module for provisioning Azure AD App Registration for consumer applications
param consumerAppName string
param apiAppClientId string
param apiScopeId string

@description('Set true if this is a public client (desktop/mobile).')
param isPublicClient bool = false

resource consumerApp 'Microsoft.Graph/applications@1.0' = {
  displayName: consumerAppName

  // If you need redirect URIs, add them here based on client type.
  publicClient: isPublicClient ? {
    redirectUris: [
      'http://localhost'
    ]
  } : null

  requiredResourceAccess: [
    {
      resourceAppId: apiAppClientId
      resourceAccess: [
        {
          id: guid(apiScopeId)
          type: 'Scope'
        }
      ]
    }
  ]
}

output consumerClientId string = consumerApp.appId
