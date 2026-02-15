using AzureFunction.Client;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using HttpMultipartParser;

public class UploadFunction
{
    private readonly ILogger<UploadFunction> _logger;
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
    [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "upload")]
    HttpRequestData req,
    FunctionContext context)
    {
        try
        {
            var validationResult = ValidateRequest(req);
            if (!validationResult.IsValid)
                return await CreateBadRequest(req, validationResult.ErrorMessage!);

            var fileName = validationResult.FileName!;

            MemoryStream? fileStream;
            try
            {
                fileStream = await ExtractFileStreamAsync(req);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to parse multipart request.");
                return await CreateBadRequest(req, "Invalid multipart request.");
            }

            if (fileStream == null)
                return await CreateBadRequest(req, "No document provided.");

            await UploadToBlobAsync(fileName, fileStream);

            string text = await ExtractTextAsync(fileStream);
            var chunks = SplitIntoChunks(text);

            await ProcessChunksAsync(
                chunks,
                fileName,
                context.CancellationToken);

            return await CreateOk(req,
                "Document uploaded and embedding process triggered.");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Upload cancelled.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Upload failed.");
            var resp = req.CreateResponse(HttpStatusCode.InternalServerError);
            await resp.WriteStringAsync("An unexpected error occurred.");
            return resp;
        }
    }


    // Validation
    public (bool IsValid, string? FileName, string? ErrorMessage)
        ValidateRequest(HttpRequestData req)
    {
        if (!req.Headers.TryGetValues("X-Filename", out var filenames))
            return (false, null, "Missing X-Filename header.");

        var fileName = filenames.FirstOrDefault();
        if (string.IsNullOrWhiteSpace(fileName))
            return (false, null, "Invalid filename.");

        if (req.Body == null || req.Body.Length == 0)
            return (false, null, "No document provided.");

        return (true, fileName, null);
    }


    // Multipart extraction
    internal async Task<MemoryStream?> ExtractFileStreamAsync(
        HttpRequestData req)
    {
        var parser = await MultipartFormDataParser.ParseAsync(req.Body);

        var file = parser.Files.FirstOrDefault();
        if (file == null)
            return null;

        var ms = new MemoryStream();
        await file.Data.CopyToAsync(ms);

        if (ms.Length == 0)
            return null;

        ms.Position = 0;
        return ms;
    }

    // blob upload
    internal async Task UploadToBlobAsync(
        string fileName,
        MemoryStream stream)
    {
        stream.Position = 0;
        await _blobServiceClient.UploadAsync(
            "documents",
            fileName,
            stream);
    }

    // Text extraction

    internal async Task<string> ExtractTextAsync(
        MemoryStream stream)
    {
        stream.Position = 0;

        using var reader =
            new StreamReader(stream, Encoding.UTF8, leaveOpen: true);

        return await reader.ReadToEndAsync();
    }

    // chunking
    internal IEnumerable<string> SplitIntoChunks(string text)
        => Utilities.SplitIntoChunks(text);

    // embedding and indexing
    public async Task ProcessChunksAsync(
        IEnumerable<string> chunks,
        string fileName,
        CancellationToken cancellationToken)
    {
        foreach (var chunk in chunks)
        {
            var embedding =
                await _embeddingClient.GetEmbeddingAsync(chunk);

            await _searchClient.StoreVectorAsync(
                embedding,
                chunk,
                fileName,
                cancellationToken);
        }
    }

    // Reponse helpers
    private static async Task<HttpResponseData> CreateBadRequest(
        HttpRequestData req,
        string message)
    {
        var resp = req.CreateResponse(HttpStatusCode.BadRequest);
        await resp.WriteStringAsync(message);
        return resp;
    }

    private static async Task<HttpResponseData> CreateOk(
        HttpRequestData req,
        string message)
    {
        var resp = req.CreateResponse(HttpStatusCode.OK);
        await resp.WriteStringAsync(message);
        return resp;
    }
}
