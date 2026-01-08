using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Moq;
using NetCoreAzureBlobServiceAPI.Interfaces;

namespace NetCoreAzureBlobServiceAPI.Tests.IntegrationTests;

public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    public Mock<IBlobStorageRepository> BlobRepositoryMock { get; } = new Mock<IBlobStorageRepository>();

    static TestWebApplicationFactory()
    {
        // Set environment variables early so they are available to Program.CreateBuilder's configuration
        Environment.SetEnvironmentVariable("BlobStorage__AccountUri", "https://fakestorage.blob.core.windows.net/");
        Environment.SetEnvironmentVariable("AzureAd__Instance", "https://login.microsoftonline.com/");
        Environment.SetEnvironmentVariable("AzureAd__TenantId", "test-tenant");
        Environment.SetEnvironmentVariable("AzureAd__ClientId", "test-client");
        Environment.SetEnvironmentVariable("AzureAd__Audience", TestJwtTokenHelper.TestAudience);
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Inject in-memory config values needed by Program.cs and Microsoft.Identity.Web
        var inMemorySettings = new Dictionary<string, string?>
        {
            ["BlobStorage:AccountUri"] = "https://fakestorage.blob.core.windows.net/",
            ["AzureAd:Instance"] = "https://login.microsoftonline.com/",
            ["AzureAd:TenantId"] = "test-tenant",
            ["AzureAd:ClientId"] = "test-client",
            ["AzureAd:Audience"] = TestJwtTokenHelper.TestAudience
        };

        builder.ConfigureAppConfiguration((context, conf) =>
        {
            // Insert the in-memory collection as the highest-priority source so it overrides appsettings.
            var memorySource = new Microsoft.Extensions.Configuration.Memory.MemoryConfigurationSource { InitialData = inMemorySettings! };
            conf.Sources.Insert(0, memorySource);
        });

        builder.ConfigureTestServices(services =>
        {
            // Replace the real blob repository with a mock to avoid any network calls
            services.AddSingleton(BlobRepositoryMock.Object);

            // Remove any existing authentication registrations to avoid duplicate scheme errors
            services.RemoveAll<Microsoft.AspNetCore.Authentication.IAuthenticationService>();
            services.RemoveAll<Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider>();
            services.RemoveAll<Microsoft.AspNetCore.Authentication.IAuthenticationHandlerProvider>();

            // Register a test-only authentication scheme named "Test". This authenticates every request
            // and injects the expected claims. Do NOT change production authentication.
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            }).AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestAuthHandler>(
                "Test", options => { });
        });
    }

    private class TestAuthHandler : Microsoft.AspNetCore.Authentication.AuthenticationHandler<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions>
    {
        public TestAuthHandler(Microsoft.Extensions.Options.IOptionsMonitor<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions> options,
            Microsoft.Extensions.Logging.ILoggerFactory logger,
            System.Text.Encodings.Web.UrlEncoder encoder,
            Microsoft.AspNetCore.Authentication.ISystemClock clock)
            : base(options, logger, encoder, clock)
        {
        }

        protected override Task<Microsoft.AspNetCore.Authentication.AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[]
            {
                new System.Security.Claims.Claim("oid", "IntegrationUser"),
                new System.Security.Claims.Claim("scp", "Files.Manage")
            };

            var identity = new System.Security.Claims.ClaimsIdentity(claims, "Test");
            var principal = new System.Security.Claims.ClaimsPrincipal(identity);
            var ticket = new Microsoft.AspNetCore.Authentication.AuthenticationTicket(principal, "Test");
            return Task.FromResult(Microsoft.AspNetCore.Authentication.AuthenticateResult.Success(ticket));
        }
    }
}
