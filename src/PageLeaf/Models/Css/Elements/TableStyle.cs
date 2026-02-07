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
        // table 設定
        public string? Width { get; set; }

        // th, td 共通設定
        public CssBorder? Border { get; set; }
        public string? CellPadding { get; set; }

        // Helpers for synchronization
        public string? BorderWidth
        {
            get => Border?.Width?.ToString();
            set { if (!string.IsNullOrEmpty(value)) EnsureBorder().Width = CssSize.Parse(value); }
        }
        public string? BorderStyle
        {
            get => Border?.Style;
            set { if (!string.IsNullOrEmpty(value)) EnsureBorder().Style = value; }
        }
        public CssColor? BorderColor
        {
            get => Border?.Color;
            set { if (value != null) EnsureBorder().Color = value; }
        }

        private CssBorder EnsureBorder() => Border ??= new CssBorder();

        /// <summary>
        /// デフォルト値を適用した実効的な枠線設定を取得します。
        /// </summary>
        public CssBorder GetEffectiveBorder()
        {
            var border = Border ?? new CssBorder();
            var defaults = CssDefaults.Instance.Table;

            var width = border.Width ?? CssSize.Parse($"{defaults.BorderWidth}{defaults.BorderWidthUnit}");
            var style = !string.IsNullOrEmpty(border.Style) ? border.Style : defaults.BorderStyle;
            var color = border.Color ?? CssColor.Parse(defaults.BorderColor);

            return new CssBorder(width, style, color);
        }

        // th 専用設定
        public CssColor? HeaderBackgroundColor { get; set; }
        public CssColor? HeaderTextColor { get; set; }
        public string? HeaderFontSize { get; set; }
        public string? HeaderAlignment { get; set; }

        public void UpdateFrom(ICssStyleSheet stylesheet)
        {
            if (stylesheet == null) return;

            // table ルール
            var tableRule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == "table");
            if (tableRule != null)
            {
                var width = tableRule.Style.GetPropertyValue("width");
                if (!string.IsNullOrEmpty(width)) Width = width;
            }

            // th, td ルール
            var thTdRule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == "th, td");
            if (thTdRule != null)
            {
                Border = CssBorder.Parse(
                    thTdRule.Style.GetPropertyValue("border-width"),
                    thTdRule.Style.GetPropertyValue("border-style"),
                    thTdRule.Style.GetPropertyValue("border-color")
                );

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

            // table
            var tableRule = GetOrCreateRule(stylesheet, "table");
            tableRule.Style.SetProperty("border-collapse", "collapse");
            if (!string.IsNullOrEmpty(Width)) tableRule.Style.SetProperty("width", Width);
            else tableRule.Style.RemoveProperty("width");

            // th, td
            var thTdRule = GetOrCreateRule(stylesheet, "th, td");

            // 枠線が未設定または部分的に未設定の場合にデフォルト値で補完する
            GetEffectiveBorder().ApplyTo(thTdRule.Style);

            if (!string.IsNullOrEmpty(CellPadding)) thTdRule.Style.SetProperty("padding", CellPadding);
            else thTdRule.Style.RemoveProperty("padding");

            // th
            var thRule = GetOrCreateRule(stylesheet, "th");
            if (HeaderBackgroundColor != null) thRule.Style.SetProperty("background-color", HeaderBackgroundColor.ToString());
            else thRule.Style.RemoveProperty("background-color");

            if (HeaderTextColor != null) thRule.Style.SetProperty("color", HeaderTextColor.ToString());
            else thRule.Style.RemoveProperty("color");

            if (!string.IsNullOrEmpty(HeaderFontSize)) thRule.Style.SetProperty("font-size", HeaderFontSize);
            else thRule.Style.RemoveProperty("font-size");

            if (!string.IsNullOrEmpty(HeaderAlignment)) thRule.Style.SetProperty("text-align", HeaderAlignment, "important");
            else thRule.Style.RemoveProperty("text-align");
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
