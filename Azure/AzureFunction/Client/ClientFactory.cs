using Azure;
using Azure.AI.OpenAI;
using Azure.Storage.Blobs;
using AzureFunction.Client;
using Azure.Search.Documents;
using Microsoft.Extensions.Options;
using AzureFunction.Config;

public class ClientFactory : IClientFactory
{
    private readonly Settings _settings;
    private readonly IHttpClientFactory _httpClientFactory;

    public ClientFactory(IOptions<Settings> options, IHttpClientFactory httpClientFactory)
    {
        _settings = options.Value;
        _httpClientFactory = httpClientFactory;
    }

    public IBlobStorage CreateBlobStorageClient()
    {
        var blobServiceClient = new BlobServiceClient(_settings.Blob.AzureConnectionString);
        return new AzureBlobStorage(blobServiceClient);
    }

    public IEmbeddingClient CreateGpuClient()
    {
        var httpClient = _httpClientFactory.CreateClient(_settings.Gpu.ServiceName);
        return new GpuEmbeddingClient(httpClient, _settings.Gpu.LocalEmbeddingUrl);
    }

    public IAiSearchClient CreateAISearchClient()
    {
        var endpoint = _settings.AiSearch.AzureSearchEndpoint;
        var apiKey = _settings.AiSearch.AzureSearchApiKey;
        var indexName = _settings.AiSearch.AzureSearchIndexName;
        var apiVersion = _settings.AiSearch.AzureSearchApiVersion;

        var client = new SearchClient(
            new Uri(endpoint),
            indexName,
            new AzureKeyCredential(apiKey)
        );

        return new AzureAISearchClient(client);
    }

    public IChatCompletionClient CreateChatClient()
    {
        AzureOpenAIClient client = new(
            new Uri(_settings.Chat.AzureOpenAiEndpoint),
            new AzureKeyCredential(_settings.Chat.AzureOpenAiApiKey));

        return new AzureOpenAIChatClient(client, _settings.Chat.AzureOpenAiDeploymentName);
    }

    
}
