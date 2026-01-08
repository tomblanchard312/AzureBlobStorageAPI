# Azure Blob Storage API

<p align="center">
  <img alt=".NET" src="https://img.shields.io/badge/.NET-8.0-blue?logo=dotnet" />
  <img alt="Azure" src="https://img.shields.io/badge/Azure-Blob%20Storage-blue?logo=microsoftazure" />
  <img alt="License: MIT" src="https://img.shields.io/badge/License-MIT-yellow.svg" />
</p>

<p align="center">
<a href="https://github.com/tomblanchard312/AzureBlobStorageAPI/actions/workflows/dotnet-build-test.yml">
  <img src="https://github.com/tomblanchard312/AzureBlobStorageAPI/actions/workflows/dotnet-build-test.yml/badge.svg" alt="CI" />
</a>
<a href="https://github.com/tomblanchard312/AzureBlobStorageAPI/actions/workflows/codeql.yml">
  <img src="https://github.com/tomblanchard312/AzureBlobStorageAPI/actions/workflows/codeql.yml/badge.svg" alt="CodeQL" />
</a>
<a href="https://codecov.io/gh/tomblanchard312/AzureBlobStorageAPI">
  <img src="https://codecov.io/gh/tomblanchard312/AzureBlobStorageAPI/branch/main/graph/badge.svg" alt="Coverage" />
</a>
</p>

A modern .NET 8 Web API for uploading, listing, and downloading files backed by Azure Blob Storage.  
The solution uses Azure Bicep for infrastructure, Microsoft Entra ID for authentication, Managed Identity for storage access, private networking, and CI validation for infrastructure templates.

---

## Table of Contents

- [Overview](#overview)
- [Architecture](#architecture)
- [Security Architecture](#security-architecture)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Local Development](#local-development)
- [API Usage](#api-usage)
- [Deployment](#deployment)
- [CI and Quality Gates](#ci-and-quality-gates)
- [Contributing](#contributing)

---

## Overview

The API exposes endpoints to upload, list, and download files. Responsibilities are split across components:

- Controllers/FileManagerController.cs: HTTP endpoints and exception translation
- Services/FileManagementService.cs: Business rules and per-user container naming using Entra ID claims
- Services/BlobStorageRepository.cs: Azure Blob Storage access via SDK
- Program.cs: Dependency injection, authentication, authorization, and health checks

---

## Architecture

High-level request flow:

```
Client → API → Azure Blob Storage
```

Key characteristics:

- OAuth2 and JWT authentication via Microsoft Entra ID
- Managed Identity for storage access
- Per-user isolation using the Entra ID oid claim
- Private Endpoint and Private DNS for Blob Storage
- Infrastructure defined using modular Azure Bicep

---

## Security Architecture

### Authentication and Authorization Flow

```mermaid
sequenceDiagram
    participant Client as Consumer App
    participant Entra as Microsoft Entra ID
    participant API as ASP.NET Core API
    participant Blob as Azure Blob Storage

    Client->>Entra: Request access token (scope: Files.Manage)
    Entra-->>Client: JWT access token
    Client->>API: HTTP request with Bearer token
    API->>API: Validate token and scope
    API->>Blob: Access via Managed Identity (RBAC)
    Blob-->>API: Blob operation result
    API-->>Client: HTTP response
```

---

### Failure Boundaries and Trust Zones

```mermaid
sequenceDiagram
    participant Internet as Untrusted Network
    participant Client as Consumer App
    participant Entra as Entra ID Trust Boundary
    participant API as API Trust Zone
    participant VNet as Azure VNet
    participant Blob as Blob Storage

    Internet->>Client: User request
    Client->>Entra: Authenticate
    Entra-->>Client: Signed JWT token
    Client->>API: Bearer token over HTTPS
    API->>API: Authorization enforcement
    API->>VNet: Private traffic
    VNet->>Blob: Private Endpoint
    Blob-->>VNet: Response
    VNet-->>API: Response
    API-->>Client: Sanitized result
```

---

## Prerequisites

- .NET 8 SDK
- Azure subscription with permissions for App Service, Storage, Networking, and Entra ID app registrations

---

## Getting Started

```bash
git clone https://github.com/tomblanchard312/AzureBlobStorageAPI.git
cd AzureBlobStorageAPI/NetCoreAzureBlobServiceAPI
dotnet build
```

---

## Configuration

- Blob Storage endpoint injected via App Service settings
- No storage keys or connection strings
- Allowed extensions configured in FileManagementService
- Max file size 100 MB

---

## Local Development

```bash
dotnet restore
dotnet build
dotnet test
```

Health check: GET /health

---

## API Usage

POST /api/FileManager/upload  
GET /api/FileManager/list  
GET /api/FileManager/download?blobName=name

---

## Deployment

Infrastructure is defined under NetCoreAzureBlobServiceAPI/infra using Bicep.

---

## CI and Quality Gates

- Build and test workflows
- CodeQL static analysis
- Infrastructure validation with az bicep build and ARM validate

---

## Contributing

Run dotnet format and dotnet test before submitting changes.

For security issues see SECURITY.md.
