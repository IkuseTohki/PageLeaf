using System.Windows;
using System.Windows.Controls;

namespace PageLeaf.Behaviors
{
    /// <summary>
    /// ボタンクリック時に親ウィンドウを閉じる添付ビヘイビア。
    /// </summary>
    public static class WindowCloseBehavior
    {
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(WindowCloseBehavior),
                new PropertyMetadata(false, OnIsEnabledChanged));

        public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);
        public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Button button)
            {
                button.Click -= OnButtonClick;
                if ((bool)e.NewValue)
                {
                    button.Click += OnButtonClick;
                }
            }
        }

        private static void OnButtonClick(object sender, RoutedEventArgs e)
        {
            if (sender is Button button)
            {
                var window = Window.GetWindow(button);
                window?.Close();
            }
        }
    }
}
