using Xunit;
using NetCoreAzureBlobServiceAPI.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NetCoreAzureBlobServiceAPI.Services;
using NetCoreAzureBlobServiceAPI.Models;
using System.Threading.Tasks;

namespace NetCoreAzureBlobServiceAPI.Tests
{
    public class FileManagerControllerTests
    {
        [Fact]
        public async Task ListFiles_ReturnsOkResult()
        {
            // Arrange
            var fileServiceMock = new Mock<IFileManagementService>();
            fileServiceMock.Setup(s => s.ListFilesAsync()).ReturnsAsync(new System.Collections.Generic.List<BlobInfo>());
            var controller = new FileManagerController(fileServiceMock.Object, null, null);
            // Act
            var result = await controller.ListFiles();
            // Assert
            Assert.IsType<OkObjectResult>(result);
        }
    }
}
