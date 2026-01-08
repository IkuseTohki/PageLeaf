namespace PageLeaf.Models
{
    /// <summary>
    /// 目次（TOC）の各アイテムを表すモデルクラスです。
    /// </summary>
    public class TocItem
    {
        /// <summary>
        /// 見出しのレベル（1-3）。
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 見出しのテキスト。
        /// </summary>
        public string Text { get; set; } = string.Empty;

        /// <summary>
        /// 見出しに対応するHTML要素のID。
        /// </summary>
        public string Id { get; set; } = string.Empty;

        /// <summary>
        /// Markdown内での行番号（0始まり）。
        /// </summary>
        public int LineNumber { get; set; }
    }
}
