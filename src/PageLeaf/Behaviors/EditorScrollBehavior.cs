using PageLeaf.Models;
using PageLeaf.Services;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Wpf;
using System;

namespace PageLeaf.Behaviors
{
    /// <summary>
    /// IEditorService の見出しスクロール要求イベントを購読し、関連付けられたコントロールをスクロールさせる添付ビヘイビアです。
    /// </summary>
    public static class EditorScrollBehavior
    {
        public static readonly DependencyProperty EditorServiceProperty =
            DependencyProperty.RegisterAttached(
                "EditorService",
                typeof(IEditorService),
                typeof(EditorScrollBehavior),
                new PropertyMetadata(null, OnEditorServiceChanged));

        public static IEditorService GetEditorService(DependencyObject obj) => (IEditorService)obj.GetValue(EditorServiceProperty);
        public static void SetEditorService(DependencyObject obj, IEditorService value) => obj.SetValue(EditorServiceProperty, value);

        private static void OnEditorServiceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                // 古いリスナーがあれば解除
                var oldListener = GetListener(element);
                if (oldListener != null)
                {
                    oldListener.Detach();
                    SetListener(element, null);
                }

                if (e.NewValue is IEditorService newService)
                {
                    var newListener = new ScrollListener(element, newService);
                    SetListener(element, newListener);
                }
            }
        }

        private static readonly DependencyProperty ListenerProperty =
            DependencyProperty.RegisterAttached("Listener", typeof(ScrollListener), typeof(EditorScrollBehavior), new PropertyMetadata(null));

        private static ScrollListener? GetListener(DependencyObject obj) => (ScrollListener?)obj.GetValue(ListenerProperty);
        private static void SetListener(DependencyObject obj, ScrollListener? value) => obj.SetValue(ListenerProperty, value);

        private class ScrollListener
        {
            private readonly FrameworkElement _element;
            private readonly IEditorService _service;

            public ScrollListener(FrameworkElement element, IEditorService service)
            {
                _element = element;
                _service = service;
                _service.ScrollToHeaderRequested += OnScrollToHeaderRequested;
            }

            public void Detach()
            {
                _service.ScrollToHeaderRequested -= OnScrollToHeaderRequested;
            }

            private async void OnScrollToHeaderRequested(object? sender, TocItem item)
            {
                if (_element is TextBox textBox)
                {
                    if (_service.SelectedMode == DisplayMode.Markdown)
                    {
                        if (item.LineNumber >= 0 && item.LineNumber < textBox.LineCount)
                        {
                            var charIndex = textBox.GetCharacterIndexFromLineIndex(item.LineNumber);
                            textBox.Focus();
                            textBox.Select(charIndex, 0);

                            var rect = textBox.GetRectFromCharacterIndex(charIndex);
                            if (!rect.IsEmpty)
                            {
                                var verticalOffset = textBox.VerticalOffset + rect.Top - (textBox.ViewportHeight / 2);
                                textBox.ScrollToVerticalOffset(verticalOffset);
                            }
                        }
                    }
                }
                else if (_element is WebView2 webView)
                {
                    if (_service.SelectedMode == DisplayMode.Viewer)
                    {
                        if (!string.IsNullOrEmpty(item.Id) && webView.CoreWebView2 != null)
                        {
                            await webView.ExecuteScriptAsync($"document.getElementById('{item.Id}').scrollIntoView();");
                        }
                    }
                }
            }
        }
    }
}
