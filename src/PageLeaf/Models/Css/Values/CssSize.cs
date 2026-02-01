using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace PageLeaf.Models.Css.Values
{
    /// <summary>
    /// CSSのサイズ単位を表す列挙型です。
    /// </summary>
    public enum CssUnit
    {
        Px,
        Em,
        Rem,
        Percent,
        None
    }

    /// <summary>
    /// CSSの数値と単位のペアを保持し、変換ロジックを提供する値オブジェクトです。
    /// </summary>
    public class CssSize
    {
        public double Value { get; }
        public CssUnit Unit { get; }

        public CssSize(double value, CssUnit unit)
        {
            Value = value;
            Unit = unit;
        }

        /// <summary>
        /// CSS文字列（例: "16px", "1.5em", "2rem"）を解析して CssSize インスタンスを生成します。
        /// </summary>
        public static CssSize Parse(string cssString)
        {
            if (string.IsNullOrWhiteSpace(cssString))
                return new CssSize(0, CssUnit.None);

            var match = Regex.Match(cssString.Trim(), @"^([0-9\.]+)\s*(px|em|rem|%|)$", RegexOptions.IgnoreCase);
            if (!match.Success)
                throw new FormatException($"Invalid CSS size format: {cssString}");

            double value = double.Parse(match.Groups[1].Value, CultureInfo.InvariantCulture);
            string unitStr = match.Groups[2].Value.ToLower();

            CssUnit unit = unitStr switch
            {
                "px" => CssUnit.Px,
                "em" => CssUnit.Em,
                "rem" => CssUnit.Rem,
                "%" => CssUnit.Percent,
                _ => CssUnit.Px // 単位なしはデフォルトでpxとする
            };

            return new CssSize(value, unit);
        }

        /// <summary>
        /// 指定された基準フォントサイズを使用して、ピクセル単位のサイズに変換します。
        /// </summary>
        public CssSize ToPx(double baseFontSizePx)
        {
            return Unit switch
            {
                CssUnit.Px => this,
                CssUnit.Em => new CssSize(Value * baseFontSizePx, CssUnit.Px),
                CssUnit.Rem => new CssSize(Value * baseFontSizePx, CssUnit.Px), // Remはルート要素のサイズだが、ここでは簡易的にbaseを使用
                CssUnit.Percent => new CssSize(Value / 100.0 * baseFontSizePx, CssUnit.Px),
                _ => this
            };
        }

        public override string ToString()
        {
            string unitStr = Unit switch
            {
                CssUnit.Px => "px",
                CssUnit.Em => "em",
                CssUnit.Rem => "rem",
                CssUnit.Percent => "%",
                _ => ""
            };
            return Value.ToString(CultureInfo.InvariantCulture) + unitStr;
        }

        public override bool Equals(object? obj)
        {
            return obj is CssSize size &&
                   Value == size.Value &&
                   Unit == size.Unit;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Value, Unit);
        }
    }
}
