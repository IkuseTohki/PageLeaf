using System;
using System.Windows;

namespace PageLeaf.Views
{
    /// <summary>
    /// 致命的な例外情報を表示し、クリップボードへのコピー機能を提供するウィンドウ。
    /// </summary>
    public partial class ErrorWindow : Window
    {
        public ErrorWindow(string message, Exception exception)
        {
            InitializeComponent();

            ErrorTextBox.Text = $"{message}\n\n" +
                               $"[Message]\n{exception.Message}\n\n" +
                               $"[Exception Type]\n{exception.GetType().FullName}\n\n" +
                               $"[Stack Trace]\n{exception.StackTrace}";
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Clipboard.SetText(ErrorTextBox.Text);
                MessageBox.Show("クリップボードにコピーしました。", "PageLeaf", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"コピーに失敗しました: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
