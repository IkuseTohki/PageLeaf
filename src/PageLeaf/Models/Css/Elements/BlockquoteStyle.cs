using AngleSharp.Css.Dom;
using PageLeaf.Models.Css.Values;
using System.Linq;

namespace PageLeaf.Models.Css.Elements
{
    /// <summary>
    /// 引用要素（blockquote）のスタイル設定を管理するドメインモデルです。
    /// </summary>
    public class BlockquoteStyle
    {
        public CssColor? TextColor { get; set; }
        public CssColor? BackgroundColor { get; set; }
        public CssBorder? Border { get; set; }
        public bool IsItalic { get; set; }
        public string? Padding { get; set; }
        public string? BorderRadius { get; set; }
        public bool ShowIcon { get; set; }

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

        public void UpdateFrom(ICssStyleSheet stylesheet)
        {
            if (stylesheet == null) return;

            var rule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == "blockquote");
            if (rule != null)
            {
                var color = rule.Style.GetPropertyValue("color");
                if (!string.IsNullOrEmpty(color)) TextColor = CssColor.Parse(color);

                var bgColor = rule.Style.GetPropertyValue("background-color");
                if (!string.IsNullOrEmpty(bgColor)) BackgroundColor = CssColor.Parse(bgColor);

                // border-left プロパティから抽出
                Border = CssBorder.Parse(
                    rule.Style.GetPropertyValue("border-left-width"),
                    rule.Style.GetPropertyValue("border-left-style"),
                    rule.Style.GetPropertyValue("border-left-color")
                );

                IsItalic = rule.Style.GetPropertyValue("font-style") == "italic";
                Padding = rule.Style.GetPropertyValue("padding");
                BorderRadius = rule.Style.GetPropertyValue("border-radius");
            }

            var beforeRule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == "blockquote::before");
            ShowIcon = beforeRule != null && !string.IsNullOrEmpty(beforeRule.Style.GetPropertyValue("content")) && beforeRule.Style.GetPropertyValue("content") != "none";
        }

        public void ApplyTo(ICssStyleSheet stylesheet)
        {
            if (stylesheet == null) return;

            var rule = GetOrCreateRule(stylesheet, "blockquote");

            if (TextColor != null) rule.Style.SetProperty("color", TextColor.ToString());
            else rule.Style.RemoveProperty("color");

            if (BackgroundColor != null) rule.Style.SetProperty("background-color", BackgroundColor.ToString());
            else rule.Style.RemoveProperty("background-color");

            if (Border != null)
            {
                Border.ApplyTo(rule.Style, "left");
            }
            else
            {
                rule.Style.RemoveProperty("border-left");
                rule.Style.RemoveProperty("border-left-width");
                rule.Style.RemoveProperty("border-left-style");
                rule.Style.RemoveProperty("border-left-color");
            }

            if (IsItalic) rule.Style.SetProperty("font-style", "italic");
            else rule.Style.RemoveProperty("font-style");

            if (!string.IsNullOrEmpty(Padding)) rule.Style.SetProperty("padding", Padding);
            else rule.Style.RemoveProperty("padding");

            if (!string.IsNullOrEmpty(BorderRadius)) rule.Style.SetProperty("border-radius", BorderRadius);
            else rule.Style.RemoveProperty("border-radius");

            // blockquote::before (アイコン)
            if (ShowIcon)
            {
                var beforeRule = GetOrCreateRule(stylesheet, "blockquote::before");
                beforeRule.Style.SetProperty("content", "\"“\"");
                beforeRule.Style.SetProperty("display", "block");
                beforeRule.Style.SetProperty("font-size", "2em");
                beforeRule.Style.SetProperty("margin-bottom", "-0.5em");
                beforeRule.Style.SetProperty("color", BorderColor?.ToString() ?? "#cccccc");
            }
            else
            {
                var beforeRule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == "blockquote::before");
                if (beforeRule != null)
                {
                    stylesheet.RemoveAt(stylesheet.Rules.ToList().IndexOf(beforeRule));
                }
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
