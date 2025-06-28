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

        private async void StartButton_Click(object sender, RoutedEventArgs e)
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
                var resultContent = await StartProcess(userPrompt, audioPath, language!, _cts.Token);
                if (_cts.IsCancellationRequested)
                {
                    MessageBox.Show("Process was cancelled.");
                    ActionPanel.Visibility = Visibility.Visible;
                    LoadingPanel.Visibility = Visibility.Collapsed;
                    return;
                }
                
                if (!string.IsNullOrEmpty(resultContent))
                {
                    var fileName = Guid.NewGuid();
                    switch (fileType)
                    {
                        case "Markdown":
                            var markdownService = new MarkdownToPdfService();
                            string outputFilePath = Path.Combine(location, $"{fileName}.pdf");
                            markdownService.ConvertMarkdownStringToPdf(resultContent, outputFilePath);
                            MessageBox.Show($"Markdown file saved to {outputFilePath}");
                            break;
                        case "Word":
                            var wordService = new MarkdownToWordService();
                            string wordOutputPath = Path.Combine(location, $"{fileName}.docx");
                            wordService.ConvertMarkdownStringToDocx(resultContent, wordOutputPath);
                            MessageBox.Show($"Word document saved to {wordOutputPath}");
                            break;
                        case "PDF":
                            var pdfService = new MarkdownToPdfService();
                            string pdfOutputPath = Path.Combine(location, $"{fileName}.pdf");
                            pdfService.ConvertMarkdownStringToPdf(resultContent, pdfOutputPath);
                            MessageBox.Show($"PDF file saved to {pdfOutputPath}");
                            break;
                    }
                }
            }
            ActionPanel.Visibility = Visibility.Visible;
            LoadingPanel.Visibility = Visibility.Collapsed;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
            ActionPanel.Visibility = Visibility.Visible;
            LoadingPanel.Visibility = Visibility.Collapsed;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                CheckFileExists = false,
                CheckPathExists = true,
                ValidateNames = false,
                FileName = "Select this folder"
            };
            if (dialog.ShowDialog() == true)
            {
                var path = Path.GetDirectoryName(dialog.FileName);
                LocationTextBox.Text = path;
            }
        }

        private async Task<string> StartProcess(string userPrompt, string audioFilePath, string language,
            CancellationToken cancellationToken)
        {
            string modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LLM", "ggml-large-v3.bin");
            string fullText = "";

            using (var whisperService = new WhisperService(modelPath))
            {
                fullText = await whisperService.TranscribeAsync(audioFilePath, language, cancellationToken);
            }

            var llm = new LocalLanguageModelService();
            return await llm.GetChatCompletionAsync(userPrompt + fullText, cancellationToken);
        }
    }
}