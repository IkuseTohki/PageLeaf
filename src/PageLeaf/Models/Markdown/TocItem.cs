using System;

namespace PageLeaf.Models.Markdown
{
    /// <summary>
    /// 目次（TOC）の各アイテムを表すモデルクラスです。
    /// </summary>
    public class TocItem
    {
        /// <summary>
        /// 見出しのレベル（1-3）。
        /// </summary>
        public int Level { get; }

        /// <summary>
        /// 見出しのテキスト。
        /// </summary>
        public string Text { get; }

        /// <summary>
        /// 見出しに対応するHTML要素のID。
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Markdown内での行番号（0始まり）。
        /// </summary>
        public int LineNumber { get; }

        /// <summary>
        /// コンストラクタ。
        /// </summary>
        /// <param name="level">見出しレベル（1以上）。</param>
        /// <param name="text">見出しテキスト。</param>
        /// <param name="id">見出しID。</param>
        /// <param name="lineNumber">行番号（0以上）。</param>
        /// <exception cref="ArgumentOutOfRangeException">レベルが1未満、または行番号が0未満の場合。</exception>
        /// <exception cref="ArgumentNullException">テキストがnullの場合。</exception>
        public TocItem(int level, string text, string id, int lineNumber)
        {
            if (level < 1)
                throw new ArgumentOutOfRangeException(nameof(level), "Level must be greater than or equal to 1.");

            if (lineNumber < 0)
                throw new ArgumentOutOfRangeException(nameof(lineNumber), "LineNumber must be greater than or equal to 0.");

            Level = level;
            Text = text ?? throw new ArgumentNullException(nameof(text));
            Id = id ?? string.Empty;
            LineNumber = lineNumber;
        }

        /// <summary>
        /// プレビュー内でのナビゲーションに使用するアンカーリンク（#id）を取得します。
        /// </summary>
        /// <returns>アンカーリンク文字列。IDが空の場合は空文字列。</returns>
        public string GetAnchorLink()
        {
            return string.IsNullOrEmpty(Id) ? string.Empty : $"#{Id}";
        }
    }
}
