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
            if (value is Models.Markdown.TocItem item)
            {
                return new Thickness((item.Level - 1) * 15, 0, 0, 0);
            }
            if (value is int level)
            {
                // 後方互換性のため残すが、基本的には TocItem を渡すことを推奨
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
