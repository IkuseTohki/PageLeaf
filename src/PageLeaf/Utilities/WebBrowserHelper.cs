using System.Windows;
using System.Windows.Controls;

namespace PageLeaf.Utilities
{
    /// <summary>
    /// WebBrowser コントロールに HTML コンテンツをバインドするためのヘルパークラスです。
    /// </summary>
    public static class WebBrowserHelper
    {
        /// <summary>
        /// WebBrowser の HTML コンテンツを管理する添付プロパティを定義します。
        /// </summary>
        public static readonly DependencyProperty HtmlProperty =
            DependencyProperty.RegisterAttached("Html", typeof(string), typeof(WebBrowserHelper), new PropertyMetadata(null, OnHtmlChanged));

        /// <summary>
        /// Html 添付プロパティの現在の値を取得します。
        /// </summary>
        /// <param name="obj">依存関係プロパティの値を設定する要素。</param>
        /// <returns>Html 添付プロパティの現在の値。</returns>
        public static string GetHtml(DependencyObject obj)
        {
            return (string)obj.GetValue(HtmlProperty);
        }

        /// <summary>
        /// Html 添付プロパティの現在の値を設定します。
        /// </summary>
        /// <param name="obj">依存関係プロパティの値を設定する要素。</param>
        /// <param name="value">設定する値。</param>
        public static void SetHtml(DependencyObject obj, string value)
        {
            obj.SetValue(HtmlProperty, value);
        }

        /// <summary>
        /// Html 添付プロパティの値が変更されたときに呼び出されます。
        /// </summary>
        /// <param name="d">プロパティが変更された依存関係オブジェクト。</param>
        /// <param name="e">この依存関係プロパティの変更に関するイベントデータ。</param>
        private static void OnHtmlChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is WebBrowser webBrowser)
            {
                webBrowser.NavigateToString(e.NewValue as string ?? string.Empty);
            }
        }
    }
}