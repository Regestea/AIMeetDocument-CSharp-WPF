using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MediaDevices;

namespace AIMeetDocument
{
    public partial class FileExplorer : UserControl, INotifyPropertyChanged
    {
        public static readonly DependencyProperty PathProperty = DependencyProperty.Register(
            "Path", typeof(string), typeof(FileExplorer), new PropertyMetadata(string.Empty, OnPathChanged));

        public string Path
        {
            get => (string)GetValue(PathProperty);
            set => SetValue(PathProperty, value);
        }

        public ObservableCollection<FileSystemInfo> Items { get; set; } = new ObservableCollection<FileSystemInfo>();

        private bool _isAudioFileSelected;
        public bool IsAudioFileSelected
        {
            get => _isAudioFileSelected;
            set { _isAudioFileSelected = value; OnPropertyChanged(nameof(IsAudioFileSelected)); }
        }

        public FileExplorer()
        {
            InitializeComponent();
            FilesListBox.ItemsSource = Items;
            FilesListBox.SelectionChanged += FilesListBox_SelectionChanged;
            DataContext = this;
        }

        private static void OnPathChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FileExplorer explorer && e.NewValue is string newPath)
            {
                explorer.LoadDirectory(newPath);
            }
        }

        private void LoadDirectory(string? path="\\Internal storage")
        {
            Path=path;
            Items.Clear();
            var device = MediaDevice.GetDevices().FirstOrDefault();
            device.Connect();
            var directories = device.GetDirectories(path);
            foreach (var dir in directories)
            {
                Items.Add(new DirectoryInfo(dir));
            }
            var files = device.GetFiles(path);
            foreach (var file in files)
            {
                Items.Add(new FileInfo(file));
            }
           
        }
        
        private void FilesListBox_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (FilesListBox.SelectedItem is DirectoryInfo dir)
            {
                var f= FilesListBox.SelectedItem as DirectoryInfo;
                LoadDirectory(f.ToString());
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(Path))
            {
                var parent = System.IO.Path.GetDirectoryName(Path.TrimEnd(System.IO.Path.DirectorySeparatorChar, System.IO.Path.AltDirectorySeparatorChar));
                if (!string.IsNullOrEmpty(parent))
                {
                    LoadDirectory(parent);
                    Path = parent;
                }
            }
        }

        private void FilesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FilesListBox.SelectedItem is FileInfo file)
            {
                var ext = System.IO.Path.GetExtension(file.Name).ToLower();
                IsAudioFileSelected = ext == ".mp3" || ext == ".wav" || ext == ".m4a";
            }
            else
            {
                IsAudioFileSelected = false;
            }
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            if (FilesListBox.SelectedItem is FileInfo file)
            {
                var f = FilesListBox.SelectedItem as FileInfo;
                CopyFileToCache(f.ToString());
                // Navigate to AiProccess and pass the file name
                var mainWindow = Application.Current.MainWindow as MainWindow;
                if (mainWindow != null)
                {
                    mainWindow.NavigateToAiProccess(f.Name);
                }
            }
        }

        private void CopyFileToCache(string path)
        {
            MediaDevice? device = MediaDevice.GetDevices().FirstOrDefault();
            if(device == null) return;
            device.Connect();
            
          var destinationPathOnPC = System.IO.Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AudioCache");
            if (!Directory.Exists(destinationPathOnPC))
            {
                Directory.CreateDirectory(destinationPathOnPC);
            }
            
            device.DownloadFile(path, System.IO.Path.Combine(destinationPathOnPC, System.IO.Path.GetFileName(path)));
            
            
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
