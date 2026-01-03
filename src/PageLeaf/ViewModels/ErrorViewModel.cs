using System;
using System.Windows;
using System.Windows.Input;
using PageLeaf.Utilities;

namespace PageLeaf.ViewModels
{
    /// <summary>
    /// エラー情報を表示および操作するための ViewModel です。
    /// </summary>
    public class ErrorViewModel : ViewModelBase
    {
        private string _errorMessage;

        /// <summary>
        /// 表示するエラーメッセージを取得します。
        /// </summary>
        public string ErrorMessage
        {
            get => _errorMessage;
            private set
            {
                _errorMessage = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// エラー情報をクリップボードにコピーするコマンドを取得します。
        /// </summary>
        public ICommand CopyCommand { get; }

        /// <summary>
        /// <see cref="ErrorViewModel"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="message">ユーザー向けの基本メッセージ。</param>
        /// <param name="exception">詳細情報を含む例外オブジェクト。</param>
        public ErrorViewModel(string message, Exception exception)
        {
            ArgumentNullException.ThrowIfNull(exception);

            _errorMessage = $"{message}\n\n" +
                            $"[Message]\n{exception.Message}\n\n" +
                            $"[Exception Type]\n{exception.GetType().FullName}\n\n" +
                            $"[Stack Trace]\n{exception.StackTrace}";

            CopyCommand = new DelegateCommand(ExecuteCopy);
        }

        private void ExecuteCopy(object? parameter)
        {
            try
            {
                Clipboard.SetText(ErrorMessage);
                // View への通知や MessageBox は ViewModel から直接行わないのが理想的だが、
                // 簡易化のため現状の挙動を維持。
                MessageBox.Show("クリップボードにコピーしました。", "PageLeaf", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"コピーに失敗しました: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
