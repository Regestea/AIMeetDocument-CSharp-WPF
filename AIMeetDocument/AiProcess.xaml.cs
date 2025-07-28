using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using AIMeetDocument.Services;
using Microsoft.Win32;
using System.Threading;
using System.Threading.Tasks;
using AIMeetDocument.Enums;
using AIMeetDocument.StaticValues;

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

            string audioLanguage = ((ComboBoxItem)AudioLanguageCombo.SelectedItem).Tag.ToString()!;
            string outputLanguage = ((ComboBoxItem)OutputLanguageCombo.SelectedItem).Tag.ToString()!;
            string fileType = ((ComboBoxItem)FileTypeCombo.SelectedItem).Content.ToString()!;
            string userPrompt = UserPromptTextBox.Text;
            string location = LocationTextBox.Text;
            string audioSubject = AudioSubjectTextBox.Text;

            var audioPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AudioCache", _fileName);
            if (File.Exists(audioPath))
            {
                _cts = new CancellationTokenSource();
                var resultContent = await StartProcess(userPrompt, audioPath, audioSubject, audioLanguage,
                    outputLanguage, _cts.Token);
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
                    
                    var textDirection = TextDirection.LTR;
                    if (outputLanguage is "fa" or "ar")
                    {
                        textDirection = TextDirection.RTL;
                    }
                    switch (fileType)
                    {
                        case "MD":
                            var markdownService = new MarkdownToMdFileService();
                            string outputFilePath = Path.Combine(location, $"{fileName}.md");
                            markdownService.SaveMarkdownToFile(resultContent, outputFilePath);
                            MessageBox.Show($"Markdown file saved to {outputFilePath}");
                            break;
                        case "Word":
                            var wordService = new MarkdownToWordService();
                            string wordOutputPath = Path.Combine(location, $"{fileName}.docx");
                            wordService.ConvertMarkdownStringToDocx(resultContent, wordOutputPath,textDirection);
                            MessageBox.Show($"Word document saved to {wordOutputPath}");
                            break;
                        case "PDF":
                            var pdfService = new MarkdownToPdfService();
                            string pdfOutputPath = Path.Combine(location, $"{fileName}.pdf");
                            pdfService.ConvertMarkdownStringToPdf(resultContent, pdfOutputPath,textDirection);
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

        private async Task<string> StartProcess(string userPrompt, string audioFilePath, string audioSubject,
            string audioLanguage, string outputLanguage,
            CancellationToken cancellationToken)
        {
            string modelPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LLM", "ggml-large-v3-turbo.bin");
            var textPartList = new List<string>();

            var audioCutService = new AudioCutService();
            var AudioAnalysisService = new AudioAnalysisService();
            var silenceSeconds = AudioAnalysisService.GetAvgMinThreshold(audioFilePath);
            var secondPeaks = AudioAnalysisService.GetSilenceSeconds(audioFilePath, 3, silenceSeconds);
            var audioPartsPath = audioCutService.CutAudioBySeconds(audioFilePath, secondPeaks);

            using (var whisperService = new WhisperService(modelPath))
            {
                foreach (var audioPartPath in audioPartsPath)
                {
                    var text = await whisperService.TranscribeAsync(audioPartPath, audioLanguage, cancellationToken);
                    textPartList.Add(text);
                }
            }

            audioCutService.CleanAudioChunksCache();


            var systemPrompt = new SystemPromptBuilder(audioSubject, outputLanguage, userPrompt);
            var settingsService = new SettingsService();
            var settings = settingsService.GetSettings();
            var fullText = new StringBuilder();
            if (settings.DefaultAI == DefaultAI.LLMStudio)
            {
                var llm = new LocalLanguageModelService();
                foreach (var text in textPartList)
                {
                    var llmResult =
                        await llm.GetChatCompletionAsync(systemPrompt.DefaultSystemPrompt, text, cancellationToken);
                    fullText.Append(llmResult);
                }
            }
            else
            {
                var gemini = new GeminiService();
                foreach (var text in textPartList)
                {
                    var geminiResult =
                        await gemini.GetChatCompletionAsync(systemPrompt.DefaultSystemPrompt + text, cancellationToken);
                    fullText.Append(geminiResult);
                    await Task.Delay(10000, cancellationToken);
                }
            }


            var finalText = fullText.ToString();

            return finalText;
        }
    }
}