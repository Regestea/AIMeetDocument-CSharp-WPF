using GenerativeAI;
using Microsoft.Extensions.Configuration;

namespace AIMeetDocument.Services;

public class GeminiService
{
    private readonly string _defaultSystemPrompt;
    private readonly string _apiKey;

    public GeminiService()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

        IConfiguration configuration = builder.Build();

        _apiKey = configuration["LocalLanguageModelStudio:ApiKey"] ?? throw new InvalidOperationException("set a valid api key in settings");
        _defaultSystemPrompt = configuration["LocalLanguageModelStudio:SystemPrompt"] ??
                               throw new InvalidOperationException("no prompt found in app settings for system");
    }

    public async Task<string?> GetChatCompletionAsync(string? userPrompt, CancellationToken cancellationToken = default)
    {
        try
        {
            // Create the generative model, specifying the model and API key
            var model = new GenerativeModel(model: "gemini-2.5-flash", apiKey: _apiKey);

            var response = await model.GenerateContentAsync(_defaultSystemPrompt + userPrompt, cancellationToken);


            return response.Text;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred: {ex.Message}");
        }

        return null;
    }
}