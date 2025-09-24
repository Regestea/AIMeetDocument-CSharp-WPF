using AIMeetDocument.DTOs;
using AIMeetDocument.Enums;
using DinkToPdf;
using Markdig;

namespace AIMeetDocument.Services;

public class MarkdownToPdfService
{
    /// <summary>
    /// Converts a Markdown string to a PDF file using a professional, clean stylesheet.
    /// </summary>
    /// <param name="markdownContent">The Markdown content to convert.</param>
    /// <param name="outputFilePath">The path where the output PDF will be saved.</param>
    /// <param name="direction">The text direction (LTR or RTL).</param>
    /// <param name="fontOptions">Optional font and size settings.</param>
    public void ConvertMarkdownStringToPdf(string markdownContent, string outputFilePath, FontOptions fontOptions)
    {


        // 1. Convert Markdown to HTML
        string htmlContent = Markdown.ToHtml(markdownContent);

        // 2. Generate a professional stylesheet based on options
        string stylesheet = GetPdfStylesheet(fontOptions, fontOptions.TextDirection);

        // 3. Prepend the stylesheet to the HTML content
        htmlContent = stylesheet + htmlContent;

        // 4. Configure and run the PDF conversion
        var converter = new SynchronizedConverter(new PdfTools());
        var doc = new HtmlToPdfDocument()
        {
            GlobalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Out = outputFilePath,
                Margins = new MarginSettings { Top = 15, Bottom = 15, Left = 15, Right = 15 }
            },
            Objects = {
                new ObjectSettings
                {
                    HtmlContent = htmlContent,
                    WebSettings = { DefaultEncoding = "utf-8" }
                }
            }
        };
        converter.Convert(doc);
    }

    /// <summary>
    /// Generates a well-structured CSS string with professional styling.
    /// </summary>
    private string GetPdfStylesheet(FontOptions options, TextDirection direction)
    {
        string dir = direction == TextDirection.RTL ? "rtl" : "ltr";
        string headerBold = options.HeaderBold ? "font-weight: 600;" : "font-weight: normal;";

        // Using a verbatim string literal (@"...") for clean, multi-line CSS
        return $@"
        <style>
            /* General Body Styles */
            body {{
                font-family: '{options.GetFontFamilyName(options.DefaultFontFamily)}', Arial, sans-serif;
                font-size: {options.DefaultFontSizePt}pt;
                font-style: {options.GetFontStyleCss(options.DefaultFontStyle)};
                line-height: 1.6;
                color: #333;
                direction: {dir};
            }}

            /* Headings */
            h1, h2, h3, h4, h5, h6 {{
                font-family: '{options.GetFontFamilyName(options.HeaderFontFamily)}', Arial, sans-serif;
                font-style: {options.GetFontStyleCss(options.HeaderFontStyle)};
                {headerBold}
                margin-bottom: 0.75em;
                line-height: 1.2;
            }}
            h1 {{ font-size: {options.Header1FontSizePt}pt; }}
            h2 {{ font-size: {options.Header2FontSizePt}pt; }}
            h3 {{ font-size: {options.Header3FontSizePt}pt; }}
            h4 {{ font-size: {options.Header4FontSizePt}pt; }}

            /* Other Elements */
            p {{ margin-bottom: 1.2em; }}
            ul, ol {{ margin-bottom: 1.2em; padding-left: 2em; }}
            a {{ color: #007bff; text-decoration: none; }}

            /* Code Blocks */
            pre {{
                background-color: #f1f1f1;
                border: 1px solid #ddd;
                border-radius: 4px;
                padding: 1em;
                margin-bottom: 1.2em;
                white-space: pre-wrap; /* Allows wrapping of long lines */
                font-size: 0.9em;
            }}
            code {{
                font-family: 'Courier New', Courier, monospace;
            }}
        </style>";
    }
}