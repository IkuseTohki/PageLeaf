using AngleSharp.Css.Dom;
using PageLeaf.Models.Css.Values;

namespace PageLeaf.Models.Css.Elements
{
    /// <summary>
    /// 段落要素（p）のスタイル設定を管理するドメインモデルです。
    /// </summary>
    public class ParagraphStyle
    {
        public string? LineHeight { get; set; }
        public CssSize? MarginBottom { get; set; }
        public CssSize? TextIndent { get; set; }

        public void UpdateFrom(ICssStyleRule rule)
        {
            if (rule == null) return;

            var lineHeight = rule.Style.GetPropertyValue("line-height");
            if (!string.IsNullOrEmpty(lineHeight)) LineHeight = lineHeight;

            var marginBottom = rule.Style.GetPropertyValue("margin-bottom");
            if (!string.IsNullOrEmpty(marginBottom)) MarginBottom = CssSize.Parse(marginBottom);

            var textIndent = rule.Style.GetPropertyValue("text-indent");
            if (!string.IsNullOrEmpty(textIndent)) TextIndent = CssSize.Parse(textIndent);
        }

        public void ApplyTo(ICssStyleRule rule)
        {
            if (rule == null) return;

            if (!string.IsNullOrEmpty(LineHeight)) rule.Style.SetProperty("line-height", LineHeight);
            if (MarginBottom != null) rule.Style.SetProperty("margin-bottom", MarginBottom.ToString());
            if (TextIndent != null) rule.Style.SetProperty("text-indent", TextIndent.ToString());
        }
    }
}
