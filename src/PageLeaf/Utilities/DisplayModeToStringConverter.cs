using PageLeaf.Models;
using PageLeaf.Models.Markdown;
using PageLeaf.Models.Css;
using PageLeaf.Models.Css.Elements;
using PageLeaf.Models.Settings;
using System;
using System.Globalization;
using System.Windows.Data;

namespace PageLeaf.Utilities
{
    /// <summary>
    /// DisplayMode enum を対応する日本語の文字列に変換します。
    /// </summary>
    public class DisplayModeToStringConverter : IValueConverter
    {
        /// <summary>
        /// DisplayMode enum を日本語の文字列に変換します。
        /// </summary>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DisplayMode mode)
            {
                return mode switch
                {
                    DisplayMode.Markdown => "Markdown 編集モード",
                    DisplayMode.Viewer => "ビューアーモード",
                    _ => mode.ToString(),
                };
            }
            return string.Empty;
        }

        /// <summary>
        /// 文字列を DisplayMode enum に変換します（未実装）。
        /// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
