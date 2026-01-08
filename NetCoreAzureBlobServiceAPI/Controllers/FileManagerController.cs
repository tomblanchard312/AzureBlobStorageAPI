//uncomment to use with akv
//using Azure.Identity;
//using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Mvc;
using NetCoreAzureBlobServiceAPI.Interfaces;
using NetCoreAzureBlobServiceAPI.Exceptions;
using Microsoft.AspNetCore.Authorization;

namespace NetCoreAzureBlobServiceAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Policy = "FilesScope")]
    public class FileManagerController(IFileManagementService fileManagementService, ILogger<FileManagerController> logger) : ControllerBase
    {
        private readonly IFileManagementService _fileManagementService = fileManagementService;
        private readonly ILogger<FileManagerController> _logger = logger;

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile(IFormFile file)
        {
            try
            {
                var blobUri = await _fileManagementService.UploadFileAsync(file, User);
                return Ok(new { BlobUri = blobUri, Message = "File uploaded successfully." });
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized upload attempt");
                return Unauthorized(new { Error = ex.Message });
            }
            catch (InvalidFileException ex)
            {
                _logger.LogWarning(ex, "Invalid file upload attempt");
                return BadRequest(new { Error = ex.Message, FileName = ex.FileName });
            }
            catch (BlobStorageException ex)
            {
                _logger.LogError(ex, "Blob storage error during upload");
                return StatusCode(500, new { Error = "An error occurred while uploading the file.", Details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during file upload");
                return StatusCode(500, new { Error = "An unexpected error occurred while uploading the file." });
            }
        }

        [HttpGet("list")]
        public async Task<IActionResult> ListBlobs()
        {
            try
            {
                var blobs = await _fileManagementService.ListBlobsAsync(User);
                return Ok(blobs);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized list attempt");
                return Unauthorized(new { Error = ex.Message });
            }
            catch (BlobStorageException ex)
            {
                _logger.LogError(ex, "Blob storage error during list operation");
                return StatusCode(500, new { Error = "An error occurred while listing blobs.", Details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during blob listing");
                return StatusCode(500, new { Error = "An unexpected error occurred while listing the blobs." });
            }
        }

        [HttpGet("download")]
        public async Task<IActionResult> DownloadBlob(string blobName)
        {
            try
            {
                var stream = await _fileManagementService.DownloadBlobAsync(blobName, User);
                return File(stream, "application/octet-stream", blobName);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger.LogWarning(ex, "Unauthorized download attempt");
                return Unauthorized(new { Error = ex.Message });
            }
            catch (BlobNotFoundException ex)
            {
                _logger.LogWarning(ex, "Blob not found");
                return NotFound(new { Error = ex.Message, BlobName = ex.BlobName, ContainerName = ex.ContainerName });
            }
            catch (BlobStorageException ex)
            {
                _logger.LogError(ex, "Blob storage error during download");
                return StatusCode(500, new { Error = "An error occurred while downloading the blob.", Details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during blob download");
                return StatusCode(500, new { Error = "An unexpected error occurred while downloading the blob." });
            }
        }
    }
}


