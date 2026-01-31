using System.Windows;
using System.Windows.Input;

namespace PageLeaf.Behaviors
{
    /// <summary>
    /// 特定のキーが押下された際にコマンドを実行するための添付ビヘイビアです。
    /// </summary>
    public static class KeyCommandBehavior
    {
        /// <summary>
        /// 監視対象のキーを指定します。デフォルトは Escape です。
        /// </summary>
        public static readonly DependencyProperty TargetKeyProperty =
            DependencyProperty.RegisterAttached(
                "TargetKey",
                typeof(Key),
                typeof(KeyCommandBehavior),
                new PropertyMetadata(Key.Escape));

        public static Key GetTargetKey(DependencyObject obj) => (Key)obj.GetValue(TargetKeyProperty);
        public static void SetTargetKey(DependencyObject obj, Key value) => obj.SetValue(TargetKeyProperty, value);

        /// <summary>
        /// キー押下時に実行するコマンドを指定します。
        /// </summary>
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached(
                "Command",
                typeof(ICommand),
                typeof(KeyCommandBehavior),
                new PropertyMetadata(null, OnCommandChanged));

        public static ICommand GetCommand(DependencyObject obj) => (ICommand)obj.GetValue(CommandProperty);
        public static void SetCommand(DependencyObject obj, ICommand value) => obj.SetValue(CommandProperty, value);

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                element.PreviewKeyDown -= OnPreviewKeyDown;
                if (e.NewValue != null)
                {
                    element.PreviewKeyDown += OnPreviewKeyDown;
                }
            }
        }

        private static void OnPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is DependencyObject d)
            {
                var targetKey = GetTargetKey(d);
                if (e.Key == targetKey)
                {
                    var command = GetCommand(d);
                    if (command != null && command.CanExecute(null))
                    {
                        command.Execute(null);
                        e.Handled = true;
                    }
                }
            }
        }
    }
}
