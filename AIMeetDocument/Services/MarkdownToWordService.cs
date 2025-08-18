using System.IO;
using System.Text.RegularExpressions;
using AIMeetDocument.DTOs;
using AIMeetDocument.Enums;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using HtmlToOpenXml;
using Markdig;
using TextDirection = AIMeetDocument.Enums.TextDirection;

namespace AIMeetDocument.Services;

public class MarkdownToWordService
{
    public void ConvertMarkdownStringToDocx(string markdownContent, string outputFilePath, TextDirection direction = TextDirection.LTR, FontOptions? fontOptions = null)
    {
        fontOptions ??= FontOptions.CreateDefaults();
        string htmlContent = Markdig.Markdown.ToHtml(markdownContent);

        using (MemoryStream mem = new MemoryStream())
        {
            using (WordprocessingDocument wordDoc = WordprocessingDocument.Create(mem, WordprocessingDocumentType.Document, true))
            {
                MainDocumentPart mainPart = wordDoc.AddMainDocumentPart();
                mainPart.Document = new Document(new Body());

                CreateDocumentStyles(mainPart, fontOptions);
                SetTextDirection(mainPart.Document.Body, direction);

                var converter = new HtmlConverter(mainPart);

                // --- NEW: Process the document by splitting it into normal parts and code blocks ---
                // This pattern splits the HTML by <pre> blocks, but keeps the blocks in the resulting array.
                string[] parts = Regex.Split(htmlContent, @"(<pre>[\s\S]*?</pre>)");

                foreach (string part in parts)
                {
                    if (string.IsNullOrWhiteSpace(part)) continue;

                    // Check if this part is a code block.
                    if (part.StartsWith("<pre"))
                    {
                        // It's a code block, so we handle it manually for perfect formatting.
                        var codeMatch = Regex.Match(part, @"<code[^>]*>([\s\S]*?)</code>");
                        string codeText = codeMatch.Success ? codeMatch.Groups[1].Value : "";

                        mainPart.Document.Body.Append(CreateFormattedCodeBlock(codeText));
                    }
                    else
                    {
                        // It's a normal HTML part, so let the converter handle it.
                        var elements = converter.Parse(part);
                        mainPart.Document.Body.Append(elements);
                    }
                }
            }
            
            File.WriteAllBytes(outputFilePath, mem.ToArray());
        }
    }
    
    /// <summary>
    /// Manually creates a Paragraph for a code block, preserving all line breaks.
    /// </summary>
    private static Paragraph CreateFormattedCodeBlock(string codeContent)
    {
        var paragraph = new Paragraph(
            new ParagraphProperties(
                new ParagraphStyleId() { Val = "Code" } // Apply the "Code" paragraph style
            ));

        // HTML-decode the content to handle characters like &lt; and &gt;
        string decodedCode = System.Net.WebUtility.HtmlDecode(codeContent);
        var lines = decodedCode.Split('\n');

        for (int i = 0; i < lines.Length; i++)
        {
            var run = new Run(
                new Text(lines[i]) { Space = SpaceProcessingModeValues.Preserve }
            );
            paragraph.Append(run);

            if (i < lines.Length - 1)
            {
                paragraph.Append(new Run(new Break()));
            }
        }
        return paragraph;
    }

    // --- The Style Definition Methods below are unchanged ---

    private static void CreateDocumentStyles(MainDocumentPart mainPart, FontOptions options)
    {
        var stylePart = mainPart.AddNewPart<StyleDefinitionsPart>();
        stylePart.Styles = new Styles();

        var runDefaults = new RunPropertiesDefault(
            new RunPropertiesBaseStyle(
                new RunFonts { Ascii = options.GetFontFamilyName(options.DefaultFontFamily), HighAnsi = options.GetFontFamilyName(options.DefaultFontFamily) },
                new FontSize { Val = (options.DefaultFontSizePt * 2).ToString() }
            )
        );
        stylePart.Styles.Append(new DocDefaults(runDefaults));

        var normalStyle = new Style(
            new StyleName { Val = "Normal" },
            new PrimaryStyle()
        ) { Type = StyleValues.Paragraph, StyleId = "Normal", Default = true };
        stylePart.Styles.Append(normalStyle);

        stylePart.Styles.Append(CreateHeadingStyle("1", options.Header1FontSizePt, options));
        stylePart.Styles.Append(CreateHeadingStyle("2", options.Header2FontSizePt, options));
        stylePart.Styles.Append(CreateHeadingStyle("3", options.Header3FontSizePt, options));
        stylePart.Styles.Append(CreateHeadingStyle("4", options.Header4FontSizePt, options));

        stylePart.Styles.Append(CreateCodeBlockStyle());
        stylePart.Styles.Append(CreateInlineCodeStyle());
    }
    
    private static Style CreateHeadingStyle(string level, int fontSizePt, FontOptions options)
    {
        return new Style(
            new StyleName { Val = $"heading {level}" },
            new BasedOn { Val = "Normal" },
            new NextParagraphStyle { Val = "Normal" },
            new StyleRunProperties(
                new RunFonts { Ascii = options.GetFontFamilyName(options.HeaderFontFamily), HighAnsi = options.GetFontFamilyName(options.HeaderFontFamily) },
                options.HeaderBold ? new Bold() : null,
                new FontSize { Val = (fontSizePt * 2).ToString() }
            )
        ) { Type = StyleValues.Paragraph, StyleId = $"Heading{level}" };
    }
    
    private static Style CreateCodeBlockStyle()
    {
        return new Style(
            new StyleName { Val = "Code" },
            new BasedOn { Val = "Normal" },
            new StyleParagraphProperties(
                new Shading() { Val = ShadingPatternValues.Clear, Fill = "F1F1F1" },
                new SpacingBetweenLines() { Before = "120", After = "120" }
            ),
            new StyleRunProperties(
                new RunFonts { Ascii = "Courier New", HighAnsi = "Courier New" },
                new FontSize { Val = "20" }
            )
        ) { Type = StyleValues.Paragraph, StyleId = "Code" };
    }
    
    private static Style CreateInlineCodeStyle()
    {
        return new Style(
            new StyleName { Val = "Code Char" },
            new StyleRunProperties(
                new RunFonts { Ascii = "Courier New", HighAnsi = "Courier New" },
                new Shading() { Val = ShadingPatternValues.Clear, Fill = "F5F5F5" },
                new FontSize { Val = "19" }
            )
        ) { Type = StyleValues.Character, StyleId = "CodeChar" };
    }

    private static void SetTextDirection(Body body, TextDirection direction)
    {
        var sectionProps = body.GetFirstChild<SectionProperties>() ?? new SectionProperties();
        if (direction == TextDirection.RTL)
        {
            sectionProps.AppendChild(new BiDi());
        }
        if (body.GetFirstChild<SectionProperties>() == null)
        {
            body.AppendChild(sectionProps);
        }
    }
}