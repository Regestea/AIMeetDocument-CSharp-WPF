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
        using (var reader = new WaveFileReader(wav16kPath))
        {
            var allCuts = new List<int> { 0 };
            allCuts.AddRange(seconds);
            allCuts.Add((int)reader.TotalTime.TotalSeconds);
            for (int i = 0; i < allCuts.Count - 1; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();
                int startSec = allCuts[i];
                int endSec = allCuts[i + 1];
                string chunkPath = Path.Combine(cacheDir, $"chunk_{i + 1}.wav");
                int startPos = (int)(startSec * reader.WaveFormat.SampleRate);
                int endPos = (int)(endSec * reader.WaveFormat.SampleRate);
                int bytesToRead = (endPos - startPos) * reader.WaveFormat.BlockAlign;

                reader.Position = startPos * reader.WaveFormat.BlockAlign;
                byte[] buffer = new byte[bytesToRead];
                int read = reader.Read(buffer, 0, buffer.Length);

                using (var writer = new WaveFileWriter(chunkPath, reader.WaveFormat))
                {
                    writer.Write(buffer, 0, read);
                }
                outputPaths.Add(chunkPath);
            }
        }
        File.Delete(wav16kPath);
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