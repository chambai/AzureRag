namespace AzureFunction.Config
{
    public class Settings
    {
        public Blob Blob { get; set; } = new();
        public Gpu Gpu { get; set; } = new();
        public AiSearch AiSearch { get; set; } = new();
        public Chat Chat { get; set; } = new();
    }

    public class Blob
    {
        public string AzureConnectionString { get; set; } = string.Empty;
    }

    public class Gpu
    {
        public string ServiceName { get; set; } = string.Empty;
        public string LocalEmbeddingUrl { get; set; } = string.Empty;
    }

    public class AiSearch
    {
        public string AzureSearchEndpoint { get; set; } = string.Empty;
        public string AzureSearchApiKey { get; set; } = string.Empty;
        public string AzureSearchIndexName { get; set; } = string.Empty;
        public string AzureSearchApiVersion { get; set; } = string.Empty;
    }

    public class Chat
    {
        public string AzureOpenAiEndpoint { get; set; } = string.Empty;
        public string AzureOpenAiApiKey { get; set; } = string.Empty;
        public string AzureOpenAiDeploymentName { get; set; } = string.Empty;
    }
}
