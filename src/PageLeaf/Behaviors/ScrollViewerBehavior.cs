using System.Windows;
using System.Windows.Controls;

namespace PageLeaf.Behaviors
{
    /// <summary>
    /// ScrollViewerのスクロール状態を監視し、スクロール可能方向を示す添付プロパティを提供するビヘイビアです。
    /// </summary>
    public static class ScrollViewerBehavior
    {
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(ScrollViewerBehavior), new PropertyMetadata(false, OnIsEnabledChanged));

        public static void SetIsEnabled(DependencyObject element, bool value) => element.SetValue(IsEnabledProperty, value);
        public static bool GetIsEnabled(DependencyObject element) => (bool)element.GetValue(IsEnabledProperty);

        public static readonly DependencyProperty HasScrollLeftProperty =
            DependencyProperty.RegisterAttached("HasScrollLeft", typeof(bool), typeof(ScrollViewerBehavior), new PropertyMetadata(false));

        public static void SetHasScrollLeft(DependencyObject element, bool value) => element.SetValue(HasScrollLeftProperty, value);
        public static bool GetHasScrollLeft(DependencyObject element) => (bool)element.GetValue(HasScrollLeftProperty);

        public static readonly DependencyProperty HasScrollRightProperty =
            DependencyProperty.RegisterAttached("HasScrollRight", typeof(bool), typeof(ScrollViewerBehavior), new PropertyMetadata(false));

        public static void SetHasScrollRight(DependencyObject element, bool value) => element.SetValue(HasScrollRightProperty, value);
        public static bool GetHasScrollRight(DependencyObject element) => (bool)element.GetValue(HasScrollRightProperty);

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ScrollViewer sv)
            {
                if ((bool)e.NewValue)
                {
                    sv.ScrollChanged += OnScrollChanged;
                    sv.Loaded += OnScrollViewerLoaded;
                    UpdateScrollStates(sv);
                }
                else
                {
                    sv.ScrollChanged -= OnScrollChanged;
                    sv.Loaded -= OnScrollViewerLoaded;
                }
            }
        }

        private static void OnScrollViewerLoaded(object sender, RoutedEventArgs e)
        {
            if (sender is ScrollViewer sv)
            {
                UpdateScrollStates(sv);
            }
        }

        private static void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (sender is ScrollViewer sv)
            {
                UpdateScrollStates(sv);
            }
        }

        private static void UpdateScrollStates(ScrollViewer sv)
        {
            // 水平スクロールの判定
            // 許容誤差を持たせる
            double tolerance = 1.0;

            bool hasLeft = sv.HorizontalOffset > tolerance;
            bool hasRight = sv.HorizontalOffset < (sv.ScrollableWidth - tolerance);

            SetHasScrollLeft(sv, hasLeft);
            SetHasScrollRight(sv, hasRight);
        }
    }
}
