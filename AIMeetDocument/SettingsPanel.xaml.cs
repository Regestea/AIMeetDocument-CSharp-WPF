using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.Configuration;

namespace AIMeetDocument
{
    public partial class SettingsPanel : UserControl
    {
        public event RoutedEventHandler BackClicked;
        private const string AppSettingsPath = "appsettings.json";

        public SettingsPanel()
        {
            InitializeComponent();
            Loaded += UserControl_Loaded;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

            IConfiguration configuration = builder.Build();

            var defaultAI = configuration["DefaultAI"];
            var systemPrompt = configuration["SystemPrompt"];
            var llmServerUrl = configuration["LLMStudio:ServerUrl"];
            var llmModel = configuration["LLMStudio:Model"];
            var geminiApiKey = configuration["Gemini:ApiKey"];
            var geminiModel = configuration["Gemini:Model"];

            // Set DefaultAI radio button
            if (defaultAI == "Gemini")
            {
                GeminiRadioButton.IsChecked = true;
            }
            else
            {
                LLMStudioRadioButton.IsChecked = true;
            }

            // Set LLMStudio fields
            ServerUrlTextBox.Text = llmServerUrl;
            LLMStudioModelTextBox.Text = llmModel;

            // Set Gemini fields
            ApiKeyTextBox.Text = geminiApiKey;
            GeminiModelTextBox.Text = geminiModel;

            // Set SystemPrompt
            SystemPromptTextBox.Text = systemPrompt;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var settings = new
            {
                DefaultAI = GeminiRadioButton.IsChecked == true ? "Gemini" : "LLMStudio",
                SystemPrompt = SystemPromptTextBox.Text,
                Gemini = new
                {
                    ApiKey = ApiKeyTextBox.Text,
                    Model = GeminiModelTextBox.Text
                },
                LLMStudio = new
                {
                    ServerUrl = ServerUrlTextBox.Text,
                    Model = LLMStudioModelTextBox.Text
                }
            };
            var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
            File.WriteAllText(AppSettingsPath, json);
            MessageBox.Show("Settings saved.");
        }
        
        private void AIProviderRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            UpdatePanelVisibility();
        }

        private void UpdatePanelVisibility()
        {
            if (GeminiRadioButton.IsChecked == true)
            {
                GeminiPanel.Visibility = Visibility.Visible;
                LLMStudioPanel.Visibility = Visibility.Collapsed;
            }
            else
            {
                GeminiPanel.Visibility = Visibility.Collapsed;
                LLMStudioPanel.Visibility = Visibility.Visible;
            }
        }
    }
}
