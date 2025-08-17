using System.Windows;
using AIMeetDocument.DTOs;
using GenerativeAI;
using Microsoft.Extensions.Configuration;

namespace AIMeetDocument.Services;

public class GeminiService
{
    private readonly Settings _settings;

    public GeminiService()
    {
        var settingsService = new SettingsService();
        _settings = settingsService.GetSettings();
    }

    public async Task<string?> GetChatCompletionAsync(string prompt, CancellationToken cancellationToken = default)
    {
        int maxRetries = 5;
        int retryCount = 0;
        while (true)
        {
            try
            {
                var model = new GenerativeModel(model: _settings.Gemini.Model, apiKey: _settings.Gemini.ApiKey);

                var response = await model.GenerateContentAsync(prompt, cancellationToken);
                // console log with warning color
                Console.WriteLine("-------------------------------------------------------");
                if (response.Text.Length < 1000)
                {
                    Console.WriteLine("warning");
                    Console.WriteLine(response.Text);
                }
                Console.WriteLine("-------------------------------------------------------");
                
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Passed Prompt Size: " + prompt.Length + " Response Size: " + response.Text.Length);
                Console.ResetColor();
                return response.Text;
            }
            catch (Exception ex)
            {
                retryCount++;
                var errorText = $"An error occurred: {ex.Message} prompt Length: {prompt.Length} (Attempt {retryCount}/{maxRetries})";
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(errorText);
                Console.ResetColor();
                if (retryCount < maxRetries)
                {
                    await Task.Delay(5000, cancellationToken);
                    continue;
                }
               
                var result = MessageBox.Show(errorText + "\nDo you want to retry?", "Error", MessageBoxButton.YesNo, MessageBoxImage.Error);
                if (result == MessageBoxResult.Yes)
                {
                    retryCount = 0;
                    continue;
                }
                else
                {
                    return null;
                }
            }
        }
    }
}