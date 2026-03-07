namespace PageLeaf.Services
{
    /// <summary>
    /// PageLeaf 固有のダイアログ操作を含むサービスインターフェースです。
    /// </summary>
    public interface IDialogService : LeafKit.UI.Services.IDialogService
    {
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
        /// バージョン情報を表示します。
        /// </summary>
        void ShowAboutDialog();
    }
}
