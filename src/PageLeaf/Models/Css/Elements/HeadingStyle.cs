using AngleSharp.Css.Dom;
using PageLeaf.Models.Css.Values;
using System.Linq;

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
        public CssAlignment TextAlignment { get; set; } = CssAlignment.None;
        public CssSize? MarginTop { get; set; }
        public CssSize? MarginBottom { get; set; }

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
            var color = rule.Style.GetPropertyValue("color");
            if (!string.IsNullOrEmpty(color)) TextColor = CssColor.Parse(color);

            var size = rule.Style.GetPropertyValue("font-size");
            if (!string.IsNullOrEmpty(size)) FontSize = CssSize.Parse(size);

            var fontFamily = rule.Style.GetPropertyValue("font-family");
            if (!string.IsNullOrEmpty(fontFamily))
            {
                FontFamily = string.Join(", ", fontFamily.Split(',')
                    .Select(f => f.Trim().Trim('"', '\'')));
            }

            TextAlignment = CssAlignmentExtensions.Parse(rule.Style.GetPropertyValue("text-align"));

            var marginTop = rule.Style.GetPropertyValue("margin-top");
            if (!string.IsNullOrEmpty(marginTop)) MarginTop = CssSize.Parse(marginTop);

            var marginBottom = rule.Style.GetPropertyValue("margin-bottom");
            if (!string.IsNullOrEmpty(marginBottom)) MarginBottom = CssSize.Parse(marginBottom);

            TextStyle.UpdateFrom(rule);
        }

        public void ApplyTo(ICssStyleRule rule)
        {
            if (rule == null) return;

            if (TextColor != null) rule.Style.SetProperty("color", TextColor.ToString());
            else rule.Style.RemoveProperty("color");

            if (FontSize != null && FontSize.Unit != CssUnit.None)
            {
                rule.Style.SetProperty("font-size", FontSize.ToString());
            }
            else
            {
                rule.Style.RemoveProperty("font-size");
            }

            if (!string.IsNullOrEmpty(FontFamily))
            {
                var formatted = FontFamily.Contains(" ") ? $"\"{FontFamily}\"" : FontFamily;
                rule.Style.SetProperty("font-family", formatted);
            }
            else rule.Style.RemoveProperty("font-family");

            if (TextAlignment != CssAlignment.None)
            {
                rule.Style.SetProperty("text-align", TextAlignment.ToCssString());
            }
            else
            {
                rule.Style.RemoveProperty("text-align");
            }

            if (MarginTop != null) rule.Style.SetProperty("margin-top", MarginTop.ToString());
            else rule.Style.RemoveProperty("margin-top");

            if (MarginBottom != null) rule.Style.SetProperty("margin-bottom", MarginBottom.ToString());
            else rule.Style.RemoveProperty("margin-bottom");

            // Apply text styles (Bold, Italic, Underline)
            TextStyle.ApplyTo(rule, TextColor?.ToString(), alwaysOutputNormal: false);
        }
    }
}
