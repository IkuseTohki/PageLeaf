using System.Drawing;

namespace PageLeaf.Utilities
{
    /// <summary>
    /// 色変換に関連するヘルパーメソッドを提供します。
    /// </summary>
    public static class ColorConverterHelper
    {
        /// <summary>
        /// Colorオブジェクトを#RRGGBB形式の文字列に変換します。
        /// </summary>
        /// <param name="color">変換するColorオブジェクト。</param>
        /// <returns>#RRGGBB形式の文字列。</returns>
        public static string ToRgbString(Color color)
        {
            return $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        }
    }
}
