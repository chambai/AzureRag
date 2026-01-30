using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace AzureFunction.Client
{
    public sealed class AzureSearchClient : ISearchClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _endpoint;
        private readonly string _apiKey;
        private readonly string _indexName;
        private readonly string _apiVersion;

        public AzureSearchClient(
            HttpClient httpClient,
            IConfiguration config)
        {
            _httpClient = httpClient;
            _endpoint = config["Search:Endpoint"]!;
            _apiKey = config["Search:ApiKey"]!;
            _indexName = config["Search:IndexName"]!;
            _apiVersion = config["Search:ApiVersion"]!;
        }

        public async Task<IReadOnlyList<string>> SearchByVectorAsync(
            float[] vector,
            int k = 5,
            CancellationToken ct = default)
        {
            var url = $"{_endpoint}/indexes/{_indexName}/docs/search?api-version={_apiVersion}";

            var payload = new
            {
                vectorQueries = new[]
                {
                new
                {
                    kind = "vector",
                    vector,
                    fields = "contentVector",
                    k
                }
            },
                select = new[] { "content" }
            };

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent(
                    JsonSerializer.Serialize(payload),
                    Encoding.UTF8,
                    "application/json")
            };

            request.Headers.Add("api-key", _apiKey);

            var response = await _httpClient.SendAsync(request, ct);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(ct);
            var result = JsonSerializer.Deserialize<SearchResponse>(json);

            return result!.Value.Select(v => v.Content).ToList();
        }
    }
}
