using System;
using System.Globalization;
using System.Windows.Data;

namespace PageLeaf.Utilities
{
    public class ListMarkerTypeToContentConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string type)
            {
                return type switch
                {
                    "disc" => "●",
                    "circle" => "○",
                    "square" => "■",
                    "none" => "🚫",
                    "decimal" => "1",
                    "decimal-leading-zero" => "01",
                    "lower-alpha" => "a",
                    "upper-alpha" => "A",
                    "lower-roman" => "i",
                    "upper-roman" => "I",
                    "decimal-nested" => "1.1",
                    _ => type
                };
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
