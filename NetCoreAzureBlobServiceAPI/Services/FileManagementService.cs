using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

using NetCoreAzureBlobServiceAPI.Interfaces;
using NetCoreAzureBlobServiceAPI.Exceptions;
using System.Security.Claims;

namespace NetCoreAzureBlobServiceAPI.Services
{
    public class FileManagementService(IBlobStorageRepository blobStorageRepository, ILogger<FileManagementService> logger) : IFileManagementService
    {
        private readonly IBlobStorageRepository _blobStorageRepository = blobStorageRepository;
        private readonly ILogger<FileManagementService> _logger = logger;
        private readonly string[] _permittedExtensions = [".txt", ".csv", ".xls", ".xlsx", ".json", ".xml"];
        private const long MaxFileSizeBytes = 100 * 1024 * 1024; // 100 MB

        private string GetContainerName(ClaimsPrincipal user)
        {
            var oid = user.FindFirst("oid")?.Value ?? "anonymous";
            return $"{oid.ToLowerInvariant()}-container";
        }

        public async Task<string> UploadFileAsync(IFormFile file, ClaimsPrincipal user)
        {
            var oid = user.FindFirst("oid")?.Value ?? "unknown";
            _logger.LogInformation("Upload request received for user {UserId}", oid);

            if (file == null || file.Length <= 0)
            {
                _logger.LogWarning("Empty or null file upload attempted");
                throw new InvalidFileException("File is empty or null.");
            }

            if (file.Length > MaxFileSizeBytes)
            {
                _logger.LogWarning("File size {FileSize} exceeds maximum {MaxSize}", file.Length, MaxFileSizeBytes);
                throw new InvalidFileException($"File size {file.Length} bytes exceeds maximum allowed size of {MaxFileSizeBytes} bytes.", file.FileName);
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !_permittedExtensions.Contains(ext))
            {
                _logger.LogWarning("Invalid file extension {Extension}", ext);
                throw new InvalidFileException($"File type '{ext}' is not permitted. Allowed types: {string.Join(", ", _permittedExtensions)}", file.FileName);
            }

            var containerName = GetContainerName(user);
            var blobName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            _logger.LogInformation("Uploading file {FileName} ({FileSize} bytes) to container {ContainerName} as {BlobName}",
                file.FileName, file.Length, containerName, blobName);

            try
            {
                var blobUri = await _blobStorageRepository.UploadBlobAsync(file, containerName, blobName);
                _logger.LogInformation("File uploaded successfully to {BlobUri}", blobUri);
                return blobUri;
            }
            catch (Exception ex) when (ex is not BlobStorageException)
            {
                _logger.LogError(ex, "Unexpected error uploading file {FileName}", file.FileName);
                throw new BlobStorageException("An error occurred while uploading the file to blob storage.", ex);
            }
        }

        public async Task<IEnumerable<Models.BlobInfo>> ListBlobsAsync(ClaimsPrincipal user)
        {
            var oid = user.FindFirst("oid")?.Value ?? "unknown";
            _logger.LogInformation("List blobs request received for user {UserId}", oid);

            var containerName = GetContainerName(user);

            try
            {
                var blobs = await _blobStorageRepository.ListBlobsAsync(containerName);
                var blobList = blobs.ToList();
                _logger.LogInformation("Listed {BlobCount} blobs from container {ContainerName}", blobList.Count, containerName);
                return blobList;
            }
            catch (Exception ex) when (ex is not BlobStorageException)
            {
                _logger.LogError(ex, "Unexpected error listing blobs");
                throw new BlobStorageException("An error occurred while listing blobs from storage.", ex);
            }
        }

        public async Task<Stream> DownloadBlobAsync(string blobName, ClaimsPrincipal user)
        {
            var oid = user.FindFirst("oid")?.Value ?? "unknown";
            // Sanitize blobName for logging to prevent log forging (e.g., newline injection)
            var safeBlobName = blobName?
                .Replace("\r", string.Empty)
                .Replace("\n", string.Empty);

            _logger.LogInformation("Download request received for blob {BlobName} by user {UserId}", safeBlobName, oid);

            if (string.IsNullOrWhiteSpace(blobName))
            {
                _logger.LogWarning("Empty blob name provided");
                throw new ArgumentException("Blob name cannot be empty.", nameof(blobName));
            }

            var containerName = GetContainerName(user);

            try
            {
                var stream = await _blobStorageRepository.DownloadBlobAsync(containerName, blobName);
                _logger.LogInformation("Blob {BlobName} downloaded successfully from container {ContainerName}", safeBlobName, containerName);
                return stream;
            }
            catch (BlobNotFoundException)
            {
                throw; // Re-throw to preserve the specific exception
            }
            catch (Exception ex) when (ex is not BlobStorageException)
            {
                _logger.LogError(ex, "Unexpected error downloading blob {BlobName}", safeBlobName);
                throw new BlobStorageException($"An error occurred while downloading blob '{blobName}'.", ex);
            }
        }
    }
}
