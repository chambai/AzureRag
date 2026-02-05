using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunction.Client
{
    public interface IAiSearchClient
    {
        Task StoreVectorAsync(
            float[] embeddingVector,
            string chunkText,
            string source,
            CancellationToken cancellationToken = default);

        Task<List<SearchDocumentChunk>> SearchByVectorAsync(
            float[] vector,
            int k,
            CancellationToken cancellationToken = default);
    }
}
