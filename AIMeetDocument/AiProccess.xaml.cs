using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;

namespace AIMeetDocument
{
    public partial class AiProccess : UserControl
    {
        private string _fileName = "FileName.txt";
        public AiProccess()
        {
            InitializeComponent();
            FileNameText.Text = _fileName;
            StartButton.Click += StartButton_Click;
            CancelButton.Click += CancelButton_Click;
            BrowseButton.Click += BrowseButton_Click;
        }

        public void SetFileName(string fileName)
        {
            _fileName = fileName;
            FileNameText.Text = fileName;
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            string language = ((ComboBoxItem)LanguageCombo.SelectedItem).Tag.ToString();
            string fileType = ((ComboBoxItem)FileTypeCombo.SelectedItem).Content.ToString();
            string location = LocationTextBox.Text;
            string msg = $"File Name: {_fileName}\nLanguage: {language}\nFile Type: {fileType}\nSave Location: {location}";
            MessageBox.Show(msg, "Form Data");
            ActionPanel.Visibility = Visibility.Collapsed;
            LoadingPanel.Visibility = Visibility.Visible;
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Process cancelled.", "Cancel");
            ActionPanel.Visibility = Visibility.Visible;
            LoadingPanel.Visibility = Visibility.Collapsed;
        }

        private void BrowseButton_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog();
            dialog.CheckFileExists = false;
            dialog.CheckPathExists = true;
            dialog.ValidateNames = false;
            dialog.FileName = "Select this folder";
            if (dialog.ShowDialog() == true)
            {
                var path = System.IO.Path.GetDirectoryName(dialog.FileName);
                LocationTextBox.Text = path;
            }
        }
    }
}
