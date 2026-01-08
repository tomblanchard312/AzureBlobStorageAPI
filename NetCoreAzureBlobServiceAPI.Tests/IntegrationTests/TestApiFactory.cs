using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NetCoreAzureBlobServiceAPI.Interfaces;
using Microsoft.AspNetCore.Mvc.Testing;

namespace NetCoreAzureBlobServiceAPI.Tests.IntegrationTests;

public class TestApiFactory : WebApplicationFactory<NetCoreAzureBlobServiceAPI.Controllers.FileManagerController>
{
    public Mock<IBlobStorageRepository> BlobRepositoryMock { get; } = new Mock<IBlobStorageRepository>();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            // Replace the real blob repository with a mock
            services.AddSingleton(BlobRepositoryMock.Object);

            // Configure JwtBearer for tests with a symmetric signing key.
            // This keeps production authentication unchanged while allowing
            // generated test JWTs to validate in the test host.
            var key = System.Text.Encoding.UTF8.GetBytes(TestJwtTokenHelper.TestSigningKey);
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "TestJwt";
                options.DefaultChallengeScheme = "TestJwt";
            }).AddJwtBearer("TestJwt", options =>
            {
                options.RequireHttpsMetadata = false;
                options.SaveToken = true;
                options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = true,
                    ValidAudience = TestJwtTokenHelper.TestAudience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(key),
                    ValidateLifetime = true
                };
            });
        });
    }
}
