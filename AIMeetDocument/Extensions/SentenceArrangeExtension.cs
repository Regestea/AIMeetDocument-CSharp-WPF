using System.Text.RegularExpressions;

namespace AIMeetDocument.Extensions;

public static class SentenceArrangeExtension
{
    public static List<string> ArrangeSentences(this List<string> input, int minLength, int maxLength)
    {
        var result = new List<string>();

        foreach (var text in input)
        {
            var sentences = Regex.Split(text, @"(?<=[.])\s*")
                .Where(s => !string.IsNullOrWhiteSpace(s))
                .ToList();
            
            sentences = sentences.Distinct().ToList();
            

            var buffer = "";

            int i = 0;
            while (i < sentences.Count)
            {
                var sentence = sentences[i];

                // Case 1: sentence too long → split it
                if (sentence.Length > maxLength)
                {
                    if (!string.IsNullOrWhiteSpace(buffer))
                    {
                        result.Add(buffer.Trim());
                        buffer = "";
                    }

                    var chunks = SplitSentenceByLength(sentence, maxLength);
                    foreach (var chunk in chunks)
                    {
                        result.Add(chunk);
                    }

                    i++;
                    continue;
                }

                // Case 2: try to pack sentences
                var temp = buffer.Length == 0 ? sentence : buffer + " " + sentence;

                if (temp.Length <= maxLength)
                {
                    buffer = temp;
                    i++;
                }
                else
                {
                    if (buffer.Length < minLength)
                    {
                        // If buffer too small, force add current sentence to it even if it exceeds max
                        buffer += " " + sentence;
                        result.Add(buffer.Trim());
                        buffer = "";
                        i++;
                    }
                    else
                    {
                        result.Add(buffer.Trim());
                        buffer = "";
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(buffer))
            {
                result.Add(buffer.Trim());
            }
        }

        return result;
    }

    private static List<string> SplitSentenceByLength(string sentence, int maxLength)
    {
        var parts = new List<string>();
        var words = sentence.Split(' ');

        var current = "";
        foreach (var word in words)
        {
            if ((current + " " + word).Trim().Length <= maxLength)
            {
                current += " " + word;
            }
            else
            {
                if (!string.IsNullOrWhiteSpace(current))
                {
                    parts.Add(current.Trim());
                    current = word;
                }
                else
                {
                    // Hard-split long word
                    for (int i = 0; i < word.Length; i += maxLength)
                    {
                        parts.Add(word.Substring(i, Math.Min(maxLength, word.Length - i)));
                    }
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(current))
        {
            parts.Add(current.Trim());
        }

        return parts;
    }
}