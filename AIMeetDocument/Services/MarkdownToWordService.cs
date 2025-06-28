using System.IO;
using AIMeetDocument.Enums;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using HtmlToOpenXml;
using TextDirection = AIMeetDocument.Enums.TextDirection;

namespace AIMeetDocument.Services;

public class MarkdownToWordService
{
    public void ConvertMarkdownStringToDocx(string markdownContent, string outputFilePath, TextDirection direction = TextDirection.LTR, WordFontOptions? fontOptions = null)
    {
        fontOptions ??= new WordFontOptions();
        string htmlContent = Markdig.Markdown.ToHtml(markdownContent);
        
        string defaultFontCss = $@"<style>body {{ font-family: '{fontOptions.GetFontFamilyName(fontOptions.DefaultFontFamily)}', 'Arial', 'Calibri', 'Segoe UI', sans-serif; font-size: {fontOptions.DefaultFontSizePt}pt; font-style: {fontOptions.GetFontStyleCss(fontOptions.DefaultFontStyle)}; }}</style>";
        htmlContent = defaultFontCss + htmlContent;

        using (MemoryStream mem = new MemoryStream())
        {
            using (WordprocessingDocument wordDoc = WordprocessingDocument.Create(mem, DocumentFormat.OpenXml.WordprocessingDocumentType.Document, true))
            {
                MainDocumentPart mainPart = wordDoc.AddMainDocumentPart();
                mainPart.Document = new Document(new Body());
                
                var stylePart = mainPart.AddNewPart<StyleDefinitionsPart>();
                stylePart.Styles = new Styles(
                    new DocDefaults(
                        new RunPropertiesDefault(
                            new RunPropertiesBaseStyle(
                                new RunFonts { Ascii = fontOptions.GetFontFamilyName(fontOptions.DefaultFontFamily), HighAnsi = fontOptions.GetFontFamilyName(fontOptions.DefaultFontFamily), EastAsia = fontOptions.GetFontFamilyName(fontOptions.DefaultFontFamily), ComplexScript = fontOptions.GetFontFamilyName(fontOptions.DefaultFontFamily) },
                                new FontSize { Val = (fontOptions.DefaultFontSizePt * 2).ToString() },
                                fontOptions.DefaultFontStyle == WordFontStyle.Italic ? new Italic() : null
                            )
                        )
                    ),
                    new Style(
                        new StyleName { Val = "Normal" },
                        new BasedOn { Val = "Normal" },
                        new UIPriority { Val = 1 },
                        new PrimaryStyle(),
                        new StyleRunProperties(
                            new RunFonts { Ascii = fontOptions.GetFontFamilyName(fontOptions.DefaultFontFamily), HighAnsi = fontOptions.GetFontFamilyName(fontOptions.DefaultFontFamily), EastAsia = fontOptions.GetFontFamilyName(fontOptions.DefaultFontFamily), ComplexScript = fontOptions.GetFontFamilyName(fontOptions.DefaultFontFamily) },
                            new FontSize { Val = (fontOptions.DefaultFontSizePt * 2).ToString() },
                            fontOptions.DefaultFontStyle == WordFontStyle.Italic ? new Italic() : null
                        )
                    ) { Type = StyleValues.Paragraph, StyleId = "Normal" },
                    // Heading1 style
                    new Style(
                        new StyleName { Val = "heading 1" },
                        new BasedOn { Val = "Normal" },
                        new NextParagraphStyle { Val = "Normal" },
                        new UIPriority { Val = 9 },
                        new PrimaryStyle(),
                        new StyleParagraphProperties(
                            new KeepNext(),
                            new KeepLines(),
                            new SpacingBetweenLines { Before = "480", After = "0" },
                            new OutlineLevel { Val = 0 }
                        ),
                        new StyleRunProperties(
                            new RunFonts { Ascii = fontOptions.GetFontFamilyName(fontOptions.HeaderFontFamily), HighAnsi = fontOptions.GetFontFamilyName(fontOptions.HeaderFontFamily), EastAsia = fontOptions.GetFontFamilyName(fontOptions.HeaderFontFamily), ComplexScript = fontOptions.GetFontFamilyName(fontOptions.HeaderFontFamily) },
                            fontOptions.HeaderBold ? new Bold() : null,
                            new FontSize { Val = (fontOptions.Header1FontSizePt * 2).ToString() },
                            fontOptions.HeaderFontStyle == WordFontStyle.Italic ? new Italic() : null
                        )
                    ) { Type = StyleValues.Paragraph, StyleId = "Heading1" },
                    // Heading2 style
                    new Style(
                        new StyleName { Val = "heading 2" },
                        new BasedOn { Val = "Normal" },
                        new NextParagraphStyle { Val = "Normal" },
                        new UIPriority { Val = 9 },
                        new PrimaryStyle(),
                        new StyleParagraphProperties(
                            new KeepNext(),
                            new KeepLines(),
                            new SpacingBetweenLines { Before = "400", After = "0" },
                            new OutlineLevel { Val = 1 }
                        ),
                        new StyleRunProperties(
                            new RunFonts { Ascii = fontOptions.GetFontFamilyName(fontOptions.HeaderFontFamily), HighAnsi = fontOptions.GetFontFamilyName(fontOptions.HeaderFontFamily), EastAsia = fontOptions.GetFontFamilyName(fontOptions.HeaderFontFamily), ComplexScript = fontOptions.GetFontFamilyName(fontOptions.HeaderFontFamily) },
                            fontOptions.HeaderBold ? new Bold() : null,
                            new FontSize { Val = (fontOptions.Header2FontSizePt * 2).ToString() },
                            fontOptions.HeaderFontStyle == WordFontStyle.Italic ? new Italic() : null
                        )
                    ) { Type = StyleValues.Paragraph, StyleId = "Heading2" },
                    // Heading3 style
                    new Style(
                        new StyleName { Val = "heading 3" },
                        new BasedOn { Val = "Normal" },
                        new NextParagraphStyle { Val = "Normal" },
                        new UIPriority { Val = 9 },
                        new PrimaryStyle(),
                        new StyleParagraphProperties(
                            new KeepNext(),
                            new KeepLines(),
                            new SpacingBetweenLines { Before = "320", After = "0" },
                            new OutlineLevel { Val = 2 }
                        ),
                        new StyleRunProperties(
                            new RunFonts { Ascii = fontOptions.GetFontFamilyName(fontOptions.HeaderFontFamily), HighAnsi = fontOptions.GetFontFamilyName(fontOptions.HeaderFontFamily), EastAsia = fontOptions.GetFontFamilyName(fontOptions.HeaderFontFamily), ComplexScript = fontOptions.GetFontFamilyName(fontOptions.HeaderFontFamily) },
                            fontOptions.HeaderBold ? new Bold() : null,
                            new FontSize { Val = (fontOptions.Header3FontSizePt * 2).ToString() },
                            fontOptions.HeaderFontStyle == WordFontStyle.Italic ? new Italic() : null
                        )
                    ) { Type = StyleValues.Paragraph, StyleId = "Heading3" },
                    // Heading4 style
                    new Style(
                        new StyleName { Val = "heading 4" },
                        new BasedOn { Val = "Normal" },
                        new NextParagraphStyle { Val = "Normal" },
                        new UIPriority { Val = 9 },
                        new PrimaryStyle(),
                        new StyleParagraphProperties(
                            new KeepNext(),
                            new KeepLines(),
                            new SpacingBetweenLines { Before = "240", After = "0" },
                            new OutlineLevel { Val = 3 }
                        ),
                        new StyleRunProperties(
                            new RunFonts { Ascii = fontOptions.GetFontFamilyName(fontOptions.HeaderFontFamily), HighAnsi = fontOptions.GetFontFamilyName(fontOptions.HeaderFontFamily), EastAsia = fontOptions.GetFontFamilyName(fontOptions.HeaderFontFamily), ComplexScript = fontOptions.GetFontFamilyName(fontOptions.HeaderFontFamily) },
                            fontOptions.HeaderBold ? new Bold() : null,
                            new FontSize { Val = (fontOptions.Header4FontSizePt * 2).ToString() },
                            fontOptions.HeaderFontStyle == WordFontStyle.Italic ? new Italic() : null
                        )
                    ) { Type = StyleValues.Paragraph, StyleId = "Heading4" }
                );
                
                var sectionProps = mainPart.Document.Body.GetFirstChild<SectionProperties>() ?? new SectionProperties();
                sectionProps.RemoveAllChildren<BiDi>();
                if (direction == TextDirection.RTL)
                {
                    sectionProps.AppendChild(new BiDi());
                }
                if (mainPart.Document.Body.GetFirstChild<SectionProperties>() == null)
                    mainPart.Document.Body.AppendChild(sectionProps);
                
                var converter = new HtmlConverter(mainPart);
                var paragraphs = converter.Parse(htmlContent);
                
                foreach (var p in paragraphs)
                {
                    if (p is Paragraph para)
                    {
                        var pPr = para.ParagraphProperties ?? new ParagraphProperties();
                        pPr.RemoveAllChildren<BiDi>();
                        if (direction == TextDirection.RTL)
                        {
                            pPr.AppendChild(new BiDi());
                        }
                        para.ParagraphProperties = pPr;
                    }
                }
                mainPart.Document.Body.Append(paragraphs);
            }
            File.WriteAllBytes(outputFilePath, mem.ToArray());
        }
    }
}
