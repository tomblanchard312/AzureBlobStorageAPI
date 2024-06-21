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


namespace NetCoreAzureBlobServiceAPI.Services
{
    public class BlobStorageRepository(BlobServiceClient blobServiceClient) : IBlobStorageRepository
    {
        private readonly BlobServiceClient _blobServiceClient = blobServiceClient;

        public async Task<Stream> DownloadBlobAsync(string containerName, string blobName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(blobName);

            var stream = new MemoryStream();
            await blobClient.DownloadToAsync(stream);
            stream.Seek(0, SeekOrigin.Begin);

            return stream;
        }

        public async Task<string> UploadBlobAsync(IFormFile file, string containerName, string blobName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();

            var blobClient = containerClient.GetBlobClient(blobName);

            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, true);

            return blobClient.Uri.ToString();
        }

        public async Task<IEnumerable<Models.BlobInfo>> ListBlobsAsync(string containerName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobs = new List<Models.BlobInfo>();

            await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
            {
                var blobClient = containerClient.GetBlobClient(blobItem.Name);
                blobs.Add(new Models.BlobInfo
                {
                    Name = blobItem.Name,
                    Url = blobClient.Uri.ToString()
                });
            }

            return blobs;
        }
    }
}