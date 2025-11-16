
# Azure Blob Storage API

<p align="center">

  <img alt=".NET" src="https://img.shields.io/badge/.NET-8.0-blue?logo=dotnet" />
  <img alt="Azure" src="https://img.shields.io/badge/Azure-Blob%20Storage-blue?logo=microsoftazure" />
  <img alt="Azurite" src="https://img.shields.io/badge/Local%20Emulator-Azurite-blueviolet?logo=microsoftazure" />
  <img alt="Key Vault" src="https://img.shields.io/badge/Key%20Vault-Optional-green?logo=microsoftazure" />
  <img alt="License: MIT" src="https://img.shields.io/badge/License-MIT-yellow.svg" />
</p>

<p align="center">

  <a href="https://github.com/tomblanchard312/AzureBlobStorageAPI/actions/workflows/dotnet-build-test.yml"><img src="https://github.com/tomblanchard312/AzureBlobStorageAPI/actions/workflows/dotnet-build-test.yml/badge.svg" alt="Build Status" /></a>
  <a href="https://github.com/tomblanchard312/AzureBlobStorageAPI/issues"><img src="https://img.shields.io/github/issues/tomblanchard312/AzureBlobStorageAPI" alt="Open Issues" /></a>
  <a href="https://github.com/tomblanchard312/AzureBlobStorageAPI/blob/master/LICENSE"><img src="https://img.shields.io/github/license/tomblanchard312/AzureBlobStorageAPI" alt="License" /></a>
</p>

---

**A modern .NET Core Web API for uploading, listing, and downloading files to Azure Blob Storage.**

Features:

- Upload, list, and download files via REST endpoints
- Supports Azure Blob Storage and local Azurite emulator
- Optional Azure Key Vault integration for secrets and client validation
- Secure file extension validation and client authentication
- Container-ready (Dockerfile included)



## Table of Contents

- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [API Usage](#api-usage)
- [Security](#security)
- [Testing](#testing)
- [Contributing](#contributing)


## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- Azure account with Blob Storage access
- (Optional) Azure Key Vault for secrets


## Getting Started

```bash
git clone https://github.com/tomblanchard312/AzureBlobStorageAPI.git
cd AzureBlobStorageAPI/NetCoreAzureBlobServiceAPI
dotnet build
dotnet run
```


## Configuration

- **Blob Storage**: Set your Azure connection string in `appsettings.json`. For local dev, use Azurite (see `azurite/` folder).
- **Key Vault**: Uncomment Key Vault code and set URI in `appsettings.json` for secure secrets.
- **Allowed Extensions**: Update `permittedExtensions` in `Controllers/FileManagerController.cs` to restrict file types.


## API Usage

### Upload File

`POST /api/FileManager/upload`

- `file`: File to upload
- `clientId`, `clientSecret`: Credentials
→ Returns: Blob URL

### List Blobs

`GET /api/FileManager/list`

- `clientId`, `clientSecret`: Credentials

→ Returns: List of blobs (name, date)

### Download Blob
`GET /api/FileManager/download`

- `clientId`, `clientSecret`: Credentials
- `blobName`: Blob to download

→ Returns: File download


## Security

- **Client Validation**: Use `clientId`/`clientSecret` (optionally via Key Vault)
- **Data Protection**: DPAPI and Key Vault supported for encryption (optional)

## Testing

Unit tests are in `NetCoreAzureBlobServiceAPI/tests/` using xUnit and Moq.

```bash
dotnet test NetCoreAzureBlobServiceAPI/tests/
```


## Contributing

Contributions are welcome! Please open issues or submit pull requests for improvements.
