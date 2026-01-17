namespace PageLeaf.Services
{
    /// <summary>
    /// エディタの編集支援機能のインターフェースです。
    /// </summary>
    public interface IEditingSupportService
    {
        /// <summary>
        /// 指定された行のインデント（先頭の空白文字列）を取得します。
        /// </summary>
        /// <param name="currentLine">判定対象の行文字列。</param>
        /// <returns>インデント文字列。</returns>
        string GetAutoIndent(string currentLine);

        /// <summary>
        /// 指定された行がリスト形式であれば、次の行に挿入すべきリストマーカーを取得します。
        /// </summary>
        /// <param name="currentLine">判定対象の行文字列。</param>
        /// <returns>次行のマーカー文字列。リストでない場合は null。リスト終了時は空文字列。</returns>
        string? GetAutoListMarker(string currentLine);

        /// <summary>
        /// 指定された文字に対して、自動補完すべき対となる文字を取得します。
        /// </summary>
        /// <param name="input">入力された文字。</param>
        /// <returns>対となる文字。存在しない場合は null。</returns>
        char? GetPairCharacter(char input);

        /// <summary>
        /// 指定された行がコードブロックの開始（バックティック3つ）であるかどうかを判定します。
        /// </summary>
        /// <param name="currentLine">判定対象の行文字列。</param>
        /// <returns>コードブロックの開始であれば true。</returns>
        bool IsCodeBlockStart(string currentLine);

        /// <summary>
        /// 設定に基づいたインデント文字列を取得します。
        /// </summary>
        /// <param name="settings">アプリケーション設定。</param>
        /// <returns>インデント文字列。</returns>
        string GetIndentString(PageLeaf.Models.ApplicationSettings settings);

        /// <summary>
        /// 行頭のインデントを1レベル分削除します。
        /// </summary>
        /// <param name="line">対象の行。</param>
        /// <param name="settings">アプリケーション設定。</param>
        /// <returns>インデント削除後の行。</returns>
        string DecreaseIndent(string line, PageLeaf.Models.ApplicationSettings settings);

        /// <summary>
        /// 行頭に1レベル分のインデントを追加します。
        /// </summary>
        /// <returns>インデント追加後の行。</returns>
        string IncreaseIndent(string line, PageLeaf.Models.ApplicationSettings settings);

        /// <summary>
        /// 指定された行の見出しレベルを切り替えます。
        /// </summary>
        /// <param name="line">対象の行。</param>
        /// <param name="level">設定する見出しレベル (1-6)。</param>
        /// <returns>変換後の行。</returns>
        string ToggleHeading(string line, int level);

        /// <summary>
        /// TSV または CSV 形式のテキストを Markdown テーブル形式に変換します。
        /// </summary>
        /// <param name="text">変換対象のテキスト。</param>
        /// <returns>Markdown テーブル文字列。表形式でない場合は null。</returns>
        string? ConvertToMarkdownTable(string text);

        /// <summary>
        /// 改ページ用のHTMLタグ文字列を取得します。
        /// </summary>
        /// <returns>改ページ用のHTMLタグ。</returns>
        string GetPageBreakString();
    }
}
