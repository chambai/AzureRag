using AzureFunction.Client;

namespace Tests.Fakes
{
    public class FakeBlobStorage : IBlobStorage
    {
        public List<(string Container, string Name, byte[] Content)> UploadedFiles { get; } = new();

        public async Task UploadAsync(string containerName, string blobName, Stream content)
        {
            using var ms = new MemoryStream();
            await content.CopyToAsync(ms);
            UploadedFiles.Add((containerName, blobName, ms.ToArray()));
        }
    }
}
