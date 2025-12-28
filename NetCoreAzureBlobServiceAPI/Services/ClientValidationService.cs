using NetCoreAzureBlobServiceAPI.Interfaces;

namespace NetCoreAzureBlobServiceAPI.Services
{
    public class ClientValidationService(IConfiguration configuration, ILogger<ClientValidationService> logger) : IClientValidationService
    {
        private readonly IConfiguration _configuration = configuration;
        private readonly ILogger<ClientValidationService> _logger = logger;

        public bool ValidateClient(string clientId, string clientSecret)
        {
            var validClientId = _configuration["ClientValidation:ClientId"];
            var validClientSecret = _configuration["ClientValidation:ClientSecret"];

            var isValid = clientId == validClientId && clientSecret == validClientSecret;

            if (!isValid)
            {
                _logger.LogWarning("Client validation failed for clientId: {ClientId}", clientId);
            }
            else
            {
                _logger.LogDebug("Client validated successfully: {ClientId}", clientId);
            }

            return isValid;
        }
    }
}
