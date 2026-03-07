using PageLeaf.Models;
using PageLeaf.ViewModels;
using PageLeaf.Views;
using System;
using System.Windows;
using LeafKit.UI.Services;

namespace PageLeaf.Services
{
    /// <summary>
    /// PageLeaf 固有のダイアログ操作を含むサービスの実装を提供します。
    /// </summary>
    public class DialogService : LeafKit.UI.Services.DialogService, IDialogService
    {
        private readonly IWindowService _windowService;

        public DialogService(IWindowService windowService)
        {
            _windowService = windowService ?? throw new ArgumentNullException(nameof(windowService));
        }

        /// <summary>
        /// 未保存の変更がある場合に、保存を促す確認ダイアログを表示します。
        /// </summary>
        /// <returns>ユーザーの選択結果。</returns>
        public SaveConfirmationResult ShowSaveConfirmationDialog()
        {
            MessageBoxResult result = MessageBox.Show(
                Application.Current.MainWindow,
                "未保存の変更があります。保存しますか？",
                "PageLeaf",
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Warning
            );

            return result switch
            {
                MessageBoxResult.Yes => SaveConfirmationResult.Save,
                MessageBoxResult.No => SaveConfirmationResult.Discard,
                MessageBoxResult.Cancel => SaveConfirmationResult.Cancel,
                _ => SaveConfirmationResult.Cancel
            };
        }

        /// <summary>
        /// 例外が発生したことをユーザーに通知するダイアログを表示します。
        /// </summary>
        /// <param name="message">ユーザー向けのわかりやすいメッセージ。</param>
        /// <param name="exception">発生した例外オブジェクト。</param>
        public void ShowExceptionDialog(string message, Exception exception)
        {
            var viewModel = new ViewModels.ErrorViewModel(message, exception);
            var errorWindow = new ErrorWindow(viewModel);
            errorWindow.Owner = Application.Current.MainWindow;
            errorWindow.ShowDialog();
        }

        /// <summary>
        /// 色選択ダイアログを表示します。
        /// </summary>
        /// <param name="initialColor">初期表示する色の文字列（#RRGGBB形式など）。</param>
        /// <returns>選択された色の文字列（#RRGGBB形式）。キャンセルされた場合は null。</returns>
        public string? ShowColorPickerDialog(string? initialColor)
        {
            var colorDialog = new System.Windows.Forms.ColorDialog();

            if (!string.IsNullOrEmpty(initialColor))
            {
                try
                {
                    var color = (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(initialColor);
                    colorDialog.Color = System.Drawing.Color.FromArgb(color.A, color.R, color.G, color.B);
                }
                catch
                {
                    // 無視
                }
            }

            if (colorDialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                return Utilities.ColorConverterHelper.ToRgbString(colorDialog.Color);
            }

            return null;
        }

        /// <summary>
        /// 設定画面を表示します。
        /// </summary>
        public void ShowSettingsDialog()
        {
            _windowService.Show<SettingsViewModel>();
        }

        /// <summary>
        /// バージョン情報を表示します。
        /// </summary>
        public void ShowAboutDialog()
        {
            _windowService.Show<AboutViewModel>();
        }
    }
}
