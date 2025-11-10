using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace PageLeaf.Utilities
{
    /// <summary>
    /// 文字列形式の色コードをSolidColorBrushに変換するコンバーターです。
    /// </summary>
    public class StringToBrushConverter : IValueConverter
    {
        /// <summary>
        /// 文字列形式の色コードをSolidColorBrushに変換します。
        /// </summary>
        /// <param name="value">変換する値（文字列形式の色コード）。</param>
        /// <param name="targetType">ターゲットプロパティの型。</param>
        /// <param name="parameter">コンバーターパラメーター。</param>
        /// <param name="culture">使用するカルチャ。</param>
        /// <returns>変換されたSolidColorBrush、または変換に失敗した場合はDependencyProperty.UnsetValue。</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string colorString)
            {
                if (string.IsNullOrWhiteSpace(colorString))
                {
                    return DependencyProperty.UnsetValue;
                }

                try
                {
                    // ColorConverterを使用して文字列をColorに変換
                    var color = (Color)ColorConverter.ConvertFromString(colorString);
                    return new SolidColorBrush(color);
                }
                catch (FormatException)
                {
                    // 無効な色文字列の場合
                    return DependencyProperty.UnsetValue;
                }
            }

            return DependencyProperty.UnsetValue;
        }

        /// <summary>
        /// SolidColorBrushを文字列形式の色コードに変換する操作はサポートされていません。
        /// </summary>
        /// <param name="value">変換する値。</param>
        /// <param name="targetType">ターゲットプロパティの型。</param>
        /// <param name="parameter">コンバーターパラメーター。</param>
        /// <param name="culture">使用するカルチャ。</param>
        /// <returns>常にNotSupportedExceptionをスローします。</returns>
        /// <exception cref="NotSupportedException">このメソッドはサポートされていません。</exception>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("ConvertBack is not supported for StringToBrushConverter.");
        }
    }
}
