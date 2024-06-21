using Azure.Storage.Blobs.Models;

using NetCoreAzureBlobServiceAPI.Models;


namespace NetCoreAzureBlobServiceAPI.Interfaces
{
    public interface IBlobStorageRepository
    {
        Task<string> UploadBlobAsync(IFormFile file, string containerName, string blobName);
        Task<IEnumerable<Models.BlobInfo>> ListBlobsAsync(string containerName);
        Task<Stream> DownloadBlobAsync(string containerName, string blobName);
    }
}
