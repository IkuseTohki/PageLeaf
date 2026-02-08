using System.Windows;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Wpf;
using Microsoft.Web.WebView2.Core;
using System.Threading.Tasks;
using System.IO;
using System;

namespace PageLeaf.Utilities
{
    /// <summary>
    /// WebBrowser コントロールに HTML コンテンツをバインドするためのヘルパークラスです。
    /// </summary>
    public static class WebBrowserHelper
    {
        private static Task<CoreWebView2Environment>? _environmentTask;

        /// <summary>
        /// WebView2 の環境設定を取得します。
        /// ユーザーデータフォルダを AppData\Local\PageLeaf\WebView2 に設定します。
        /// </summary>
        /// <returns>WebView2 環境設定。</returns>
        private static Task<CoreWebView2Environment> GetEnvironmentAsync()
        {
            if (_environmentTask == null)
            {
                var userDataFolder = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "PageLeaf",
                    "WebView2"
                );
                _environmentTask = CoreWebView2Environment.CreateAsync(null, userDataFolder);
            }
            return _environmentTask;
        }

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

        public static readonly DependencyProperty EditorServiceProperty =
            DependencyProperty.RegisterAttached("EditorService", typeof(PageLeaf.Services.IEditorService), typeof(WebBrowserHelper), new PropertyMetadata(OnEditorServiceChanged));

        public static PageLeaf.Services.IEditorService GetEditorService(DependencyObject obj)
        {
            return (PageLeaf.Services.IEditorService)obj.GetValue(EditorServiceProperty);
        }

        public static void SetEditorService(DependencyObject obj, PageLeaf.Services.IEditorService value)
        {
            obj.SetValue(EditorServiceProperty, value);
        }

        private static void OnEditorServiceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            if (obj is WebView2 webView2)
            {
                if (e.OldValue is PageLeaf.Services.IEditorService oldService)
                {
                    oldService.SyncQuoteSettingsRequested -= (s, ev) => SyncQuoteSettings(webView2, oldService);
                    oldService.UserCssChanged -= (s, path) => SwapUserCss(webView2, path);
                }

                if (e.NewValue is PageLeaf.Services.IEditorService newService)
                {
                    newService.SyncQuoteSettingsRequested += (s, ev) => SyncQuoteSettings(webView2, newService);
                    newService.UserCssChanged += (s, path) => SwapUserCss(webView2, path);
                }
            }
        }

        private static async void SwapUserCss(WebView2 webView2, string cssPath)
        {
            if (webView2.CoreWebView2 != null && !string.IsNullOrEmpty(cssPath))
            {
                var cssUri = new Uri(cssPath).ToString();
                // キャッシュ回避のためにタイムスタンプを付与
                var timestamp = DateTime.Now.Ticks;
                var finalUri = cssUri + (cssUri.Contains("?") ? "&" : "?") + "t=" + timestamp;
                var escapedUri = System.Text.Json.JsonSerializer.Serialize(finalUri);

                var script = $@"
                    (function() {{
                        var link = document.getElementById('user-css');
                        if (link) {{
                            link.href = {escapedUri};
                        }}
                    }})();";

                try
                {
                    await webView2.CoreWebView2.ExecuteScriptAsync(script);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error swapping user CSS: {ex.Message}");
                }
            }
        }

        private static async void SyncQuoteSettings(WebView2 webView2, PageLeaf.Services.IEditorService service)
        {
            if (webView2.CoreWebView2 != null && service.CurrentDocument != null)
            {
                var doc = service.CurrentDocument;
                var quoteStyle = doc.QuoteStyle;
                var quoteIcon = doc.QuoteIcon;

                var className = $"{(quoteStyle != null && quoteStyle != "none" ? $"bq-{quoteStyle}" : "")} {(quoteIcon ? "has-icon" : "no-icon")}".Trim();
                var escapedClassName = System.Text.Json.JsonSerializer.Serialize(className);

                var script = $@"document.body.className = {escapedClassName};";

                try
                {
                    await webView2.CoreWebView2.ExecuteScriptAsync(script);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"Error syncing quote settings: {ex.Message}");
                }
            }
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
                    var env = await GetEnvironmentAsync();
                    await webView2.EnsureCoreWebView2Async(env);
                }

                // Subscribe to events
                webView2.NavigationCompleted -= WebView2_NavigationCompleted;
                webView2.NavigationCompleted += WebView2_NavigationCompleted;

                if (webView2.CoreWebView2 != null)
                {
                    webView2.CoreWebView2.NavigationStarting -= CoreWebView2_NavigationStarting;
                    webView2.CoreWebView2.NavigationStarting += CoreWebView2_NavigationStarting;
                }

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

        private static async void CoreWebView2_NavigationStarting(object? sender, CoreWebView2NavigationStartingEventArgs e)
        {
            if (sender is CoreWebView2 coreWebView2)
            {
                var uri = e.Uri;

                // If it's a fragment link (contains #), handle it manually to avoid external navigation issues
                if (uri.Contains("#"))
                {
                    var fragmentIndex = uri.IndexOf('#');
                    var fragment = uri.Substring(fragmentIndex + 1);

                    if (!string.IsNullOrEmpty(fragment))
                    {
                        e.Cancel = true;

                        // Execute script to scroll into view
                        var script = $@"
                            (function() {{
                                var id = decodeURIComponent('{fragment}');
                                var element = document.getElementById(id);
                                if (element) {{
                                    element.scrollIntoView();
                                }}}}
                            )();";
                        await coreWebView2.ExecuteScriptAsync(script);
                    }
                }
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
