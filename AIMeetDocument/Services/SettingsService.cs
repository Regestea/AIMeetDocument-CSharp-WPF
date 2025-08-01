﻿using System.IO;
using System.Text.Json;
using AIMeetDocument.DTOs;
using AIMeetDocument.Enums;
using AIMeetDocument.StaticValues;
using Microsoft.Extensions.Configuration;

namespace AIMeetDocument.Services;

public class SettingsService
{
    private readonly IConfiguration configuration;
    private const string AppSettingsPath = "appsettings.json";

    public SettingsService()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
        configuration = builder.Build();
    }


    public Settings GetSettings()
    {
        var settings = new Settings()
        {
            DefaultAI = configuration["DefaultAI"] == "1" ? DefaultAI.Gemini : DefaultAI.LLMStudio,
            Gemini = new GeminiSettings()
            {
                ApiKey = configuration["Gemini:ApiKey"]!,
                Model = configuration["Gemini:Model"]! , 
            },
            LLMStudio = new LLMStudioSettings()
            {
                ServerUrl = configuration["LLMStudio:ServerUrl"]!,
                Model = configuration["LLMStudio:Model"]!
            }
        };
        return settings;
    }

    public void SaveSettings(Settings settings)
    {
        settings.LLMStudio.ServerUrl= settings.LLMStudio.ServerUrl.EndsWith("/v1") ? settings.LLMStudio.ServerUrl : settings.LLMStudio.ServerUrl + "/v1";
        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(AppSettingsPath, json);
    }

    public void ResetSettings()
    {
        var defaultSettings = new Settings
        {
            DefaultAI = DefaultAI.LLMStudio,
            LLMStudio = new LLMStudioSettings
            {
                ServerUrl = "http://127.0.0.1:2355/v1",
                Model = "qwen/qwen3-8b"
            },
            Gemini = new GeminiSettings
            {
                ApiKey = "your-gemini-api-key",
                Model = GeminiModel.Gemini25Flash
            }
        };

        SaveSettings(defaultSettings);
    }
}