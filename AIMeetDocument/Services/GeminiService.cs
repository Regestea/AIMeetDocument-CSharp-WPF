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
        try
        {
            var model = new GenerativeModel(model: _settings.Gemini.Model, apiKey: _settings.Gemini.ApiKey);

            var response = await model.GenerateContentAsync(prompt, cancellationToken);


            return response.Text;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"An error occurred: {ex.Message}");
        }

        return null;
    }
}