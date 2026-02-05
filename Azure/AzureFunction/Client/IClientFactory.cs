using AzureFunction.Client;

public interface IClientFactory
{
    IBlobStorage CreateBlobStorageClient();
    IEmbeddingClient CreateGpuClient();
    IAiSearchClient CreateAISearchClient();
    IChatCompletionClient CreateChatClient();
}
