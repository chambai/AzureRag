using Azure.Storage.Blobs;
using AzureFunction.Client;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using System.Text.Json;

public class UploadFunction
{
    private readonly ILogger _logger;
    private readonly IBlobStorage _blobServiceClient;
    private readonly IEmbeddingClient _embeddingClient;

    public UploadFunction(
        ILoggerFactory loggerFactory,
        IBlobStorage blobServiceClient,
        IEmbeddingClient embeddingClient)
    {
        _logger = loggerFactory.CreateLogger<UploadFunction>();
        _blobServiceClient = blobServiceClient;
        _embeddingClient = embeddingClient;
    }

    [Function("UploadDocument")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "upload")]
        HttpRequestData req)
    {
        // Require filename header
        if (!req.Headers.TryGetValues("X-Filename", out var filenames))
        {
            return await BadRequest(req, "Missing X-Filename header.");
        }

        var fileName = filenames.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return await BadRequest(req, "Invalid filename.");
        }

        if (req.Body == null || req.Body.Length == 0)
        {
            return await BadRequest(req, "No document provided.");
        }

        // Upload to Blob Storage
        await _blobServiceClient.UploadAsync("documents", fileName, req.Body);

        // Reset stream position
        req.Body.Seek(0, SeekOrigin.Begin);

        // Read text
        string text;
        using (var reader = new StreamReader(req.Body, Encoding.UTF8))
        {
            text = await reader.ReadToEndAsync();
        }

        // Chunk text
        var chunks = Utilities.SplitIntoChunks(text);

        // Send chunks for embeddings
        foreach (var chunk in chunks)
        {
            var embedding = await _embeddingClient.GetEmbeddingAsync(chunk);
            // TODO: do something with the embedding
        }

        var resp = req.CreateResponse(HttpStatusCode.OK);
        await resp.WriteStringAsync("Document uploaded and embedding process triggered.");
        return resp;
    }

    private static async Task<HttpResponseData> BadRequest(HttpRequestData req, string message)
    {
        var resp = req.CreateResponse(HttpStatusCode.BadRequest);
        await resp.WriteStringAsync(message);
        return resp;
    }
}
