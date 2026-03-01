using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace PageLeaf.Utilities
{
    /// <summary>
    /// double 型の値を WPF の GridLength 型に変換するコンバーターです。
    /// 主に ViewModel の数値を Grid の幅や高さにバインドする際に使用します。
    /// </summary>
    [ValueConversion(typeof(double), typeof(GridLength))]
    public class DoubleToGridLengthConverter : IValueConverter
    {
        /// <summary>
        /// double 値を GridLength に変換します。
        /// </summary>
        /// <param name="value">変換元の double 値。</param>
        /// <param name="targetType">バインディング ターゲット プロパティの型。</param>
        /// <param name="parameter">使用するコンバーター パラメーター。</param>
        /// <param name="culture">コンバーターで使用するカルチャ。</param>
        /// <returns>変換された GridLength オブジェクト。値が不正な場合は 0 の GridLength を返します。</returns>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                // 負の値やNaNの場合は0にする
                if (double.IsNaN(doubleValue) || doubleValue < 0)
                {
                    return new GridLength(0);
                }
                return new GridLength(doubleValue);
            }
            return new GridLength(0);
        }

        /// <summary>
        /// GridLength を double 値に変換し戻します。
        /// </summary>
        /// <param name="value">変換元の GridLength オブジェクト。</param>
        /// <param name="targetType">変換先の型。</param>
        /// <param name="parameter">使用するコンバーター パラメーター。</param>
        /// <param name="culture">コンバーターで使用するカルチャ。</param>
        /// <returns>GridLength の数値部分。変換できない場合は 0.0 を返します。</returns>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is GridLength gridLength)
            {
                return gridLength.Value;
            }
            return 0.0;
        }
    }
}
