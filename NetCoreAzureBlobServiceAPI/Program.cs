using Microsoft.Extensions.Azure;
using NetCoreAzureBlobServiceAPI.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Azure.Storage.Blobs;
using NetCoreAzureBlobServiceAPI.Interfaces;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Azure.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration);
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("FilesScope", policy => policy.RequireClaim("scp", "Files.Manage"));
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuration validation
// Prefer explicit environment variable (helps tests injecting values early),
// fall back to configured values.
var accountUriStr = Environment.GetEnvironmentVariable("BlobStorage__AccountUri") ?? builder.Configuration["BlobStorage:AccountUri"];
if (string.IsNullOrWhiteSpace(accountUriStr))
{
    throw new InvalidOperationException("Blob Storage account URI is not configured. Set 'BlobStorage:AccountUri' in configuration.");
}

if (!Uri.TryCreate(accountUriStr, UriKind.Absolute, out var accountUri) || (accountUri.Scheme != Uri.UriSchemeHttps && accountUri.Scheme != Uri.UriSchemeHttp))
{
    throw new InvalidOperationException($"Blob Storage account URI '{accountUriStr}' is not a valid absolute URI.");
}

var credential = new DefaultAzureCredential();
var blobServiceClient = new BlobServiceClient(accountUri, credential);

builder.Services.AddSingleton(blobServiceClient);

builder.Services.AddSingleton<IFileManagementService, FileManagementService>();
builder.Services.AddSingleton<IBlobStorageRepository, BlobStorageRepository>();

// Add health checks
builder.Services.AddHealthChecks()
    .AddAzureBlobStorage(accountUri, credential, name: "azureblob");

builder.Services.AddLogging(config =>
{
    config.AddConsole();
    config.AddDebug();
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
