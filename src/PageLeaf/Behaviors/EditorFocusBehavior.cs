using PageLeaf.Models;
using PageLeaf.Services;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Wpf;

namespace PageLeaf.Behaviors
{
    /// <summary>
    /// IEditorService のフォーカス要求イベントを購読し、関連付けられたコントロールにフォーカスを移動する添付ビヘイビアです。
    /// </summary>
    public static class EditorFocusBehavior
    {
        public static readonly DependencyProperty EditorServiceProperty =
            DependencyProperty.RegisterAttached(
                "EditorService",
                typeof(IEditorService),
                typeof(EditorFocusBehavior),
                new PropertyMetadata(null, OnEditorServiceChanged));

        public static IEditorService GetEditorService(DependencyObject obj) => (IEditorService)obj.GetValue(EditorServiceProperty);
        public static void SetEditorService(DependencyObject obj, IEditorService value) => obj.SetValue(EditorServiceProperty, value);

        public static readonly DependencyProperty TargetModeProperty =
            DependencyProperty.RegisterAttached(
                "TargetMode",
                typeof(DisplayMode),
                typeof(EditorFocusBehavior),
                new PropertyMetadata(DisplayMode.Markdown));

        public static DisplayMode GetTargetMode(DependencyObject obj) => (DisplayMode)obj.GetValue(TargetModeProperty);
        public static void SetTargetMode(DependencyObject obj, DisplayMode value) => obj.SetValue(TargetModeProperty, value);

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
                    var newListener = new FocusListener(element, newService);
                    SetListener(element, newListener);
                }
            }
        }

        private static readonly DependencyProperty ListenerProperty =
            DependencyProperty.RegisterAttached("Listener", typeof(FocusListener), typeof(EditorFocusBehavior), new PropertyMetadata(null));

        private static FocusListener? GetListener(DependencyObject obj) => (FocusListener?)obj.GetValue(ListenerProperty);
        private static void SetListener(DependencyObject obj, FocusListener? value) => obj.SetValue(ListenerProperty, value);

        private class FocusListener
        {
            private readonly FrameworkElement _element;
            private readonly IEditorService _service;

            public FocusListener(FrameworkElement element, IEditorService service)
            {
                _element = element;
                _service = service;
                _service.FocusRequested += OnFocusRequested;
            }

            public void Detach()
            {
                _service.FocusRequested -= OnFocusRequested;
            }

            private void OnFocusRequested(object? sender, DisplayMode mode)
            {
                var targetMode = GetTargetMode(_element);
                if (mode == targetMode)
                {
                    _element.Focus();
                }
            }
        }
    }
}
