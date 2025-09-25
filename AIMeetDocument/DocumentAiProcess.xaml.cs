using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Win32;
using AIMeetDocument.DTOs;
using AIMeetDocument.Enums;
using AIMeetDocument.Extensions;
using AIMeetDocument.Services;
using AIMeetDocument.StaticValues;
using PageRange = AIMeetDocument.DTOs.PageRange;

namespace AIMeetDocument;

public partial class DocumentAiProcess : UserControl
{
    private string? _pdfFilePath;
    private List<PageRange> _pageRanges = new() { new PageRange() { From = 1, To = 1 } };
    private CancellationTokenSource? _cts;
    private Task? _runningTask;

    public DocumentAiProcess()
    {
        InitializeComponent();

        // Set default save location to Desktop
        LocationTextBox.Text = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);

        // Populate the UI with the default page ranges
        PageRangeContainer.Children.Clear();
        for (int i = 0; i < _pageRanges.Count; i++)
        {
            var row = CreatePageRangeRow(_pageRanges[i], i);
            PageRangeContainer.Children.Add(row);
        }

        // Wire up event handlers
        StartButton.Click += StartButton_Click;
        BrowseButton.Click += BrowseButton_Click;
        AddPageRangeButton.Click += AddPageRangeButton_Click;


        // Load font family options from enum
        LoadFontFamilyOptions();
    }

    public void SetPdfFilePath(string pdfFilePath)
    {
        _pdfFilePath = pdfFilePath;
        PdfFilesText.Text = $"Selected: {Path.GetFileName(_pdfFilePath)}";
    }

    private async void StartButton_Click(object sender, RoutedEventArgs e)
    {
        // Show loading panel
        ActionPanel.Visibility = Visibility.Collapsed;
        LoadingPanel.Visibility = Visibility.Visible;
        StartButton.IsEnabled = false;
        var options = BuildDocumentGeneratorOptions();

        _cts = new CancellationTokenSource();
        _runningTask = Task.Run(async () =>
        {
            var resultContent = await StartProcess(options, _cts.Token);
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
                    switch (options.FileType)
                    {
                        case FileType.MD:
                            var markdownService = new MarkdownToMdFileService();
                            string outputFilePath = Path.Combine(options.OutputLocation, $"{fileName}.md");
                            markdownService.SaveMarkdownToFile(resultContent, outputFilePath);
                            MessageBox.Show($"Markdown file saved to {outputFilePath}");
                            break;
                        case FileType.Word:
                            var wordService = new MarkdownToWordService();
                            string wordOutputPath = Path.Combine(options.OutputLocation, $"{fileName}.docx");
                            wordService.ConvertMarkdownStringToDocx(resultContent, wordOutputPath, options.FontOptions);
                            MessageBox.Show($"Word document saved to {wordOutputPath}");
                            break;
                        case FileType.PDF:
                            var pdfService = new MarkdownToPdfService();
                            string pdfOutputPath = Path.Combine(options.OutputLocation, $"{fileName}.pdf");
                            pdfService.ConvertMarkdownStringToPdf(resultContent, pdfOutputPath, options.FontOptions);
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

    private DocumentGeneratorOptions BuildDocumentGeneratorOptions()
    {
        var operationTypeTag = ((ComboBoxItem)OperationTypeCombo.SelectedItem)?.Tag?.ToString() ?? "none";
        var operationType = operationTypeTag switch
        {
            "Pamphlet" => OperationType.Pamphlet,
            "Summerize" => OperationType.Summerize,
            "Remake" => OperationType.Remake,
            "QuestionAnswer" => OperationType.QuestionAnswer,
            _ => OperationType.None
        };
        var outputLanguage = ((ComboBoxItem)OutputLanguageCombo.SelectedItem).Tag.ToString();
        var pdfLanguage = ((ComboBoxItem)PdfLanguageCombo.SelectedItem).Tag.ToString();
        var userPrompt = CustomPromptTextBox.Text.Trim();
        var fontFamily = (Enums.FontFamily)((ComboBoxItem)FontFamilyCombo.SelectedItem).Tag;
        var fontSize = int.TryParse(((ComboBoxItem)FontSizeCombo.SelectedItem).Content.ToString(), out var size)
            ? size
            : 12;
        var fileType = ((ComboBoxItem)FileTypeCombo.SelectedItem).Content.ToString();
        var saveLocation = LocationTextBox.Text.Trim();
        var pageRanges = GetPageRanges();
        var pdfFilePath = _pdfFilePath;
        var textDirection = (outputLanguage is "fa" or "ar") ? TextDirection.RTL : TextDirection.LTR;
        var fontOptions = FontOptions.CreateDefaults(fontFamily, fontSize, textDirection);
        var contentStyleTag = ((ComboBoxItem)ContentStyleCombo.SelectedItem).Tag.ToString();
        var pagePreRequestTag = ((ComboBoxItem)PagePreRequestComboBox.SelectedItem).Tag.ToString();
        PagePreRequest requestPrePage;
        if (int.TryParse(pagePreRequestTag, out int intValue))
        {
            requestPrePage = (PagePreRequest)intValue;
        }
        else
        {
            requestPrePage = PagePreRequest.Auto;
        }

        var contentDetailsText = ((ComboBoxItem)ContentDetailsCombo.SelectedItem).Content.ToString();
        var contentStyle = contentStyleTag switch
        {
            "formal" => ContentStyle.Formal,
            "informal" => ContentStyle.Informal,
            _ => ContentStyle.None
        };
        var contentDetails = contentDetailsText switch
        {
            "Summary" => ContentDetails.Summary,
            "Maximum details" => ContentDetails.MaximumDetails,
            _ => ContentDetails.Regular
        };
        var pdfSubject = AudioSubjectTextBox.Text.Trim();
        return new DocumentGeneratorOptions
        {
            OperationType = operationType,
            OutputLanguage = outputLanguage,
            FileLanguage = pdfLanguage,
            UserPrompt = userPrompt,
            ContentStyle = contentStyle,
            ContentDetails = contentDetails,
            FileType = fileType switch
            {
                "Word" => FileType.Word,
                "PDF" => FileType.PDF,
                _ => FileType.MD
            },
            OutputLocation = saveLocation,
            PageRanges = pageRanges,
            PdfFilePath = pdfFilePath,
            FontOptions = fontOptions,
            Subject = pdfSubject,
            PagePreRequest = requestPrePage
        };
    }

    private async Task<string> StartProcess(DocumentGeneratorOptions options, CancellationToken cancellationToken)
    {
        try
        {
            var systemPrompt = new SystemPromptBuilder(
                options.OperationType,
                options.Subject,
                options.OutputLanguage,
                options.UserPrompt,
                false,
                options.ContentDetails,
                options.ContentStyle
            );
            Dispatcher.Invoke((Action)(() => { ProgressText.Visibility = Visibility.Visible; }));
            using var pdfToImageService = new PdfPageExtractorService();
            var imageToTextService = new ImageTextExtractorService();
            Dispatcher.Invoke((Action)(() => { ProgressText.Text = $"Converting PDF to images....."; }));
            var imageList = pdfToImageService.ExtractPages(_pdfFilePath, _pageRanges);
            Dispatcher.Invoke((Action)(() => { ProgressText.Text = $"Detecting text form images....."; }));
            var textList = imageToTextService
                .ReadTextFromImage(imageList, ImageToTextLanguage.eng);

            List<string> arrangedList;
            if (options.PagePreRequest == PagePreRequest.Auto)
            {
                arrangedList = textList
                    .ArrangeSentences(6000);
            }
            else
            {
                arrangedList = textList
                    .ArrangeByPage((int)options.PagePreRequest);
            }


            var settingsService = new SettingsService();
            var settings = settingsService.GetSettings();
            var fullText = new StringBuilder();
            if (settings.DefaultAI == DefaultAI.LLMStudio)
            {
                Dispatcher.Invoke((Action)(() =>
                {
                    ProgressText.Text = $"Sending requests to Local LLM (this may take a while) .....";
                }));
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
                Dispatcher.Invoke((Action)(() => { ProgressText.Text = $"Sending requests to Gemini api ....."; }));
                var gemini = new GeminiService();
                int requestDuration = 0;
                for (int i = 0; i < arrangedList.Count; i++)
                {
                    int delayMs = 15000;
                    if (requestDuration != 0)
                    {
                        int remain = arrangedList.Count - i;
                        delayMs = (60 / settings.Gemini.RequestPerMinute) * 1000;
                        int etaSeconds = ((delayMs / 1000) + requestDuration) * remain;

                        Dispatcher.Invoke((Action)(() => { ProgressText.Text = $"{i} of {arrangedList.Count}, Finish in about {etaSeconds}s"; }));
                    }
                    
                    var sw = new Stopwatch();
                    if (requestDuration ==0)
                    {
                        sw.Start();
                    }
                    

                    var geminiResult =
                        await gemini.GetChatCompletionAsync(systemPrompt.DefaultSystemPrompt + arrangedList[i],
                            cancellationToken);
                    fullText.Append(geminiResult);
                    
                    if (requestDuration == 0)
                    {
                        sw.Stop();
                        requestDuration = (int)sw.Elapsed.TotalSeconds;
                    }
                    
                    await Task.Delay(delayMs, cancellationToken);
                }

                Dispatcher.Invoke((Action)(() => { ProgressText.Visibility = Visibility.Collapsed; }));
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

    private void AddPageRangeButton_Click(object sender, RoutedEventArgs e)
    {
        AddPageRangeRow();
    }

    private void AddPageRangeRow()
    {
        var pageRange = new PageRange { From = 1, To = 1 };
        _pageRanges.Add(pageRange);

        var row = CreatePageRangeRow(pageRange, _pageRanges.Count - 1);
        PageRangeContainer.Children.Add(row);
    }

    private void RemovePageRangeRow(int index, StackPanel row)
    {
        if (index < _pageRanges.Count)
        {
            _pageRanges.RemoveAt(index);
            PageRangeContainer.Children.Remove(row);

            // Update indices for remaining rows
            for (int i = 0; i < PageRangeContainer.Children.Count; i++)
            {
                if (PageRangeContainer.Children[i] is StackPanel remainingRow)
                {
                    UpdateRowIndices(remainingRow, i);
                }
            }
        }
    }

    private void UpdateRowIndices(StackPanel row, int newIndex)
    {
        var textBoxes = row.Children.OfType<TextBox>().ToList();
        if (textBoxes.Count >= 2)
        {
            var fromTextBox = textBoxes[0];
            var toTextBox = textBoxes[1];
            fromTextBox.TextChanged -= FromTextBox_TextChanged;
            toTextBox.TextChanged -= ToTextBox_TextChanged;
            fromTextBox.TextChanged += FromTextBox_TextChanged;
            toTextBox.TextChanged += ToTextBox_TextChanged;
        }
    }

    private void FromTextBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is TextBox fromTextBox)
        {
            int index = PageRangeContainer.Children.IndexOf(fromTextBox.Parent as UIElement);
            UpdatePageRangeFrom(index, fromTextBox.Text);
        }
    }

    private void ToTextBox_TextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is TextBox toTextBox)
        {
            int index = PageRangeContainer.Children.IndexOf(toTextBox.Parent as UIElement);
            UpdatePageRangeTo(index, toTextBox.Text);
        }
    }

    private List<PageRange> GetPageRanges()
    {
        return _pageRanges.ToList();
    }

    private void UpdatePageRangeFrom(int index, string text)
    {
        int from = 1;
        if (!string.IsNullOrWhiteSpace(text) && int.TryParse(text, out int parsedFrom))
        {
            from = parsedFrom;
        }

        if (index < _pageRanges.Count)
        {
            _pageRanges[index].From = from;
        }
    }

    private void UpdatePageRangeTo(int index, string text)
    {
        int to = 1;
        if (!string.IsNullOrWhiteSpace(text) && int.TryParse(text, out int parsedTo))
        {
            to = parsedTo;
        }

        if (index < _pageRanges.Count)
        {
            _pageRanges[index].To = to;
        }
    }

    private StackPanel CreatePageRangeRow(PageRange pageRange, int index)
    {
        var row = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            Margin = new Thickness(0, 2, 0, 2)
        };

        // From label and input
        var fromLabel = new TextBlock
        {
            Text = "From:",
            Foreground = System.Windows.Media.Brushes.LightGoldenrodYellow,
            FontWeight = FontWeights.SemiBold,
            FontSize = 14,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 5, 0)
        };

        var fromTextBox = new TextBox
        {
            Text = pageRange.From.ToString(),
            Width = 60,
            Height = 28,
            FontSize = 14,
            VerticalContentAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0)
        };
        fromTextBox.TextChanged += (s, e) => UpdatePageRangeFrom(index, fromTextBox.Text);

        // To label and input
        var toLabel = new TextBlock
        {
            Text = "To:",
            Foreground = System.Windows.Media.Brushes.LightGoldenrodYellow,
            FontWeight = FontWeights.SemiBold,
            FontSize = 14,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 5, 0)
        };

        var toTextBox = new TextBox
        {
            Text = pageRange.To.ToString(),
            Width = 60,
            Height = 28,
            FontSize = 14,
            VerticalContentAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 0, 10, 0)
        };
        toTextBox.TextChanged += (s, e) => UpdatePageRangeTo(index, toTextBox.Text);

        // Remove button
        var removeButton = new Button
        {
            Content = "Remove",
            Width = 70,
            Height = 28,
            FontSize = 12,
            Margin = new Thickness(10, 0, 0, 0),
            Style = (Style)FindResource("ModernButton"),
            Cursor = System.Windows.Input.Cursors.Hand
        };
        removeButton.Click += (s, e) => RemovePageRangeRow(index, row);

        // Apply styling to textboxes
        var textBoxStyle = new Style(typeof(TextBox));
        textBoxStyle.Setters.Add(new Setter(TextBox.BorderBrushProperty,
            new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#efa449"))));
        textBoxStyle.Setters.Add(new Setter(TextBox.BorderThicknessProperty, new Thickness(2)));
        textBoxStyle.Setters.Add(new Setter(TextBox.BackgroundProperty,
            new System.Windows.Media.SolidColorBrush(
                (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString("#fcdc93"))));
        textBoxStyle.Setters.Add(new Setter(TextBox.TemplateProperty, CreateTextBoxTemplate()));

        fromTextBox.Style = textBoxStyle;
        toTextBox.Style = textBoxStyle;

        row.Children.Add(fromLabel);
        row.Children.Add(fromTextBox);
        row.Children.Add(toLabel);
        row.Children.Add(toTextBox);
        row.Children.Add(removeButton);

        return row;
    }

    /// <summary>
    /// Loads font family options from the FontFamily enum
    /// </summary>
    private void LoadFontFamilyOptions()
    {
        try
        {
            // Clear existing items
            FontFamilyCombo.Items.Clear();

            // Get all values from the FontFamily enum
            var fontFamilyValues = Enum.GetValues<FontFamily>();

            // Add each font family as a ComboBoxItem
            foreach (var fontFamily in fontFamilyValues)
            {
                var fontOptions = new FontOptions();
                string displayName = fontOptions.GetFontFamilyName(fontFamily);

                var item = new ComboBoxItem
                {
                    Content = displayName,
                    Tag = fontFamily // Store the enum value in Tag
                };
                FontFamilyCombo.Items.Add(item);
            }

            // Select Calibri by default (index 0)
            if (FontFamilyCombo.Items.Count > 0)
            {
                FontFamilyCombo.SelectedIndex = 0;
            }
        }
        catch (Exception ex)
        {
            // Handle any errors gracefully
            Console.WriteLine($"Error loading font family options: {ex.Message}");

            // Add fallback items
            FontFamilyCombo.Items.Clear();
            var fallbackItems = new[]
            {
                new ComboBoxItem { Content = "Calibri", Tag = Enums.FontFamily.Calibri },
                new ComboBoxItem { Content = "Arial", Tag = Enums.FontFamily.Arial },
                new ComboBoxItem { Content = "Times New Roman", Tag = Enums.FontFamily.TimesNewRoman }
            };

            foreach (var item in fallbackItems)
            {
                FontFamilyCombo.Items.Add(item);
            }

            FontFamilyCombo.SelectedIndex = 0;
        }
    }

    private ControlTemplate CreateTextBoxTemplate()
    {
        var template = new ControlTemplate(typeof(TextBox));
        var border = new FrameworkElementFactory(typeof(Border));
        border.Name = "Bd";
        border.SetBinding(Border.BorderBrushProperty,
            new System.Windows.Data.Binding("BorderBrush")
                { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });
        border.SetBinding(Border.BorderThicknessProperty,
            new System.Windows.Data.Binding("BorderThickness")
                { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });
        border.SetBinding(Border.BackgroundProperty,
            new System.Windows.Data.Binding("Background")
                { RelativeSource = new RelativeSource(RelativeSourceMode.TemplatedParent) });
        border.SetValue(Border.CornerRadiusProperty, new CornerRadius(5));

        var scrollViewer = new FrameworkElementFactory(typeof(ScrollViewer));
        scrollViewer.Name = "PART_ContentHost";
        border.AppendChild(scrollViewer);
        template.VisualTree = border;

        return template;
    }
}