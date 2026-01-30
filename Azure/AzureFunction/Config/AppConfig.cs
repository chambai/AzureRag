using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunction.Config
{
    public sealed class AppConfig : IConfig
    {
        public string GpuEmbeddingUrl =>
            Get("GPU_EMBEDDING_URL");

        public string SearchEndpoint =>
            Get("SEARCH_ENDPOINT");

        public string SearchApiKey =>
            Get("SEARCH_API_KEY");

        public string SearchIndexName =>
            Get("SEARCH_INDEX_NAME");

        public string SearchApiVersion =>
            Get("SEARCH_API_VERSION");

        public string OpenAIEndpoint =>
            Get("OPENAI_ENDPOINT");

        public string OpenAIApiKey =>
            Get("OPENAI_API_KEY");

        public string OpenAIDeploymentName =>
            Get("OPENAI_DEPLOYMENT_NAME");

        private static string Get(string key, string? defaultValue = null)
        {
            var value = Environment.GetEnvironmentVariable(key);
            if (!string.IsNullOrEmpty(value))
                return value;

            if (defaultValue != null)
                return defaultValue;

            throw new InvalidOperationException(
                $"Missing required configuration value: {key}");
        }
    }
}
