using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
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
            Items.Clear();
            var device=MediaDevice.GetDevices().FirstOrDefault();
            device.Connect();
            var directories=device.GetDirectories(path);
            if (directories.Any())
            {
               foreach (var dir in directories)
               {
                   Items.Add(new DirectoryInfo(dir));
               }
            }
        }
    }
}

