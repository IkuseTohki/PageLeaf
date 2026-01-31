using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace PageLeaf.Views.Controls
{
    /// <summary>
    /// FootnoteInputPopup.xaml の相互作用ロジック
    /// </summary>
    public partial class FootnoteInputPopup : UserControl
    {
        /// <summary>
        /// 入力が確定されたときに発生します。
        /// </summary>
        public event EventHandler<string>? Submitted;

        /// <summary>
        /// 入力がキャンセルされたとき、またはフォーカスを失ったときに発生します。
        /// </summary>
        public event EventHandler? Cancelled;

        /// <summary>
        /// 確定アクションを実行するコマンドです。
        /// </summary>
        public ICommand SubmitCommand { get; }

        /// <summary>
        /// キャンセルアクションを実行するコマンドです。
        /// </summary>
        public ICommand CancelCommand { get; }

        private bool _isHandled;

        /// <summary>
        /// <see cref="FootnoteInputPopup"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        public FootnoteInputPopup()
        {
            InitializeComponent();
            SubmitCommand = new PageLeaf.Utilities.DelegateCommand(_ => HandleSubmit());
            CancelCommand = new PageLeaf.Utilities.DelegateCommand(_ => HandleCancel());
        }

        /// <summary>
        /// キー入力を処理します。Enter での確定のみをハンドルします。
        /// Esc は KeyCommandBehavior によって処理されます。
        /// </summary>
        private void InputTextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                HandleSubmit();
                e.Handled = true;
            }
        }

        /// <summary>
        /// フォーカスを失った際、自動的にキャンセル処理を行います（Light-dismiss）。
        /// </summary>
        private void InputTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            HandleCancel();
        }

        /// <summary>
        /// 入力内容を確定し、イベントを発行します。
        /// </summary>
        private void HandleSubmit()
        {
            if (_isHandled) return;
            _isHandled = true;

            // 前後の空白を除去して発行
            var text = InputTextBox.Text?.Trim() ?? string.Empty;
            Submitted?.Invoke(this, text);
        }

        /// <summary>
        /// 入力をキャンセルし、イベントを発行します。
        /// </summary>
        private void HandleCancel()
        {
            if (_isHandled) return;
            _isHandled = true;

            Cancelled?.Invoke(this, EventArgs.Empty);
        }
    }
}
