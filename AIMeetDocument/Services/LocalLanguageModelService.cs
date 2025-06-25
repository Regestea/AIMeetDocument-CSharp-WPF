using System;
using System.Collections.Generic;
using System.ClientModel;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using OpenAI;
using OpenAI.Chat;

namespace AIMeetDocument.Services
{
    public class LocalLanguageModelService
    {
        private readonly OpenAIClient _client;

        public LocalLanguageModelService(string serverUrl, string apiKey = "not_needed")
        {
            if (string.IsNullOrWhiteSpace(serverUrl))
            {
                throw new ArgumentException("Server URL cannot be null or whitespace.", nameof(serverUrl));
            }
            
            var options = new OpenAIClientOptions
            {
                Endpoint = new Uri(serverUrl)
            };
            
            _client = new OpenAIClient(new ApiKeyCredential(apiKey), options);
        }

        public async Task<string> GetChatCompletionAsync(string userPrompt, string systemPrompt, string model, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(model))
            {
                throw new ArgumentException("Model name cannot be null or whitespace.", nameof(model));
            }

            try
            {
                cancellationToken.ThrowIfCancellationRequested();

                var chatClient = _client.GetChatClient(model);
                
                var messages = new List<ChatMessage>
                {
                    new SystemChatMessage(systemPrompt),
                    new UserChatMessage(userPrompt)
                };
                
                // Pass the CancellationToken to the asynchronous API call.
                var response = await chatClient.CompleteChatAsync(messages, cancellationToken: cancellationToken);

                if (response != null && response.Value.Content.Count > 0)
                {
                    string output = response.Value.Content[0].Text;
                    
                    output = Regex.Replace(output, @"<think>[\s\S]*?<\/think>", string.Empty, RegexOptions.IgnoreCase);
                    output = Regex.Replace(output, @"<think>[\s\S]*?<think\/>", string.Empty, RegexOptions.IgnoreCase);

                    return output.Trim();
                }
                
                return string.Empty;
            }
            // Catch the specific exception for when the task is canceled.
            catch (OperationCanceledException)
            {
                Console.WriteLine("Chat completion was canceled.");
                throw; // Re-throw the exception so the caller knows cancellation occurred.
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                throw;
            }
        }
    }
}
