using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunction.Client
{
    public interface ISearchClient
    {
        Task<IReadOnlyList<string>> SearchByVectorAsync(
        float[] vector,
        int k,
        CancellationToken cancellationToken = default);
    }
}
