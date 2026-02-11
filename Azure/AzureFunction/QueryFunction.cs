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
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "query")] HttpRequestData req, FunctionContext context)
    {
        // read query parameter
        string? question = req.Query["q"];

        if (string.IsNullOrEmpty(question))
        {
            var badResp = req.CreateResponse(HttpStatusCode.BadRequest);
            await badResp.WriteStringAsync("Missing query parameter 'q'.");
            return badResp;
        }
        _logger.LogInformation("Received query: {Question}", question);

        // Create embedding for the question
        float[] queryVector = await _embeddingClient.GetEmbeddingAsync(question);

        // vector search
        List<SearchDocumentChunk> topChunks =
            await _searchClient.SearchByVectorAsync(
            queryVector,
            k: 5,
            context.CancellationToken);

        // Build LLM prompt
        string prompt =
        "Answer the question using ONLY the information below.\n\n" +
        string.Join("\n", topChunks.Select(t => t.Content)) +
        "\n\nQuestion: " + question;


        // Ask Chat client
        string answer = await _chatClient.GetAnswerAsync(
        systemPrompt: "You are a helpful assistant.",
        userPrompt: prompt,
        cancellationToken: context.CancellationToken);


        // Return result
        var response = req.CreateResponse(HttpStatusCode.OK);
        await response.WriteStringAsync(answer);
        return response;
    }
}


