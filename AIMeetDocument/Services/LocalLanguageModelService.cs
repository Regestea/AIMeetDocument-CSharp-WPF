using System;
using System.Collections.Generic;
using System.ClientModel;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;

namespace AIMeetDocument.Services
{
    public class LocalLanguageModelService
    {
        private readonly OpenAIClient _client;
        private readonly string _defaultSystemPrompt;
        private readonly string _defaultModel;

        public LocalLanguageModelService()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfiguration configuration = builder.Build();

            var serverUrl = configuration["LLMStudio:ServerUrl"];
            
            _defaultSystemPrompt = configuration["SystemPrompt"] ??
                                   throw new InvalidOperationException("no prompt found in app settings for system");
            _defaultModel = configuration["LLMStudio:Model"] ??
                            throw new InvalidOperationException("no model found in app settings");

            if (string.IsNullOrWhiteSpace(serverUrl))
            {
                throw new ArgumentException("Server URL cannot be null or whitespace.", nameof(serverUrl));
            }

            var options = new OpenAIClientOptions
            {
                Endpoint = new Uri(serverUrl),
                // Set the network timeout to 1 day
                NetworkTimeout = TimeSpan.FromDays(1)
            };

            _client = new OpenAIClient(new ApiKeyCredential("no need"), options);
        }

        public async Task<string> GetChatCompletionAsync(string? userPrompt, CancellationToken cancellationToken = default)
        {
          

            if (string.IsNullOrWhiteSpace(_defaultModel))
            {
                throw new ArgumentException("Model name cannot be null or whitespace.", nameof(_defaultModel));
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var chatClient = _client.GetChatClient(_defaultModel);

                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage(_defaultSystemPrompt),
                    new UserChatMessage(userPrompt)
                };

                var response = await chatClient.CompleteChatAsync(messages, cancellationToken: cancellationToken);

                if (response != null && response.Value.Content.Count > 0)
                {
                    string output = response.Value.Content[0].Text;

                    output = Regex.Replace(output, @"<think>[\s\S]*?</think>", string.Empty, RegexOptions.IgnoreCase);
                    output = Regex.Replace(output, @"<think>[\s\S]*?<think/>", string.Empty, RegexOptions.IgnoreCase);

                    return output.Trim();
                }

                return string.Empty;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("Chat completion was canceled.");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
    }
}