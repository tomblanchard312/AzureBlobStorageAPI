namespace NetCoreAzureBlobServiceAPI.Exceptions;

public class BlobNotFoundException : Exception
{
    public string BlobName { get; }
    public string ContainerName { get; }

    public BlobNotFoundException(string blobName, string containerName)
        : base($"Blob '{blobName}' not found in container '{containerName}'.")
    {
        BlobName = blobName;
        ContainerName = containerName;
    }

    public BlobNotFoundException(string blobName, string containerName, Exception innerException)
        : base($"Blob '{blobName}' not found in container '{containerName}'.", innerException)
    {
        BlobName = blobName;
        ContainerName = containerName;
    }
}
