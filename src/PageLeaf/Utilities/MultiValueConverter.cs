using System;
using System.Globalization;
using System.Windows.Data;

namespace PageLeaf.Utilities
{
    /// <summary>
    /// 複数のバインディング値を配列としてそのまま渡すコンバーターです。
    /// MultiBinding において、複数の値を一つのオブジェクト（配列）として ViewModel や他のコンバーターに渡す際に使用します。
    /// </summary>
    public class MultiValueConverter : IMultiValueConverter
    {
        /// <summary>
        /// ソース値を配列としてクローンし、そのまま返します。
        /// </summary>
        /// <param name="values">ソース バインディングによって生成された値の配列。</param>
        /// <param name="targetType">バインディング ターゲット プロパティの型。</param>
        /// <param name="parameter">使用するコンバーター パラメーター。</param>
        /// <param name="culture">コンバーターで使用するカルチャ。</param>
        /// <returns>入力された値の配列のクローン。values が null の場合は null を返します。</returns>
        public object? Convert(object[]? values, Type targetType, object? parameter, CultureInfo culture)
        {
            return values?.Clone();
        }

        /// <summary>
        /// このコンバーターでは逆変換はサポートされていません。
        /// </summary>
        /// <param name="value">バインディング ターゲットによって生成される値。</param>
        /// <param name="targetTypes">変換先の型の配列。</param>
        /// <param name="parameter">使用するコンバーター パラメーター。</param>
        /// <param name="culture">コンバーターで使用するカルチャ。</param>
        /// <returns>常に NotImplementedException をスローします。</returns>
        /// <exception cref="NotImplementedException">逆変換は実装されていません。</exception>
        public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
