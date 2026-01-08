using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System.Security.Claims;
namespace NetCoreAzureBlobServiceAPI.Interfaces
{
    public interface IFileManagementService
    {
        Task<string> UploadFileAsync(IFormFile file, ClaimsPrincipal user);
        Task<IEnumerable<Models.BlobInfo>> ListBlobsAsync(ClaimsPrincipal user);
        Task<Stream> DownloadBlobAsync(string blobName, ClaimsPrincipal user);
    }
}
