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
string accountUri = builder.Configuration["BlobStorage:AccountUri"] ?? throw new InvalidOperationException("Blob Storage account URI is not configured.");

var credential = new DefaultAzureCredential();
var blobServiceClient = new BlobServiceClient(new Uri(accountUri), credential);

builder.Services.AddSingleton(blobServiceClient);

builder.Services.AddSingleton<IFileManagementService, FileManagementService>();
builder.Services.AddSingleton<IBlobStorageRepository, BlobStorageRepository>();

// Add health checks
builder.Services.AddHealthChecks()
    .AddAzureBlobStorage(new Uri(accountUri), credential, name: "azureblob");

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
