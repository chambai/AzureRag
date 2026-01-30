using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using AzureFunction.Client;

public class AzureBlobStorage : IBlobStorage
{
    private readonly BlobServiceClient _blobServiceClient;

    public AzureBlobStorage(BlobServiceClient blobServiceClient)
    {
        _blobServiceClient = blobServiceClient;
    }

    public async Task UploadAsync(string containerName, string blobName, Stream content)
    {
        var container = _blobServiceClient.GetBlobContainerClient(containerName);

        // Creates the container if it does not exist
        await container.CreateIfNotExistsAsync(PublicAccessType.None);

        var blobClient = container.GetBlobClient(blobName);
        await blobClient.UploadAsync(content, overwrite: true);
    }
}