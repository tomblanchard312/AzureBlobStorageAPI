using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NetCoreAzureBlobServiceAPI.Interfaces;
using NetCoreAzureBlobServiceAPI.Services;
using NetCoreAzureBlobServiceAPI.Exceptions;

namespace NetCoreAzureBlobServiceAPI.Tests;

public class FileManagementServiceTests
{
    private readonly Mock<IBlobStorageRepository> _mockBlobRepository;
    private readonly Mock<ILogger<FileManagementService>> _mockLogger;
    private readonly FileManagementService _service;

    public FileManagementServiceTests()
    {
        _mockBlobRepository = new Mock<IBlobStorageRepository>();
        _mockLogger = new Mock<ILogger<FileManagementService>>();
        _service = new FileManagementService(_mockBlobRepository.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task UploadFileAsync_WithNullFile_ThrowsInvalidFileException()
    {
        // Arrange
        var user = CreateUserPrincipal("TestUser");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidFileException>(() => _service.UploadFileAsync(null!, user));
        Assert.Equal("File is empty or null.", ex.Message);
    }

    [Fact]
    public async Task UploadFileAsync_WithInvalidExtension_ThrowsInvalidFileException()
    {
        // Arrange
        var mockFile = CreateMockFormFile("test.exe", "application/octet-stream", "content");
        var user = CreateUserPrincipal("UserX");

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidFileException>(() => _service.UploadFileAsync(mockFile, user));
        Assert.Contains("not permitted", ex.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(".txt")]
    [InlineData(".csv")]
    [InlineData(".json")]
    [InlineData(".xml")]
    [InlineData(".xls")]
    [InlineData(".xlsx")]
    public async Task UploadFileAsync_WithPermittedExtensions_CallsRepository(string extension)
    {
        // Arrange
        var mockFile = CreateMockFormFile($"test{extension}", "application/octet-stream", "content");
        var user = CreateUserPrincipal("SomeUserId");
        var expectedUri = "https://example.blob.core.windows.net/container/blob";

        _mockBlobRepository.Setup(x => x.UploadBlobAsync(It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(expectedUri);

        // Act
        var result = await _service.UploadFileAsync(mockFile, user);

        // Assert
        Assert.Equal(expectedUri, result);
        var expectedContainer = "someuserid-container";
        _mockBlobRepository.Verify(x => x.UploadBlobAsync(mockFile, expectedContainer, It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task ListBlobsAsync_UsesContainerName_FromClaims()
    {
        // Arrange
        var user = CreateUserPrincipal("ListUser");
        var expected = new List<Models.BlobInfo> { new() { Name = "a.txt", Url = "u" } };
        _mockBlobRepository.Setup(x => x.ListBlobsAsync("listuser-container")).ReturnsAsync(expected);

        // Act
        var result = await _service.ListBlobsAsync(user);

        // Assert
        Assert.Single(result);
        _mockBlobRepository.Verify(x => x.ListBlobsAsync("listuser-container"), Times.Once);
    }

    [Fact]
    public async Task DownloadBlobAsync_UsesContainerName_FromClaims()
    {
        // Arrange
        var user = CreateUserPrincipal("DLUser");
        var blobName = "file.txt";
        var stream = new MemoryStream();
        _mockBlobRepository.Setup(x => x.DownloadBlobAsync("dluser-container", blobName)).ReturnsAsync(stream);

        // Act
        var result = await _service.DownloadBlobAsync(blobName, user);

        // Assert
        Assert.Same(stream, result);
        _mockBlobRepository.Verify(x => x.DownloadBlobAsync("dluser-container", blobName), Times.Once);
    }

    private static ClaimsPrincipal CreateUserPrincipal(string oid)
    {
        var claims = new[] { new Claim("oid", oid) };
        var identity = new ClaimsIdentity(claims, "test");
        return new ClaimsPrincipal(identity);
    }

    private static IFormFile CreateMockFormFile(string fileName, string contentType, string content)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);
        return new FormFile(stream, 0, bytes.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }
}
