using System.IO;
using NAudio.Wave;
using Whisper.net;
// Kept for the initial fast check of .wav files

// Using FFMpegCore for conversion

namespace AIMeetDocument.Services
{
    /// <summary>
    /// A service to handle audio transcription using Whisper.net.
    /// It ensures audio is in the correct format (16kHz WAV) before processing.
    /// This version uses FFmpeg for fast and robust audio conversion.
    /// </summary>
    public class WhisperService : IDisposable
    {
        private readonly string _modelPath;
        private readonly WhisperFactory _whisperFactory;
        private bool _disposed;

        /// <summary>
        /// Initializes a new instance of the WhisperService.
        /// </summary>
        /// <param name="modelPath">The file path to the Whisper GGUF model.</param>
        /// <exception cref="FileNotFoundException">Thrown if the model file does not exist.</exception>
        public WhisperService(string modelPath)
        {
            if (!File.Exists(modelPath))
            {
                throw new FileNotFoundException("Whisper model file not found.", modelPath);
            }
            _modelPath = modelPath;
            _whisperFactory = WhisperFactory.FromPath(_modelPath);

            // Optional: Point FFMpegCore to the location of your ffmpeg.exe if it's not in the system's PATH
            // GlobalFFOptions.Configure(new FFOptions { BinaryFolder = @"c:\path\to\ffmpeg\bin" });
        }

        /// <summary>
        /// Transcribes the audio from the given file path.
        /// </summary>
        /// <param name="audioPath">The path to the audio file to transcribe.</param>
        /// <returns>A Task representing the asynchronous transcription operation. The result will be the full transcribed text.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the audio file does not exist.</exception>
        /// <exception cref="Exception">Propagates exceptions from the transcription process.</exception>
        public async Task<string> TranscribeAsync(string audioPath, string language = "en", CancellationToken cancellationToken = default)
        {
            if (!File.Exists(audioPath))
            {
                throw new FileNotFoundException("Audio file not found.", audioPath);
            }

            string processedAudioPath = null;
            try
            {
                // Ensure the audio is in 16kHz WAV format, converting if necessary.
                processedAudioPath = await EnsureWav16KHzAsync(audioPath);
                
                var fullTranscription = new System.Text.StringBuilder();

                // Build the processor and configure it for transcription.
                await using var processor = _whisperFactory.CreateBuilder()
                    .WithLanguage(language) // Automatic language detection
                    .Build();

                await using var fileStream = File.OpenRead(processedAudioPath);
                
                // Process the audio file and stream the results.
                await foreach (var result in processor.ProcessAsync(fileStream).WithCancellation(cancellationToken))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    fullTranscription.AppendLine(result.Text);
                }

                return fullTranscription.ToString();
            }
            catch (OperationCanceledException)
            {
                // Handle cancellation gracefully
                return "Transcription cancelled.";
            }
            catch (Exception ex)
            {
                // Rethrow exceptions to be handled by the caller (e.g., the UI layer).
                Console.WriteLine($"Error during transcription: {ex.Message}");
                throw;
            }
            finally
            {
                // Clean up the temporary converted file if one was created.
                if (processedAudioPath != null && processedAudioPath != audioPath && File.Exists(processedAudioPath))
                {
                    File.Delete(processedAudioPath);
                }
            }
        }

        /// <summary>
        /// Ensures that the input audio file is a WAV file with a 16kHz sample rate.
        /// If it isn't, it converts the file using FFmpeg and saves it to a temporary path.
        /// </summary>
        /// <param name="inputPath">The path to the input audio file.</param>
        /// <returns>The path to the compliant 16kHz WAV file (either the original or a temporary one).</returns>
        private async Task<string> EnsureWav16KHzAsync(string inputPath)
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
                        // Convert float samples to 16-bit PCM
                        for (int i = 0; i < samplesRead; i++)
                        {
                            var sample = (short)(Math.Max(-1.0f, Math.Min(1.0f, buffer[i])) * short.MaxValue);
                            waveFileWriter.WriteByte((byte)(sample & 0xff));
                            waveFileWriter.WriteByte((byte)((sample >> 8) & 0xff));
                        }
                    }
                }
            }
            return tempPath;
        }

        /// <summary>
        /// Disposes the resources used by the WhisperService.
        /// </summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                if (_whisperFactory is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                _disposed = true;
            }
        }
    }
}
