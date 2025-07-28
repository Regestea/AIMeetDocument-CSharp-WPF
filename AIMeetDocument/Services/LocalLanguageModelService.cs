using System;
using System.Collections.Generic;
using System.ClientModel;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using AIMeetDocument.DTOs;
using Microsoft.Extensions.Configuration;
using OpenAI;
using OpenAI.Chat;

namespace AIMeetDocument.Services
{
    public class LocalLanguageModelService
    {
        private readonly OpenAIClient _client;
        private readonly Settings _settings;

        public LocalLanguageModelService()
        {
            var settingsService = new SettingsService();
            _settings = settingsService.GetSettings();

            var options = new OpenAIClientOptions
            {
                Endpoint = new Uri(_settings.LLMStudio.ServerUrl),
                // Set the network timeout to 1 day
                NetworkTimeout = TimeSpan.FromDays(1)
            };

            _client = new OpenAIClient(new ApiKeyCredential("no need"), options);
        }

        public async Task<string> GetChatCompletionAsync(string systemPrompt,string message, CancellationToken cancellationToken = default)
        {
            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var chatClient = _client.GetChatClient(_settings.LLMStudio.Model);

                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage(systemPrompt),
                    new UserChatMessage(message)
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