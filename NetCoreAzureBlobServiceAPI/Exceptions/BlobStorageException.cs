namespace NetCoreAzureBlobServiceAPI.Exceptions;

public class BlobStorageException : Exception
{
    public BlobStorageException(string message) : base(message)
    {
    }

    public BlobStorageException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
