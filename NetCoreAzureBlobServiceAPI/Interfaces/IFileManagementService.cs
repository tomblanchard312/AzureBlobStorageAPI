using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
namespace NetCoreAzureBlobServiceAPI.Interfaces
{
    public interface IFileManagementService
    {
        Task<string> UploadFileAsync(IFormFile file, string clientId, string clientSecret);
        Task<IEnumerable<Models.BlobInfo>> ListBlobsAsync(string clientId, string clientSecret);
        Task<Stream> DownloadBlobAsync(string clientId, string clientSecret, string blobName);
    }
}
