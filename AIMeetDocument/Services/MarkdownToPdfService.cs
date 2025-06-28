using AIMeetDocument.Enums;
using DinkToPdf;
using Markdig;

namespace AIMeetDocument.Services;

public class MarkdownToPdfService
{
    public void ConvertMarkdownStringToPdf(string markdownContent, string outputFilePath, PdfTextDirection direction = PdfTextDirection.LTR, PdfFontOptions? fontOptions = null)
    {
        fontOptions ??= new PdfFontOptions();
        string htmlContent = Markdown.ToHtml(markdownContent);
        
        string bold = fontOptions.HeaderBold ? "font-weight:bold;" : "";
        string dir = direction == PdfTextDirection.RTL ? "rtl" : "ltr";
        string defaultFontCss = $@"<style>
            html, body, h1, h2, h3, h4, h5, h6, p, div, span, ul, ol, li, table, th, td, blockquote, pre, code, strong, em, b, i, u, a, img, section, article, aside, header, footer, nav, main, figure, figcaption, details, summary, mark, small, sub, sup, caption, label, input, textarea, select, option, button, dl, dt, dd, hr, br, address, cite, abbr, acronym, del, ins, kbd, samp, var, s, strike, tt, big, center, fieldset, legend, form, output, progress, meter {{
                font-family: '{fontOptions.GetFontFamilyName(fontOptions.DefaultFontFamily)}', 'Arial', sans-serif !important;
                font-size: {fontOptions.DefaultFontSizePt}pt !important;
                font-style: {fontOptions.GetFontStyleCss(fontOptions.DefaultFontStyle)} !important;
                direction: {dir} !important;
            }}
            h1 {{ font-family: '{fontOptions.GetFontFamilyName(fontOptions.HeaderFontFamily)}', 'Arial', sans-serif !important; font-size: {fontOptions.Header1FontSizePt}pt !important; font-style: {fontOptions.GetFontStyleCss(fontOptions.HeaderFontStyle)} !important; {bold} }}
            h2 {{ font-family: '{fontOptions.GetFontFamilyName(fontOptions.HeaderFontFamily)}', 'Arial', sans-serif !important; font-size: {fontOptions.Header2FontSizePt}pt !important; font-style: {fontOptions.GetFontStyleCss(fontOptions.HeaderFontStyle)} !important; {bold} }}
            h3 {{ font-family: '{fontOptions.GetFontFamilyName(fontOptions.HeaderFontFamily)}', 'Arial', sans-serif !important; font-size: {fontOptions.Header3FontSizePt}pt !important; font-style: {fontOptions.GetFontStyleCss(fontOptions.HeaderFontStyle)} !important; {bold} }}
            h4 {{ font-family: '{fontOptions.GetFontFamilyName(fontOptions.HeaderFontFamily)}', 'Arial', sans-serif !important; font-size: {fontOptions.Header4FontSizePt}pt !important; font-style: {fontOptions.GetFontStyleCss(fontOptions.HeaderFontStyle)} !important; {bold} }}
        </style>";
        htmlContent = defaultFontCss + htmlContent;
        
        var converter = new SynchronizedConverter(new PdfTools());
        var doc = new HtmlToPdfDocument()
        {
            GlobalSettings = new GlobalSettings
            {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Out = outputFilePath
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
}