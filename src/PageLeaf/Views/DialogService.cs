using Microsoft.Win32;
using PageLeaf.Services;
using System;

namespace PageLeaf.Views
{
    /// <summary>
    /// ファイルダイアログ操作の具体的な実装を提供します。
    /// </summary>
    public class DialogService : IDialogService
    {
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
    }
}
