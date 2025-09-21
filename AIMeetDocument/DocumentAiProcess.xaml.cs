using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Win32;
using AIMeetDocument.DTOs;
using PageRange = AIMeetDocument.DTOs.PageRange;

namespace AIMeetDocument;

public partial class DocumentAiProcess : UserControl
{
    private List<string> _pdfFilePaths = new List<string>();
    private List<PageRange> _pageRanges = new() { new PageRange() { From = 1, To = 1 } };

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
    }

    public void SetPdfFilePaths(List<string> pdfFilePaths)
    {
        _pdfFilePaths = pdfFilePaths ?? new List<string>();

        if (_pdfFilePaths.Count == 0)
        {
            PdfFilesText.Text = "No PDF files selected";
        }
        else if (_pdfFilePaths.Count == 1)
        {
            PdfFilesText.Text = $"Selected: {System.IO.Path.GetFileName(_pdfFilePaths[0])}";
        }
        else
        {
            PdfFilesText.Text = $"{_pdfFilePaths.Count} PDF files selected";
        }
    }

    private void StartButton_Click(object sender, RoutedEventArgs e)
    {
        if (_pdfFilePaths.Count == 0)
        {
            MessageBox.Show("Please select PDF files first.", "No Files Selected", MessageBoxButton.OK,
                MessageBoxImage.Warning);
            return;
        }

        // Get selected values from all form fields
        var operationType = ((ComboBoxItem)OperationTypeCombo.SelectedItem)?.Tag?.ToString() ?? "none";
        var outputLanguage = ((ComboBoxItem)OutputLanguageCombo.SelectedItem)?.Tag?.ToString() ?? "en";
        var customPrompt = CustomPromptTextBox.Text?.Trim() ?? "";
        var pdfLanguage = ((ComboBoxItem)PdfLanguageCombo.SelectedItem)?.Tag?.ToString() ?? "en";
        var fontFamily = ((ComboBoxItem)FontFamilyCombo.SelectedItem)?.Content?.ToString() ?? "Arial";
        var contentStyle = ((ComboBoxItem)ContentStyleCombo.SelectedItem)?.Tag?.ToString() ?? "formal";
        var fontSize = ((ComboBoxItem)FontSizeCombo.SelectedItem)?.Content?.ToString() ?? "12";
        var fileType = ((ComboBoxItem)FileTypeCombo.SelectedItem)?.Content?.ToString() ?? "Word";
        var saveLocation = LocationTextBox.Text?.Trim() ?? "";

        // Get page ranges
        var pageRanges = GetPageRanges();

        // Show loading panel
        ActionPanel.Visibility = Visibility.Collapsed;
        LoadingPanel.Visibility = Visibility.Visible;
        StartButton.IsEnabled = false;

        // Validate required fields
        if (string.IsNullOrEmpty(saveLocation))
        {
            MessageBox.Show("Please select a save location.", "Save Location Required", MessageBoxButton.OK,
                MessageBoxImage.Warning);
            ActionPanel.Visibility = Visibility.Visible;
            LoadingPanel.Visibility = Visibility.Collapsed;
            StartButton.IsEnabled = true;
            return;
        }

        // Log the selected options for debugging
        System.Diagnostics.Debug.WriteLine($"Processing with options:");
        System.Diagnostics.Debug.WriteLine($"- Operation Type: {operationType}");
        System.Diagnostics.Debug.WriteLine($"- Output Language: {outputLanguage}");
        System.Diagnostics.Debug.WriteLine($"- Custom Prompt: {customPrompt}");
        System.Diagnostics.Debug.WriteLine($"- PDF Language: {pdfLanguage}");
        System.Diagnostics.Debug.WriteLine($"- Font Family: {fontFamily}");
        System.Diagnostics.Debug.WriteLine($"- Content Style: {contentStyle}");
        System.Diagnostics.Debug.WriteLine($"- Font Size: {fontSize}");
        System.Diagnostics.Debug.WriteLine($"- File Type: {fileType}");
        System.Diagnostics.Debug.WriteLine($"- Page Ranges: {pageRanges.Count} ranges");
        foreach (var range in pageRanges)
        {
            System.Diagnostics.Debug.WriteLine($"  - Pages {range.From} to {range.To}");
        }

        System.Diagnostics.Debug.WriteLine($"- Save Location: {saveLocation}");

        // TODO: Implement PDF processing logic here
        // For now, just simulate processing
        Processing();
    }

    private async void Processing()
    {

        var test = _pageRanges;
        // try
        // {
        //     ProgressBar.Visibility = Visibility.Visible;
        //     ProgressText.Visibility = Visibility.Visible;
        //     
        //     for (int i = 0; i <= 100; i += 10)
        //     {
        //         ProgressBar.Value = i;
        //         ProgressText.Text = $"{i}%";
        //         await Task.Delay(200); // Simulate processing time
        //     }
        //     
        //     MessageBox.Show("PDF processing completed successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        // }
        // catch (Exception ex)
        // {
        //     MessageBox.Show($"Error processing PDFs: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        // }
        // finally
        // {
        //     // Reset UI
        //     ActionPanel.Visibility = Visibility.Visible;
        //     LoadingPanel.Visibility = Visibility.Collapsed;
        //     ProgressBar.Visibility = Visibility.Collapsed;
        //     ProgressText.Visibility = Visibility.Collapsed;
        //     StartButton.IsEnabled = true;
        // }
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
            var path = System.IO.Path.GetDirectoryName(dialog.FileName);
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
        // Update the event handlers for the textboxes in this row
        var textBoxes = row.Children.OfType<TextBox>().ToList();
        if (textBoxes.Count >= 2)
        {
            var fromTextBox = textBoxes[0];
            var toTextBox = textBoxes[1];

            // Remove old handlers and add new ones
            fromTextBox.TextChanged -= (s, e) => { };
            toTextBox.TextChanged -= (s, e) => { };

            fromTextBox.TextChanged += (s, e) => UpdatePageRangeFrom(newIndex, fromTextBox.Text);
            toTextBox.TextChanged += (s, e) => UpdatePageRangeTo(newIndex, toTextBox.Text);
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