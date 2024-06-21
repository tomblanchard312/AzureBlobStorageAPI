//uncomment to use with akv
//using Azure.Identity;
//using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Mvc;

using NetCoreAzureBlobServiceAPI.Interfaces;

namespace NetCoreAzureBlobServiceAPI.Controllers
{
    public class FileManagerController(IFileManagementService fileManagementService) : ControllerBase
    {
        private readonly IFileManagementService _fileManagementService = fileManagementService;

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file, string clientId, string clientSecret)
        {
            try
            {
                var blobUri = await _fileManagementService.UploadFileAsync(file, clientId, clientSecret);
                return Ok(new { BlobUri = blobUri });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while uploading the file.");
            }
        }

        [HttpGet("list")]
        public async Task<IActionResult> ListBlobs(string clientId, string clientSecret)
        {
            try
            {
                var blobs = await _fileManagementService.ListBlobsAsync(clientId, clientSecret);
                return Ok(blobs);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while listing the blobs.");
            }
        }

        [HttpGet("download")]
        public async Task<IActionResult> DownloadBlob(string clientId, string clientSecret, string blobName)
        {
            try
            {
                var stream = await _fileManagementService.DownloadBlobAsync(clientId, clientSecret, blobName);
                return File(stream, "application/octet-stream", blobName);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (Exception)
            {
                return StatusCode(500, "An error occurred while downloading the blob.");
            }
        }
    }
}


