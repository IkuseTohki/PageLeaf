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

        public static CssColor? Parse(string? cssString)
        {
            if (string.IsNullOrWhiteSpace(cssString) || cssString == "transparent")
                return new CssColor(cssString ?? "transparent");

            var color = cssString.Trim();

            // Handle hex normalization
            if (color.StartsWith("#"))
            {
                color = color.ToUpper();
                if (color.Length == 4) // #RGB
                {
                    color = "#" + color[1] + color[1] + color[2] + color[2] + color[3] + color[3];
                }
                return new CssColor(color);
            }

            // Handle rgb/rgba to hex conversion
            if (color.StartsWith("rgb", StringComparison.OrdinalIgnoreCase))
            {
                var match = Regex.Match(color, @"rgba?\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*(?:,\s*([\d\.]+)\s*)?\)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    int r = int.Parse(match.Groups[1].Value);
                    int g = int.Parse(match.Groups[2].Value);
                    int b = int.Parse(match.Groups[3].Value);
                    // alpha は無視して HEX (#RRGGBB) に変換
                    return new CssColor($"#{r:X2}{g:X2}{b:X2}");
                }
            }

            return new CssColor(color);
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
