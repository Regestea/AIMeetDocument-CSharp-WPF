using System.IO;
using System.Windows;
using System.Windows.Controls;
using AIMeetDocument.Services;
using Microsoft.Win32;

namespace AIMeetDocument
{
    public partial class AiProcess : UserControl
    {
        private string _fileName = "FileName.txt";

        public AiProcess()
        {
            InitializeComponent();
            FileNameText.Text = _fileName;
            // Set default save location to Desktop
            LocationTextBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            StartButton.Click += StartButton_Click;
            CancelButton.Click += CancelButton_Click;
            BrowseButton.Click += BrowseButton_Click;
        }

        public void SetFileName(string fileName)
        {
            _fileName = fileName;
            FileNameText.Text = fileName;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            string language = ((ComboBoxItem)LanguageCombo.SelectedItem).Tag.ToString();
            string fileType = ((ComboBoxItem)FileTypeCombo.SelectedItem).Content.ToString();
            string location = LocationTextBox.Text;
            string msg =
                $"File Name: {_fileName}\nLanguage: {language}\nFile Type: {fileType}\nSave Location: {location}";
            MessageBox.Show(msg, "Form Data");
            ActionPanel.Visibility = Visibility.Collapsed;
            LoadingPanel.Visibility = Visibility.Visible;
            var pp = AppDomain.CurrentDomain.BaseDirectory;
            var audioPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AudioCache", _fileName);
            if (File.Exists(audioPath))
            {
                _ = StartProcess(audioPath, language);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Process cancelled.", "Cancel");
            ActionPanel.Visibility = Visibility.Visible;
            LoadingPanel.Visibility = Visibility.Collapsed;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.CheckFileExists = false;
            dialog.CheckPathExists = true;
            dialog.ValidateNames = false;
            dialog.FileName = "Select this folder";
            if (dialog.ShowDialog() == true)
            {
                var path = System.IO.Path.GetDirectoryName(dialog.FileName);
                LocationTextBox.Text = path;
            }
        }

        private async Task StartProcess(string audioFilePath,string language)
        {
            string modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,"LLM", "ggml-large-v3.bin");
            var whisperService = new WhisperService(modelPath);

            var fullText =await whisperService.TranscribeAsync(audioFilePath, language);
        }
    }
}