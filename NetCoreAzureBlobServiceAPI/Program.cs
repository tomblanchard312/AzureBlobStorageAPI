using Microsoft.Extensions.Azure;
using NetCoreAzureBlobServiceAPI.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Azure.Storage.Blobs;
using NetCoreAzureBlobServiceAPI.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configuration validation
string blobServiceConnectionString = builder.Configuration.GetConnectionString("AzureBlobStorage") ?? throw new InvalidOperationException("Azure Blob Storage connection string is not configured.");
string? clientId = builder.Configuration["ClientValidation:ClientId"];
string? clientSecret = builder.Configuration["ClientValidation:ClientSecret"];

if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
{
    throw new InvalidOperationException("ClientValidation:ClientId and ClientValidation:ClientSecret must be configured.");
}

builder.Services.AddSingleton(new BlobServiceClient(blobServiceConnectionString));

builder.Services.AddSingleton<IFileManagementService, FileManagementService>();
builder.Services.AddSingleton<IClientValidationService, ClientValidationService>();
builder.Services.AddSingleton<IBlobStorageRepository, BlobStorageRepository>();

// Add health checks
builder.Services.AddHealthChecks()
    .AddAzureBlobStorage(blobServiceConnectionString, name: "azureblob");

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

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
