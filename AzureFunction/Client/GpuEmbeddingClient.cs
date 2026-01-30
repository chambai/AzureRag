using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace AzureFunction.Client
{
    public sealed class GpuEmbeddingClient : IEmbeddingClient
    {
        private readonly HttpClient _httpClient;
        private readonly string _gpuServiceUrl;

        public GpuEmbeddingClient(HttpClient httpClient, string gpuServiceUrl)
        {
            _httpClient = httpClient;
            _gpuServiceUrl = gpuServiceUrl;
        }

        public async Task<float[]> GetEmbeddingAsync(string text, CancellationToken cancellationToken)
        {
            var payload = new { texts = new[] { text } };
            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync(_gpuServiceUrl, content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var vectorObj = JsonSerializer.Deserialize<VectorResponse>(json, options);
            var queryVector = vectorObj!.Vectors[0].Select(f => (float)f).ToArray();

            return queryVector;
        }
    }
}
