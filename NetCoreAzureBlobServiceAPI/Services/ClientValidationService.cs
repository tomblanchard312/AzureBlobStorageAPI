using NetCoreAzureBlobServiceAPI.Interfaces;

namespace NetCoreAzureBlobServiceAPI.Services
{
    public class ClientValidationService(IConfiguration configuration) : IClientValidationService
    {
        private readonly IConfiguration _configuration = configuration;

        public bool ValidateClient(string clientId, string clientSecret)
        {
            var validClientId = _configuration["ClientValidation:ClientId"];
            var validClientSecret = _configuration["ClientValidation:ClientSecret"];

            return clientId == validClientId && clientSecret == validClientSecret;
        }
    }
}
