using AzureFunction.Client;

internal sealed class FakeSearchClient : IAiSearchClient
{
    public Task StoreVectorAsync(
            float[] embeddingVector,
            string chunkText,
            string source,
            CancellationToken cancellationToken = default)
    {
       return Task.CompletedTask;
    }

    public Task<List<SearchDocumentChunk>> SearchByVectorAsync(
        float[] vector,
        int k,
        CancellationToken cancellationToken = default)
    {
        var fakeChunks = new List<SearchDocumentChunk>
        {
            new SearchDocumentChunk
            {
                Id = Guid.NewGuid().ToString(),
                Content = "Monthly, on the last working day of the month",
                Embedding = new float[] { 0.12f, -0.54f, 0.89f },
                Source = "teacher_contract1.txt"
            },
            new SearchDocumentChunk
            {
                Id = Guid.NewGuid().ToString(),
                Content = "Each month, on the last working day of the month.",
                Embedding = new float[] { 0.45f, 0.22f, -0.11f },
                Source = "teacher_contract2.txt"
            },
            new SearchDocumentChunk
            {
                Id = Guid.NewGuid().ToString(),
                Content = "You are salaried on the last thursday of each month.",
                Embedding = new float[] { -0.33f, 0.78f, 0.05f },
                Source = "teacher_contract3.txt"
            }
        };

        return Task.FromResult(fakeChunks);
    }
}
