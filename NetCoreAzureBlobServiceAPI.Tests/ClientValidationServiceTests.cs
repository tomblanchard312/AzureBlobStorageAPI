using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using NetCoreAzureBlobServiceAPI.Services;

namespace NetCoreAzureBlobServiceAPI.Tests;

public class ClientValidationServiceTests
{
    [Fact]
    public void ValidateClient_WithMatchingCredentials_ReturnsTrue()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ClientValidation:ClientId"] = "testClient",
                ["ClientValidation:ClientSecret"] = "testSecret"
            }!)
            .Build();

        var mockLogger = new Mock<ILogger<ClientValidationService>>();
        var service = new ClientValidationService(configuration, mockLogger.Object);

        // Act
        var result = service.ValidateClient("testClient", "testSecret");

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void ValidateClient_WithInvalidClientId_ReturnsFalse()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ClientValidation:ClientId"] = "testClient",
                ["ClientValidation:ClientSecret"] = "testSecret"
            }!)
            .Build();

        var mockLogger = new Mock<ILogger<ClientValidationService>>();
        var service = new ClientValidationService(configuration, mockLogger.Object);

        // Act
        var result = service.ValidateClient("wrongClient", "testSecret");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateClient_WithInvalidSecret_ReturnsFalse()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ClientValidation:ClientId"] = "testClient",
                ["ClientValidation:ClientSecret"] = "testSecret"
            }!)
            .Build();

        var mockLogger = new Mock<ILogger<ClientValidationService>>();
        var service = new ClientValidationService(configuration, mockLogger.Object);

        // Act
        var result = service.ValidateClient("testClient", "wrongSecret");

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void ValidateClient_WithBothInvalid_ReturnsFalse()
    {
        // Arrange
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ClientValidation:ClientId"] = "testClient",
                ["ClientValidation:ClientSecret"] = "testSecret"
            }!)
            .Build();

        var mockLogger = new Mock<ILogger<ClientValidationService>>();
        var service = new ClientValidationService(configuration, mockLogger.Object);

        // Act
        var result = service.ValidateClient("wrongClient", "wrongSecret");

        // Assert
        Assert.False(result);
    }
}
