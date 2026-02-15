using AzureFunction.Client;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System.Net;

public class QueryFunction
{
    private readonly ILogger _logger;
    private readonly IEmbeddingClient _embeddingClient;
    private readonly IAiSearchClient _searchClient;
    private readonly IChatCompletionClient _chatClient;

    public QueryFunction(
        ILoggerFactory loggerFactory,
        IEmbeddingClient embeddingClient,
        IAiSearchClient searchClient,
        IChatCompletionClient chatClient)
    {
        _logger = loggerFactory.CreateLogger<QueryFunction>();
        _embeddingClient = embeddingClient;
        _searchClient = searchClient;
        _chatClient = chatClient;
    }

    [Function("QueryDocument")]
    public async Task<HttpResponseData> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "query")]
        HttpRequestData req,
        FunctionContext context)
    {
        string? question = ExtractQuestion(req);

        if (string.IsNullOrWhiteSpace(question))
            return await CreateBadRequest(req);

        try
        {
            string answer = await ProcessQueryAsync(
                question,
                context.CancellationToken);

            return await CreateOkResponse(req, answer);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Request was cancelled.");
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception.");
            var response = req.CreateResponse(HttpStatusCode.InternalServerError);
            await response.WriteStringAsync("An unexpected error occurred.");
            return response;
        }
    }

    internal string? ExtractQuestion(HttpRequestData req)
        => req.Query["q"];

    internal async Task<string> ProcessQueryAsync(
        string question,
        CancellationToken cancellationToken)
    {
        _logger.LogInformation("Received query: {Question}", question);

        float[] queryVector =
            await _embeddingClient.GetEmbeddingAsync(question);

        List<SearchDocumentChunk> topChunks =
            await _searchClient.SearchByVectorAsync(
                queryVector,
                5,
                cancellationToken);

        string prompt = BuildPrompt(question, topChunks);

        return await _chatClient.GetAnswerAsync(
            systemPrompt: "You are a helpful assistant.",
            userPrompt: prompt,
            cancellationToken: cancellationToken);
    }

    public string BuildPrompt(
        string question,
        IEnumerable<SearchDocumentChunk> chunks)
    {
        return
            "Answer the question using ONLY the information below.\n\n" +
            string.Join("\n", chunks.Select(c => c.Content)) +
            "\n\nQuestion: " + question;
    }

    private static async Task<HttpResponseData> CreateBadRequest(HttpRequestData req)
    {
        var response = req.CreateResponse(HttpStatusCode.BadRequest);
        await response.WriteStringAsync("Missing query parameter 'q'.");
        return response;
    }

    private static async Task<HttpResponseData> CreateOkResponse(
        HttpRequestData req,
        string body)
    {
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync(body);
        return response;
    }
}


