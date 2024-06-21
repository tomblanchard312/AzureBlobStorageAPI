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

string blobServiceConnectionString = builder.Configuration.GetConnectionString("AzureBlobStorage");
builder.Services.AddSingleton(new BlobServiceClient(blobServiceConnectionString));

builder.Services.AddSingleton<IFileManagementService, FileManagementService>();
builder.Services.AddSingleton<IClientValidationService, ClientValidationService>();
builder.Services.AddSingleton<IBlobStorageRepository, BlobStorageRepository>();
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

app.Run();
