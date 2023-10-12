using Azure;
//uncomment to use with akv
//using Azure.Identity;
//using Azure.Security.KeyVault.Secrets;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;

using System.Security.Cryptography;
using NetCoreAzureBlobServiceAPI.Classes;
using BlobInfo = NetCoreAzureBlobServiceAPI.Classes.BlobInfo;

namespace NetCoreAzureBlobServiceAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileManagerController : ControllerBase
    {
        private readonly BlobServiceClient _blobServiceClient;
        private readonly IDataProtector _dataProtector;
        private readonly IConfiguration _configuration;
        /// <summary>
        /// this is an example of restricting file types for upload.
        /// You can change this to whatever you like.
        /// </summary>
        private readonly string[] permittedExtensions = { ".txt", ".csv", ".xls", ".xlsx", ".json", ".xml" };
        /// <summary>
        /// This example is using local storage azurite (node.js) it works for the sample.
        /// Make sure to go into connected services and ensure that you connect to local storage azureite (node.js) and use "StorageConnection" as the connection string.
        /// To use this with azure storage, the keys are in the appsettings.json file. Point them to your Azure Storage.
        /// Same with Keyvault, edit the keyvault uri, and you can uncomment the dependent lines.
        /// </summary>
        /// <param name="blobServiceClient"></param>
        /// <param name="dataProtectionProvider"></param>
        /// <param name="configuration"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public FileManagerController(BlobServiceClient blobServiceClient, IDataProtectionProvider dataProtectionProvider, IConfiguration configuration)
        {
            _blobServiceClient = blobServiceClient ?? throw new ArgumentNullException(nameof(blobServiceClient));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            /**
             * uncomment the following to use with your key vault to make this more secure
             * **/
            //var vaulturi = _configuration.GetValue<string>("vaulturi");
            //var client = new SecretClient(new Uri(vaulturi), new DefaultAzureCredential());
            //var vaultsecret = client.GetSecretAsync(_configuration.GetValue<string>("vaultsecret")).Result;
            //_dataProtector = dataProtectionProvider.CreateProtector(vaultsecret.Value.Name) ?? throw new ArgumentNullException(nameof(dataProtectionProvider));
        }
        /// <summary>
        /// Validates a client by comparing the provided client secret with the one stored in Azure Key Vault.
        /// </summary>
        /// <param name="clientId">The client ID to validate.</param>
        /// <param name="clientSecret">The client secret to validate.</param>
        /// <returns>True if the client is valid; otherwise, false.</returns>
        private static bool ValidateClient(string clientId, string clientSecret)
        {
            // Validate the clientID parameter for non-null
            if (clientId == null)
            {
                throw new ArgumentNullException(nameof(clientId), "clientId cannot be null.");
            }
            try
            {
                /**
                 * uncomment the following to use with your key vault to make this actually validate clientId and clientSecret
                 * This checks the name of the secret and compares if the value of client secret equals the value of what is returned.
                 * It works, it is effective and it is easy.
                 * see: the following for the recommended way using AzureEntra B2C https://learn.microsoft.com/en-us/azure/industry/training-services/microsoft-community-training/frequently-asked-questions/generate-new-clientsecret-link-to-key-vault
                 * **/
                //// Retrieve the client secret from Azure Key Vault
                //var vaultUri = _configuration.GetValue<string>("vaulturi");
                //var secretClient = new SecretClient(new Uri(vaultUri), new DefaultAzureCredential());
                //var storedClientSecret = secretClient.GetSecret(clientId);

                //// Compare the stored client secret with the provided one
                //return storedClientSecret.Value.Value == clientSecret;

                //do not use this, it will allow anyone to pass anything
                return true;
            }
            catch (RequestFailedException ex)
            {
                // Handle Azure Key Vault request-related exceptions
                // Log the error and return false or throw an exception
                // For security reasons, it's essential to handle exceptions gracefully
                return false;
            }
            catch (Exception)
            {
                // Handle other exceptions, such as Key Vault access issues or unexpected errors
                // Log the error and return false or throw an exception
                // For security reasons, it's essential to handle exceptions gracefully
                return false;
            }
        }
        /// <summary>
        /// Uploads a file to Azure Blob Storage.
        /// </summary>
        /// <param name="file">The file to upload.</param>
        /// <param name="ClientID">ex: MyClientID</param>
        /// <param name="ClientSecret">ex: MyClientSecet</param>
        /// <returns>A response with the URL of the uploaded blob.</returns>
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file, string clientId, string clientSecret)
        {
            // Validate the clientID parameter for non-null
            if (clientId == null)
            {
                throw new ArgumentNullException(nameof(clientId), "ClientID cannot be null.");
            }
            // Validate the client using the ValidateClient method
            if (!ValidateClient(clientId, clientSecret))
            {
                return BadRequest("Client ID/Secret Not Found or Invalid.");
            }
            try
            {
                // Validate the file
                if (file == null || file.Length <= 0)
                    return BadRequest("File is empty or null.");

                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (string.IsNullOrEmpty(ext) || !permittedExtensions.Contains(ext))
                    return BadRequest("File type invalid.");

                // Get or create the Blob Container Client based on ClientID
                BlobContainerClient _containerClient = _blobServiceClient.GetBlobContainerClient(clientId.ToLowerInvariant() + "blobcontainer");
                if (!await _containerClient.ExistsAsync().ConfigureAwait(false))
                {
                    await _containerClient.CreateAsync().ConfigureAwait(false);
                }
                // Generate a unique blob name for the uploaded file
                //if not you will get a file exists error if uploading the same name twice.
                string blobName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);

                // Read and encrypt the file content
                using (Stream stream = file.OpenReadStream())
                using (MemoryStream memoryStream = new())
                {
                    await stream.CopyToAsync(memoryStream).ConfigureAwait(false);
                    byte[] fileBytes = memoryStream.ToArray();

                    /**
                     * using this sends the stream as plain text
                     * **/
                    using Stream strfileBytes = new MemoryStream(fileBytes);
                    var response = await _containerClient.UploadBlobAsync(blobName, strfileBytes).ConfigureAwait(false);
                    /**
                     * 
                     * uncomment the following to ecnrypt the filestream using DPAPI and Kevault.
                     * 
                     Encrypt the file content using DPAPI
                     byte[] encryptedFileBytes = _dataProtector.Protect(fileBytes);
                     Upload the encrypted file to Azure Blob Storage
                    using Stream encryptedStream = new MemoryStream(encryptedFileBytes);
                    var response = await _containerClient.UploadBlobAsync(blobName, encryptedStream).ConfigureAwait(false);
                    **/
                }

                // Get the URL of the uploaded blob
                var blobUri = _containerClient.GetBlobClient(blobName).Uri.ToString();

                return Ok(new { BlobUri = blobUri });
            }
            catch (RequestFailedException ex)
            {
                // Handle Azure Storage-related errors
                return StatusCode(403, $"Access to Azure Blob Storage denied: {ex.Message}");
            }
            catch (IOException ex)
            {
                // Handle file-related errors
                return StatusCode(500, $"File operation error: {ex.Message}");
            }
            catch (CryptographicException ex)
            {
                // Handle data protection errors
                return StatusCode(500, $"Data protection error: {ex.Message}");
            }
            catch (Exception ex)
            {
                // Handle other unexpected errors
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
        [HttpGet("list")]
        public IActionResult ListBlobs(string ClientID, string ClientSecret)
        {
            // Validate the clientID parameter for non-null
            if (ClientID == null)
            {
                throw new ArgumentNullException(nameof(ClientID), "ClientID cannot be null.");
            }

            try
            {
                // Validate the client using the ValidateClient method
                if (!ValidateClient(ClientID, ClientSecret))
                {
                    return BadRequest("Client ID/Secret Not Found or Invalid.");
                }

                BlobContainerClient _containerClient = _blobServiceClient.GetBlobContainerClient(ClientID.ToLowerInvariant() + "blobcontainer");

                //List all blobs in the container with their metadata
                List<BlobInfo> blobInfos = _containerClient
                    .GetBlobs()
                    .OrderByDescending(blobItem => blobItem.Properties.CreatedOn) // Sort by creation date descending
                    .Select(blobItem => new BlobInfo
                    {
                        Name = blobItem.Name,
                        CreatedOn = (DateTimeOffset)blobItem.Properties.CreatedOn
                    })
                    .ToList();

                return Ok(blobInfos);
            }
            catch (RequestFailedException ex) // Azure Storage-related errors
            {
                // Handle Azure Storage-related errors
                // Example: Unauthorized access
                return StatusCode(403, $"Access to Azure Blob Storage denied: {ex.Message}");
            }
            // Handle other exceptions as before...
        }
        /// <summary>
        /// Downloads a specific blob from the Azure Blob Storage container.
        /// </summary>
        /// <param name="ClientID">MyClientID</param>
        /// <param name="ClientSecret">MyClientSecret</param>
        /// <param name="blobName">The name of the blob to download.</param>
        /// <returns>The blob's content as a file download response.</returns>
        [HttpGet("download")]
        public IActionResult DownloadBlob(string ClientID, string ClientSecret, string blobName)
        {
            // Validate the clientID parameter for non-null
            if (ClientID == null)
            {
                throw new ArgumentNullException(nameof(ClientID), "ClientID cannot be null.");
            }
            try
            {
                // Validate the client using the ValidateClient method
                if (!ValidateClient(ClientID, ClientSecret))
                {
                    return BadRequest("Client ID/Secret Not Found or Invalid.");
                }
                //this gets or creates a container from client id named clientidblobcontainer.
                //you could pass it in as a variable if you wish.
                BlobContainerClient _containerClient = _blobServiceClient.GetBlobContainerClient(ClientID.ToLowerInvariant() + "blobcontainer");

                if (string.IsNullOrEmpty(blobName))
                {
                    return BadRequest("Blob name is required.");
                }

                // Get the blob reference
                BlobClient blobClient = _containerClient.GetBlobClient(blobName);

                // Check if the blob exists
                if (!blobClient.Exists())
                {
                    return NotFound($"Blob '{blobName}' not found.");
                }

                // Download the blob's encrypted content
                BlobDownloadInfo blobDownloadInfo = blobClient.Download();

                // Decrypt the blob content using DPAPI
                using (var memoryStream = new MemoryStream())
                {
                    /**
                     * 
                     * if you decide to encrypt the files, uncomment to decrypt encrypted files content stream
                     *

                    blobDownloadInfo.Content.CopyTo(memoryStream);
                    byte[] encryptedFileBytes = memoryStream.ToArray();
                    byte[] decryptedFileBytes = _dataProtector.Unprotect(encryptedFileBytes);

                    // Return the decrypted blob's content as a file download
                    Response.Headers.Add("Content-Disposition", $"attachment; filename={blobName}");
                    return File(decryptedFileBytes, blobDownloadInfo.ContentType);

                     *
                     * comment out the following if you decide to use encryption
                     * **/
                    blobDownloadInfo.Content.CopyTo(memoryStream);
                    byte[] downFileBytes = memoryStream.ToArray();
                    Response.Headers.Add("Content-Disposition", $"attachment; filename={blobName}");
                    return File(downFileBytes, blobDownloadInfo.ContentType);
                }
            }
            catch (RequestFailedException ex) // Azure Storage-related errors
            {
                // Handle Azure Storage-related errors
                // Example: Unauthorized access
                return StatusCode(403, $"Access to Azure Blob Storage denied: {ex.Message}");
            }
            catch (IOException ex) // File-related errors
            {
                // Handle file-related errors
                // Example: File read or write error
                return StatusCode(500, $"File operation error: {ex.Message}");
            }
            catch (CryptographicException ex) // Data Protection errors
            {
                // Handle data protection errors
                // Example: Encryption or decryption failure
                return StatusCode(500, $"Data protection error: {ex.Message}");
            }
            catch (HttpRequestException ex) // HTTP errors
            {
                // Handle HTTP request errors
                // Example: Network issues
                return StatusCode(503, $"HTTP request error: {ex.Message}");
            }
            catch (ArgumentNullException ex) // Validation errors
            {
                // Handle validation errors
                // Example: Null parameter
                return BadRequest($"Validation error: {ex.Message}");
            }
            catch (OperationCanceledException ex) // Cancellation errors
            {
                // Handle cancellation requests
                // Example: Request was canceled
                return StatusCode(499, $"Request canceled: {ex.Message}");
            }
            catch (Exception ex) // Generic catch-all for other unexpected errors
            {
                // Handle generic exceptions
                // Example: Unexpected application error
                return StatusCode(500, $"Internal Server Error: {ex.Message}");
            }
        }
    }
}


