using AngleSharp.Css.Dom;
using PageLeaf.Models.Css.Values;

namespace PageLeaf.Models.Css.Elements
{
    /// <summary>
    /// 引用要素（blockquote）のスタイル設定を管理するドメインモデルです。
    /// </summary>
    public class BlockquoteStyle
    {
        public CssColor? TextColor { get; set; }
        public CssColor? BackgroundColor { get; set; }
        public CssColor? BorderColor { get; set; }
        public string? BorderWidth { get; set; }
        public string? BorderStyle { get; set; }

        public void UpdateFrom(ICssStyleRule rule)
        {
            if (rule == null) return;

            var color = rule.Style.GetPropertyValue("color");
            if (!string.IsNullOrEmpty(color)) TextColor = CssColor.Parse(color);

            var bgColor = rule.Style.GetPropertyValue("background-color");
            if (!string.IsNullOrEmpty(bgColor)) BackgroundColor = CssColor.Parse(bgColor);

            // border-left プロパティから抽出（既存の実装に準拠）
            var borderColor = rule.Style.GetPropertyValue("border-left-color");
            if (!string.IsNullOrEmpty(borderColor)) BorderColor = CssColor.Parse(borderColor);

            var borderWidth = rule.Style.GetPropertyValue("border-left-width");
            if (!string.IsNullOrEmpty(borderWidth)) BorderWidth = borderWidth;

            var borderStyle = rule.Style.GetPropertyValue("border-left-style");
            if (!string.IsNullOrEmpty(borderStyle)) BorderStyle = borderStyle;
        }

        public void ApplyTo(ICssStyleRule rule)
        {
            if (rule == null) return;

            if (TextColor != null) rule.Style.SetProperty("color", TextColor.ToString());
            if (BackgroundColor != null) rule.Style.SetProperty("background-color", BackgroundColor.ToString());

            if (BorderColor != null || !string.IsNullOrEmpty(BorderWidth) || !string.IsNullOrEmpty(BorderStyle))
            {
                var width = !string.IsNullOrEmpty(BorderWidth) ? BorderWidth : "medium";
                var style = !string.IsNullOrEmpty(BorderStyle) ? BorderStyle : "none";
                var color = BorderColor?.ToString() ?? "currentcolor";
                rule.Style.SetProperty("border-left", $"{width} {style} {color}");
            }
        }
    }
}
