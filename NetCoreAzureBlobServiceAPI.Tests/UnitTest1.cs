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
    private readonly Mock<IClientValidationService> _mockClientValidation;
    private readonly Mock<ILogger<FileManagementService>> _mockLogger;
    private readonly FileManagementService _service;

    public FileManagementServiceTests()
    {
        _mockBlobRepository = new Mock<IBlobStorageRepository>();
        _mockClientValidation = new Mock<IClientValidationService>();
        _mockLogger = new Mock<ILogger<FileManagementService>>();
        _service = new FileManagementService(_mockBlobRepository.Object, _mockClientValidation.Object, _mockLogger.Object);
    }

    [Fact]
    public async Task UploadFileAsync_WithValidCredentials_UploadsFile()
    {
        // Arrange
        var mockFile = CreateMockFormFile("test.txt", "text/plain", "test content");
        var clientId = "testClient";
        var clientSecret = "testSecret";
        var expectedUri = "https://test.blob.core.windows.net/testclient-container/blob.txt";

        _mockClientValidation.Setup(x => x.ValidateClient(clientId, clientSecret)).Returns(true);
        _mockBlobRepository.Setup(x => x.UploadBlobAsync(It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(expectedUri);

        // Act
        var result = await _service.UploadFileAsync(mockFile, clientId, clientSecret);

        // Assert
        Assert.Equal(expectedUri, result);
        _mockBlobRepository.Verify(x => x.UploadBlobAsync(mockFile, "testclient-container", It.IsAny<string>()), Times.Once);
    }

    [Fact]
    public async Task UploadFileAsync_WithInvalidCredentials_ThrowsUnauthorizedException()
    {
        // Arrange
        var mockFile = CreateMockFormFile("test.txt", "text/plain", "test content");
        _mockClientValidation.Setup(x => x.ValidateClient(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.UploadFileAsync(mockFile, "bad", "bad"));
        Assert.Equal("Invalid client credentials.", ex.Message);
    }

    [Fact]
    public async Task UploadFileAsync_WithNullFile_ThrowsArgumentException()
    {
        // Arrange
        _mockClientValidation.Setup(x => x.ValidateClient(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidFileException>(() =>
            _service.UploadFileAsync(null!, "client", "secret"));
        Assert.Equal("File is empty or null.", ex.Message);
    }

    [Fact]
    public async Task UploadFileAsync_WithInvalidExtension_ThrowsArgumentException()
    {
        // Arrange
        var mockFile = CreateMockFormFile("test.exe", "application/octet-stream", "test content");
        _mockClientValidation.Setup(x => x.ValidateClient(It.IsAny<string>(), It.IsAny<string>())).Returns(true);

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidFileException>(() =>
            _service.UploadFileAsync(mockFile, "client", "secret"));
        Assert.Contains("not permitted", ex.Message);
    }

    [Theory]
    [InlineData(".txt")]
    [InlineData(".csv")]
    [InlineData(".json")]
    [InlineData(".xml")]
    [InlineData(".xls")]
    [InlineData(".xlsx")]
    public async Task UploadFileAsync_WithPermittedExtensions_Succeeds(string extension)
    {
        // Arrange
        var mockFile = CreateMockFormFile($"test{extension}", "application/octet-stream", "test content");
        _mockClientValidation.Setup(x => x.ValidateClient(It.IsAny<string>(), It.IsAny<string>())).Returns(true);
        _mockBlobRepository.Setup(x => x.UploadBlobAsync(It.IsAny<IFormFile>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync("https://test.blob.core.windows.net/test.blob");

        // Act
        var result = await _service.UploadFileAsync(mockFile, "client", "secret");

        // Assert
        Assert.NotNull(result);
        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task ListBlobsAsync_WithValidCredentials_ReturnsBlobs()
    {
        // Arrange
        var clientId = "testClient";
        var clientSecret = "testSecret";
        var expectedBlobs = new List<Models.BlobInfo>
        {
            new() { Name = "blob1.txt", Url = "https://test.com/blob1.txt" },
            new() { Name = "blob2.txt", Url = "https://test.com/blob2.txt" }
        };

        _mockClientValidation.Setup(x => x.ValidateClient(clientId, clientSecret)).Returns(true);
        _mockBlobRepository.Setup(x => x.ListBlobsAsync("testclient-container")).ReturnsAsync(expectedBlobs);

        // Act
        var result = await _service.ListBlobsAsync(clientId, clientSecret);

        // Assert
        var resultList = result.ToList();
        Assert.Equal(2, resultList.Count);
        Assert.Equal("blob1.txt", resultList[0].Name);
        Assert.Equal("blob2.txt", resultList[1].Name);
    }

    [Fact]
    public async Task ListBlobsAsync_WithInvalidCredentials_ThrowsUnauthorizedException()
    {
        // Arrange
        _mockClientValidation.Setup(x => x.ValidateClient(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.ListBlobsAsync("bad", "bad"));
    }

    [Fact]
    public async Task DownloadBlobAsync_WithValidCredentials_ReturnsStream()
    {
        // Arrange
        var clientId = "testClient";
        var clientSecret = "testSecret";
        var blobName = "test.txt";
        var expectedStream = new MemoryStream();

        _mockClientValidation.Setup(x => x.ValidateClient(clientId, clientSecret)).Returns(true);
        _mockBlobRepository.Setup(x => x.DownloadBlobAsync("testclient-container", blobName)).ReturnsAsync(expectedStream);

        // Act
        var result = await _service.DownloadBlobAsync(clientId, clientSecret, blobName);

        // Assert
        Assert.Same(expectedStream, result);
    }

    [Fact]
    public async Task DownloadBlobAsync_WithInvalidCredentials_ThrowsUnauthorizedException()
    {
        // Arrange
        _mockClientValidation.Setup(x => x.ValidateClient(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            _service.DownloadBlobAsync("bad", "bad", "blob.txt"));
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
