using System.Collections.Generic;
using PageLeaf.Models.Css.Elements;

namespace PageLeaf.Models.Css
{
    public class CssStyleInfo
    {
        public BodyStyle Body { get; } = new BodyStyle();
        public TitleStyle Title { get; } = new TitleStyle();
        public ParagraphStyle Paragraph { get; } = new ParagraphStyle();
        public Dictionary<string, HeadingStyle> Headings { get; } = new Dictionary<string, HeadingStyle>
        {
            { "h1", new HeadingStyle() },
            { "h2", new HeadingStyle() },
            { "h3", new HeadingStyle() },
            { "h4", new HeadingStyle() },
            { "h5", new HeadingStyle() },
            { "h6", new HeadingStyle() }
        };
        public ListStyle List { get; } = new ListStyle();
        public BlockquoteStyle Blockquote { get; } = new BlockquoteStyle();
        public TableStyle Table { get; } = new TableStyle();
        public CodeStyle Code { get; } = new CodeStyle();
        public FootnoteStyle Footnote { get; } = new FootnoteStyle();

        public string? BodyTextColor { get; set; }
        public string? BodyBackgroundColor { get; set; }
        public string? BodyFontSize { get; set; }
        public string? BodyFontFamily { get; set; }
        public string? ParagraphLineHeight { get; set; }
        public string? ParagraphMarginBottom { get; set; }
        public string? ParagraphTextIndent { get; set; }
        public string? TitleTextColor { get; set; }
        public string? TitleFontSize { get; set; }
        public string? TitleFontFamily { get; set; }
        public string? TitleAlignment { get; set; }
        public string? TitleMarginBottom { get; set; }
        public HeadingStyleFlags TitleStyleFlags { get; set; } = new HeadingStyleFlags();
        public string? HeadingTextColor { get; set; }
        public Dictionary<string, string?> HeadingTextColors { get; } = new Dictionary<string, string?>();
        public Dictionary<string, string?> HeadingFontSizes { get; } = new Dictionary<string, string?>();
        public Dictionary<string, string?> HeadingFontFamilies { get; } = new Dictionary<string, string?>();
        public Dictionary<string, string?> HeadingAlignments { get; } = new Dictionary<string, string?>();
        public Dictionary<string, string?> HeadingMarginTops { get; } = new Dictionary<string, string?>();
        public Dictionary<string, string?> HeadingMarginBottoms { get; } = new Dictionary<string, string?>();
        public Dictionary<string, HeadingStyleFlags> HeadingStyleFlags { get; } = new Dictionary<string, HeadingStyleFlags>();
        public string? QuoteTextColor { get; set; }
        public string? QuoteBackgroundColor { get; set; }
        public string? QuoteBorderColor { get; set; }
        public string? QuoteBorderWidth { get; set; }
        public string? QuoteBorderStyle { get; set; }
        public string? QuotePadding { get; set; }
        public string? QuoteBorderRadius { get; set; }
        public string? TableBorderColor { get; set; }
        public string? TableHeaderBackgroundColor { get; set; }
        public string? TableHeaderTextColor { get; set; }
        public string? TableHeaderFontSize { get; set; }
        public string? TableBorderWidth { get; set; }
        public string? TableBorderStyle { get; set; }
        public string? TableHeaderAlignment { get; set; }
        public string? TableCellPadding { get; set; }
        public string? TableWidth { get; set; }
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
        public string? ListLineHeight { get; set; }
        public Dictionary<string, bool> HeadingNumberingStates { get; } = new Dictionary<string, bool>();

        // Footnote Legacy Properties
        public string? FootnoteMarkerTextColor { get; set; }
        public string? FootnoteAreaFontSize { get; set; }
        public string? FootnoteAreaTextColor { get; set; }
        public string? FootnoteAreaMarginTop { get; set; }
        public string? FootnoteAreaBorderTopColor { get; set; }
        public string? FootnoteAreaBorderTopWidth { get; set; }
        public string? FootnoteAreaBorderTopStyle { get; set; }
        public string? FootnoteListItemLineHeight { get; set; }
    }
}
