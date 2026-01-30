using Azure;
using Azure.AI.OpenAI;
using Azure.AI.OpenAI.Chat;
using Azure.Search.Documents;
using AzureFunction.Client;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using OpenAI.Embeddings;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;

public class QueryFunction
{
    private readonly ILogger _logger;
    private readonly IConfig _config;
    private readonly IEmbeddingClient _embeddingClient;
    private readonly ISearchClient _searchClient;
    private readonly IChatCompletionClient _chatClient;

    public QueryFunction(
        ILoggerFactory loggerFactory,
        IConfig config,
        IEmbeddingClient embeddingClient,
        ISearchClient searchClient,
        IChatCompletionClient chatClient)
    {
        _logger = loggerFactory.CreateLogger<QueryFunction>();
        _config = config;
        _embeddingClient = embeddingClient;
        _searchClient = searchClient;
        _chatClient = chatClient;
    }

    [Function("QueryDocument")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "query")] HttpRequestData req, FunctionContext context)
    {
        CancellationToken ct = context.CancellationToken;

        // read query parameter
        string question = req.Query["q"];

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
        IReadOnlyList<string> topChunks =
            await _searchClient.SearchByVectorAsync(
            queryVector,
            k: 5);

        // Build LLM prompt
        string prompt =
        "Answer the question using ONLY the information below.\n\n" +
        string.Join("\n", topChunks) +
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

// Models for deserialization
public class VectorResponse
{
    public float[][] Vectors { get; set; }
}

public class SearchResponse
{
    public SearchDocument[] Value { get; set; }
}

public class SearchDocument
{
    public string Content { get; set; }
}
