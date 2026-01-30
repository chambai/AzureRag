using System;
using System.Collections.Generic;
using System.Text;

namespace Tests.Fakes
{
    internal sealed class FakeConfig : IConfig
    {
        public string GpuEmbeddingUrl => "http://fake-gpu";
        public string SearchEndpoint => "https://fake-search";
        public string SearchApiKey => "fake-key";
        public string SearchIndexName => "documents";
        public string SearchApiVersion => "2023-07-01-Preview";
        public string OpenAIEndpoint => "https://fake-openai";
        public string OpenAIApiKey => "fake-key";
        public string OpenAIDeploymentName => "fake-deployment";
    }
}
