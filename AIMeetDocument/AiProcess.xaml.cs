using System.IO;
using System.Windows;
using System.Windows.Controls;
using AIMeetDocument.Services;
using Microsoft.Win32;
using System.Threading;
using System.Threading.Tasks;

namespace AIMeetDocument
{
    public partial class AiProcess : UserControl
    {
        private string _fileName = "FileName.txt";
        private CancellationTokenSource _cts;

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
            ActionPanel.Visibility = Visibility.Collapsed;
            LoadingPanel.Visibility = Visibility.Visible;

            string language = ((ComboBoxItem)LanguageCombo.SelectedItem).Tag.ToString()!;
            string fileType = ((ComboBoxItem)FileTypeCombo.SelectedItem).Content.ToString()!;
            string userPrompt = UserPromptTextBox.Text;
            string location = LocationTextBox.Text;

            string msg =
                $"File Name: {_fileName}\nLanguage: {language}\nFile Type: {fileType}\nSave Location: {location}\nPrompt: {userPrompt}";

            var audioPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AudioCache", _fileName);
            if (File.Exists(audioPath))
            {
                _cts = new CancellationTokenSource();
                _ = StartProcess(userPrompt, audioPath, language!, _cts.Token);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
            ActionPanel.Visibility = Visibility.Visible;
            LoadingPanel.Visibility = Visibility.Collapsed;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                CheckFileExists = false,
                CheckPathExists = true,
                ValidateNames = false,
                FileName = "Select this folder"
            };
            if (dialog.ShowDialog() == true)
            {
                var path = System.IO.Path.GetDirectoryName(dialog.FileName);
                LocationTextBox.Text = path;
            }
        }

        private async Task StartProcess(string userPrompt,string audioFilePath, string language, CancellationToken cancellationToken)
        {
            string modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LLM", "ggml-large-v3.bin");
            string fullText = "";
            
            using (var whisperService = new WhisperService(modelPath))
            {
                fullText = await whisperService.TranscribeAsync(audioFilePath, language, cancellationToken);
            }

            var llm = new LocalLanguageModelService();
            var aIResult= await llm.GetChatCompletionAsync(userPrompt+fullText,cancellationToken);
        }
    }
}