using Azure.Storage.Blobs;

using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.Rewrite;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Azure;
using Azure.Identity;

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

builder.Services.AddDataProtection()
    .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration()
    {
        EncryptionAlgorithm = EncryptionAlgorithm.AES_256_GCM,
        ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
    });
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAzureClients(clientBuilder =>
{
    clientBuilder.AddBlobServiceClient(builder.Configuration["StorageConnection:blob"], preferMsi: true);
    clientBuilder.AddQueueServiceClient(builder.Configuration["StorageConnection:queue"], preferMsi: true);
    clientBuilder.UseCredential(new DefaultAzureCredential());
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = "v1",
        Title = "Net Core Blob Service API",
        Description = "An API that allows you to upload/list/download files from Azure Blob Storage",
        Contact = new OpenApiContact
        {
            Name = "Tom Blanchard",
            Email = "tomblanchard3@outlook.com"
        }
    });
});

var app = builder.Build();
app.UseSwagger();
app.UseStaticFiles();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("./v1/swagger.json", "File and String Managements");
});

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseRouting();
var option = new RewriteOptions();
option.AddRedirect("^$", "swagger");
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
