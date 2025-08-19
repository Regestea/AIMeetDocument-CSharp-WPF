using AIMeetDocument.Enums;

namespace AIMeetDocument.DTOs;

public class FontOptions
{
    // Default font properties
    public FontFamily DefaultFontFamily { get; set; } = FontFamily.Calibri;
    public FontFamily HeaderFontFamily { get; set; } = FontFamily.Calibri;
    public int DefaultFontSizePt { get; set; } = 12;
    public int Header1FontSizePt { get; set; } = 16;
    public int Header2FontSizePt { get; set; } = 14;
    public int Header3FontSizePt { get; set; } = 13;
    public int Header4FontSizePt { get; set; } = 12;
    public bool HeaderBold { get; set; } = true;
    public FontStyle DefaultFontStyle { get; set; } = FontStyle.Normal;
    public FontStyle HeaderFontStyle { get; set; } = FontStyle.Normal;

    /// <summary>
    /// Gets the font family name as a string for the specified FontFamily enum
    /// </summary>
    /// <param name="family">The FontFamily enum value</param>
    /// <returns>The font family name as a string</returns>
    public string GetFontFamilyName(FontFamily family)
    {
        return family switch
        {
            FontFamily.Calibri => "Calibri",
            FontFamily.Arial => "Arial",
            FontFamily.ComicSansMS => "Comic Sans MS",
            FontFamily.SegoeUI => "Segoe UI",
            FontFamily.TimesNewRoman => "Times New Roman",
            FontFamily.Gadugi => "Gadugi",
            _ => "Calibri"
        };
    }

    /// <summary>
    /// Gets the CSS font style value for the specified FontStyle enum
    /// </summary>
    /// <param name="style">The FontStyle enum value</param>
    /// <returns>The CSS font style string</returns>
    public string GetFontStyleCss(FontStyle style) => style == FontStyle.Italic ? "italic" : "normal";

    /// <summary>
    /// Creates a FontOptions instance with defaults based on the provided font family and base font size.
    /// Other sizes are derived from the base size.
    /// </summary>
    /// <param name="family">The default/header font family.</param>
    /// <param name="baseSizePt">The base font size in points for normal text.</param>
    /// <returns>A FontOptions instance configured accordingly.</returns>
    public static FontOptions CreateDefaults(FontFamily family, int baseSizePt)
    {
        return new FontOptions
        {
            DefaultFontFamily = family,
            HeaderFontFamily = family,
            DefaultFontSizePt = baseSizePt,
            Header1FontSizePt = baseSizePt + 4,
            Header2FontSizePt = baseSizePt + 2,
            Header3FontSizePt = baseSizePt + 1,
            Header4FontSizePt = baseSizePt,
            HeaderBold = true,
            DefaultFontStyle = FontStyle.Normal,
            HeaderFontStyle = FontStyle.Normal
        };
    }

    /// <summary>
    /// Creates a FontOptions instance with default values.
    /// </summary>
    /// <returns>A FontOptions instance configured for default usage.</returns>
    public static FontOptions CreateDefaults()
    {
        return CreateDefaults(FontFamily.Gadugi, 12);
    }
}
