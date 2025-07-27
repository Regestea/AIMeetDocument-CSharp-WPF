using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using AIMeetDocument.DTOs;
using AIMeetDocument.Enums;
using AIMeetDocument.Services;
using Microsoft.Extensions.Configuration;

namespace AIMeetDocument
{
    public partial class SettingsPanel : UserControl
    {
        public event RoutedEventHandler BackClicked;


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
            var settingsService = new SettingsService();
            var settings = settingsService.GetSettings();

            // Set the DefaultAI radio button
            if (settings.DefaultAI == DefaultAI.Gemini)
            {
                GeminiRadioButton.IsChecked = true;
            }
            else
            {
                LLMStudioRadioButton.IsChecked = true;
            }

            // Set LLMStudio fields
            ServerUrlTextBox.Text = settings.LLMStudio.ServerUrl;
            LLMStudioModelTextBox.Text = settings.LLMStudio.Model;

            // Set Gemini fields
            ApiKeyTextBox.Text = settings.Gemini.ApiKey;

            GeminiModelComboBox.SelectedIndex = settings.Gemini.Model switch
            {
                "Gemini-2.5-Flash" => 0,
                "Gemini-2.5-Pro" => 1,
                _ => -1 // Default to no selection if model is not recognized
            };
            // Set SystemPrompt
            SystemPromptTextBox.Text = settings.SystemPrompt;
            ;
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var settingsService = new SettingsService();
            var settings = new Settings()
            {
                DefaultAI = GeminiRadioButton.IsChecked == true ? DefaultAI.Gemini : DefaultAI.LLMStudio,
                SystemPrompt = SystemPromptTextBox.Text,
                Gemini = new GeminiSettings()
                {
                    ApiKey = ApiKeyTextBox.Text,
                    Model = (GeminiModelComboBox.SelectedItem as ComboBoxItem)?.Content.ToString()
                },
                LLMStudio = new LLMStudioSettings()
                {
                    ServerUrl = ServerUrlTextBox.Text,
                    Model = LLMStudioModelTextBox.Text
                },
            };
            settingsService.SaveSettings(settings);

            MessageBox.Show("Settings saved.");
        }

        private void ResetDefault_Click(object sender, RoutedEventArgs e)
        {
            var settingsService = new SettingsService();
            settingsService.ResetSettings();
            LoadSettings();
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