using System.Collections.Generic;

namespace PageLeaf.Models
{
    public class CssStyleInfo
    {
        public string? BodyTextColor { get; set; }
        public string? BodyBackgroundColor { get; set; }
        public string? BodyFontSize { get; set; }
        public string? HeadingTextColor { get; set; }
        public Dictionary<string, string> HeadingTextColors { get; } = new();
        public Dictionary<string, string> HeadingFontSizes { get; } = new();
        public Dictionary<string, string> HeadingFontFamilies { get; } = new();
        public Dictionary<string, HeadingStyleFlags> HeadingStyleFlags { get; } = new();
        public string? QuoteTextColor { get; set; }
        public string? QuoteBackgroundColor { get; set; }
        public string? QuoteBorderColor { get; set; }
        public string? QuoteBorderWidth { get; set; }
        public string? QuoteBorderStyle { get; set; }
        public string? TableBorderColor { get; set; }
        public string? TableHeaderBackgroundColor { get; set; }
        public string? TableBorderWidth { get; set; }
        public string? TableBorderStyle { get; set; }
        public string? TableCellPadding { get; set; }
        public string? CodeTextColor { get; set; }
        public string? CodeBackgroundColor { get; set; }
        public string? CodeFontFamily { get; set; }
        public string? ListMarkerType { get; set; }
        public string? ListIndent { get; set; }
    }

    public class HeadingStyleFlags
    {
        public bool IsBold { get; set; }
        public bool IsItalic { get; set; }
        public bool IsUnderline { get; set; }
        public bool IsStrikethrough { get; set; }
    }
}
