using AngleSharp.Css.Dom;
using PageLeaf.Models.Css.Values;

namespace PageLeaf.Models.Css.Elements
{
    /// <summary>
    /// body 要素のスタイル設定を管理するドメインモデルです。
    /// </summary>
    public class BodyStyle
    {
        public CssColor? TextColor { get; set; }
        public CssColor? BackgroundColor { get; set; }
        public CssSize? FontSize { get; set; }

        /// <summary>
        /// CSSルールから自身のプロパティを更新します。
        /// </summary>
        /// <param name="rule">body要素に対応するCSSルール。</param>
        public void UpdateFrom(ICssStyleRule rule)
        {
            if (rule == null) return;

            var color = rule.Style.GetPropertyValue("color");
            if (!string.IsNullOrEmpty(color)) TextColor = CssColor.Parse(color);

            var bgColor = rule.Style.GetPropertyValue("background-color");
            if (!string.IsNullOrEmpty(bgColor)) BackgroundColor = CssColor.Parse(bgColor);

            var fontSize = rule.Style.GetPropertyValue("font-size");
            if (!string.IsNullOrEmpty(fontSize)) FontSize = CssSize.Parse(fontSize);
        }

        /// <summary>
        /// 自身のプロパティをCSSルールへ反映します。
        /// </summary>
        /// <param name="rule">反映先のCSSルール。</param>
        public void ApplyTo(ICssStyleRule rule)
        {
            if (rule == null) return;

            if (TextColor != null) rule.Style.SetProperty("color", TextColor.ToString());
            if (BackgroundColor != null) rule.Style.SetProperty("background-color", BackgroundColor.ToString());
            if (FontSize != null) rule.Style.SetProperty("font-size", FontSize.ToString());
        }
    }
}
