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
        /// 単位付きのCSS文字列を解析し、指定されたターゲット単位の数値文字列に変換します。
        /// </summary>
        /// <param name="input">"16px", "1.2em" などのCSS文字列。</param>
        /// <param name="targetUnit">"px", "em", "%" のいずれか。</param>
        /// <param name="defaultValue">解析失敗時のデフォルト値。</param>
        public static string? ParseAndConvert(string? input, string targetUnit, string? defaultValue = "")
        {
            if (string.IsNullOrWhiteSpace(input)) return defaultValue;

            // 数値と単位を分離
            var numPart = new string(input.TakeWhile(c => char.IsDigit(c) || c == '.').ToArray());
            var unitPart = input.Substring(numPart.Length).Trim().ToLower();

            if (!double.TryParse(numPart, out var value)) return defaultValue;

            // 入力単位を特定（指定がない場合はpxとみなす）
            if (string.IsNullOrEmpty(unitPart)) unitPart = "px";

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
