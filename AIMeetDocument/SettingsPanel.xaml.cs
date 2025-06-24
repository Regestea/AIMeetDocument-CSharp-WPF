using System.Windows;
using System.Windows.Controls;

namespace AIMeetDocument
{
    public partial class SettingsPanel : UserControl
    {
        public event RoutedEventHandler BackClicked;
        public SettingsPanel()
        {
            InitializeComponent();
        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            BackClicked?.Invoke(this, e);
        }
    }
}
