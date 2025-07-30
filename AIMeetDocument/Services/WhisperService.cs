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
        private static WhisperFactory? _sharedFactory;
        private static string? _sharedModelPath;
        private WhisperProcessor _whisperProcessor;
        private string _language = "en";

        /// <summary>
        /// Initializes a new instance of the WhisperService.
        /// </summary>
        /// <param name="modelPath">The file path to the Whisper GGUF model.</param>
        /// <exception cref="FileNotFoundException">Thrown if the model file does not exist.</exception>
        public WhisperService(string modelPath, string language = "en")
        {
            if (!File.Exists(modelPath))
            {
                throw new FileNotFoundException("Whisper model file not found.", modelPath);
            }
            _language = language;
            // Only create the factory once per app lifetime
            if (_sharedFactory == null || _sharedModelPath != modelPath)
            {
                _sharedFactory?.Dispose();
                _sharedFactory = WhisperFactory.FromPath(modelPath);
                _sharedModelPath = modelPath;
            }
            // Build a new processor for each service instance (thread safety)
            _whisperProcessor = _sharedFactory.CreateBuilder()
                .WithLanguage(language)
                .Build();
        }

        /// <summary>
        /// Transcribes an audio file and reports progress.
        /// </summary>
        /// <param name="audioPath">The path to the audio file.</param>
        /// <param name="progress">An optional provider to report progress percentage (0.0 to 100.0).</param>
        /// <param name="cancellationToken">A cancellation token.</param>
        /// <returns>The full transcription text.</returns>
        public async Task<string> TranscribeAsync(
            string audioPath,
            IProgress<double> progress = null,
            CancellationToken cancellationToken = default)
        {
            if (!File.Exists(audioPath))
            {
                throw new FileNotFoundException("Audio file not found.", audioPath);
            }

            try
            {
                var fullTranscription = new System.Text.StringBuilder();

                // Open the file and get its total size
                await using var fileStream = File.OpenRead(audioPath);
                long totalBytes = fileStream.Length;

                // Process the audio file and stream the results.
                await foreach (var result in _whisperProcessor.ProcessAsync(fileStream, cancellationToken))
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    // Calculate progress and report it if a progress handler is provided
                    if (totalBytes > 0)
                    {
                        // Calculate percentage from current position and total size
                        double percentage = (double)fileStream.Position / totalBytes * 100;
                        progress?.Report(percentage);
                    }

                    fullTranscription.AppendLine(result.Text);
                }

                // Report 100% completion at the end
                progress?.Report(100.0);

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
        }


        /// <summary>
        /// Disposes the resources used by the WhisperService.
        /// </summary>
        public void Dispose()
        {
            try
            {
                _whisperProcessor?.Dispose();
                // Do NOT dispose _sharedFactory here; only on app exit
            }
            catch (Exception)
            {
                // ignored
            }
        }

        // Call this on app exit to free GPU memory
        public static void DisposeFactory()
        {
            try
            {
                _sharedFactory?.Dispose();
                _sharedFactory = null;
                _sharedModelPath = null;
            }
            catch (Exception)
            {
                // ignored
            }
        }
    }
}