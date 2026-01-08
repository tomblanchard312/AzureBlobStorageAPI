// Module for provisioning Private DNS Zone for Blob Storage
param location string = resourceGroup().location
param vnetId string

var zoneName = 'privatelink.blob.core.windows.net'

resource zone 'Microsoft.Network/privateDnsZones@2020-06-01' = {
  name: zoneName
  location: 'global'
}

resource vnetLink 'Microsoft.Network/privateDnsZones/virtualNetworkLinks@2020-06-01' = {
  name: '${zone.name}/${uniqueString(vnetId)}-link'
  location: 'global'
  properties: {
    virtualNetwork: {
      id: vnetId
    }
    registrationEnabled: false
  }
}

output privateDnsZoneId string = zone.id
output privateDnsZoneName string = zone.name
