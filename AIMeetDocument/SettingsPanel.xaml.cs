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
            GeminiRequestPerMinuteTextBox.Text = settings.Gemini.RequestPerMinute.ToString();

            GeminiModelComboBox.SelectedIndex = settings.Gemini.Model switch
            {
                "gemini-2.5-flash" => 0,
                "gemini-2.5-pro" => 1,
                "gemini-2.5-flash-lite" => 2,
                _ => -1 // Default to no selection if model is not recognized
            };
            
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            var settingsService = new SettingsService();
            var settings = new Settings()
            {
                DefaultAI = GeminiRadioButton.IsChecked == true ? DefaultAI.Gemini : DefaultAI.LLMStudio,
                Gemini = new GeminiSettings()
                {
                    ApiKey = ApiKeyTextBox.Text,
                    Model = (GeminiModelComboBox.SelectedItem as ComboBoxItem)?.Content.ToString(),
                    RequestPerMinute = int.TryParse(GeminiRequestPerMinuteTextBox.Text, out var rpm) ? rpm : 0
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

        private void NumberOnly_PreviewTextInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Allow only digits
            e.Handled = !int.TryParse(e.Text, out _);
        }
    }
}