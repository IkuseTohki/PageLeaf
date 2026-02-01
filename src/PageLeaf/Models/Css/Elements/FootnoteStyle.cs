using AngleSharp.Css.Dom;
using PageLeaf.Models.Css.Values;
using System.Linq;

namespace PageLeaf.Models.Css.Elements
{
    /// <summary>
    /// 脚注要素（.footnote-ref, .footnotes 等）のスタイル設定を管理するドメインモデルです。
    /// </summary>
    public class FootnoteStyle
    {
        // マーカー (.footnote-ref)
        public CssColor? MarkerTextColor { get; set; }
        public bool IsMarkerBold { get; set; }
        public bool HasMarkerBrackets { get; set; }

        // エリア (.footnotes)
        public CssSize? AreaFontSize { get; set; }
        public CssColor? AreaTextColor { get; set; }
        public CssSize? AreaMarginTop { get; set; }

        // 区切り線 (.footnotes hr)
        public CssColor? AreaBorderTopColor { get; set; }
        public CssSize? AreaBorderTopWidth { get; set; }
        public string? AreaBorderTopStyle { get; set; }

        // リスト項目 (.footnotes li)
        public string? ListItemLineHeight { get; set; }

        // 戻りリンク (.footnote-back-ref)
        public bool IsBackLinkVisible { get; set; } = true;

        public void UpdateFrom(ICssStyleSheet stylesheet)
        {
            if (stylesheet == null) return;

            // Marker
            var markerRule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == ".footnote-ref");
            if (markerRule != null)
            {
                var color = markerRule.Style.GetPropertyValue("color");
                if (!string.IsNullOrEmpty(color)) MarkerTextColor = CssColor.Parse(color);

                var fontWeight = markerRule.Style.GetPropertyValue("font-weight");
                IsMarkerBold = fontWeight == "bold" || (int.TryParse(fontWeight, out var w) && w >= 700);
            }

            var markerBeforeRule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == ".footnote-ref::before");
            HasMarkerBrackets = markerBeforeRule?.Style.GetPropertyValue("content")?.Contains("[") == true;

            // Area
            var areaRule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == ".footnotes");
            if (areaRule != null)
            {
                var size = areaRule.Style.GetPropertyValue("font-size");
                if (!string.IsNullOrEmpty(size)) AreaFontSize = CssSize.Parse(size);

                var color = areaRule.Style.GetPropertyValue("color");
                if (!string.IsNullOrEmpty(color)) AreaTextColor = CssColor.Parse(color);

                var margin = areaRule.Style.GetPropertyValue("margin-top");
                if (!string.IsNullOrEmpty(margin)) AreaMarginTop = CssSize.Parse(margin);
            }

            // HR
            var hrRule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == ".footnotes hr");
            if (hrRule != null)
            {
                var width = hrRule.Style.GetPropertyValue("border-top-width");
                if (!string.IsNullOrEmpty(width)) AreaBorderTopWidth = CssSize.Parse(width);

                var color = hrRule.Style.GetPropertyValue("border-top-color");
                if (!string.IsNullOrEmpty(color)) AreaBorderTopColor = CssColor.Parse(color);

                var style = hrRule.Style.GetPropertyValue("border-top-style");
                if (!string.IsNullOrEmpty(style)) AreaBorderTopStyle = style;
            }

            // List Item
            var liRule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == ".footnotes li");
            if (liRule != null) ListItemLineHeight = liRule.Style.GetPropertyValue("line-height");

            // Back Link
            var backLinkRule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == ".footnote-back-ref");
            IsBackLinkVisible = backLinkRule?.Style.GetPropertyValue("display") != "none";
        }

        public void ApplyTo(ICssStyleSheet stylesheet)
        {
            if (stylesheet == null) return;

            // Marker
            var markerRule = GetOrCreateRule(stylesheet, ".footnote-ref");
            if (MarkerTextColor != null) markerRule.Style.SetProperty("color", MarkerTextColor.ToString());
            else markerRule.Style.RemoveProperty("color");

            markerRule.Style.SetProperty("font-weight", IsMarkerBold ? "bold" : "normal");
            markerRule.Style.SetProperty("vertical-align", "super");
            markerRule.Style.SetProperty("font-size", "smaller");
            markerRule.Style.SetProperty("text-decoration", "none");

            // Prevent double superscript
            var subSupRule = GetOrCreateRule(stylesheet, ".footnote-ref sup");
            subSupRule.Style.SetProperty("vertical-align", "baseline");
            subSupRule.Style.SetProperty("font-size", "100%");

            var beforeRule = GetOrCreateRule(stylesheet, ".footnote-ref::before");
            if (HasMarkerBrackets) beforeRule.Style.SetProperty("content", "'['");
            else beforeRule.Style.RemoveProperty("content");

            var afterRule = GetOrCreateRule(stylesheet, ".footnote-ref::after");
            if (HasMarkerBrackets) afterRule.Style.SetProperty("content", "']'");
            else afterRule.Style.RemoveProperty("content");

            // Area
            var areaRule = GetOrCreateRule(stylesheet, ".footnotes");
            if (AreaFontSize != null) areaRule.Style.SetProperty("font-size", AreaFontSize.ToString());
            if (AreaTextColor != null) areaRule.Style.SetProperty("color", AreaTextColor.ToString());
            if (AreaMarginTop != null) areaRule.Style.SetProperty("margin-top", AreaMarginTop.ToString());

            // HR
            var hrRule = GetOrCreateRule(stylesheet, ".footnotes hr");
            if (AreaBorderTopWidth != null || AreaBorderTopColor != null || !string.IsNullOrEmpty(AreaBorderTopStyle))
            {
                hrRule.Style.SetProperty("border", "0");
                var width = AreaBorderTopWidth?.ToString() ?? "1px";
                var style = !string.IsNullOrEmpty(AreaBorderTopStyle) ? AreaBorderTopStyle : "solid";
                var color = AreaBorderTopColor?.ToString() ?? "currentColor";
                hrRule.Style.SetProperty("border-top", $"{width} {style} {color}");
            }

            // List Item
            var liRule = GetOrCreateRule(stylesheet, ".footnotes li");
            if (!string.IsNullOrEmpty(ListItemLineHeight)) liRule.Style.SetProperty("line-height", ListItemLineHeight);

            // Back Link
            var backLinkRule = GetOrCreateRule(stylesheet, ".footnote-back-ref");
            if (!IsBackLinkVisible) backLinkRule.Style.SetProperty("display", "none");
            else backLinkRule.Style.RemoveProperty("display");
        }

        private ICssStyleRule GetOrCreateRule(ICssStyleSheet stylesheet, string selector)
        {
            var rule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == selector);
            if (rule == null)
            {
                stylesheet.Insert(selector + " {}", stylesheet.Rules.Length);
                rule = stylesheet.Rules.OfType<ICssStyleRule>().Last();
            }
            return rule;
        }
    }
}
