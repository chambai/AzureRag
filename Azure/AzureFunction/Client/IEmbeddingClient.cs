using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunction.Client
{
    public interface IEmbeddingClient
    {
        Task<float[]> GetEmbeddingAsync(
        string text,
        CancellationToken cancellationToken = default);
    }
}
