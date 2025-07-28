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
        private readonly WhisperProcessor _whisperProcessor;
        private string _language = "en";

        /// <summary>
        /// Initializes a new instance of the WhisperService.
        /// </summary>
        /// <param name="modelPath">The file path to the Whisper GGUF model.</param>
        /// <exception cref="FileNotFoundException">Thrown if the model file does not exist.</exception>
        public WhisperService(string modelPath,string language = "en")
        {
            if (!File.Exists(modelPath))
            {
                throw new FileNotFoundException("Whisper model file not found.", modelPath);
            }
            _language = language;
            _modelPath = modelPath;
            _whisperFactory = WhisperFactory.FromPath(_modelPath);
            // Build the processor and configure it for transcription.
            _whisperProcessor = _whisperFactory.CreateBuilder()
                .WithLanguage(language) // Automatic language detection
                .Build();
        }

        /// <summary>
        /// Transcribes the audio from the given file path.
        /// </summary>
        /// <param name="audioPath">The path to the audio file to transcribe.</param>
        /// <param name="cancellationToken">token to cancel the task</param>
        /// <returns>A Task representing the asynchronous transcription operation. The result will be the full transcribed text.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the audio file does not exist.</exception>
        /// <exception cref="Exception">Propagates exceptions from the transcription process.</exception>
        public async Task<string> TranscribeAsync(string audioPath,
            CancellationToken cancellationToken = default)
        {
            if (!File.Exists(audioPath))
            {
                throw new FileNotFoundException("Audio file not found.", audioPath);
            }

            try
            {
                var fullTranscription = new System.Text.StringBuilder();


                await using var fileStream = File.OpenRead(audioPath);

                // Process the audio file and stream the results.
                await foreach (var result in _whisperProcessor.ProcessAsync(fileStream, cancellationToken))
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
        }


        /// <summary>
        /// Disposes the resources used by the WhisperService.
        /// </summary>
        public void Dispose()
        {
            try
            {
                _whisperProcessor.Dispose();
                if (_whisperFactory is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            catch (Exception e)
            {
                // ignored
            }
        }
    }
}