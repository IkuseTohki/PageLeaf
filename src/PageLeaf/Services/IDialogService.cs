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
        /// 例外の詳細を表示するダイアログを表示します。
        /// </summary>
        /// <param name="message">ユーザーに表示するメッセージ。</param>
        /// <param name="exception">表示対象の例外。</param>
        void ShowExceptionDialog(string message, System.Exception exception);

        /// <summary>
        /// 色選択ダイアログを表示します。
        /// </summary>
        /// <param name="initialColor">初期表示する色の文字列（#RRGGBB形式など）。</param>
        /// <returns>選択された色の文字列（#RRGGBB形式）。キャンセルされた場合は null。</returns>
        string? ShowColorPickerDialog(string? initialColor);

        /// <summary>
        /// 設定画面を表示します。
        /// </summary>
        void ShowSettingsDialog();

        /// <summary>
        /// メッセージボックスを表示します。
        /// </summary>
        /// <param name="message">メッセージ本文。</param>
        /// <param name="title">タイトル。</param>
        void ShowMessage(string message, string title);
    }
}
