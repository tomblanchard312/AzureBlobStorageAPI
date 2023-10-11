namespace NetCoreAzureBlobServiceAPI.Classes
{    public class BlobInfo
    {
        public string Name { get; set; }
        public DateTimeOffset CreatedOn { get; set; }

        public BlobInfo() { } // Add a parameterless constructor

        public BlobInfo(string name, DateTimeOffset createdOn)
        {
            Name = name;
            CreatedOn = createdOn;
        }
    }
}
