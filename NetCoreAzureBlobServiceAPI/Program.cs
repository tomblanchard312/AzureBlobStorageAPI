using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using NetCoreAzureBlobServiceAPI.Classes;

var builder = WebApplication.CreateBuilder(args);
/**
 * uncomment this once you add your key vault to implement secrets
 * **/
// Add services to the container.
//var vaulturi = builder.Configuration["vaulturi"];
//var keyVaultEndpoint = new Uri(vaulturi);
//builder.Configuration.AddAzureKeyVault(keyVaultEndpoint, new DefaultAzureCredential());
//builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
//        .AddMicrosoftIdentityWebApi(options =>
//        {
//            builder.Configuration.Bind("AzureAd", options);
//            options.TokenValidationParameters.NameClaimType = "name";
//        }, options => { builder.Configuration.Bind("AzureAd", options); });

//builder.Services.AddAuthorization(config =>
//{
//    config.AddPolicy("AuthZPolicy", policyBuilder =>
//        policyBuilder.Requirements.Add(new ScopeAuthorizationRequirement() { RequiredScopesConfigurationKey = $"AzureAd:Scopes" }));
//});
// Configure Data Protection
builder.Services.AddDataProtection()
    .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration()
    {
        EncryptionAlgorithm = EncryptionAlgorithm.AES_256_GCM,
        ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
    });

// Configure Azure Clients
builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddBlobServiceClient(builder.Configuration["StorageConnection:blob"], preferMsi: true);
    clientBuilder.AddQueueServiceClient(builder.Configuration["StorageConnection:queue"], preferMsi: true);
    clientBuilder.UseCredential(new DefaultAzureCredential());
});

// Configure Controllers
builder.Services.AddControllers();

// Configure Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Net Core Blob Service API",
        Description = "An API that allows you to upload/list/download/read files from Azure Blob Storage",
        Contact = new OpenApiContact
        {
            Name = "Tom Blanchard",
            Email = "tomblanchard3@outlook.com"
        }
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    // Development Exception Page
    app.UseDeveloperExceptionPage();
    // Swagger UI for Development
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Blob File Management");
    });
}
else
{
    // Exception Handling for Production
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// HTTPS Redirection
app.UseHttpsRedirection();
// Static Files (e.g., Swagger UI)
app.UseStaticFiles();
// Routing
app.UseRouting();
//force redirect to swagger, helps if you are publishing to azure PaaS.
var option = new RewriteOptions();
option.AddRedirect("^$", "swagger");
// Authorization
app.UseAuthorization();
// Map Controllers
app.MapControllers();

app.Run();
