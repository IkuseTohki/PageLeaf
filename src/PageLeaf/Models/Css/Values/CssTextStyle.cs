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

            var fontWeight = rule.Style.GetPropertyValue("font-weight");
            IsBold = fontWeight == "bold" || (int.TryParse(fontWeight, out var w) && w >= 700);

            var fontStyle = rule.Style.GetPropertyValue("font-style");
            IsItalic = fontStyle == "italic";

            var textDecoration = rule.Style.GetPropertyValue("text-decoration-line");
            if (string.IsNullOrEmpty(textDecoration)) textDecoration = rule.Style.GetPropertyValue("text-decoration");
            IsUnderline = textDecoration != null && textDecoration.Contains("underline");
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
                    rule.Style.SetProperty("text-decoration-style", "initial");
                    rule.Style.SetProperty("text-decoration-line", "none");
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
