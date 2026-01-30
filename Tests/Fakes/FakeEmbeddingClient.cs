using AzureFunction.Client;

internal sealed class FakeEmbeddingClient : IEmbeddingClient
{
    public Task<float[]> GetEmbeddingAsync(string text, CancellationToken cancellationToken = default)
    {
        // Deterministic fake vector
        return Task.FromResult(new float[] { 0.1f, 0.2f, 0.3f });
    }
}
