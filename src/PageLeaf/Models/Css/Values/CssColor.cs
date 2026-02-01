using System;
using System.Text.RegularExpressions;

namespace PageLeaf.Models.Css.Values
{
    /// <summary>
    /// CSSの色設定を保持し、変換ロジックを提供する値オブジェクトです。
    /// </summary>
    public class CssColor
    {
        public string HexCode { get; }

        public CssColor(string hexCode)
        {
            HexCode = hexCode ?? throw new ArgumentNullException(nameof(hexCode));
        }

        /// <summary>
        /// CSSの色指定文字列（HEX, RGB, 名前付き）を解析して CssColor インスタンスを生成します。
        /// </summary>
        public static CssColor Parse(string colorString)
        {
            if (string.IsNullOrWhiteSpace(colorString))
                return new CssColor("transparent");

            var trimmed = colorString.Trim();

            // RGB / RGBA 形式の解析
            if (trimmed.StartsWith("rgb", StringComparison.OrdinalIgnoreCase))
            {
                var match = Regex.Match(trimmed, @"rgba?\((\d+),\s*(\d+),\s*(\d+)");
                if (match.Success)
                {
                    var r = byte.Parse(match.Groups[1].Value);
                    var g = byte.Parse(match.Groups[2].Value);
                    var b = byte.Parse(match.Groups[3].Value);
                    return new CssColor($"#{r:X2}{g:X2}{b:X2}");
                }
            }

            // HEX 形式または名前付きカラー
            return new CssColor(trimmed);
        }

        public override string ToString() => HexCode;

        public override bool Equals(object? obj)
        {
            return obj is CssColor color &&
                   HexCode.Equals(color.HexCode, StringComparison.OrdinalIgnoreCase);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(HexCode.ToLowerInvariant());
        }
    }
}
