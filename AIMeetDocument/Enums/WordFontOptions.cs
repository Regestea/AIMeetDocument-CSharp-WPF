namespace AIMeetDocument.Enums;

public class WordFontOptions
{
    public WordFontFamily DefaultFontFamily { get; set; } = WordFontFamily.Calibri;
    public WordFontFamily HeaderFontFamily { get; set; } = WordFontFamily.Calibri;
    public int DefaultFontSizePt { get; set; } = 12; // in points
    public int Header1FontSizePt { get; set; } = 16;
    public int Header2FontSizePt { get; set; } = 14;
    public int Header3FontSizePt { get; set; } = 13;
    public int Header4FontSizePt { get; set; } = 12;
    public bool HeaderBold { get; set; } = true;
    public WordFontStyle DefaultFontStyle { get; set; } = WordFontStyle.Normal;
    public WordFontStyle HeaderFontStyle { get; set; } = WordFontStyle.Normal;

    public string GetFontFamilyName(WordFontFamily family)
    {
        return family switch
        {
            WordFontFamily.Calibri => "Calibri",
            WordFontFamily.Arial => "Arial",
            WordFontFamily.ComicSansMS => "Comic Sans MS",
            WordFontFamily.SegoeUI => "Segoe UI",
            WordFontFamily.TimesNewRoman => "Times New Roman",
            _ => "Calibri"
        };
    }
    public string GetFontStyleCss(WordFontStyle style) => style == WordFontStyle.Italic ? "italic" : "normal";
}