using System.Windows;
using System.Windows.Input;

namespace PageLeaf.Behaviors
{
    /// <summary>
    /// ウィンドウをマウスドラッグで移動させるための添付ビヘイビアを提供します。
    /// </summary>
    public static class WindowDragBehavior
    {
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(WindowDragBehavior),
                new PropertyMetadata(false, OnIsEnabledChanged));

        public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);
        public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window)
            {
                if ((bool)e.NewValue)
                {
                    window.MouseLeftButtonDown += OnMouseLeftButtonDown;
                }
                else
                {
                    window.MouseLeftButtonDown -= OnMouseLeftButtonDown;
                }
            }
        }

        private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed && sender is Window window)
            {
                window.DragMove();
            }
        }
    }
}
