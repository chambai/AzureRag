using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunction.Client
{
    public interface IChatCompletionClient
    {
        Task<string> GetAnswerAsync(
        string systemPrompt,
        string userPrompt,
        CancellationToken cancellationToken = default);
    }
}
