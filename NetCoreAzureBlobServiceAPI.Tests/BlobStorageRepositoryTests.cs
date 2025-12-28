using Azure;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Moq;
using NetCoreAzureBlobServiceAPI.Services;

namespace NetCoreAzureBlobServiceAPI.Tests;

public class BlobStorageRepositoryTests
{
    [Fact]
    public async Task UploadBlobAsync_CreatesContainerIfNotExists()
    {
        // Arrange
        var mockBlobServiceClient = new Mock<BlobServiceClient>();
        var mockContainerClient = new Mock<BlobContainerClient>();
        var mockBlobClient = new Mock<BlobClient>();
        var mockLogger = new Mock<ILogger<BlobStorageRepository>>();

        mockBlobServiceClient.Setup(x => x.GetBlobContainerClient(It.IsAny<string>()))
            .Returns(mockContainerClient.Object);

        mockContainerClient.Setup(x => x.CreateIfNotExistsAsync(PublicAccessType.None, null, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<Response<BlobContainerInfo>>());

        mockContainerClient.Setup(x => x.GetBlobClient(It.IsAny<string>()))
            .Returns(mockBlobClient.Object);

        mockBlobClient.Setup(x => x.UploadAsync(It.IsAny<Stream>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(Mock.Of<Response<BlobContentInfo>>());

        mockBlobClient.Setup(x => x.Uri).Returns(new Uri("https://test.blob.core.windows.net/container/blob.txt"));

        var repository = new BlobStorageRepository(mockBlobServiceClient.Object, mockLogger.Object);
        var mockFile = CreateMockFormFile("test.txt", "text/plain", "test content");

        // Act
        var result = await repository.UploadBlobAsync(mockFile, "test-container", "blob.txt");

        // Assert
        mockContainerClient.Verify(x => x.CreateIfNotExistsAsync(PublicAccessType.None, null, null, It.IsAny<CancellationToken>()), Times.Once);
        mockBlobClient.Verify(x => x.UploadAsync(It.IsAny<Stream>(), true, It.IsAny<CancellationToken>()), Times.Once);
        Assert.Equal("https://test.blob.core.windows.net/container/blob.txt", result);
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
