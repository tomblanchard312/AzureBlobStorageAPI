using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

using NetCoreAzureBlobServiceAPI.Interfaces;
using NetCoreAzureBlobServiceAPI.Exceptions;

namespace NetCoreAzureBlobServiceAPI.Services
{
    public class FileManagementService(IBlobStorageRepository blobStorageRepository, IClientValidationService clientValidationService, ILogger<FileManagementService> logger) : IFileManagementService
    {
        private readonly IBlobStorageRepository _blobStorageRepository = blobStorageRepository;
        private readonly IClientValidationService _clientValidationService = clientValidationService;
        private readonly ILogger<FileManagementService> _logger = logger;
        private readonly string[] _permittedExtensions = [".txt", ".csv", ".xls", ".xlsx", ".json", ".xml"];
        private const long MaxFileSizeBytes = 100 * 1024 * 1024; // 100 MB

        public async Task<string> UploadFileAsync(IFormFile file, string clientId, string clientSecret)
        {
            _logger.LogInformation("Upload request received for client: {ClientId}", clientId);

            if (!_clientValidationService.ValidateClient(clientId, clientSecret))
            {
                _logger.LogWarning("Invalid credentials provided for client: {ClientId}", clientId);
                throw new UnauthorizedAccessException("Invalid client credentials.");
            }

            if (file == null || file.Length <= 0)
            {
                _logger.LogWarning("Empty or null file upload attempted by client: {ClientId}", clientId);
                throw new InvalidFileException("File is empty or null.");
            }

            if (file.Length > MaxFileSizeBytes)
            {
                _logger.LogWarning("File size {FileSize} exceeds maximum {MaxSize} for client: {ClientId}, File: {FileName}",
                    file.Length, MaxFileSizeBytes, clientId, file.FileName);
                throw new InvalidFileException($"File size {file.Length} bytes exceeds maximum allowed size of {MaxFileSizeBytes} bytes.", file.FileName);
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !_permittedExtensions.Contains(ext))
            {
                _logger.LogWarning("Invalid file extension {Extension} attempted by client: {ClientId}, File: {FileName}", ext, clientId, file.FileName);
                throw new InvalidFileException($"File type '{ext}' is not permitted. Allowed types: {string.Join(", ", _permittedExtensions)}", file.FileName);
            }

            var containerName = $"{clientId.ToLowerInvariant()}-container";
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
                _logger.LogError(ex, "Unexpected error uploading file {FileName} for client: {ClientId}", file.FileName, clientId);
                throw new BlobStorageException("An error occurred while uploading the file to blob storage.", ex);
            }
        }

        public async Task<IEnumerable<Models.BlobInfo>> ListBlobsAsync(string clientId, string clientSecret)
        {
            _logger.LogInformation("List blobs request received for client: {ClientId}", clientId);

            if (!_clientValidationService.ValidateClient(clientId, clientSecret))
            {
                _logger.LogWarning("Invalid credentials provided for list operation, client: {ClientId}", clientId);
                throw new UnauthorizedAccessException("Invalid client credentials.");
            }

            var containerName = $"{clientId.ToLowerInvariant()}-container";

            try
            {
                var blobs = await _blobStorageRepository.ListBlobsAsync(containerName);
                var blobList = blobs.ToList();
                _logger.LogInformation("Listed {BlobCount} blobs from container {ContainerName}", blobList.Count, containerName);
                return blobList;
            }
            catch (Exception ex) when (ex is not BlobStorageException)
            {
                _logger.LogError(ex, "Unexpected error listing blobs for client: {ClientId}", clientId);
                throw new BlobStorageException("An error occurred while listing blobs from storage.", ex);
            }
        }

        public async Task<Stream> DownloadBlobAsync(string clientId, string clientSecret, string blobName)
        {
            _logger.LogInformation("Download request received for blob {BlobName} by client: {ClientId}", blobName, clientId);

            if (!_clientValidationService.ValidateClient(clientId, clientSecret))
            {
                _logger.LogWarning("Invalid credentials provided for download operation, client: {ClientId}", clientId);
                throw new UnauthorizedAccessException("Invalid client credentials.");
            }

            if (string.IsNullOrWhiteSpace(blobName))
            {
                _logger.LogWarning("Empty blob name provided by client: {ClientId}", clientId);
                throw new ArgumentException("Blob name cannot be empty.", nameof(blobName));
            }

            var containerName = $"{clientId.ToLowerInvariant()}-container";

            try
            {
                var stream = await _blobStorageRepository.DownloadBlobAsync(containerName, blobName);
                _logger.LogInformation("Blob {BlobName} downloaded successfully from container {ContainerName}", blobName, containerName);
                return stream;
            }
            catch (BlobNotFoundException)
            {
                throw; // Re-throw to preserve the specific exception
            }
            catch (Exception ex) when (ex is not BlobStorageException)
            {
                _logger.LogError(ex, "Unexpected error downloading blob {BlobName} for client: {ClientId}", blobName, clientId);
                throw new BlobStorageException($"An error occurred while downloading blob '{blobName}'.", ex);
            }
        }
    }
}
