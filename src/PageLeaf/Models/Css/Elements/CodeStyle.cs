using AngleSharp.Css.Dom;
using PageLeaf.Models.Css.Values;
using System.Linq;

namespace PageLeaf.Models.Css.Elements
{
    /// <summary>
    /// コード要素（code, pre code）のスタイル設定を管理するドメインモデルです。
    /// </summary>
    public class CodeStyle
    {
        // インラインコード / 全般
        public CssColor? TextColor { get; set; }
        public CssColor? BackgroundColor { get; set; }
        public string? FontFamily { get; set; }

        // ブロックコード専用
        public CssColor? BlockTextColor { get; set; }
        public CssColor? BlockBackgroundColor { get; set; }
        public bool IsBlockOverrideEnabled { get; set; }

        public void UpdateFrom(ICssStyleSheet stylesheet)
        {
            if (stylesheet == null) return;

            // code (Inline)
            var codeRule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == "code");
            if (codeRule != null)
            {
                var color = codeRule.Style.GetPropertyValue("color");
                if (!string.IsNullOrEmpty(color)) TextColor = CssColor.Parse(color);

                var bgColor = codeRule.Style.GetPropertyValue("background-color");
                if (!string.IsNullOrEmpty(bgColor)) BackgroundColor = CssColor.Parse(bgColor);

                var family = codeRule.Style.GetPropertyValue("font-family");
                if (!string.IsNullOrEmpty(family)) FontFamily = family;
            }

            // pre code (Block)
            var preCodeRule = stylesheet.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText == "pre code");
            if (preCodeRule != null)
            {
                var color = preCodeRule.Style.GetPropertyValue("color");
                if (!string.IsNullOrEmpty(color))
                {
                    BlockTextColor = CssColor.Parse(color);
                    IsBlockOverrideEnabled = true; // 色が設定されていればオーバーライド有効とみなす（暫定）
                }

                var bgColor = preCodeRule.Style.GetPropertyValue("background-color");
                if (!string.IsNullOrEmpty(bgColor))
                {
                    BlockBackgroundColor = CssColor.Parse(bgColor);
                    IsBlockOverrideEnabled = true;
                }
            }
        }

        public void ApplyTo(ICssStyleSheet stylesheet)
        {
            if (stylesheet == null) return;

            // code (Inline)
            if (TextColor != null || BackgroundColor != null || !string.IsNullOrEmpty(FontFamily))
            {
                var rule = GetOrCreateRule(stylesheet, "code");
                if (TextColor != null) rule.Style.SetProperty("color", TextColor.ToString());
                if (BackgroundColor != null) rule.Style.SetProperty("background-color", BackgroundColor.ToString());
                if (!string.IsNullOrEmpty(FontFamily)) rule.Style.SetProperty("font-family", FontFamily);
            }

            // pre code (Block)
            var blockRule = GetOrCreateRule(stylesheet, "pre code");
            if (IsBlockOverrideEnabled)
            {
                if (BlockTextColor != null) blockRule.Style.SetProperty("color", BlockTextColor.ToString(), "important");
                if (BlockBackgroundColor != null) blockRule.Style.SetProperty("background-color", BlockBackgroundColor.ToString(), "important");
            }
            else
            {
                blockRule.Style.RemoveProperty("color");
                blockRule.Style.RemoveProperty("background-color");
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
