namespace NetCoreAzureBlobServiceAPI.Exceptions;

public class InvalidFileException : ArgumentException
{
    public string? FileName { get; }

    public InvalidFileException(string message) : base(message)
    {
    }

    public InvalidFileException(string message, string fileName) : base(message)
    {
        FileName = fileName;
    }
}
