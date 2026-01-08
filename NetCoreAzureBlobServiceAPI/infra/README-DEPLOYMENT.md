# Deployment Guide: NetCore Azure Blob Service API

This deployment provisions:
- Azure Storage Account (Blob)
- App Service with System Assigned Managed Identity
- RBAC assignment: Storage Blob Data Contributor
- VNet + Private Endpoint for Blob
- Private DNS Zone for Blob and VNet link
- Azure AD app registration for the API with scope Files.Manage
- Azure AD consumer app registration that requests Files.Manage

## Prerequisites

1. Azure CLI installed and logged in:
   - `az login`
2. Subscription selected:
   - `az account set --subscription "<subscription-id>"`
3. Resource group exists (or create it):
   - `az group create -n <rg-name> -l <location>`

### Permissions

To deploy everything:
- Contributor on the target resource group (for Azure resources)
- Permissions to create Entra ID app registrations if using Microsoft.Graph/applications in Bicep

If Graph deployments are blocked in your tenant, deploy the Azure resources with Bicep and create app registrations via CLI or pipeline tasks instead.

## Deploy

From repo root:

```bash
az deployment group create \
  --resource-group <rg-name> \
  --template-file infra/main.bicep \
  --parameters infra/main.parameters.json
	