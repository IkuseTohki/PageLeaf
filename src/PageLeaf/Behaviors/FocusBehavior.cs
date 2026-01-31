using System.Windows;

namespace PageLeaf.Behaviors
{
    /// <summary>
    /// コントロールのフォーカスを制御するための添付ビヘイビアです。
    /// </summary>
    public static class FocusBehavior
    {
        /// <summary>
        /// コントロールがロードされたときにフォーカスを設定するかどうかを決定する添付プロパティです。
        /// </summary>
        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.RegisterAttached(
                "IsFocused",
                typeof(bool),
                typeof(FocusBehavior),
                new PropertyMetadata(false, OnIsFocusedChanged));

        public static bool GetIsFocused(DependencyObject obj) => (bool)obj.GetValue(IsFocusedProperty);
        public static void SetIsFocused(DependencyObject obj, bool value) => obj.SetValue(IsFocusedProperty, value);

        private static void OnIsFocusedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element && (bool)e.NewValue)
            {
                if (element.IsLoaded)
                {
                    element.Focus();
                }
                else
                {
                    element.Loaded += (s, ev) => element.Focus();
                }
            }
        }
    }
}
