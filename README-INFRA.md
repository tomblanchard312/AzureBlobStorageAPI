# Infrastructure Reference (Bicep)

Purpose of this file

- This document describes the infrastructure only. It complements `README.md` by focusing on Azure resources, Bicep modules, networking, identity, and operational considerations for engineers deploying or consuming the infrastructure.

Infrastructure overview

- This repository uses modular Azure Bicep under `NetCoreAzureBlobServiceAPI/infra` to provision the API platform and its supporting resources. Deployed resources include:
  - App Service hosting the API with a system-assigned Managed Identity
  - Azure Storage Account configured for Blob Storage
  - RBAC assignment granting the App Service identity Storage Blob Data Contributor role
  - Virtual Network and subnets
  - Private Endpoint for Blob Storage and a Private DNS zone for blob resolution
  - Microsoft Entra ID app registrations for the API (API app) and for clients (consumer app)

Relationship between components

- The App Service presents the API to clients. Authentication and authorization are handled by Entra ID (the API requires the `Files.Manage` scope). The App Service uses its Managed Identity to obtain access to Blob Storage. The storage account is reachable only via the Private Endpoint from the VNet or peered networks that resolve the private DNS zone.

Module breakdown

- The Bicep modules are under `NetCoreAzureBlobServiceAPI/infra/modules`. Each module has a focused responsibility.

- `storageAccount.bicep`

  - Creates the storage account and blob endpoints.
  - Exposes the blob endpoint output used to configure the App Service app setting.

- `vnet.bicep`

  - Creates the virtual network and subnets used for the private endpoint and any future VNet-bound resources.
  - Exposes subnet ids used by the private endpoint module.

- `privateEndpointBlob.bicep`

  - Provisions the Private Endpoint for the storage account in the specified subnet.
  - Connects the private endpoint to the storage resource.

- `privateDnsZoneBlob.bicep`

  - Creates the Private DNS zone for Blob Storage hostnames and links it to the VNet so the storage account hostname resolves to the private endpoint IP.

- `aadApiApp.bicep`

  - Creates the Entra ID application registration for the API and defines the `Files.Manage` scope.
  - Outputs API client id, scope id, and identifier URI used to configure clients and the App Service.

- `aadConsumerApp.bicep`

  - Optionally creates a consumer app registration representing a client/service that will request tokens for the API. Outputs the consumer client id.

- `appService.bicep`

  - Creates the Web App and enables a system-assigned Managed Identity.
  - Writes app settings such as `BlobStorage__AccountUri` and `AzureAd` configuration values required by the API.
  - Outputs the principal id of the Web App identity used by RBAC.

- `rbacBlobContributor.bicep`
  - Assigns the Storage Blob Data Contributor role to the App Service principal id for the target storage account scope.

Security architecture

- Identity flow

  1. A client (user or service) authenticates with Entra ID and obtains an access token for the API using the `Files.Manage` scope.
  2. The client calls the API; the API validates the token and obtains caller claims (including `oid`).
  3. When the API needs to access Blob Storage it uses the App Service system-assigned Managed Identity. The API code does not hold secrets or storage keys.
  4. RBAC grants the App Service identity `Storage Blob Data Contributor` permission on the storage account so token exchange and access succeed.

- Why Managed Identity and RBAC

  - Avoids embedding credentials or connection strings in configuration.
  - Enables least privilege: assign only the Storage Blob Data Contributor role at the required scope.
  - Supports credential rotation automatically and reduces blast radius.

- Private networking boundaries
  - The storage account is accessible via Private Endpoint inside the configured VNet. DNS resolves the storage account hostname to the private endpoint IP only for VNets linked to the Private DNS zone.
  - The App Service communicates with the storage account over the private network (via VNet integration or service injection depending on deployment). Public access is not required.

Security architecture diagram (ASCII)

[Consumer App]
|
| OAuth2 token (Files.Manage)
v
[Entra ID]
|
v
[Client -> HTTPS]
|
v
[API App Service] -- Managed Identity --> [Azure Resource Manager / OAuth2]
| |
| Private network v
| [Storage Account]
| |
v |
VNet + Private DNS --------------------------- Private Endpoint

Networking model

- VNet usage

  - A VNet is deployed to host the Private Endpoint. Subnets are separated for private endpoints and any future internal workloads.

- Private Endpoint behavior

  - The Private Endpoint creates a network interface in the specified subnet with a private IP. Storage traffic from resources that resolve the account name to this private IP will traverse the VNet and private link.
  - The private endpoint provides inbound network connectivity to the storage service from within the VNet. Outbound access from the App Service to storage uses this private path when DNS is configured.

- Private DNS zone purpose

  - The Private DNS zone maps the storage account hostnames to the private endpoint IPs inside linked VNets.
  - Linking the DNS zone to the VNet ensures native hostname resolution; clients outside linked VNets will not resolve to the private IP.

- Allowed and blocked traffic
  - Allowed: traffic from resources within the linked VNet (or peered VNets with DNS forwarding) to the storage account private IP.
  - Blocked: direct public network access to storage account hostnames for clients that do not resolve to the private endpoint.

Consumer onboarding flow

- Registering a new client

  - Create an Entra ID application registration and configure it to request the `Files.Manage` scope exposed by the API app registration.
  - Grant delegated permissions as needed for users or service principals depending on the client type.

- Obtaining tokens and calling the API
  - The client requests an OAuth2 token from Entra ID with the `Files.Manage` scope.
  - The client calls the API with the bearer token in the Authorization header.
  - The API validates the token, extracts the `oid` claim, and enforces per-user container naming and access semantics.

Deployment model

- `main.bicep` composes the modules listed above. It wires outputs between modules, for example providing the storage blob endpoint to the App Service app settings and supplying the App Service principal id to the RBAC module.
- Parameterization strategy

  - Parameters live in `main.parameters.json` or are supplied at deployment time. Typical parameters include `appName`, `storageAccountName`, `location`, and AAD app names. Parameterization allows the same template to target different environments by changing values only.

- Environment separation
  - Use separate resource groups and parameter sets per environment (for example dev, test, prod) to isolate resources and scope RBAC assignments.
  - Keep AAD app registration names and reply URLs environment specific to avoid collisions and accidental cross-environment access.

CI and validation

- The repository contains an infra validation workflow at `.github/workflows/infra-validate.yml`.

  - It runs `az bicep build` to compile Bicep to ARM JSON and validate syntax.
  - It runs `az deployment group validate` against a resource group to ensure template-level ARM validation succeeds with provided parameters.
  - It also runs an ARM what-if to show potential changes.

- Validation failure indicators
  - Bicep build errors indicate syntax or language problems in modules.
  - ARM validate failures typically indicate missing or invalid parameter values or incorrect resource constraints.

Operational notes

- RBAC propagation

  - Role assignments may take a few minutes to propagate. If access fails immediately after deployment, wait and retry before troubleshooting other causes.

- Private Endpoint DNS

  - Ensure the Private DNS zone is properly linked to the VNet used by the App Service or other clients. DNS misconfiguration is a common cause of connectivity issues.
  - If using VNet peering, ensure DNS resolution is configured across peered VNets or use DNS forwarding.

- Common deployment pitfalls
  - Not supplying the correct `BlobStorage__AccountUri` value to the App Service app settings prevents the API from constructing the `BlobServiceClient` correctly.
  - Forgetting to assign RBAC permissions to the App Service identity for the storage account scope results in authorization failures when the API attempts storage operations.
  - Creating AAD app registrations with overlapping names or incorrect reply URLs can prevent clients from obtaining tokens.

Where to look in the repo

- `NetCoreAzureBlobServiceAPI/infra/main.bicep`
- `NetCoreAzureBlobServiceAPI/infra/main.parameters.json`
- `NetCoreAzureBlobServiceAPI/infra/modules/*` (module definitions)

Operational escalation

- For runtime storage access failures check:
  1. App Service identity exists and its principal id matches the RBAC assignment scope.

2.  RBAC role assignment for Storage Blob Data Contributor is present on the storage account scope.
3.  DNS resolution inside the VNet returns the Private Endpoint IP for the storage account hostname.
