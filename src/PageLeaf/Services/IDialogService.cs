namespace PageLeaf.Services
{
    /// <summary>
    /// ファイルダイアログ操作を提供するサービスインターフェースです。
    /// </summary>
    public interface IDialogService
    {
        /// <summary>
        /// ファイルを開くダイアログを表示し、選択されたファイルのパスを返します。
        /// </summary>
        /// <param name="title">ダイアログのタイトル。</param>
        /// <param name="filter">ファイルフィルタ文字列 (例: "Markdown files (*.md)|*.md|All files (*.*)|*.*")。</param>
        /// <returns>選択されたファイルの絶対パス。キャンセルされた場合はnull。</returns>
        string? ShowOpenFileDialog(string title, string filter);

        /// <summary>
        /// ファイルを保存するダイアログを表示し、選択されたファイルのパスを返します。
        /// </summary>
        /// <param name="title">ダイアログのタイトル。</param>
        /// <param name="filter">ファイルフィルタ文字列 (例: "Markdown files (*.md)|*.md|All files (*.*)|*.*")。</param>
        /// <param name="initialFileName">ダイアログの初期ファイル名。</param>
        /// <returns>選択されたファイルの絶対パス。キャンセルされた場合はnull。</returns>
        string? ShowSaveFileDialog(string title, string filter, string? initialFileName = null);

        /// <summary>
        /// 未保存の変更がある場合に、保存を促す確認ダイアログを表示します。
        /// </summary>
        /// <returns>ユーザーの選択結果。</returns>
        PageLeaf.Models.SaveConfirmationResult ShowSaveConfirmationDialog();

        /// <summary>
        /// 例外が発生したことをユーザーに通知するダイアログを表示します。
        /// </summary>
        /// <param name="message">ユーザー向けのわかりやすいメッセージ。</param>
        /// <param name="exception">発生した例外オブジェクト。</param>
        void ShowExceptionDialog(string message, System.Exception exception);
    }
}
