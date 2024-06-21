namespace NetCoreAzureBlobServiceAPI.Interfaces
{
    public interface IClientValidationService
    {
        bool ValidateClient(string clientId, string clientSecret);
    }
}
