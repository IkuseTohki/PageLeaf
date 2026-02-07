namespace PageLeaf.Models.Settings
{
    /// <summary>
    /// ログ出力のレベルを定義します。
    /// </summary>
    public enum LogOutputLevel
    {
        /// <summary>
        /// 通常の操作履歴、警告、エラーを出力します。
        /// </summary>
        Standard,

        /// <summary>
        /// 開発・調査用の詳細なデバッグ情報を含めてすべて出力します。
        /// </summary>
        Development
    }
}
