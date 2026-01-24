using Azure;
using Azure.AI.OpenAI;
using Azure.AI.OpenAI.Chat;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using OpenAI.Chat;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;

public class QueryFunction
{
    private readonly ILogger _logger;
    private readonly HttpClient _httpClient;
    private readonly string gpuServiceUrl = "http://YOUR_GPU_SERVER:8000/embed"; // Linux GPU service
    private readonly string searchEndpoint = "https://YOUR_SEARCH_SERVICE.search.windows.net";
    private readonly string searchApiKey = "YOUR_API_KEY";
    private readonly string indexName = "documents";
    private readonly string apiVersion = "2023-07-01-Preview";
    private readonly AzureOpenAIClient _azureOpenAIClient;

    public QueryFunction(ILoggerFactory loggerFactory)
    {
        _logger = loggerFactory.CreateLogger<QueryFunction>();
        _httpClient = new HttpClient();

        // Initialize AzureOpenAIClient for your Azure OpenAI endpoint
        _azureOpenAIClient = new AzureOpenAIClient(
            new Uri("https://YOUR_OPENAI_ENDPOINT.openai.azure.com/"),
            new AzureKeyCredential("YOUR_API_KEY")
        );
    }

    [Function("QueryDocument")]
    public async Task<HttpResponseData> Run([HttpTrigger(AuthorizationLevel.Function, "get", Route = "query")] HttpRequestData req)
    {
        string question = req.Query["q"];

        if (string.IsNullOrEmpty(question))
        {
            var badResp = req.CreateResponse(HttpStatusCode.BadRequest);
            await badResp.WriteStringAsync("Missing query parameter 'q'.");
            return badResp;
        }

        // 1️⃣ Get embedding from GPU service
        var payload = new { texts = new[] { question } };
        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var gpuResp = await _httpClient.PostAsync(gpuServiceUrl, content);
        gpuResp.EnsureSuccessStatusCode();

        var vectorObj = JsonSerializer.Deserialize<VectorResponse>(await gpuResp.Content.ReadAsStringAsync());
        var queryVector = vectorObj.Vectors[0].Select(f => (float)f).ToArray();

        // 2️⃣ Call Azure Search REST API for vector search
        var searchUrl = $"{searchEndpoint}/indexes/{indexName}/docs/search?api-version={apiVersion}";
        var restPayload = new
        {
            vectorQueries = new[]
            {
                new
                {
                    kind = "vector",
                    vector = queryVector,
                    fields = "contentVector", // Your vector field name
                    k = 5
                }
            },
            select = new[] { "content" }
        };

        var restContent = new StringContent(JsonSerializer.Serialize(restPayload), Encoding.UTF8, "application/json");
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, searchUrl)
        {
            Content = restContent
        };
        requestMessage.Headers.Add("api-key", searchApiKey);

        var searchResp = await _httpClient.SendAsync(requestMessage);
        searchResp.EnsureSuccessStatusCode();

        var searchJson = await searchResp.Content.ReadAsStringAsync();
        var searchResult = JsonSerializer.Deserialize<SearchResponse>(searchJson);
        var topChunks = searchResult.Value.Select(v => v.Content).ToList();

        // 3Build prompt for LLM
        string prompt = "Answer the question based only on the following text:\n" +
                        string.Join("\n", topChunks) +
                        "\nQuestion: " + question;

        // Call Azure OpenAI using AzureOpenAIClient (chat completion)
        var chatClient = _azureOpenAIClient.GetChatClient("deployment-name");


        // Prepare your messages
        ChatMessage[] messages = new ChatMessage[]
        {
            new SystemChatMessage("You are a helpful assistant."),
            new UserChatMessage(prompt)
        };

        // Call chat completion directly
        ChatCompletion response = chatClient.CompleteChat(messages);

        // Extract answer
        string answer = response.Content.Count > 0
            ? response.Content[0].Text
            : "No answer generated.";

        var resp = req.CreateResponse(HttpStatusCode.OK);
        await resp.WriteStringAsync(answer);
        return resp;
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
