using AzureFunction.Client;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tests.Fakes
{
    internal sealed class FakeChatCompletionClient : IChatCompletionClient
    {
        public Task<string> GetAnswerAsync(
            string systemPrompt,
            string userPrompt,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult("This is a fake LLM response.");
        }
    }
}
