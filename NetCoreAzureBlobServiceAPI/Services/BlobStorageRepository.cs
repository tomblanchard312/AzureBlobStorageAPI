using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

using NetCoreAzureBlobServiceAPI.Interfaces;
using NetCoreAzureBlobServiceAPI.Models;

using static System.Net.Mime.MediaTypeNames;

using System.Collections.Generic;
using System.Threading.Tasks;


using NetCoreAzureBlobServiceAPI.Exceptions;

namespace NetCoreAzureBlobServiceAPI.Services
{
    public class BlobStorageRepository(BlobServiceClient blobServiceClient, ILogger<BlobStorageRepository> logger) : IBlobStorageRepository
    {
        private readonly BlobServiceClient _blobServiceClient = blobServiceClient;
        private readonly ILogger<BlobStorageRepository> _logger = logger;

        public async Task<Stream> DownloadBlobAsync(string containerName, string blobName)
        {
            // Sanitize blobName for logging to prevent log forging (e.g., newline injection)
            var safeBlobName = blobName?
                .Replace("\r", string.Empty)
                .Replace("\n", string.Empty);

            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobClient = containerClient.GetBlobClient(blobName);

                _logger.LogDebug("Downloading blob {BlobName} from container {ContainerName}", safeBlobName, containerName);

                if (!await blobClient.ExistsAsync())
                {
                    _logger.LogWarning("Blob {BlobName} not found in container {ContainerName}", safeBlobName, containerName);
                    throw new BlobNotFoundException(blobName, containerName);
                }

                var stream = new MemoryStream();
                await blobClient.DownloadToAsync(stream);
                stream.Seek(0, SeekOrigin.Begin);

                _logger.LogDebug("Downloaded {ByteCount} bytes from blob {BlobName}", stream.Length, safeBlobName);
                return stream;
            }
            catch (BlobNotFoundException)
            {
                throw; // Re-throw to preserve stack trace
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error downloading blob {BlobName} from container {ContainerName}", safeBlobName, containerName);
                throw new BlobStorageException($"Failed to download blob '{blobName}' from container '{containerName}'.", ex);
            }
        }

        public async Task<string> UploadBlobAsync(IFormFile file, string containerName, string blobName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                await containerClient.CreateIfNotExistsAsync();

                var blobClient = containerClient.GetBlobClient(blobName);

                _logger.LogDebug("Uploading blob {BlobName} to container {ContainerName}", blobName, containerName);

                using var stream = file.OpenReadStream();
                await blobClient.UploadAsync(stream, true);

                _logger.LogDebug("Blob uploaded to {BlobUri}", blobClient.Uri);
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading blob {BlobName} to container {ContainerName}", blobName, containerName);
                throw new BlobStorageException($"Failed to upload blob '{blobName}' to container '{containerName}'.", ex);
            }
        }

        public async Task<IEnumerable<Models.BlobInfo>> ListBlobsAsync(string containerName)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
                var blobs = new List<Models.BlobInfo>();

                // Check if container exists
                if (!await containerClient.ExistsAsync())
                {
                    _logger.LogInformation("Container {ContainerName} does not exist, returning empty list", containerName);
                    return blobs;
                }

                _logger.LogDebug("Listing blobs in container {ContainerName}", containerName);

                await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
                {
                    var blobClient = containerClient.GetBlobClient(blobItem.Name);
                    blobs.Add(new Models.BlobInfo
                    {
                        Name = blobItem.Name,
                        Url = blobClient.Uri.ToString()
                    });
                }

                _logger.LogDebug("Found {BlobCount} blobs in container {ContainerName}", blobs.Count, containerName);
                return blobs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error listing blobs in container {ContainerName}", containerName);
                throw new BlobStorageException($"Failed to list blobs in container '{containerName}'.", ex);
            }
        }
    }
}