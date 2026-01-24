using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;

public class UploadFunction
{
    private readonly ILogger _logger;
    private readonly BlobServiceClient _blobServiceClient;
    private readonly HttpClient _httpClient;

    private readonly string gpuServiceUrl = "http://192.168.122.1:8000/embed"; // Replace with your Linux server

    public UploadFunction(ILoggerFactory loggerFactory, BlobServiceClient blobServiceClient)
    {
        _logger = loggerFactory.CreateLogger<UploadFunction>();
        _blobServiceClient = blobServiceClient;
        _httpClient = new HttpClient();
    }

    [Function("UploadDocument")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "upload")] HttpRequestData req)
    {
        //var form = await req.ReadAsStringAsync();
        //var file = form.Files.First();

        // Read JSON from body
        var file = await JsonSerializer.DeserializeAsync<IFormFile>(req.Body);

        if (file == null || file.Length==0)
        {
            var badResp = req.CreateResponse(HttpStatusCode.BadRequest);
            await badResp.WriteStringAsync("No document provided.");
            return badResp;
        }

        //// ... continue processing, e.g., chunking, embedding, uploading
        //var resp = req.CreateResponse(HttpStatusCode.OK);
        //await resp.WriteStringAsync("Document uploaded successfully.");
        //return resp;

        // Store in Blob
        var container = _blobServiceClient.GetBlobContainerClient("documents");
        await container.CreateIfNotExistsAsync();
        var blob = container.GetBlobClient(file.FileName);
        using (var stream = file.OpenReadStream())
        {
            await blob.UploadAsync(stream, overwrite: true);
        }

        // 2️⃣ Read text from file (simplified)
        string text;
        using (var reader = new StreamReader(file.OpenReadStream()))
            text = await reader.ReadToEndAsync();

        // 3️⃣ Chunk text
        var chunks = Utilities.SplitIntoChunks(text);

        // 4️⃣ Send each chunk to GPU service for embeddings
        foreach (var chunk in chunks)
        {
            var payload = new { texts = new[] { chunk } };
            var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(gpuServiceUrl, content);
            // TODO: parse response and index into Azure AI Search
        }

        var resp = req.CreateResponse(HttpStatusCode.OK);
        await resp.WriteStringAsync("Document uploaded and embedding process triggered.");
        return resp;
    }
}

//public class DocumentUpload :   IFormFile
//{
//    public string Text { get; set; }
//}
