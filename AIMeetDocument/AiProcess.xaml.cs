using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using AIMeetDocument.Services;
using Microsoft.Win32;
using System.Threading;
using System.Threading.Tasks;
using AIMeetDocument.Enums;
using AIMeetDocument.Extensions;
using AIMeetDocument.StaticValues;

namespace AIMeetDocument
{
    public partial class AiProcess : UserControl
    {
        private IList<string> _fileNameList = new List<string>() { "FileName.txt" };
        private CancellationTokenSource _cts;
        private Task _runningTask;

        public AiProcess()
        {
            InitializeComponent();
            if (_fileNameList.Count == 1)
            {
                FileNameText.Text = _fileNameList[0];
            }
            else
            {
                FileNameText.Text = _fileNameList.Count + " Audio Files Selected";
            }

            // Set default save location to Desktop
            LocationTextBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            StartButton.Click += StartButton_Click;
            CancelButton.Click += CancelButton_Click;
            BrowseButton.Click += BrowseButton_Click;
            
            // Load audio detection models
            LoadAudioDetectionModels();
        }

        public void SetFileName(List<string> fileNameList)
        {
            _fileNameList = fileNameList;
            if (_fileNameList.Count == 1)
            {
                FileNameText.Text = _fileNameList[0];
            }
            else
            {
                FileNameText.Text = _fileNameList.Count + " Audio Files Selected";
            }
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

            List<string> audioPaths = new List<string>();

            foreach (var fileName in _fileNameList)
            {
                var audioPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AudioCache", fileName);
                if (File.Exists(audioPath))
                {
                    audioPaths.Add(audioPath);
                }
            }


            _cts = new CancellationTokenSource();
            _runningTask = Task.Run(async () =>
            {
                var resultContent = await StartProcess(userPrompt, audioPaths, audioSubject, audioLanguage,
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

            ActionPanel.Visibility = Visibility.Visible;
            LoadingPanel.Visibility = Visibility.Collapsed;
            StartButton.IsEnabled = true;
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

        private async Task<string> StartProcess(string userPrompt, List<string> audioFilePathList, string audioSubject,
            string audioLanguage, string outputLanguage,
            CancellationToken cancellationToken)
        {
            try
            {
                // Get the selected audio detection model from the combo box
                string selectedModelPath = GetSelectedAudioDetectionModel();
                
                // If no model is selected or the path is empty, use the default model
                string modelPath = !string.IsNullOrEmpty(selectedModelPath) 
                    ? selectedModelPath 
                    : Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LLM", "ggml-large-v3-turbo.bin");
                var textPartList = new List<string>();
                var audioCutService = new AudioCutService();
                var AudioAnalysisService = new AudioAnalysisService();
                var allAudioPartsPath = new List<string>();
                foreach (var audioFilePath in audioFilePathList)
                {
                    var silenceSeconds = AudioAnalysisService.GetAvgMinThreshold(audioFilePath, cancellationToken);
                    var secondPeaks =
                        AudioAnalysisService.GetSilenceSeconds(audioFilePath, 3, silenceSeconds, cancellationToken);
                    var audioPartsPath = audioCutService.CutAudioBySeconds(audioFilePath, secondPeaks, cancellationToken);
                    if (cancellationToken.IsCancellationRequested)
                    {
                        audioCutService.CleanAudioChunksCache();
                        return string.Empty;
                    }
                    allAudioPartsPath.AddRange(audioPartsPath);
                }

                using (var whisperService = new WhisperService(modelPath, audioLanguage))
                {
                    Dispatcher.Invoke(() =>
                    {
                        WhisperProgressBar.Visibility = Visibility.Visible;
                        WhisperProgressText.Visibility = Visibility.Visible;
                        GeminiProgressText.Visibility = Visibility.Collapsed;
                        WhisperProgressBar.Value = 0;
                        WhisperProgressText.Text = "0%";
                    });
                    int totalParts = allAudioPartsPath.Count;
                    for (int i = 0; i < totalParts; i++)
                    {
                        var audioPartPath = allAudioPartsPath[i];
                        var progress = new Progress<double>(partProgress =>
                        {
                            double overallProgress = ((double)i + partProgress / 100.0) / totalParts * 100.0;
                            Dispatcher.Invoke(() =>
                            {
                                WhisperProgressBar.Value = overallProgress;
                                WhisperProgressText.Text = $"{(int)overallProgress}%";
                            });
                        });
                        var text = await whisperService.TranscribeAsync(audioPartPath, progress, cancellationToken);
                        textPartList.Add(text);
                    }

                    Dispatcher.Invoke(() =>
                    {
                        WhisperProgressBar.Visibility = Visibility.Collapsed;
                        WhisperProgressText.Visibility = Visibility.Collapsed;
                    });
                }

                audioCutService.CleanAudioChunksCache();

                if (cancellationToken.IsCancellationRequested)
                {
                    return string.Empty;
                }

                var arrangedList= textPartList.ArrangeSentences(7000, 8000);
                var systemPrompt = new SystemPromptBuilder(audioSubject, outputLanguage, userPrompt);
                var settingsService = new SettingsService();
                var settings = settingsService.GetSettings();
                var fullText = new StringBuilder();
                if (settings.DefaultAI == DefaultAI.LLMStudio)
                {
                    var llm = new LocalLanguageModelService();
                    foreach (var text in arrangedList)
                    {
                        var llmResult =
                            await llm.GetChatCompletionAsync(systemPrompt.DefaultSystemPrompt, text, cancellationToken);
                        fullText.Append(llmResult);
                    }
                }
                else
                {
                    Dispatcher.Invoke(() => { GeminiProgressText.Visibility = Visibility.Visible; });
                    var gemini = new GeminiService();
                    for (int i = 0; i < arrangedList.Count; i++)
                    {
                        int remain = arrangedList.Count - i;
                        int delayMs = 15000; // default for Gemini25Pro
                        int etaSeconds = 23 * remain; // default for Gemini25Pro
                        if (settings.Gemini.Model == GeminiModel.Gemini25Flash)
                        {
                            delayMs = 8000;
                            etaSeconds = 15 * remain;
                        }
                        Dispatcher.Invoke(() => { GeminiProgressText.Text = $"Finish in about {etaSeconds}s"; });
                        var geminiResult =
                            await gemini.GetChatCompletionAsync(systemPrompt.DefaultSystemPrompt + arrangedList[i],
                                cancellationToken);
                        fullText.Append(geminiResult);
                        await Task.Delay(delayMs, cancellationToken);
                    }

                    Dispatcher.Invoke(() => { GeminiProgressText.Visibility = Visibility.Collapsed; });
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

        /// <summary>
        /// Loads audio detection models from the LLM folder
        /// </summary>
        private void LoadAudioDetectionModels()
        {
            try
            {
                // Get the LLM folder path (next to the executable)
                string llmFolderPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LLM");
                
                // Check if the LLM folder exists
                if (!Directory.Exists(llmFolderPath))
                {
                    // Create the folder if it doesn't exist
                    Directory.CreateDirectory(llmFolderPath);
                }

                // Get all .bin files from the LLM folder
                string[] binFiles = Directory.GetFiles(llmFolderPath, "*.bin");

                // Clear existing items
                AudioDetectionModelCombo.Items.Clear();

                if (binFiles.Length == 0)
                {
                    // Add a default item if no .bin files found
                    var defaultItem = new ComboBoxItem
                    {
                        Content = "No models found",
                        Tag = string.Empty,
                        IsEnabled = false
                    };
                    AudioDetectionModelCombo.Items.Add(defaultItem);
                    AudioDetectionModelCombo.SelectedIndex = 0;
                }
                else
                {
                    // Add each .bin file as a ComboBoxItem
                    foreach (string binFile in binFiles)
                    {
                        string fileName = Path.GetFileName(binFile);
                        var item = new ComboBoxItem
                        {
                            Content = fileName,
                            Tag = binFile // Store the full path in Tag
                        };
                        AudioDetectionModelCombo.Items.Add(item);
                    }

                    // Select the first item by default
                    if (AudioDetectionModelCombo.Items.Count > 0)
                    {
                        AudioDetectionModelCombo.SelectedIndex = 0;
                    }
                }
            }
            catch (Exception ex)
            {
                // Handle any errors gracefully
                Console.WriteLine($"Error loading audio detection models: {ex.Message}");
                
                // Add a fallback item
                AudioDetectionModelCombo.Items.Clear();
                var errorItem = new ComboBoxItem
                {
                    Content = "Error loading models",
                    Tag = string.Empty,
                    IsEnabled = false
                };
                AudioDetectionModelCombo.Items.Add(errorItem);
                AudioDetectionModelCombo.SelectedIndex = 0;
            }
        }

        /// <summary>
        /// Gets the selected audio detection model path
        /// </summary>
        /// <returns>The full path to the selected model file, or empty string if none selected</returns>
        public string GetSelectedAudioDetectionModel()
        {
            if (AudioDetectionModelCombo.SelectedItem is ComboBoxItem selectedItem)
            {
                return selectedItem.Tag?.ToString() ?? string.Empty;
            }
            return string.Empty;
        }

        /// <summary>
        /// Refreshes the audio detection models list from the LLM folder
        /// </summary>
        public void RefreshAudioDetectionModels()
        {
            LoadAudioDetectionModels();
        }
    }
}