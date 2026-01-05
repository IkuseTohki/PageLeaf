using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace PageLeaf.Behaviors
{
    /// <summary>
    /// ファイルのドラッグ＆ドロップ操作をコマンドに結びつける添付ビヘイビアを提供します。
    /// </summary>
    public static class FileDropBehavior
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.RegisterAttached(
                "Command",
                typeof(ICommand),
                typeof(FileDropBehavior),
                new PropertyMetadata(null, OnCommandChanged));

        public static ICommand GetCommand(DependencyObject obj) => (ICommand)obj.GetValue(CommandProperty);
        public static void SetCommand(DependencyObject obj, ICommand value) => obj.SetValue(CommandProperty, value);

        private static void OnCommandChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is UIElement element)
            {
                if (e.NewValue != null)
                {
                    element.AllowDrop = true;
                    element.PreviewDragOver += OnDragOver;
                    element.PreviewDrop += OnDrop;
                }
                else
                {
                    element.AllowDrop = false;
                    element.PreviewDragOver -= OnDragOver;
                    element.PreviewDrop -= OnDrop;
                }
            }
        }

        private static void OnDragOver(object sender, DragEventArgs e)
        {
            // ファイルがドラッグされている場合のみ受け入れる
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                e.Effects = DragDropEffects.Copy;
            }
            else
            {
                e.Effects = DragDropEffects.None;
            }
            e.Handled = true;
        }

        private static void OnDrop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                var files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length > 0)
                {
                    // 最初のファイルのみを対象とする
                    var filePath = files[0];
                    var extension = Path.GetExtension(filePath).ToLowerInvariant();

                    // Markdownファイルのみを許可
                    if (extension == ".md" || extension == ".markdown")
                    {
                        var command = GetCommand((DependencyObject)sender);
                        if (command != null && command.CanExecute(filePath))
                        {
                            command.Execute(filePath);
                        }
                    }
                }
            }
            e.Handled = true;
        }
    }
}
