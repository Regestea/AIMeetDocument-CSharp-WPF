using System.IO;
using FFMpegCore;
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
    public class WhisperService
    {
        private readonly string _modelPath;
        private readonly WhisperFactory _whisperFactory;

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
        /// <param name="progress">An optional progress reporter to receive transcription segments as they are processed.</param>
        /// <returns>A Task representing the asynchronous transcription operation. The result will be the full transcribed text.</returns>
        /// <exception cref="FileNotFoundException">Thrown if the audio file does not exist.</exception>
        /// <exception cref="Exception">Propagates exceptions from the transcription process.</exception>
        public async Task<string> TranscribeAsync(string audioPath, IProgress<string> progress = null)
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
                using var processor = _whisperFactory.CreateBuilder()
                    .WithLanguage("auto") // Automatic language detection
                    .Build();

                using var fileStream = File.OpenRead(processedAudioPath);
                
                // Process the audio file and stream the results.
                await foreach (var result in processor.ProcessAsync(fileStream))
                {
                    string segment = $"{result.Start} -> {result.End}: {result.Text}";
                    fullTranscription.AppendLine(segment);
                    progress?.Report(segment); // Report progress to the UI thread if a handler is provided.
                }

                return fullTranscription.ToString();
            }
            catch (Exception ex)
            {
                // Rethrow exceptions to be handled by the caller (e.g., the UI layer).
                Console.WriteLine($"Error during transcription: {ex.Message}");
                throw;
            }
            finally
            {
                // Clean up the temporary converted file, if one was created.
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
            // First, do a quick check to see if the file is already a compliant WAV file.
            // This avoids unnecessary conversion.
            if (Path.GetExtension(inputPath).Equals(".wav", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    using var reader = new WaveFileReader(inputPath);
                    if (reader.WaveFormat.SampleRate == 16000 && reader.WaveFormat.Channels == 1 && reader.WaveFormat.Encoding == WaveFormatEncoding.Pcm)
                    {
                        // File is already in the correct format (16kHz, mono, PCM).
                        return inputPath;
                    }
                }
                catch (Exception ex)
                {
                     // Could be a malformed WAV header, proceed to FFmpeg for a more robust conversion.
                     Console.WriteLine($"NAudio could not read WAV file, falling back to FFmpeg. Error: {ex.Message}");
                }
            }

            // If the file is not a compliant WAV, use FFmpeg to convert it.
            string tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}.wav");

            await FFMpegArguments
                .FromFileInput(inputPath)
                .OutputToFile(tempPath, true, options => options
                    .WithAudioCodec("pcm_s16le")      // Standard for WAV files
                    .WithAudioSamplingRate(16000)     // Resample to 16kHz
                    .WithCustomArgument("-ac 1"))     // Set audio channels to 1 (mono). This is a more robust method.
                .ProcessAsynchronously();
            
            return tempPath;
        }
    }
}
