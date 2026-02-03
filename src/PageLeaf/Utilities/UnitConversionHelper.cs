using System;
using System.Linq;

namespace PageLeaf.Utilities
{
    /// <summary>
    /// フォントサイズの単位（px, em, %）間の変換をサポートするヘルパークラスです。
    /// 16px を基準（1.0em / 100%）として計算します。
    /// </summary>
    public static class UnitConversionHelper
    {
        private const double BasePx = 16.0;

        public static double PxToEm(double px) => Round(px / BasePx);
        public static double EmToPx(double em) => Round(em * BasePx);
        public static double PxToPercent(double px) => Round((px / BasePx) * 100.0);
        public static double PercentToPx(double percent) => Round((percent / 100.0) * BasePx);
        public static double EmToPercent(double em) => Round(em * 100.0);
        public static double PercentToEm(double percent) => Round(percent / 100.0);

        public static double Round(double value) => Math.Round(value, 2, MidpointRounding.AwayFromZero);

        /// <summary>
        /// CSS文字列を数値部分と単位部分に分離します。
        /// </summary>
        /// <param name="input">"16px", "1.2em" などのCSS文字列。</param>
        /// <returns>数値と単位のタプル。</returns>
        public static (double Value, string Unit) Split(string? input)
        {
            if (string.IsNullOrWhiteSpace(input)) return (0.0, "");

            // 数値と単位を分離
            var numPart = new string(input.TakeWhile(c => char.IsDigit(c) || c == '.' || c == '-').ToArray());
            var unitPart = input.Substring(numPart.Length).Trim().ToLower();

            if (!double.TryParse(numPart, out var value)) return (0.0, "");

            // 入力単位を特定
            return (value, unitPart);
        }

        /// <summary>
        /// 数値を指定された単位間で変換します。
        /// </summary>
        /// <param name="value">変換元の数値。</param>
        /// <param name="fromUnit">現在の単位。</param>
        /// <param name="toUnit">変換先の単位。</param>
        /// <returns>変換後の数値。</returns>
        public static double Convert(double value, string fromUnit, string toUnit)
        {
            if (fromUnit == toUnit) return value;

            // 一旦 px に統一
            double px = fromUnit switch
            {
                "em" => EmToPx(value),
                "rem" => EmToPx(value), // このアプリでは rem = em とみなす
                "%" => PercentToPx(value),
                _ => value // px または未知の単位
            };

            // ターゲット単位へ変換
            double result = toUnit switch
            {
                "em" => PxToEm(px),
                "%" => PxToPercent(px),
                _ => px
            };

            return result;
        }

        /// <summary>
        /// 数値と単位を結合してCSS文字列を生成します。
        /// </summary>
        /// <param name="value">数値。</param>
        /// <param name="unit">単位。</param>
        /// <returns>"14px" などのCSS文字列。</returns>
        public static string Format(double value, string unit)
        {
            return $"{Round(value)}{unit}";
        }

        /// <summary>
        /// 単位付きのCSS文字列を解析し、指定されたターゲット単位の数値文字列に変換します。
        /// </summary>
        /// <param name="input">"16px", "1.2em" などのCSS文字列。</param>
        /// <param name="targetUnit">"px", "em", "%" のいずれか。</param>
        /// <param name="defaultValue">解析失敗時のデフォルト値。</param>
        public static string? ParseAndConvert(string? input, string targetUnit, string? defaultValue = "")
        {
            if (string.IsNullOrWhiteSpace(input)) return defaultValue;

            // 数値と単位を分離
            var numPart = new string(input.TakeWhile(c => char.IsDigit(c) || c == '.' || c == '-').ToArray());
            var unitPart = input.Substring(numPart.Length).Trim().ToLower();

            if (!double.TryParse(numPart, out var value)) return defaultValue;

            // 入力単位を特定（指定がない場合はターゲット単位と同じとみなす＝変換なし）
            if (string.IsNullOrEmpty(unitPart)) unitPart = targetUnit;

            // 一旦 px に統一
            double px = unitPart switch
            {
                "em" => EmToPx(value),
                "rem" => EmToPx(value), // このアプリでは rem = em とみなす
                "%" => PercentToPx(value),
                _ => value // px または未知の単位
            };

            // ターゲット単位へ変換
            double result = targetUnit switch
            {
                "em" => PxToEm(px),
                "%" => PxToPercent(px),
                _ => px
            };

            return Round(result).ToString();
        }
    }
}
