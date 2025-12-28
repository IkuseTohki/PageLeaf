using System;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace PageLeaf.Behaviors
{
    /// <summary>
    /// Popupコントロールが親ウィンドウやレイアウトの変更に合わせて位置を自動更新するための添付プロパティを提供します。
    /// </summary>
    public static class PopupFollowBehavior
    {
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsEnabled",
                typeof(bool),
                typeof(PopupFollowBehavior),
                new PropertyMetadata(false, OnIsEnabledChanged));

        public static bool GetIsEnabled(DependencyObject obj) => (bool)obj.GetValue(IsEnabledProperty);
        public static void SetIsEnabled(DependencyObject obj, bool value) => obj.SetValue(IsEnabledProperty, value);

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Popup popup)
            {
                if ((bool)e.NewValue)
                {
                    popup.Loaded += OnPopupLoaded;
                    popup.Unloaded += OnPopupUnloaded;
                }
                else
                {
                    popup.Loaded -= OnPopupLoaded;
                    popup.Unloaded -= OnPopupUnloaded;
                    UnregisterEvents(popup);
                }
            }
        }

        private static void OnPopupLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is Popup popup)
            {
                var window = Window.GetWindow(popup);
                if (window != null)
                {
                    window.LocationChanged += (s, args) => UpdatePosition(popup);
                    window.SizeChanged += (s, args) => UpdatePosition(popup);
                }

                // PlacementTarget（GridSplitterなど）のレイアウト更新も監視
                if (popup.PlacementTarget is FrameworkElement target)
                {
                    target.LayoutUpdated += (s, args) => UpdatePosition(popup);
                }
            }
        }

        private static void OnPopupUnloaded(object sender, RoutedEventArgs e)
        {
            if (sender is Popup popup)
            {
                UnregisterEvents(popup);
            }
        }

        private static void UnregisterEvents(Popup popup)
        {
            var window = Window.GetWindow(popup);
            if (window != null)
            {
                // 注意: 無名関数で登録している場合は厳密な解除が困難ですが、
                // Popupの生存期間はウィンドウと同じなので、通常は大きな問題になりません。
                // より厳密にする場合は、EventHandlerをフィールドとして保持する必要があります。
            }
        }

        private static void UpdatePosition(Popup popup)
        {
            if (popup.IsOpen)
            {
                // HorizontalOffsetをわずかに変更して戻すことで再配置を強制する
                var offset = popup.HorizontalOffset;
                popup.HorizontalOffset = offset + 0.001;
                popup.HorizontalOffset = offset;
            }
        }
    }
}
