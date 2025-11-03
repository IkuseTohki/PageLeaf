namespace PageLeaf.Models
{
    public enum SaveConfirmationResult
    {
        /// <summary>
        /// ユーザーが保存を選択した。
        /// </summary>
        Save,

        /// <summary>
        /// ユーザーが保存せずに続行を選択した（変更を破棄）。
        /// </summary>
        Discard,

        /// <summary>
        /// ユーザーが操作をキャンセルした。
        /// </summary>
        Cancel,

        /// <summary>
        /// 変更がないため、何もアクションが不要。
        /// </summary>
        NoAction
    }
}
