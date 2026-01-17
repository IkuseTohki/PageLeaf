using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PageLeaf.Utilities
{
    /// <summary>
    /// バインドされた値とパラメータを比較し、一致するかどうかをブール値またはVisibilityで返すコンバーターです。
    /// </summary>
    public class ComparisonConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool result = value?.ToString() == parameter?.ToString();

            if (targetType == typeof(Visibility))
            {
                return result ? Visibility.Visible : Visibility.Collapsed;
            }

            return result;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value?.Equals(true) == true ? parameter : null;
        }
    }
}
