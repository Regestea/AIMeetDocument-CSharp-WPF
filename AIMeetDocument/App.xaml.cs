using System.Configuration;
using System.Data;
using System.IO;
using System.Windows;

namespace AIMeetDocument;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        var mainWindow = new MainWindow();
        mainWindow.WindowState = WindowState.Maximized;
        mainWindow.Show();
        CleanAudioCache();
    }
    
    /// <summary>
    /// Deletes all files inside the AudioChunksCache folder.
    /// </summary>
    public void CleanAudioCache()
    {
        var cacheDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "AudioCache");
        if (Directory.Exists(cacheDir))
        {
            foreach (var file in Directory.GetFiles(cacheDir))
            {
                File.Delete(file);
            }
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        Services.WhisperService.DisposeFactory();
        base.OnExit(e);
    }
}