using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PageLeaf.Utilities
{
    /// <summary>
    /// 見出しレベルをインデント用の Margin (Thickness) に変換するコンバーターです。
    /// </summary>
    public class LevelToMarginConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int level)
            {
                // レベル1 -> 0, レベル2 -> 15, レベル3 -> 30 のようにインデントを生成
                return new Thickness((level - 1) * 15, 0, 0, 0);
            }
            return new Thickness(0);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
