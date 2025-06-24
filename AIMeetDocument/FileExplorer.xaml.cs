using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;

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

        private void LoadDirectory(string path)
        {
            Items.Clear();
            if (Directory.Exists(path))
            {
                foreach (var dir in new DirectoryInfo(path).GetDirectories())
                    Items.Add(dir);
                foreach (var file in new DirectoryInfo(path).GetFiles())
                    Items.Add(file);
            }
        }
    }
}

