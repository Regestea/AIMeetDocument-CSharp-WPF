namespace AIMeetDocument.Enums;

public class PdfFontOptions
{
    public PdfFontFamily DefaultFontFamily { get; set; } = PdfFontFamily.Calibri;
    public PdfFontFamily HeaderFontFamily { get; set; } = PdfFontFamily.Calibri;
    public int DefaultFontSizePt { get; set; } = 12;
    public int Header1FontSizePt { get; set; } = 16;
    public int Header2FontSizePt { get; set; } = 14;
    public int Header3FontSizePt { get; set; } = 13;
    public int Header4FontSizePt { get; set; } = 12;
    public bool HeaderBold { get; set; } = true;
    public PdfFontStyle DefaultFontStyle { get; set; } = PdfFontStyle.Normal;
    public PdfFontStyle HeaderFontStyle { get; set; } = PdfFontStyle.Normal;

    public string GetFontFamilyName(PdfFontFamily family)
    {
        return family switch
        {
            PdfFontFamily.Calibri => "Calibri",
            PdfFontFamily.Arial => "Arial",
            PdfFontFamily.ComicSansMS => "Comic Sans MS",
            PdfFontFamily.SegoeUI => "Segoe UI",
            PdfFontFamily.TimesNewRoman => "Times New Roman",
            _ => "Calibri"
        };
    }
    public string GetFontStyleCss(PdfFontStyle style) => style == PdfFontStyle.Italic ? "italic" : "normal";
}