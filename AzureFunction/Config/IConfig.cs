using System;
using System.Collections.Generic;
using System.Text;

public interface IConfig
{
    // GPU / embedding
    string GpuEmbeddingUrl { get; }

    // Azure AI Search
    string SearchEndpoint { get; }
    string SearchApiKey { get; }
    string SearchIndexName { get; }
    string SearchApiVersion { get; }

    // Azure OpenAI
    string OpenAIEndpoint { get; }
    string OpenAIApiKey { get; }
    string OpenAIDeploymentName { get; }
}
