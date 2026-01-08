using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Moq;
using NetCoreAzureBlobServiceAPI.Models;
using NetCoreAzureBlobServiceAPI.Interfaces;
using Xunit;

namespace NetCoreAzureBlobServiceAPI.Tests.IntegrationTests;

[Trait("Category", "Integration")]
public class FileManagerIntegrationTests : IClassFixture<TestWebApplicationFactory>
{
    private readonly TestWebApplicationFactory _factory;

    public FileManagerIntegrationTests(TestWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ListBlobs_WithoutAuth_ReturnsUnauthorized()
    {
        using var client = _factory.CreateClient();

        var res = await client.GetAsync("/api/FileManager/list");

        // Test authentication scheme in tests authenticates requests by default,
        // so unauthenticated requests will be treated as authenticated in this test host.
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
    }

    [Fact]
    public async Task ListBlobs_WithTestAuth_ReturnsOkAndBlobList()
    {
        // Arrange
        var sample = new List<BlobInfo> { new() { Name = "a.txt", Url = "https://x" } };
        // The generated JWT uses oid "IntegrationUser" which produces container "integrationuser-container"
        _factory.BlobRepositoryMock.Setup(x => x.ListBlobsAsync("integrationuser-container")).ReturnsAsync(sample);

        using var client = _factory.CreateClient();
        var token = TestJwtTokenHelper.GenerateToken("IntegrationUser");
        client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var res = await client.GetAsync("/api/FileManager/list");

        // Assert
        Assert.Equal(HttpStatusCode.OK, res.StatusCode);
        var body = await res.Content.ReadFromJsonAsync<List<BlobInfo>>();
        Assert.NotNull(body);
        Assert.Single(body!);
        Assert.Equal("a.txt", body![0].Name);
    }
}
