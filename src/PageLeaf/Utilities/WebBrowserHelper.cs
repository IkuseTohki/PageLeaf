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

        public static readonly DependencyProperty InjectedCssProperty =
            DependencyProperty.RegisterAttached("InjectedCss", typeof(string), typeof(WebBrowserHelper), new PropertyMetadata(OnInjectedCssChanged));

        public static string GetInjectedCss(DependencyObject obj)
        {
            return (string)obj.GetValue(InjectedCssProperty);
        }

        public static void SetInjectedCss(DependencyObject obj, string value)
        {
            obj.SetValue(InjectedCssProperty, value);
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
                var filePath = e.NewValue as string;

                if (webView2.CoreWebView2 == null)
                {
                    await webView2.EnsureCoreWebView2Async(null);
                }

                // Subscribe to NavigationCompleted to re-inject CSS after reload
                webView2.NavigationCompleted -= WebView2_NavigationCompleted;
                webView2.NavigationCompleted += WebView2_NavigationCompleted;

                if (webView2.CoreWebView2 != null && !string.IsNullOrEmpty(filePath))
                {
                    webView2.Source = new Uri(filePath);
                }
                else if (webView2.CoreWebView2 != null && string.IsNullOrEmpty(filePath))
                {
                    webView2.NavigateToString("<html><body></body></html>");
                }
            }
        }

        private static void WebView2_NavigationCompleted(object? sender, Microsoft.Web.WebView2.Core.CoreWebView2NavigationCompletedEventArgs e)
        {
            if (sender is WebView2 webView2)
            {
                var cssContent = GetInjectedCss(webView2);
                InjectCss(webView2, cssContent);
            }
        }

        private static void OnInjectedCssChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is WebView2 webView2)
            {
                var cssContent = e.NewValue as string;
                InjectCss(webView2, cssContent);
            }
        }

        private static async void InjectCss(WebView2 webView2, string? cssContent)
        {
            if (webView2.CoreWebView2 != null && !string.IsNullOrEmpty(cssContent))
            {
                var escapedCss = System.Text.Json.JsonSerializer.Serialize(cssContent);
                var script = $@"
                    (function() {{
                        var style = document.getElementById('dynamic-style');
                        if (style) {{
                            style.textContent = {escapedCss};
                        }}
                    }})();";

                try
                {
                    await webView2.CoreWebView2.ExecuteScriptAsync(script);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error injecting CSS: {ex.Message}");
                }
            }
        }
    }
}
