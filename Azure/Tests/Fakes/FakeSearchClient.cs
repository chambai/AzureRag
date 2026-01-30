using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunction.Client
{
    internal sealed class FakeSearchClient : ISearchClient
    {
        public Task<IReadOnlyList<string>> SearchByVectorAsync(
            float[] vector,
            int k = 5,
            CancellationToken ct = default)
        {
            return Task.FromResult<IReadOnlyList<string>>(new[]
            {
            "This is a fake document chunk for testing.",
            "Another test chunk."
        });
        }
    }
}
