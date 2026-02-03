using AngleSharp.Css.Dom;
using PageLeaf.Models.Css.Values;
using System;
using System.Linq;
using System.Text.RegularExpressions;

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
        public CssBorder? AreaBorder { get; set; }

        // Helpers for synchronization
        public string? AreaBorderTopWidth
        {
            get => AreaBorder?.Width?.ToString();
            set { if (!string.IsNullOrEmpty(value)) EnsureBorder().Width = CssSize.Parse(value); }
        }
        public string? AreaBorderTopStyle
        {
            get => AreaBorder?.Style;
            set { if (!string.IsNullOrEmpty(value)) EnsureBorder().Style = value; }
        }
        public CssColor? AreaBorderTopColor
        {
            get => AreaBorder?.Color;
            set { if (value != null) EnsureBorder().Color = value; }
        }

        private CssBorder EnsureBorder() => AreaBorder ??= new CssBorder();

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
                // 全プロパティから border-top 関連を探す
                string? topWidth = null;
                string? topStyle = null;
                string? topColor = null;

                foreach (var prop in hrRule.Style)
                {
                    string val = prop.Value?.Trim() ?? "";
                    if (string.IsNullOrEmpty(val) || val == "initial" || val == "inherit" || val == "unset") continue;

                    if (prop.Name.Equals("border-top-width", StringComparison.OrdinalIgnoreCase)) topWidth = val;
                    else if (prop.Name.Equals("border-top-style", StringComparison.OrdinalIgnoreCase)) topStyle = val;
                    else if (prop.Name.Equals("border-top-color", StringComparison.OrdinalIgnoreCase)) topColor = val;
                }

                // 一括指定 (border-top, border) のパース
                var shorthands = new[] { "border-top", "border" };
                foreach (var shName in shorthands)
                {
                    var shorthand = hrRule.Style.GetPropertyValue(shName);
                    if (!string.IsNullOrEmpty(shorthand) && shorthand != "initial" && shorthand != "unset")
                    {
                        var parts = Regex.Split(shorthand, @"\s+(?![^\(]*\))").Where(s => !string.IsNullOrEmpty(s)).Select(s => s.Trim()).ToArray();
                        string? w = parts.FirstOrDefault(p => char.IsDigit(p[0]) || p.EndsWith("px") || p.EndsWith("em") || p.EndsWith("rem") || p.EndsWith("%"));
                        var knownStyles = new[] { "none", "hidden", "dotted", "dashed", "solid", "double", "groove", "ridge", "inset", "outset" };
                        string? s = parts.FirstOrDefault(p => knownStyles.Contains(p.ToLower()));
                        string? c = parts.FirstOrDefault(p => p.StartsWith("#") || p.StartsWith("rgb") || p.StartsWith("rgba") || p.StartsWith("hsl") || p.StartsWith("transparent"));

                        if (topWidth == null && !string.IsNullOrEmpty(w)) topWidth = w;
                        if (topStyle == null && !string.IsNullOrEmpty(s)) topStyle = s;
                        if (topColor == null && !string.IsNullOrEmpty(c)) topColor = c;
                    }
                }

                // 0 は太さがないことを意味するため、パース対象から除外して Unset 状態を維持しやすくする
                if (topWidth == "0" || topWidth == "0px" || topWidth == "medium") topWidth = null;
                if (topStyle == "none") topStyle = null;

                if (!string.IsNullOrEmpty(topWidth) || !string.IsNullOrEmpty(topStyle) || !string.IsNullOrEmpty(topColor))
                {
                    AreaBorder = CssBorder.Parse(topWidth, topStyle, topColor);
                }
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
            else areaRule.Style.RemoveProperty("font-size");

            if (AreaTextColor != null) areaRule.Style.SetProperty("color", AreaTextColor.ToString());
            else areaRule.Style.RemoveProperty("color");

            if (AreaMarginTop != null) areaRule.Style.SetProperty("margin-top", AreaMarginTop.ToString());
            else areaRule.Style.RemoveProperty("margin-top");

            // HR
            var hrRule = GetOrCreateRule(stylesheet, ".footnotes hr");
            if (AreaBorder != null)
            {
                hrRule.Style.SetProperty("border", "0");
                AreaBorder.ApplyTo(hrRule.Style, "top");
            }
            else
            {
                hrRule.Style.RemoveProperty("border");
                hrRule.Style.RemoveProperty("border-top");
                hrRule.Style.RemoveProperty("border-top-width");
                hrRule.Style.RemoveProperty("border-top-style");
                hrRule.Style.RemoveProperty("border-top-color");
            }

            // List Item
            var liRule = GetOrCreateRule(stylesheet, ".footnotes li");
            if (!string.IsNullOrEmpty(ListItemLineHeight)) liRule.Style.SetProperty("line-height", ListItemLineHeight);
            else liRule.Style.RemoveProperty("line-height");

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
