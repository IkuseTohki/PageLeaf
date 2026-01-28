using System.Windows;
using System.Windows.Input;

namespace PageLeaf.Behaviors
{
    /// <summary>
    /// ウィンドウ表示時に最初の要素へフォーカスし、Tab移動をループさせるためのビヘイビア。
    /// </summary>
    public static class FocusTrapBehavior
    {
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(FocusTrapBehavior),
                new PropertyMetadata(false, OnIsEnabledChanged));

        public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);
        public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window)
            {
                window.Loaded -= Window_Loaded;
                if ((bool)e.NewValue)
                {
                    window.Loaded += Window_Loaded;
                    // ウィンドウ内のTabNavigationを強制的にCycleにする
                    KeyboardNavigation.SetTabNavigation(window, KeyboardNavigationMode.Cycle);
                }
            }
        }

        private static void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is Window window)
            {
                // 最初の要素へフォーカスを移動
                window.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
            }
        }
    }
}
