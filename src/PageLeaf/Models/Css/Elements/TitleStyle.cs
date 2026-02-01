using AngleSharp.Css.Dom;
using PageLeaf.Models.Css.Values;

namespace PageLeaf.Models.Css.Elements
{
    /// <summary>
    /// タイトル要素（#page-title）のスタイル設定を管理するドメインモデルです。
    /// </summary>
    public class TitleStyle
    {
        public CssColor? TextColor { get; set; }
        public CssSize? FontSize { get; set; }
        public string? FontFamily { get; set; }
        public string? TextAlignment { get; set; }
        public CssSize? MarginBottom { get; set; }

        public CssTextStyle TextStyle { get; } = new CssTextStyle();

        // Convenience property for backward compatibility or simple binding
        public bool IsBold
        {
            get => TextStyle.IsBold;
            set => TextStyle.IsBold = value;
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

            var marginBottom = rule.Style.GetPropertyValue("margin-bottom");
            if (!string.IsNullOrEmpty(marginBottom)) MarginBottom = CssSize.Parse(marginBottom);

            TextStyle.UpdateFrom(rule);
        }

        public void ApplyTo(ICssStyleRule rule)
        {
            if (rule == null) return;

            if (TextColor != null) rule.Style.SetProperty("color", TextColor.ToString());
            if (FontSize != null) rule.Style.SetProperty("font-size", FontSize.ToString());
            if (!string.IsNullOrEmpty(FontFamily)) rule.Style.SetProperty("font-family", FontFamily);
            if (!string.IsNullOrEmpty(TextAlignment)) rule.Style.SetProperty("text-align", TextAlignment);
            if (MarginBottom != null) rule.Style.SetProperty("margin-bottom", MarginBottom.ToString());

            // Apply text styles (Bold, Italic, Underline)
            // Pass TextColor string for decoration color if needed.
            // Title element requires explicit normal/none output for tests.
            TextStyle.ApplyTo(rule, TextColor?.ToString(), alwaysOutputNormal: true);
        }
    }
}
