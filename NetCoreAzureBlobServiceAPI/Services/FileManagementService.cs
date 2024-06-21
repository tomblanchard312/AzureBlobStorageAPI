using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

using NetCoreAzureBlobServiceAPI.Interfaces;

namespace NetCoreAzureBlobServiceAPI.Services
{
    public class FileManagementService(IBlobStorageRepository blobStorageRepository, IClientValidationService clientValidationService) : IFileManagementService
    {
        private readonly IBlobStorageRepository _blobStorageRepository = blobStorageRepository;
        private readonly IClientValidationService _clientValidationService = clientValidationService;
        private readonly string[] _permittedExtensions = [".txt", ".csv", ".xls", ".xlsx", ".json", ".xml"];

        public async Task<string> UploadFileAsync(IFormFile file, string clientId, string clientSecret)
        {
            if (!_clientValidationService.ValidateClient(clientId, clientSecret))
            {
                throw new UnauthorizedAccessException("Invalid client credentials.");
            }

            if (file == null || file.Length <= 0)
            {
                throw new ArgumentException("File is empty or null.");
            }

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (string.IsNullOrEmpty(ext) || !_permittedExtensions.Contains(ext))
            {
                throw new ArgumentException("File type is not permitted.");
            }

            var containerName = $"{clientId.ToLowerInvariant()}-container";
            var blobName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";

            var blobUri = await _blobStorageRepository.UploadBlobAsync(file, containerName, blobName);
            return blobUri;
        }

        public async Task<IEnumerable<Models.BlobInfo>> ListBlobsAsync(string clientId, string clientSecret)
        {
            if (!_clientValidationService.ValidateClient(clientId, clientSecret))
            {
                throw new UnauthorizedAccessException("Invalid client credentials.");
            }

            var containerName = $"{clientId.ToLowerInvariant()}-container";
            var blobs = await _blobStorageRepository.ListBlobsAsync(containerName);

            return (IEnumerable<Models.BlobInfo>)blobs;
        }

        public async Task<Stream> DownloadBlobAsync(string clientId, string clientSecret, string blobName)
        {
            if (!_clientValidationService.ValidateClient(clientId, clientSecret))
            {
                throw new UnauthorizedAccessException("Invalid client credentials.");
            }

            var containerName = $"{clientId.ToLowerInvariant()}-container";
            var stream = await _blobStorageRepository.DownloadBlobAsync(containerName, blobName);

            return stream;
        }
    }
}
