using System;
using System.Globalization;
using System.Windows.Data;

namespace PageLeaf.Utilities
{
    /// <summary>
    /// バインドされた値とパラメータを比較し、一致するかどうかをブール値で返すコンバーターです。
    /// </summary>
    public class ComparisonConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value?.ToString() == parameter?.ToString();
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value?.Equals(true) == true ? parameter : null;
        }
    }
}
