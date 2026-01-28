using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace PageLeaf.Behaviors
{
    /// <summary>
    /// ウィンドウの種類に応じた出現アニメーションを制御するための列挙型。
    /// </summary>
    public enum WindowAnimationType
    {
        None,
        Modal,
        Modeless
    }

    /// <summary>
    /// ウィンドウの出現アニメーション（フェード、スケール）を制御するビヘイビア。
    /// </summary>
    public static class WindowAnimationBehavior
    {
        public static readonly DependencyProperty AnimationTypeProperty =
            DependencyProperty.RegisterAttached(
                "AnimationType",
                typeof(WindowAnimationType),
                typeof(WindowAnimationBehavior),
                new PropertyMetadata(WindowAnimationType.None, OnAnimationTypeChanged));

        public static WindowAnimationType GetAnimationType(DependencyObject obj) => (WindowAnimationType)obj.GetValue(AnimationTypeProperty);
        public static void SetAnimationType(DependencyObject obj, WindowAnimationType value) => obj.SetValue(AnimationTypeProperty, value);

        private static void OnAnimationTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window)
            {
                window.Loaded -= Window_Loaded;
                if ((WindowAnimationType)e.NewValue != WindowAnimationType.None)
                {
                    window.Loaded += Window_Loaded;
                    // 初期状態を非表示（透明）に設定
                    window.Opacity = 0;
                }
            }
        }

        private static void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (!(sender is Window window)) return;

            var type = GetAnimationType(window);
            var storyboard = new Storyboard();
            var duration = TimeSpan.FromMilliseconds(200);
            var easing = new QuadraticEase { EasingMode = EasingMode.EaseOut };

            // フェードインアニメーション (Window自体に適用可能)
            var fadeAnimation = new DoubleAnimation(0, 1, duration) { EasingFunction = easing };
            Storyboard.SetTarget(fadeAnimation, window);
            Storyboard.SetTargetProperty(fadeAnimation, new PropertyPath(Window.OpacityProperty));
            storyboard.Children.Add(fadeAnimation);

            // モーダルの場合は中身の要素にスケールアップアニメーションを追加
            if (type == WindowAnimationType.Modal && window.Content is FrameworkElement content)
            {
                // RenderTransformOrigin を中央に設定
                content.RenderTransformOrigin = new Point(0.5, 0.5);

                var scaleTransform = new ScaleTransform(0.95, 0.95);
                content.RenderTransform = scaleTransform;

                var scaleXAnimation = new DoubleAnimation(0.95, 1.0, duration) { EasingFunction = easing };
                Storyboard.SetTarget(scaleXAnimation, content);
                Storyboard.SetTargetProperty(scaleXAnimation, new PropertyPath("RenderTransform.ScaleX"));
                storyboard.Children.Add(scaleXAnimation);

                var scaleYAnimation = new DoubleAnimation(0.95, 1.0, duration) { EasingFunction = easing };
                Storyboard.SetTarget(scaleYAnimation, content);
                Storyboard.SetTargetProperty(scaleYAnimation, new PropertyPath("RenderTransform.ScaleY"));
                storyboard.Children.Add(scaleYAnimation);
            }

            storyboard.Begin();
        }
    }
}
