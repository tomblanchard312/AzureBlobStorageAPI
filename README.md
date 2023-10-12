# NetCoreAzureBlobServiceAPI

This repository contains a sample API for uploading, listing, and downloading files to and from Azure Blob Storage. The API is built using .NET Core and leverages Azure SDK libraries for interacting with Azure Blob Storage. Additionally, it provides options for client validation, local storage (Azurite), Azure Storage, and Azure Key Vault for secure access.

## Table of Contents

- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
  - [Configuration](#configuration)
- [Usage](#usage)
  - [Uploading a File](#uploading-a-file)
  - [Listing Blobs](#listing-blobs)
  - [Downloading a Blob](#downloading-a-blob)
- [Security Considerations](#security-considerations)
- [Contributing](#contributing)
- [License](#license)

## Prerequisites

Before using this API, make sure you have the following prerequisites:

- .NET Core SDK
- An Azure account with access to Azure Blob Storage
- (Optional) Azure Key Vault for added security

## Getting Started

1. Clone this repository to your local machine:

   ```bash
   git [clone https://github.com/yourusername/netcore-azure-blob-service-api.git](https://github.com/tomblanchard312/NetCoreAzBlobServiceAPI.git)
   cd NetCoreAzBlobServiceAPI
   ```

2. Build the project using the .NET Core CLI:

   ```bash
   dotnet build
   ```

3. Run the application:

   ```bash
   dotnet run
   ```

### Configuration

The API can be configured in various ways, depending on your requirements. Here are some key configuration options:

- **Azure Blob Storage**: To use Azure Blob Storage, provide the necessary connection string in the `appsettings.json` file. Alternatively, you can use local storage (Azurite) for testing.

- **Azure Key Vault (Optional)**: If you choose to use Azure Key Vault for added security, you can uncomment the related code and configure the Key Vault URI in the `appsettings.json` file.

- **Allowed File Extensions**: You can define the allowed file extensions by modifying the `permittedExtensions` array in the `FileManagerController` class.

## Usage

The API provides the following endpoints:

### Uploading a File

Upload a file to Azure Blob Storage.

- **URL**: `/api/FileManager/upload`
- **Method**: `POST`
- **Parameters**:
  - `file` (file): The file to upload.
  - `clientId` (string): Your client ID.
  - `clientSecret` (string): Your client secret.
- **Response**: The URL of the uploaded blob.

### Listing Blobs

List all blobs in your Azure Blob Storage container.

- **URL**: `/api/FileManager/list`
- **Method**: `GET`
- **Parameters**:
  - `clientId` (string): Your client ID.
  - `clientSecret` (string): Your client secret.
- **Response**: A list of blob information, including names and creation dates.

### Downloading a Blob

Download a specific blob from the Azure Blob Storage container.

- **URL**: `/api/FileManager/download`
- **Method**: `GET`
- **Parameters**:
  - `clientId` (string): Your client ID.
  - `clientSecret` (string): Your client secret.
  - `blobName` (string): The name of the blob to download.
- **Response**: The blob's content as a file download response.

## Security Considerations

This API provides options for securing your data:

- **Client Validation**: You can validate clients using the provided `clientId` and `clientSecret`. Uncomment the related code to utilize Azure Key Vault for secure client validation.

- **Data Protection**: If needed, you can encrypt and decrypt the file content using DPAPI and Key Vault. Be cautious when implementing encryption, as it adds an extra layer of complexity.

## Contributing

Contributions to this project are welcome. If you have improvements or feature additions, please create a pull request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
