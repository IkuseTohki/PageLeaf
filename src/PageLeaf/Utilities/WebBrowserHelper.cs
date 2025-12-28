using System.Windows;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Wpf;
using System.Threading.Tasks;
using System; // Added for Uri

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
        private static async void OnHtmlChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is WebView2 webView2)
            {
                var filePath = e.NewValue as string; // Changed from html to filePath

                // Ensure CoreWebView2 is initialized
                if (webView2.CoreWebView2 == null)
                {
                    await webView2.EnsureCoreWebView2Async(null);
                }

                // Navigate to the HTML file only if CoreWebView2 is initialized and filePath is not empty
                if (webView2.CoreWebView2 != null && !string.IsNullOrEmpty(filePath))
                {
                    // For local files, we need to convert the path to a file URI
                    webView2.Source = new Uri(filePath); // Changed to set Source property
                }
                else if (webView2.CoreWebView2 != null && string.IsNullOrEmpty(filePath))
                {
                    // If filePath is empty, navigate to a blank page
                    webView2.NavigateToString("<html><body></body></html>");
                }
            }
        }
    }
}
