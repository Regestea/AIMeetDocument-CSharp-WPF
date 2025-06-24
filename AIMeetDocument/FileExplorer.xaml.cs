using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using MediaDevices;

namespace AIMeetDocument
{
    public partial class FileExplorer : UserControl
    {
        public static readonly DependencyProperty PathProperty = DependencyProperty.Register(
            "Path", typeof(string), typeof(FileExplorer), new PropertyMetadata(string.Empty, OnPathChanged));

        public string Path
        {
            get => (string)GetValue(PathProperty);
            set => SetValue(PathProperty, value);
        }

        public ObservableCollection<FileSystemInfo> Items { get; set; } = new ObservableCollection<FileSystemInfo>();

        public FileExplorer()
        {
            InitializeComponent();
            FilesListBox.ItemsSource = Items;
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
    }
}
