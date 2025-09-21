using System.IO;
using System.Windows;
using Microsoft.Win32;
using MediaDevices;
using System.Collections.ObjectModel;

namespace AIMeetDocument;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public ObservableCollection<MediaDeviceViewModel> MediaDevices { get; set; } =
        new ObservableCollection<MediaDeviceViewModel>();

    public MainWindow()
    {
        InitializeComponent();
        this.DataContext = this;
    }

    private void ScanNow_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Multiselect = true, // Enable multiple file selection
            Title = "Select PDF Files",
            Filter = "PDF Files (*.pdf)|*.pdf"
        };
        
        if (dlg.ShowDialog() == true)
        {
            var selectedFilePaths = dlg.FileNames.ToList();
            NavigateToDocumentAiProcess(selectedFilePaths);
        }
    }

    private void SelectFile_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Multiselect = true, // Enable multiple file selection
            Title = "Select Audio File",
            Filter = "Audio Files (*.mp3;*.wav;*.m4a)|*.mp3;*.wav;*.m4a"
        };
        if (dlg.ShowDialog() == true)
        {
            var selectedFilePaths = dlg.FileNames;
            var fileNames = new List<string>();
            string audioCacheDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AudioCache");
            if (!Directory.Exists(audioCacheDir))
                Directory.CreateDirectory(audioCacheDir);
            foreach (var selectedFilePath in selectedFilePaths)
            {
                string fileName = Path.GetFileName(selectedFilePath);
                string destPath = Path.Combine(audioCacheDir, fileName);
                if (!File.Exists(destPath))
                    File.Copy(selectedFilePath, destPath, true);
                fileNames.Add(fileName);
            }
            NavigateToAiProccess(fileNames); // Pass list of file names
        }
    }


    private void Home_Click(object sender, RoutedEventArgs e)
    {
        Home();
    }

    private void Settings_Click(object sender, RoutedEventArgs e)
    {
        // Show SettingsPanel, hide Home UI
        ClearComponents();
        if (SettingsPanelUC != null)
            SettingsPanelUC.Visibility = Visibility.Visible;
    }

    public void Home()
    {
        ClearComponents();
        if (ScanCard != null)
            ScanCard.Visibility = Visibility.Visible;
        if (SelectFileCard != null)
            SelectFileCard.Visibility = Visibility.Visible;
    }

    private void ClearComponents()
    {
        // CardsPanel.Visibility = Visibility.Visible;
        if (AiProccessUC != null)
            AiProccessUC.Visibility = Visibility.Collapsed;
        
        if (DocumentAiProcessUC != null)
            DocumentAiProcessUC.Visibility = Visibility.Collapsed;
        
        if (SettingsPanelUC != null)
            SettingsPanelUC.Visibility = Visibility.Collapsed;
        
        if (DevicesList != null)
            DevicesList.Visibility = Visibility.Collapsed;
        
        if (ScanCard != null)
            ScanCard.Visibility = Visibility.Collapsed;
        
        if (SelectFileCard != null)
            SelectFileCard.Visibility = Visibility.Collapsed;
    }

    public void NavigateToFileExplorer(string path)
    {
        ClearComponents();
    }

    public void NavigateToAiProccess(List<string> fileNameList)
    {
        ClearComponents();
        if (AiProccessUC != null)
        {
            AiProccessUC.Visibility = Visibility.Visible;
            AiProccessUC.SetFileName(fileNameList);
        }
    }

    public void NavigateToDocumentAiProcess(List<string> pdfFilePaths)
    {
        ClearComponents();
        if (DocumentAiProcessUC != null)
        {
            DocumentAiProcessUC.Visibility = Visibility.Visible;
            DocumentAiProcessUC.SetPdfFilePaths(pdfFilePaths);
        }
    }
}