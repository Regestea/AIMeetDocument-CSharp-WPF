using AIMeetDocument.DTOs;
using NAudio.Wave;

namespace AIMeetDocument.Services;

public class AudioAnalysisService
{
     /// <summary>
    /// Analyzes an audio file and returns the peak volume level for each one-second interval.
    /// </summary>
    /// <param name="filePath">The path to the audio file (e.g., .mp3, .wav).</param>
    /// <returns>Array of SecondPeakLevel objects per second.</returns>
    /// 0.025494637
    public static List<int> GetSilenceSeconds(string filePath, int minSilenceSecond = 3, float silenceThreshold = 0.03f)
    {
        var peaks = new List<SecondSilenceStatus>();
        var silentIntervals = new List<int>();
        int p1 = 0, p2 = minSilenceSecond;
        try
        {
            using (var reader = new AudioFileReader(filePath))
            {
                int samplesPerSecond = reader.WaveFormat.SampleRate * reader.WaveFormat.Channels;
                var buffer = new float[samplesPerSecond];
                int samplesRead, currentSecond = 0;
                while ((samplesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                {
                    float maxAmplitude = buffer.Take(samplesRead).Select(Math.Abs).DefaultIfEmpty(0f).Max();
                    peaks.Add(new SecondSilenceStatus { Second = currentSecond, Silence = maxAmplitude < silenceThreshold });
                    if (peaks.Count >= minSilenceSecond)
                    {
                        int silenceCount = peaks.Skip(p1).Take(minSilenceSecond).Count(x => x.Silence);
                        if (silenceCount > minSilenceSecond / 2)
                        {
                            if (!silentIntervals.Any() || p1 - silentIntervals.Last() > 300)
                                silentIntervals.Add(p1);
                        }
                        p1++;
                        p2++;
                    }
                    currentSecond++;
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error analyzing audio: {ex.Message}");
        }
        
        if (silentIntervals.Any())
        {
            silentIntervals.RemoveAt(0);
        }
        
        return silentIntervals;
    }
     
    public float GetAvgMinThreshold(string filePath)
    {
        var thresholds = new List<float>();
        try
        {
            using (var reader = new AudioFileReader(filePath))
            {
                int samplesPerSecond = reader.WaveFormat.SampleRate * reader.WaveFormat.Channels;
                var buffer = new float[samplesPerSecond];
                int samplesRead;
                while ((samplesRead = reader.Read(buffer, 0, buffer.Length)) > 0)
                {
                    float maxAmplitude = buffer.Take(samplesRead).Select(Math.Abs).DefaultIfEmpty(0f).Max();
                    thresholds.Add(maxAmplitude);
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error calculating thresholds: {ex.Message}");
        }

        if (thresholds.Count == 0)
            return 0.02f;
        return thresholds.OrderBy(x => x).Take(Math.Max(1, thresholds.Count / 10)).Average();
    }
}