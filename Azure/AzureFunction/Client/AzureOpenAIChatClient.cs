using Azure;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Configuration;
using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Text;

namespace AzureFunction.Client
{
    public sealed class AzureOpenAIChatClient : IChatCompletionClient
    {
        private readonly AzureOpenAIClient _client;
        private readonly string _deploymentName;

        public AzureOpenAIChatClient(
            AzureOpenAIClient client,
            IConfiguration config)
        {   
            _client = client;
            _deploymentName = config["OpenAI:DeploymentName"]!;
        }

        public async Task<string> GetAnswerAsync(
            string systemPrompt,
            string userPrompt,
            CancellationToken cancellationToken = default)
        {
            var chatClient = _client.GetChatClient(_deploymentName);

            ChatMessage[] messages =
            {
                new SystemChatMessage(systemPrompt),
                new UserChatMessage(userPrompt)
            };

            var options = new ChatCompletionOptions
            {
                Temperature = 0.0f,      // Lower = more deterministic
                MaxOutputTokenCount = 512,   // Limit response size
            };

            var completionResult =
                await chatClient.CompleteChatAsync(messages, options, cancellationToken);

            ChatCompletion completion = completionResult.Value;

            return completion.Content.Count > 0
                ? completion.Content[0].Text
                : string.Empty;
        }
    }
}
