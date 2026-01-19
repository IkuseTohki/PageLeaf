namespace PageLeaf.Models
{
    /// <summary>
    /// チートシートに表示する項目を表すモデルクラス。
    /// </summary>
    public class CheatSheetItem
    {
        /// <summary>
        /// 記法またはキー操作 (例: "**Bold**", "Ctrl+S")
        /// </summary>
        public string Syntax { get; set; } = string.Empty;

        /// <summary>
        /// 説明 (例: "太字", "上書き保存")
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// カテゴリ (例: "見出し", "ファイル操作") - グルーピング用
        /// </summary>
        public string Category { get; set; } = string.Empty;

        /// <summary>
        /// 補足情報 (任意)
        /// </summary>
        public string Note { get; set; } = string.Empty;

        /// <summary>
        /// 関連するショートカット (Markdown項目用)
        /// </summary>
        public string RelatedShortcut { get; set; } = string.Empty;
    }
}
