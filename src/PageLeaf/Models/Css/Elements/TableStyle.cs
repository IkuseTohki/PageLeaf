using AngleSharp.Css.Dom;
using PageLeaf.Models.Css.Values;
using System.Linq;

namespace PageLeaf.Models.Css.Elements
{
    /// <summary>
    /// テーブル要素（table, th, td）のスタイル設定を管理するドメインモデルです。
    /// </summary>
    public class TableStyle
    {
        // th, td 共通設定
        public string? BorderWidth { get; set; }
        public string? BorderStyle { get; set; }
        public CssColor? BorderColor { get; set; }
        public string? CellPadding { get; set; }

        // th 専用設定
        public CssColor? HeaderBackgroundColor { get; set; }
        public CssColor? HeaderTextColor { get; set; }
        public string? HeaderFontSize { get; set; }
        public string? HeaderAlignment { get; set; }

        public void UpdateFrom(ICssStyleSheet stylesheet)
        {
            if (stylesheet == null) return;

            // th, td ルール
            var thTdRule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == "th, td");
            if (thTdRule != null)
            {
                var width = thTdRule.Style.GetPropertyValue("border-width");
                if (!string.IsNullOrEmpty(width)) BorderWidth = width;

                var style = thTdRule.Style.GetPropertyValue("border-style");
                if (!string.IsNullOrEmpty(style)) BorderStyle = style;

                var color = thTdRule.Style.GetPropertyValue("border-color");
                if (!string.IsNullOrEmpty(color)) BorderColor = CssColor.Parse(color);

                var padding = thTdRule.Style.GetPropertyValue("padding");
                if (!string.IsNullOrEmpty(padding)) CellPadding = padding;
            }

            // th ルール
            var thRule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == "th");
            if (thRule != null)
            {
                var bgColor = thRule.Style.GetPropertyValue("background-color");
                if (!string.IsNullOrEmpty(bgColor)) HeaderBackgroundColor = CssColor.Parse(bgColor);

                var textColor = thRule.Style.GetPropertyValue("color");
                if (!string.IsNullOrEmpty(textColor)) HeaderTextColor = CssColor.Parse(textColor);

                var fontSize = thRule.Style.GetPropertyValue("font-size");
                if (!string.IsNullOrEmpty(fontSize)) HeaderFontSize = fontSize;

                var textAlign = thRule.Style.GetPropertyValue("text-align");
                if (!string.IsNullOrEmpty(textAlign)) HeaderAlignment = textAlign;
            }
        }

        public void ApplyTo(ICssStyleSheet stylesheet)
        {
            if (stylesheet == null) return;

            // table (固定スタイル)
            var tableRule = GetOrCreateRule(stylesheet, "table");
            tableRule.Style.SetProperty("border-collapse", "collapse");

            // th, td
            if (!string.IsNullOrEmpty(BorderWidth) || !string.IsNullOrEmpty(BorderStyle) || BorderColor != null || !string.IsNullOrEmpty(CellPadding))
            {
                var rule = GetOrCreateRule(stylesheet, "th, td");
                if (!string.IsNullOrEmpty(BorderWidth) || !string.IsNullOrEmpty(BorderStyle) || BorderColor != null)
                {
                    var width = !string.IsNullOrEmpty(BorderWidth) ? BorderWidth : "1px";
                    var style = !string.IsNullOrEmpty(BorderStyle) ? BorderStyle : "solid";
                    var color = BorderColor?.ToString() ?? "black";
                    rule.Style.SetProperty("border", $"{width} {style} {color}");
                }
                if (!string.IsNullOrEmpty(CellPadding)) rule.Style.SetProperty("padding", CellPadding);
            }

            // th
            if (HeaderBackgroundColor != null || HeaderTextColor != null || !string.IsNullOrEmpty(HeaderFontSize) || !string.IsNullOrEmpty(HeaderAlignment))
            {
                var rule = GetOrCreateRule(stylesheet, "th");
                if (HeaderBackgroundColor != null) rule.Style.SetProperty("background-color", HeaderBackgroundColor.ToString());
                if (HeaderTextColor != null) rule.Style.SetProperty("color", HeaderTextColor.ToString());
                if (!string.IsNullOrEmpty(HeaderFontSize)) rule.Style.SetProperty("font-size", HeaderFontSize);
                if (!string.IsNullOrEmpty(HeaderAlignment)) rule.Style.SetProperty("text-align", HeaderAlignment, "important");
            }
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
