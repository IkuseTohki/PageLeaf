using System;
using System.Windows;
using System.Windows.Input;

namespace PageLeaf.Behaviors
{
    /// <summary>
    /// ウィンドウのライフサイクルイベント（Closedなど）をコマンドにバインドするためのビヘイビア。
    /// </summary>
    public static class WindowLifecycleBehavior
    {
        public static readonly DependencyProperty ClosedCommandProperty =
            DependencyProperty.RegisterAttached(
                "ClosedCommand",
                typeof(ICommand),
                typeof(WindowLifecycleBehavior),
                new PropertyMetadata(null, OnClosedCommandChanged));

        public static ICommand GetClosedCommand(DependencyObject obj)
        {
            return (ICommand)obj.GetValue(ClosedCommandProperty);
        }

        public static void SetClosedCommand(DependencyObject obj, ICommand value)
        {
            obj.SetValue(ClosedCommandProperty, value);
        }

        private static void OnClosedCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Window window)
            {
                window.Closed -= Window_Closed;
                if (e.NewValue != null)
                {
                    window.Closed += Window_Closed;
                }
            }
        }

        private static void Window_Closed(object? sender, EventArgs e)
        {
            if (sender is Window window)
            {
                var command = GetClosedCommand(window);
                if (command != null && command.CanExecute(null))
                {
                    command.Execute(null);
                }
            }
        }
    }
}
