using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using PageLeaf.Models;
using PageLeaf.Models.Markdown;
using PageLeaf.Models.Css;
using PageLeaf.Models.Css.Elements;
using PageLeaf.Models.Settings;
using PageLeaf.ViewModels;
using PageLeaf.Views;
using System;
using System.Windows; // MessageBox

namespace PageLeaf.Services
{
    /// <summary>
    /// ファイルダイアログ操作の具体的な実装を提供します。
    /// </summary>
    public class DialogService : IDialogService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IWindowService _windowService;

        public DialogService(IServiceProvider serviceProvider, IWindowService windowService)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _windowService = windowService ?? throw new ArgumentNullException(nameof(windowService));
        }

        /// <summary>
        /// ファイルを開くダイアログを表示し、選択されたファイルのパスを返します。
        /// </summary>
        /// <param name="title">ダイアログのタイトル。</param>
        /// <param name="filter">ファイルフィルタ文字列 (例: "Markdown files (*.md)|*.md|All files (*.*)|*.*")。</param>
        /// <returns>選択されたファイルの絶対パス。キャンセルされた場合はnull。</returns>
        public string? ShowOpenFileDialog(string title, string filter)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Title = title;
            openFileDialog.Filter = filter;

            if (openFileDialog.ShowDialog() == true)
            {
                return openFileDialog.FileName;
            }
            return null;
        }

        /// <summary>
        /// ファイルを保存するダイアログを表示し、選択されたファイルのパスを返します。
        /// </summary>
        /// <param name="title">ダイアログのタイトル。</param>
        /// <param name="filter">ファイルフィルタ文字列 (例: "Markdown files (*.md)|*.md|All files (*.*)|*.*")。</param>
        /// <param name="initialFileName">ダイアログの初期ファイル名。</param>
        /// <returns>選択されたファイルの絶対パス。キャンセルされた場合はnull。</returns>
        public string? ShowSaveFileDialog(string title, string filter, string? initialFileName = null)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Title = title;
            saveFileDialog.Filter = filter;
            if (!string.IsNullOrEmpty(initialFileName))
            {
                saveFileDialog.FileName = System.IO.Path.GetFileName(initialFileName); // ファイル名のみ設定
                saveFileDialog.InitialDirectory = System.IO.Path.GetDirectoryName(initialFileName); // 初期ディレクトリを設定
            }

            if (saveFileDialog.ShowDialog() == true)
            {
                return saveFileDialog.FileName;
            }
            return null;
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
                _ => SaveConfirmationResult.Cancel // デフォルトはキャンセル
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
            _windowService.ShowWindow<SettingsViewModel>();
        }

        /// <summary>
        /// バージョン情報を表示します。
        /// </summary>
        public void ShowAboutDialog()
        {
            _windowService.ShowWindow<AboutViewModel>();
        }

        public bool ShowConfirmationDialog(string message, string title)
        {
            return MessageBox.Show(Application.Current.MainWindow, message, title, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes;
        }

        public void ShowMessage(string message, string title)
        {
            MessageBox.Show(Application.Current.MainWindow, message, title, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public string? ShowInputDialog(string title, string message, string defaultInput = "")
        {
            var viewModel = new ViewModels.InputViewModel
            {
                Title = title,
                Message = message,
                InputText = defaultInput
            };

            var dialog = new InputDialog();
            dialog.DataContext = viewModel;
            dialog.Owner = Application.Current.MainWindow;

            if (dialog.ShowDialog() == true)
            {
                return viewModel.InputText;
            }
            return null;
        }
    }
}
