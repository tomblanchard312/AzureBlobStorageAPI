using System.Threading.Tasks;
using Xunit;
using NetCoreAzureBlobServiceAPI.Services;
using NetCoreAzureBlobServiceAPI.Models;
using Moq;
using System.Collections.Generic;

namespace NetCoreAzureBlobServiceAPI.Tests
{
    public class BlobStorageRepositoryTests
    {
        [Fact]
        public async Task ListBlobsAsync_ReturnsBlobInfoList()
        {
            // Arrange
            var repo = new BlobStorageRepository(/* pass required dependencies or mock them */);
            // Act
            var result = await repo.ListBlobsAsync();
            // Assert
            Assert.IsType<List<BlobInfo>>(result);
        }
    }
}
