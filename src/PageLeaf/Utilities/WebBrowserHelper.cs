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
            DependencyProperty.RegisterAttached("Html", typeof(string), typeof(WebBrowserHelper), new PropertyMetadata(OnHtmlChanged));

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
        /// <param name="obj">プロパティが変更された依存関係オブジェクト。</param>
        /// <param name="e">この依存関係プロパティの変更に関するイベントデータ。</param>
        private static void OnHtmlChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is WebBrowser webBrowser)
            {
                var html = e.NewValue as string;
                var contentToNavigate = string.IsNullOrEmpty(html) ? "<html><body></body></html>" : html;
                webBrowser.NavigateToString(contentToNavigate);
            }
        }
    }
}