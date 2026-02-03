using AngleSharp.Css.Dom;
using System.Collections.Generic;
using System.Linq;

namespace PageLeaf.Models.Css.Values
{
    /// <summary>
    /// テキストの装飾スタイル（太字、斜体、下線）を管理する値オブジェクトです。
    /// </summary>
    public class CssTextStyle
    {
        public bool IsBold { get; set; }
        public bool IsItalic { get; set; }
        public bool IsUnderline { get; set; }

        public void UpdateFrom(ICssStyleRule rule)
        {
            if (rule == null) return;

            // font-weight
            string? fontWeight = rule.Style.GetPropertyValue("font-weight");
            if (string.IsNullOrEmpty(fontWeight))
            {
                fontWeight = rule.Style.FirstOrDefault(p => p.Name.Equals("font-weight", System.StringComparison.OrdinalIgnoreCase))?.Value;
            }
            fontWeight = fontWeight?.Trim().ToLower();
            IsBold = fontWeight == "bold" || fontWeight == "bolder" ||
                     (int.TryParse(fontWeight, out var w) && w >= 600);

            // font-style
            string? fontStyle = rule.Style.GetPropertyValue("font-style");
            if (string.IsNullOrEmpty(fontStyle))
            {
                fontStyle = rule.Style.FirstOrDefault(p => p.Name.Equals("font-style", System.StringComparison.OrdinalIgnoreCase))?.Value;
            }
            fontStyle = fontStyle?.Trim().ToLower();
            IsItalic = fontStyle == "italic" || fontStyle == "oblique";

            // text-decoration
            string? textDecoration = rule.Style.GetPropertyValue("text-decoration-line");
            if (string.IsNullOrEmpty(textDecoration)) textDecoration = rule.Style.GetPropertyValue("text-decoration");

            if (string.IsNullOrEmpty(textDecoration))
            {
                // 全プロパティから探す
                foreach (var prop in rule.Style)
                {
                    if (prop.Name.Contains("text-decoration", System.StringComparison.OrdinalIgnoreCase) &&
                        prop.Value.Contains("underline", System.StringComparison.OrdinalIgnoreCase))
                    {
                        IsUnderline = true;
                        return;
                    }
                }
                IsUnderline = false;
            }
            else
            {
                IsUnderline = textDecoration.ToLower().Contains("underline");
            }
        }

        /// <summary>
        /// 自身のプロパティをCSSルールへ反映します。
        /// </summary>
        /// <param name="rule">反映先のCSSルール。</param>
        /// <param name="decorationColor">下線の色。</param>
        /// <param name="alwaysOutputNormal">trueの場合、デフォルト値（normal等）を明示的に出力します。</param>
        public void ApplyTo(ICssStyleRule rule, string? decorationColor = null, bool alwaysOutputNormal = false)
        {
            if (rule == null) return;

            if (IsBold) rule.Style.SetProperty("font-weight", "bold");
            else if (alwaysOutputNormal) rule.Style.SetProperty("font-weight", "normal");
            else rule.Style.RemoveProperty("font-weight");

            if (IsItalic) rule.Style.SetProperty("font-style", "italic");
            else if (alwaysOutputNormal) rule.Style.SetProperty("font-style", "normal");
            else rule.Style.RemoveProperty("font-style");

            var textDecorations = new List<string>();
            if (IsUnderline) textDecorations.Add("underline");

            if (textDecorations.Any())
            {
                rule.Style.SetProperty("text-decoration", string.Join(" ", textDecorations));
                if (!string.IsNullOrEmpty(decorationColor))
                {
                    rule.Style.SetProperty("text-decoration-color", decorationColor);
                }
                else
                {
                    rule.Style.RemoveProperty("text-decoration-color");
                }
            }
            else
            {
                if (alwaysOutputNormal)
                {
                    rule.Style.SetProperty("text-decoration", "none");
                    rule.Style.RemoveProperty("text-decoration-color");
                }
                else
                {
                    rule.Style.RemoveProperty("text-decoration");
                    rule.Style.RemoveProperty("text-decoration-color");
                }
            }
        }
    }
}
