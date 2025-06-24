using System.Windows.Controls;
using System.Windows;

namespace AIMeetDocument
{
    public partial class MediaDeviceCard : UserControl
    {
        public MediaDeviceCard()
        {
            InitializeComponent();
        }

        private void Border_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (this.DataContext is MediaDeviceViewModel device)
            {
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null && !string.IsNullOrEmpty(device.RootPath))
                {
                  //  mainWindow.NavigateToFileExplorer(device.FriendlyName, device.RootPath);
                }
            }
        }
    }
}
