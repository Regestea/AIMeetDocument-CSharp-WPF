using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using System.Threading.Tasks;
using MediaDevices;
using System.Collections.ObjectModel;

namespace AIMeetDocument;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private bool isLoading = false;

    public ObservableCollection<MediaDeviceViewModel> MediaDevices { get; set; } =
        new ObservableCollection<MediaDeviceViewModel>();

    public MainWindow()
    {
        InitializeComponent();
        this.DataContext = this;
    }

    private async void ScanNow_Click(object sender, RoutedEventArgs e)
    {
        await Task.Delay(300); // Short delay for UI feedback
        MediaDevices.Clear();
        foreach (var device in MediaDevice.GetDevices())
        {
            try
            {
                device.Connect();
                string rootPath = null;
                try
                {
                    var roots = device.GetDirectories("\\");
                    if (roots != null && roots.Length > 0)
                        rootPath = roots[0];
                }
                catch
                {
                }

                var deviceViewModel = new MediaDeviceViewModel
                {
                    FriendlyName = device.FriendlyName,
                    Description = device.Description,
                    Manufacturer = device.Manufacturer,
                    Model = device.Model,
                    SerialNumber = device.SerialNumber,
                    RootPath = rootPath
                };
                MediaDevices.Add(deviceViewModel);
                device.Disconnect();
            }
            catch
            {
                /* Optionally handle device connection errors */
            }
        }

        // Show MediaDeviceCard(s) if any device is found
        if (MediaDevices.Count > 0)
        {
            DevicesList.Visibility = Visibility.Visible;
            ScanCard.Visibility = Visibility.Collapsed;
            SelectFileCard.Visibility = Visibility.Collapsed;
        }
        else
        {
            DevicesList.Visibility = Visibility.Collapsed;
            ScanCard.Visibility = Visibility.Visible;
            SelectFileCard.Visibility = Visibility.Visible;
        }
    }

    private void SelectFile_Click(object sender, RoutedEventArgs e)
    {
        var dlg = new OpenFileDialog
        {
            Multiselect = true,
            Title = "Select Files"
        };
        if (dlg.ShowDialog() == true)
        {
            MessageBox.Show($"Selected files:\n{string.Join("\n", dlg.FileNames)}", "Files Selected");
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

    private void Home()
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
        
        if (SettingsPanelUC != null)
            SettingsPanelUC.Visibility = Visibility.Collapsed;
        
        if (DevicesList != null)
            DevicesList.Visibility = Visibility.Collapsed;
        
        if (FileExplorerUC != null)
            FileExplorerUC.Visibility = Visibility.Collapsed;
        
        if (ScanCard != null)
            ScanCard.Visibility = Visibility.Collapsed;
        
        if (SelectFileCard != null)
            SelectFileCard.Visibility = Visibility.Collapsed;
    }

    public void NavigateToFileExplorer(string path)
    {
        // Hide main cards and show FileExplorer
        ClearComponents();
        FileExplorerUC.Visibility = Visibility.Visible;
        FileExplorerUC.Path = path;
    }

    public void NavigateToAiProccess(string fileName)
    {
        ClearComponents();
        if (AiProccessUC != null)
        {
            AiProccessUC.Visibility = Visibility.Visible;
            AiProccessUC.SetFileName(fileName);
        }
    }
}