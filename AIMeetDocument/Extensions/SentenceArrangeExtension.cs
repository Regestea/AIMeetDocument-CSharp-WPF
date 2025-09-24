using System.Text.RegularExpressions;

namespace AIMeetDocument.Extensions;

public static class SentenceArrangeExtension
{
    
    public static List<string> ArrangeByPage(this List<string> input, int pagePreRequest)
    {
        if (pagePreRequest <= 0)
        {
            return input;
        }

        var result = new List<string>();
        for (int i = 0; i < input.Count; i += pagePreRequest)
        {
            var page = input.Skip(i).Take(pagePreRequest);
            result.Add(string.Join(" ", page));
        }

        return result;
    }
    
    public static List<string> ArrangeSentences(this List<string> input, int minLength)
    {
        // Handle null or empty input list by returning a new empty list.
        if (input == null || !input.Any())
        {
            return new List<string>();
        }

        var result = new List<string>();

        foreach (var sentence in input)
        {
            // Check if the result list has any items and if the *last* item added is shorter than the minimum length.
            if (result.Any() && result.Last().Length < minLength)
            {
                // If it is, append the current sentence to the last one.
                // We use the index for a slight performance gain over calling Last() again.
                result[result.Count - 1] += " " + sentence;
            }
            else
            {
                // Otherwise, the last sentence was long enough (or the list is empty),
                // so add the current sentence as a new item.
                result.Add(sentence);
            }
        }

        return result;
    }
    
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