# Azure Blob Storage API

A concise, modern .NET Web API that provides per-user file management backed by Azure Blob Storage. This project has been updated to use Azure Bicep, Entra ID for authentication, Managed Identity for storage access, private networking, and CI validation for infra templates.

Overview

- The API exposes endpoints to upload, list, and download files. It is implemented as a minimal ASP.NET Core Web API with the following responsibilities split across components:
  - `Controllers/FileManagerController.cs`: HTTP surface and HTTP-to-domain exception translation.
  - `Services/FileManagementService.cs`: Business rules, validation, per-user container naming (uses the `oid` claim), and orchestration.
  - `Services/BlobStorageRepository.cs`: Direct Azure Blob Storage operations using a `BlobServiceClient` injected from DI.
  - `Program.cs`: DI registration, authentication/authorization setup, health checks, and app wiring.

Architecture (ASCII)

Client -> API (FileManagerController) -> FileManagementService -> BlobStorageRepository -> Azure Blob Storage

Authentication and authorization

- The API uses Microsoft Entra ID (Azure AD) via `Microsoft.Identity.Web` for JWT bearer authentication. A scope named `Files.Manage` is enforced by a policy called `FilesScope` (see `Program.cs`).
- Clients obtain tokens from Entra ID and call the API with a bearer token. The API enforces the presence of the `oid` claim for per-user isolation.

Managed Identity and Blob Storage access

- The runtime uses a Managed Identity (preferred) to access Azure Blob Storage. `Program.cs` constructs a `BlobServiceClient` with a `DefaultAzureCredential` and the configured storage account endpoint.
- Storage operations in `BlobStorageRepository` rely on the `BlobServiceClient` and use `CreateIfNotExistsAsync`, `ExistsAsync`, `UploadAsync`, and `DownloadToAsync` patterns. Do not add code paths that rely on storage account keys or connection strings.

Tenant isolation and container naming

- Tenants/users are isolated by naming containers with the user's `oid` claim lowercased and suffixed with `-container` (see `FileManagementService.GetContainerName`).
- All read/write operations use that container name so one Entra ID principal cannot access another tenant's container via the API unless the `oid` claim matches.

Infrastructure layout and module responsibilities

- All infrastructure lives under `NetCoreAzureBlobServiceAPI/infra` and is authored in Bicep. Key modules:
  - `modules/storageAccount.bicep`: storage account and blob endpoints
  - `modules/vnet.bicep`: virtual network and subnets for private endpoints
  - `modules/privateEndpointBlob.bicep` and `modules/privateDnsZoneBlob.bicep`: private endpoint and private DNS configuration for blob
  - `modules/aadApiApp.bicep` and `modules/aadConsumerApp.bicep`: Entra ID app registrations and API scope
  - `modules/appService.bicep`: Web App with system-assigned identity and app settings
  - `modules/rbacBlobContributor.bicep`: RBAC role assignment granting the Web App identity access to blob resources

Private networking and DNS behavior

- The infra creates a Private Endpoint for the storage account and links the VNet to a Private DNS zone for `blob` endpoints. This ensures that when the Web App (or other VNet-attached resources) resolves the storage account hostname it gets the private IP.
- When developing or testing locally you can use the Azurite emulator. Production deployments assume network-restricted storage behind the private endpoint.

Local development and authentication flow

- Local dev uses `dotnet run` and can target Azurite for blob emulation. See `azurite/` for emulator state files and `Classes/AzuriteManager.cs` for helper code.
- For API calls against deployed environments, clients must authenticate with Entra ID and request the `Files.Manage` scope. Locally you may use developer credentials that can obtain tokens from your tenant (for example, `az account get-access-token` or Visual Studio credential helpers).

Deployment (high level)

1. Author or update parameters in `NetCoreAzureBlobServiceAPI/infra/main.parameters.json`.
2. Build and validate Bicep: `az bicep build --file NetCoreAzureBlobServiceAPI/infra/main.bicep`.
3. Deploy using `az deployment group create` or through your pipeline. The Bicep template creates AAD app registrations, storage account, VNet, private endpoint, App Service, and RBAC bindings.

CI validation and checks

- The repository includes a CI job that validates infra templates: `.github/workflows/infra-validate.yml` runs `az bicep build` and `az deployment group validate` against the Bicep templates.
- CI jobs also run build, test, and static analysis workflows. Infra validation ensures the Bicep compiles and ARM validation is successful before merging.

Security posture and design decisions

- No client secrets or storage connection strings are used at runtime. Authentication is delegated to Entra ID and storage access is granted via Managed Identity and RBAC.
- Network isolation is enforced with Private Endpoint and Private DNS to reduce public exposure of storage endpoints.
- Validation and exception patterns: domain exceptions (`InvalidFileException`, `BlobNotFoundException`, `BlobStorageException`) are thrown by services and translated to HTTP responses in the controller.

What changed from earlier versions

- Removed reliance on clientId/clientSecret and storage connection strings in favor of Entra ID scopes and Managed Identity.
- Infra moved from ad-hoc scripts to modular Bicep under `NetCoreAzureBlobServiceAPI/infra` with CI validation.

Quick commands

- Build and run locally:

```bash
dotnet build NetCoreAzureBlobServiceAPI.sln
dotnet run --project NetCoreAzureBlobServiceAPI
```

- Run tests:

```bash
dotnet test NetCoreAzureBlobServiceAPI.Tests
```

Files to inspect

- `NetCoreAzureBlobServiceAPI/Program.cs`
- `NetCoreAzureBlobServiceAPI/Controllers/FileManagerController.cs`
- `NetCoreAzureBlobServiceAPI/Services/FileManagementService.cs`
- `NetCoreAzureBlobServiceAPI/Services/BlobStorageRepository.cs`
- `NetCoreAzureBlobServiceAPI/infra/main.bicep`

For security issues see `SECURITY.md`.

# Azure Blob Storage API

<p align="center">

  <img alt=".NET" src="https://img.shields.io/badge/.NET-8.0-blue?logo=dotnet" />
  <img alt="Azure" src="https://img.shields.io/badge/Azure-Blob%20Storage-blue?logo=microsoftazure" />
  <img alt="Azurite" src="https://img.shields.io/badge/Local%20Emulator-Azurite-blueviolet?logo=microsoftazure" />
  <img alt="Key Vault" src="https://img.shields.io/badge/Key%20Vault-Optional-green?logo=microsoftazure" />
  <img alt="License: MIT" src="https://img.shields.io/badge/License-MIT-yellow.svg" />
</p>

<p align="center">

<a href="https://github.com/tomblanchard312/AzureBlobStorageAPI/actions/workflows/dotnet-build-test.yml"><img src="https://github.com/tomblanchard312/AzureBlobStorageAPI/actions/workflows/dotnet-build-test.yml/badge.svg" alt="CI" /></a>
<a href="https://github.com/tomblanchard312/AzureBlobStorageAPI/actions/workflows/codeql.yml"><img src="https://github.com/tomblanchard312/AzureBlobStorageAPI/actions/workflows/codeql.yml/badge.svg" alt="CodeQL" /></a>
<a href="https://codecov.io/gh/tomblanchard312/AzureBlobStorageAPI"><img src="https://codecov.io/gh/tomblanchard312/AzureBlobStorageAPI/branch/main/graph/badge.svg" alt="Coverage" /></a>
<a href="https://github.com/tomblanchard312/AzureBlobStorageAPI/issues"><img src="https://img.shields.io/github/issues/tomblanchard312/AzureBlobStorageAPI" alt="Open Issues" /></a>
<a href="https://github.com/tomblanchard312/AzureBlobStorageAPI/blob/master/LICENSE"><img src="https://img.shields.io/github/license/tomblanchard312/AzureBlobStorageAPI" alt="License" /></a>

</p>

---

**A modern .NET Core Web API for uploading, listing, and downloading files to Azure Blob Storage.**

Features:

- Upload, list, and download files via REST endpoints
- Supports Azure Blob Storage and local Azurite emulator
- Optional Azure Key Vault integration for secrets and client validation
- Secure file extension validation and client authentication (100 MB max, curated extensions)
- Health check endpoint at `/health`
- Container-ready (Dockerfile included)

## Table of Contents

- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Local Development](#local-development)
- [API Usage](#api-usage)
- [Security](#security)
- [Contributing](#contributing)
- [CI / Quality Gates](#ci--quality-gates)

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- Azure account with Blob Storage access
- (Optional) Azure Key Vault for secrets

## Getting Started

```bash
git clone https://github.com/tomblanchard312/AzureBlobStorageAPI.git
cd AzureBlobStorageAPI/NetCoreAzureBlobServiceAPI

dotnet build
```
