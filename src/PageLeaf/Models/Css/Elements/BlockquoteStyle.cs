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
        public CssBorder? Border { get; set; }

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

        public void UpdateFrom(ICssStyleRule rule)
        {
            if (rule == null) return;

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
        }

        public void ApplyTo(ICssStyleRule rule)
        {
            if (rule == null) return;

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
        }
    }
}
