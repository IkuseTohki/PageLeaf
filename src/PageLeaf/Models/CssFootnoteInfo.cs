namespace PageLeaf.Models
{
    public class CssFootnoteInfo
    {
        // Marker (.footnote-ref)
        public string? MarkerTextColor { get; set; }
        public bool IsMarkerBold { get; set; }
        public bool HasMarkerBrackets { get; set; } // [1] vs 1

        // Area (.footnotes)
        public string? AreaFontSize { get; set; }
        public string? AreaTextColor { get; set; }
        public string? AreaMarginTop { get; set; }

        // Area Border (.footnotes hr) - Markdown renderers often use <hr> or border on div
        public string? AreaBorderTopColor { get; set; }
        public string? AreaBorderTopWidth { get; set; }
        public string? AreaBorderTopStyle { get; set; }

        // List Item (.footnotes li)
        public string? ListItemLineHeight { get; set; }

        // Back Link (.footnote-back-ref)
        public bool IsBackLinkVisible { get; set; } = true;
    }
}
