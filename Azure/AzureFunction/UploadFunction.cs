using Azure.Search.Documents;
using Azure.Storage.Blobs;
using AzureFunction.Client;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using System.Text.Json;
using HttpMultipartParser;

public class UploadFunction
{
    private readonly ILogger _logger;
    private readonly IBlobStorage _blobServiceClient;
    private readonly IEmbeddingClient _embeddingClient;
    private readonly IAiSearchClient _searchClient;

    public UploadFunction(
        ILoggerFactory loggerFactory,
        IBlobStorage blobServiceClient,
        IEmbeddingClient embeddingClient,
        IAiSearchClient searchClient)
    {
        _logger = loggerFactory.CreateLogger<UploadFunction>();
        _blobServiceClient = blobServiceClient;
        _embeddingClient = embeddingClient;
        _searchClient = searchClient;
    }

    [Function("UploadDocument")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Function, "post", Route = "upload")]
        HttpRequestData req, FunctionContext context)
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

        // Parse the stream
        var parser = await MultipartFormDataParser.ParseAsync(req.Body);

        // Get the file(s)
        var file = parser.Files.FirstOrDefault();

        if (file == null)
        {
            return await BadRequest(req, "No document provided.");
        }

        using var ms = new MemoryStream();
        await file.Data.CopyToAsync(ms);

        if (ms.Length == 0)
        {
            return await BadRequest(req, "No document provided.");
        }

        // reset memory position to zero
        ms.Position = 0;

        // Upload to Blob Storage
        await _blobServiceClient.UploadAsync("documents", fileName, ms);

        ms.Position = 0;

        // Read text
        string text;
        using (var reader = new StreamReader(ms, Encoding.UTF8, leaveOpen: true))
        {
            text = await reader.ReadToEndAsync();
        }

        // Chunk text
        var chunks = Utilities.SplitIntoChunks(text);

        // Send chunks for embeddings
        foreach (var chunk in chunks)
        {
            var embedding = await _embeddingClient.GetEmbeddingAsync(chunk);
            // store the embedding vector
            await _searchClient.StoreVectorAsync(embedding, chunk, fileName, context.CancellationToken);
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
