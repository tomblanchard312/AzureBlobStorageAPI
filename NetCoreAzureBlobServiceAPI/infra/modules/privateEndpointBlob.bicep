// Module for provisioning Private Endpoint for Blob Storage
param name string
param location string = resourceGroup().location
param subnetId string
param storageAccountId string
param privateDnsZoneId string

resource pe 'Microsoft.Network/privateEndpoints@2023-09-01' = {
  name: name
  location: location
  properties: {
    subnet: {
      id: subnetId
    }
    privateLinkServiceConnections: [
      {
        name: '${name}-blob'
        properties: {
          privateLinkServiceId: storageAccountId
          groupIds: [
            'blob'
          ]
        }
      }
    ]
  }
}

resource dnsGroup 'Microsoft.Network/privateEndpoints/privateDnsZoneGroups@2023-09-01' = {
  name: '${pe.name}/default'
  properties: {
    privateDnsZoneConfigs: [
      {
        name: 'blobZone'
        properties: {
          privateDnsZoneId: privateDnsZoneId
        }
      }
    ]
  }
}

output privateEndpointId string = pe.id
