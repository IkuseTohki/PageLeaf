
using System;
using System.Globalization;
using System.Windows.Data;

namespace PageLeaf.Utilities
{
    public class RgbaToHexConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string rgbaString && !string.IsNullOrWhiteSpace(rgbaString))
            {
                try
                {
                    var parts = rgbaString.Trim().Replace("rgba(", "").Replace(")", "").Split(',');
                    if (parts.Length == 4)
                    {
                        int r = int.Parse(parts[0].Trim());
                        int g = int.Parse(parts[1].Trim());
                        int b = int.Parse(parts[2].Trim());
                        return $"#{r:x2}{g:x2}{b:x2}";
                    }
                }
                catch (Exception)
                {
                    // Parsing error, return original value
                    return value;
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string hexString && !string.IsNullOrWhiteSpace(hexString) && hexString.StartsWith("#"))
            {
                try
                {
                    if (hexString.Length == 7)
                    {
                        int r = System.Convert.ToInt32(hexString.Substring(1, 2), 16);
                        int g = System.Convert.ToInt32(hexString.Substring(3, 2), 16);
                        int b = System.Convert.ToInt32(hexString.Substring(5, 2), 16);
                        return $"rgba({r}, {g}, {b}, 1)";
                    }
                }
                catch (Exception)
                {
                    // Parsing error, return original value
                    return value;
                }
            }
            return value;
        }
    }
}
