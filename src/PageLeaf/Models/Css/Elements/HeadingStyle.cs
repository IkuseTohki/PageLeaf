using AngleSharp.Css.Dom;
using PageLeaf.Models.Css.Values;

namespace PageLeaf.Models.Css.Elements
{
    /// <summary>
    /// 各レベルの見出し（h1-h6）のスタイル設定を管理するドメインモデルです。
    /// </summary>
    public class HeadingStyle
    {
        public CssColor? TextColor { get; set; }
        public CssSize? FontSize { get; set; }
        public string? FontFamily { get; set; }
        public string? TextAlignment { get; set; }

        public CssTextStyle TextStyle { get; } = new CssTextStyle();

        // Convenience properties for backward compatibility or simple binding
        public bool IsBold
        {
            get => TextStyle.IsBold;
            set => TextStyle.IsBold = value;
        }
        public bool IsItalic
        {
            get => TextStyle.IsItalic;
            set => TextStyle.IsItalic = value;
        }
        public bool IsUnderline
        {
            get => TextStyle.IsUnderline;
            set => TextStyle.IsUnderline = value;
        }

        public void UpdateFrom(ICssStyleRule rule)
        {
            if (rule == null) return;

            var color = rule.Style.GetPropertyValue("color");
            if (!string.IsNullOrEmpty(color)) TextColor = CssColor.Parse(color);

            var fontSize = rule.Style.GetPropertyValue("font-size");
            if (!string.IsNullOrEmpty(fontSize)) FontSize = CssSize.Parse(fontSize);

            var fontFamily = rule.Style.GetPropertyValue("font-family");
            if (!string.IsNullOrEmpty(fontFamily)) FontFamily = fontFamily;

            var textAlign = rule.Style.GetPropertyValue("text-align");
            if (!string.IsNullOrEmpty(textAlign)) TextAlignment = textAlign;

            TextStyle.UpdateFrom(rule);
        }

        public void ApplyTo(ICssStyleRule rule)
        {
            if (rule == null) return;

            if (TextColor != null) rule.Style.SetProperty("color", TextColor.ToString());
            if (FontSize != null) rule.Style.SetProperty("font-size", FontSize.ToString());
            if (!string.IsNullOrEmpty(FontFamily)) rule.Style.SetProperty("font-family", FontFamily);
            if (!string.IsNullOrEmpty(TextAlignment)) rule.Style.SetProperty("text-align", TextAlignment);

            // Apply text styles (Bold, Italic, Underline)
            TextStyle.ApplyTo(rule, TextColor?.ToString());
        }
    }
}
