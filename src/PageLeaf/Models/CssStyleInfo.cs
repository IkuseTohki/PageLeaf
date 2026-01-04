using System.Collections.Generic;

namespace PageLeaf.Models
{
    public class CssStyleInfo
    {
        public string? BodyTextColor { get; set; }
        public string? BodyBackgroundColor { get; set; }
        public string? BodyFontSize { get; set; }
        public string? HeadingTextColor { get; set; }
        public Dictionary<string, string?> HeadingTextColors { get; } = new Dictionary<string, string?>();
        public Dictionary<string, string?> HeadingFontSizes { get; } = new Dictionary<string, string?>();
        public Dictionary<string, string?> HeadingFontFamilies { get; } = new Dictionary<string, string?>();
        public Dictionary<string, string?> HeadingAlignments { get; } = new Dictionary<string, string?>();
        public Dictionary<string, HeadingStyleFlags> HeadingStyleFlags { get; } = new Dictionary<string, HeadingStyleFlags>();
        public string? QuoteTextColor { get; set; }
        public string? QuoteBackgroundColor { get; set; }
        public string? QuoteBorderColor { get; set; }
        public string? QuoteBorderWidth { get; set; }
        public string? QuoteBorderStyle { get; set; }
        public string? TableBorderColor { get; set; }
        public string? TableHeaderBackgroundColor { get; set; }
        public string? TableHeaderTextColor { get; set; }
        public string? TableHeaderFontSize { get; set; }
        public string? TableBorderWidth { get; set; }
        public string? TableBorderStyle { get; set; }
        public string? TableHeaderAlignment { get; set; }
        public string? TableCellPadding { get; set; }
        public string? CodeTextColor { get; set; } // General/Inline
        public string? CodeBackgroundColor { get; set; } // General/Inline
        public string? InlineCodeTextColor { get; set; }
        public string? InlineCodeBackgroundColor { get; set; }
        public string? BlockCodeTextColor { get; set; }
        public string? BlockCodeBackgroundColor { get; set; }
        public bool IsCodeBlockOverrideEnabled { get; set; }
        public string? CodeFontFamily { get; set; }
        public string? ListMarkerType { get; set; }
        public string? NumberedListMarkerType { get; set; }
        public string? ListMarkerSize { get; set; }
        public string? ListIndent { get; set; }
        public Dictionary<string, bool> HeadingNumberingStates { get; } = new Dictionary<string, bool>();
    }
}
