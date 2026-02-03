using AngleSharp.Css.Dom;
using System;

namespace PageLeaf.Models.Css.Values
{
    /// <summary>
    /// CSSのテキスト配置（left, center, right, justify）を管理する列挙型です。
    /// </summary>
    public enum CssAlignment
    {
        None,
        Left,
        Center,
        Right,
        Justify
    }

    public static class CssAlignmentExtensions
    {
        /// <summary>
        /// 文字列から解析を試みます。
        /// </summary>
        public static CssAlignment Parse(string? value)
        {
            if (string.IsNullOrEmpty(value)) return CssAlignment.None;

            return value.ToLower() switch
            {
                "left" => CssAlignment.Left,
                "center" => CssAlignment.Center,
                "right" => CssAlignment.Right,
                "justify" => CssAlignment.Justify,
                _ => CssAlignment.None
            };
        }

        /// <summary>
        /// CSSプロパティ値としての文字列を返します。
        /// </summary>
        public static string? ToCssString(this CssAlignment alignment)
        {
            return alignment switch
            {
                CssAlignment.Left => "left",
                CssAlignment.Center => "center",
                CssAlignment.Right => "right",
                CssAlignment.Justify => "justify",
                _ => null
            };
        }

        public static void ApplyTo(this CssAlignment alignment, ICssStyleDeclaration style, string propertyName = "text-align")
        {
            var value = alignment.ToCssString();
            if (value != null)
            {
                style.SetProperty(propertyName, value);
            }
            else
            {
                style.RemoveProperty(propertyName);
            }
        }
    }
}
