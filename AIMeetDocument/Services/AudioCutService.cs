using System.IO;
using NAudio.Wave;

namespace AIMeetDocument.Services;

public class AudioCutService
{
    /// <summary>
    /// Ensures the input audio file is a WAV file with a 16kHz sample rate.
    /// If not, converts it to 16kHz WAV and returns the new path.
    /// </summary>
    /// <param name="inputPath">Path to the input audio file.</param>
    /// <returns>Path to a 16kHz WAV file.</returns>
    private string EnsureWav16KHz(string inputPath)
    {
        if (Path.GetExtension(inputPath).Equals(".wav", StringComparison.OrdinalIgnoreCase))
        {
            using var reader = new WaveFileReader(inputPath);
            if (reader.WaveFormat.SampleRate == 16000)
            {
                return inputPath;
            }
        }
        // Convert to 16kHz WAV
        string tempPath = Path.Combine(Path.GetTempPath(), $"converted_{Guid.NewGuid()}.wav");
        using (var reader = new AudioFileReader(inputPath))
        {
            var outFormat = new WaveFormat(16000, reader.WaveFormat.Channels);
            var resampler = new NAudio.Wave.SampleProviders.WdlResamplingSampleProvider(reader, 16000);
            using (var waveFileWriter = new WaveFileWriter(tempPath, outFormat))
            {
                float[] buffer = new float[4096];
                int samplesRead;
                while ((samplesRead = resampler.Read(buffer, 0, buffer.Length)) > 0)
                {
                    waveFileWriter.WriteSamples(buffer, 0, samplesRead);
                }
            }
        }
        return tempPath;
    }

public List<string> CutAudioBySeconds(string audioFilePath, List<int> seconds, CancellationToken cancellationToken = default)
{
    var outputPaths = new List<string>();
    var projectDir = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location)!.Parent!.Parent!.FullName;
    var cacheDir = Path.Combine(projectDir, "AudioChunksCache");
    Directory.CreateDirectory(cacheDir);

    string wav16kPath = EnsureWav16KHz(audioFilePath);
    // Use a try/finally block for robust cleanup, as recommended previously
    bool isTempFile = !wav16kPath.Equals(audioFilePath, StringComparison.OrdinalIgnoreCase);
    try
    {
        using (var reader = new WaveFileReader(wav16kPath))
        {
            // ✅ **FIX: Clean, sort, and validate the incoming timestamps**
            var validCutSeconds = seconds
                .Where(s => s >= 0 && s < reader.TotalTime.TotalSeconds) // Ensure timestamps are non-negative and within the audio's duration
                .Distinct()                                             // Remove duplicates
                .OrderBy(s => s)                                        // Sort ascending
                .ToList();

            var allCuts = new List<int> { 0 };
            allCuts.AddRange(validCutSeconds);
            // Ensure the final cut goes to the very end of the audio
            if (!allCuts.Contains((int)reader.TotalTime.TotalSeconds))
            {
                allCuts.Add((int)reader.TotalTime.TotalSeconds);
            }
            
            // The rest of your loop is correct
            for (int i = 0; i < allCuts.Count - 1; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                int startSec = allCuts[i];
                int endSec = allCuts[i + 1];

                // Add a sanity check to avoid zero-length clips if duplicates existed (e.g., 0, 5, 5, 10)
                if (startSec >= endSec) continue;

                string chunkPath = Path.Combine(cacheDir, $"chunk_{i + 1}.wav");
                long startPos = (long)startSec * reader.WaveFormat.AverageBytesPerSecond;
                long endPos = (long)endSec * reader.WaveFormat.AverageBytesPerSecond;
                
                // Set position and read data
                reader.Position = startPos;
                long bytesToRead = endPos - startPos;
                byte[] buffer = new byte[bytesToRead];
                int read = reader.Read(buffer, 0, buffer.Length);

                using (var writer = new WaveFileWriter(chunkPath, reader.WaveFormat))
                {
                    writer.Write(buffer, 0, read);
                }
                outputPaths.Add(chunkPath);
            }
        }
    }
    finally
    {
        if (isTempFile && File.Exists(wav16kPath))
        {
            File.Delete(wav16kPath);
        }
    }
    return outputPaths;
}

    /// <summary>
    /// Deletes all files inside the AudioChunksCache folder.
    /// </summary>
    public void CleanAudioChunksCache()
    {
        var projectDir = Directory.GetParent(System.Reflection.Assembly.GetExecutingAssembly().Location)!.Parent!.Parent!.FullName;
        var cacheDir = Path.Combine(projectDir, "AudioChunksCache");
        if (Directory.Exists(cacheDir))
        {
            foreach (var file in Directory.GetFiles(cacheDir))
            {
                File.Delete(file);
            }
        }
    }
}