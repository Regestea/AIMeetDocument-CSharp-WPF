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
        private Task _runningTask;

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
            StartButton.IsEnabled = false;

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
                _runningTask = Task.Run(async () =>
                {
                    var resultContent = await StartProcess(userPrompt, audioPath, audioSubject, audioLanguage,
                        outputLanguage, _cts.Token);
                    Dispatcher.Invoke(() =>
                    {
                        if (_cts.IsCancellationRequested)
                        {
                            ActionPanel.Visibility = Visibility.Visible;
                            LoadingPanel.Visibility = Visibility.Collapsed;
                            StartButton.IsEnabled = true;
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
                                    wordService.ConvertMarkdownStringToDocx(resultContent, wordOutputPath,
                                        textDirection);
                                    MessageBox.Show($"Word document saved to {wordOutputPath}");
                                    break;
                                case "PDF":
                                    var pdfService = new MarkdownToPdfService();
                                    string pdfOutputPath = Path.Combine(location, $"{fileName}.pdf");
                                    pdfService.ConvertMarkdownStringToPdf(resultContent, pdfOutputPath, textDirection);
                                    MessageBox.Show($"PDF file saved to {pdfOutputPath}");
                                    break;
                            }
                        }

                        ActionPanel.Visibility = Visibility.Visible;
                        LoadingPanel.Visibility = Visibility.Collapsed;
                        StartButton.IsEnabled = true;
                    });
                });
                await _runningTask;
            }
            else
            {
                ActionPanel.Visibility = Visibility.Visible;
                LoadingPanel.Visibility = Visibility.Collapsed;
                StartButton.IsEnabled = true;
            }
        }

        private async void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            // Hide Cancel button, show Canceling text
            CancelButton.Visibility = Visibility.Collapsed;
            CancelingText.Visibility = Visibility.Visible;
            

            await _cts?.CancelAsync();
            if (_runningTask != null)
            {
                try
                {
                    await _runningTask;
                }
                catch
                {
                    /* ignore */
                }
            }

            // Restore UI state
            CancelingText.Visibility = Visibility.Collapsed;
            CancelButton.Visibility = Visibility.Visible;
            
            ActionPanel.Visibility = Visibility.Visible;
            LoadingPanel.Visibility = Visibility.Collapsed;
            StartButton.IsEnabled = true;
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
            try
            {
                string modelPath =
                    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LLM", "ggml-large-v3-turbo.bin");
                var textPartList = new List<string>();

                var audioCutService = new AudioCutService();
                var AudioAnalysisService = new AudioAnalysisService();
                var silenceSeconds = AudioAnalysisService.GetAvgMinThreshold(audioFilePath, cancellationToken);
                var secondPeaks =
                    AudioAnalysisService.GetSilenceSeconds(audioFilePath, 3, silenceSeconds, cancellationToken);
                var audioPartsPath = audioCutService.CutAudioBySeconds(audioFilePath, secondPeaks, cancellationToken);
                if (cancellationToken.IsCancellationRequested)
                {
                    audioCutService.CleanAudioChunksCache();
                    return string.Empty;
                }

                using (var whisperService = new WhisperService(modelPath, audioLanguage))
                {
                    Dispatcher.Invoke(() => {
                        WhisperProgressBar.Visibility = Visibility.Visible;
                        WhisperProgressText.Visibility = Visibility.Visible;
                        GeminiProgressText.Visibility = Visibility.Collapsed;
                        WhisperProgressBar.Value = 0;
                        WhisperProgressText.Text = "0%";
                    });
                    int totalParts = audioPartsPath.Count;
                    for (int i = 0; i < totalParts; i++)
                    {
                        var audioPartPath = audioPartsPath[i];
                        var progress = new Progress<double>(partProgress =>
                        {
                            double overallProgress = ((double)i + partProgress / 100.0) / totalParts * 100.0;
                            Dispatcher.Invoke(() => {
                                WhisperProgressBar.Value = overallProgress;
                                WhisperProgressText.Text = $"{(int)overallProgress}%";
                            });
                        });
                        var text = await whisperService.TranscribeAsync(audioPartPath, progress, cancellationToken);
                        textPartList.Add(text);
                    }
                    Dispatcher.Invoke(() => {
                        WhisperProgressBar.Visibility = Visibility.Collapsed;
                        WhisperProgressText.Visibility = Visibility.Collapsed;
                    });
                }

                audioCutService.CleanAudioChunksCache();

                if (cancellationToken.IsCancellationRequested)
                {
                    return string.Empty;
                }

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
                    Dispatcher.Invoke(() => {
                        GeminiProgressText.Visibility = Visibility.Visible;
                    });
                    var gemini = new GeminiService();
                    for (int i = 0; i < textPartList.Count; i++)
                    {
                        int remain = textPartList.Count - i;
                        Dispatcher.Invoke(() => {
                            GeminiProgressText.Text = $"Finish in about {remain * 18}s";
                        });
                        var geminiResult =
                            await gemini.GetChatCompletionAsync(systemPrompt.DefaultSystemPrompt + textPartList[i],
                                cancellationToken);
                        fullText.Append(geminiResult);
                        await Task.Delay(18000, cancellationToken);
                    }
                    Dispatcher.Invoke(() => {
                        GeminiProgressText.Visibility = Visibility.Collapsed;
                    });
                }

                var finalText = fullText.ToString();
                return finalText;
            }
            catch (TaskCanceledException)
            {
                Console.WriteLine("canceled task exeption");
                
                return string.Empty;
            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("cancel operation exeption");
                
                return string.Empty;
            }
        }
    }
}