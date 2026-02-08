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

        // Helpers for synchronization
        public string? BorderColor
        {
            get => Border?.Color?.ToString();
            set { if (value != null) EnsureBorder().Color = CssColor.Parse(value); }
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

                // border または border-left プロパティから色のみを抽出
                var colorText = rule.Style.GetPropertyValue("border-color");
                if (string.IsNullOrEmpty(colorText)) colorText = rule.Style.GetPropertyValue("border-left-color");

                Border = new CssBorder { Color = !string.IsNullOrEmpty(colorText) ? CssColor.Parse(colorText) : null };

                IsItalic = rule.Style.GetPropertyValue("font-style") == "italic";
                Padding = rule.Style.GetPropertyValue("padding");
                BorderRadius = rule.Style.GetPropertyValue("border-radius");
            }
        }

        public void ApplyTo(ICssStyleSheet stylesheet)
        {
            if (stylesheet == null) return;

            var rule = GetOrCreateRule(stylesheet, "blockquote");

            if (TextColor != null) rule.Style.SetProperty("color", TextColor.ToString(), "important");
            else rule.Style.RemoveProperty("color");

            if (BackgroundColor != null) rule.Style.SetProperty("background-color", BackgroundColor.ToString(), "important");
            else rule.Style.RemoveProperty("background-color");

            if (Border?.Color != null)
            {
                // プリセットのデザインを上書きするため、色のみをセットする（太さと種類はプリセットに従う）
                rule.Style.SetProperty("border-color", Border.Color.ToString(), "important");
            }
            else
            {
                rule.Style.RemoveProperty("border-color");
            }

            if (IsItalic) rule.Style.SetProperty("font-style", "italic", "important");
            else rule.Style.RemoveProperty("font-style");

            if (!string.IsNullOrEmpty(Padding)) rule.Style.SetProperty("padding", Padding, "important");
            else rule.Style.RemoveProperty("padding");

            if (!string.IsNullOrEmpty(BorderRadius)) rule.Style.SetProperty("border-radius", BorderRadius, "important");
            else rule.Style.RemoveProperty("border-radius");
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
