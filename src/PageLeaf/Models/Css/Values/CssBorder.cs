using AngleSharp.Css.Dom;
using System;
using System.Linq;

namespace PageLeaf.Models.Css.Values
{
    /// <summary>
    /// CSSの境界線（太さ、種類、色）を管理する値オブジェクトです。
    /// </summary>
    public class CssBorder
    {
        public CssSize? Width { get; set; }
        public string? Style { get; set; }
        public CssColor? Color { get; set; }

        public CssBorder(CssSize? width = null, string? style = null, CssColor? color = null)
        {
            Width = width;
            Style = style;
            Color = color;
        }

        /// <summary>
        /// 個別のプロパティから解析を試みます。全て空の場合は null を返します。
        /// </summary>
        public static CssBorder? Parse(string? width, string? style, string? color)
        {
            if (string.IsNullOrEmpty(width) && string.IsNullOrEmpty(style) && string.IsNullOrEmpty(color))
                return null;

            return new CssBorder(
                !string.IsNullOrEmpty(width) ? CssSize.Parse(width) : null,
                style,
                !string.IsNullOrEmpty(color) ? CssColor.Parse(color) : null
            );
        }

        public override string ToString()
        {
            var parts = new[] { Width?.ToString(), Style, Color?.ToString() }
                .Where(s => !string.IsNullOrEmpty(s));
            return string.Join(" ", parts);
        }

        public void ApplyTo(ICssStyleDeclaration declaration, string side = "")
        {
            var propertyName = string.IsNullOrEmpty(side) ? "border" : $"border-{side}";
            var value = ToString();
            if (!string.IsNullOrEmpty(value))
            {
                declaration.SetProperty(propertyName, value);
            }
            else
            {
                declaration.RemoveProperty(propertyName);
            }
        }
    }
}
