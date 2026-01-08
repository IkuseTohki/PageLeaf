
namespace PageLeaf.Services
{
    /// <summary>
    /// MarkdownテキストをHTMLに変換するサービスインターフェースです。
    /// </summary>
    public interface IMarkdownService
    {
        /// <summary>
        /// Markdown文字列を、プレビュー用の完全なHTMLドキュメント（headタグ等を含む）に変換します。
        /// </summary>
        /// <param name="markdown">変換対象のMarkdown文字列。</param>
        /// <param name="cssPath">HTMLにリンクするCSSファイルの絶対パス。nullの場合はCSSリンクを含めません。</param>
        /// <param name="baseDirectory">相対リンク解決のためのベースディレクトリパス。nullの場合は設定されません。</param>
        /// <returns>生成されたHTML文字列。</returns>
        string ConvertToHtml(string markdown, string? cssPath, string? baseDirectory = null);

        /// <summary>
        /// 指定されたMarkdownテキストからフロントマターを読み取ります。
        /// </summary>
        /// <param name="markdown">解析対象のMarkdown文字列。</param>
        /// <returns>キーと値のペアを含む辞書。フロントマターが存在しない場合は空の辞書。</returns>
        System.Collections.Generic.Dictionary<string, object> ParseFrontMatter(string markdown);

        /// <summary>
        /// 指定されたMarkdownテキストのフロントマターを更新（または新規作成）します。
        /// </summary>
        /// <param name="markdown">更新対象のMarkdown文字列。</param>
        /// <param name="newFrontMatter">設定するフロントマターの内容。</param>
        /// <returns>更新されたMarkdown文字列。</returns>
        string UpdateFrontMatter(string markdown, System.Collections.Generic.Dictionary<string, object> newFrontMatter);

        /// <summary>
        /// 指定されたMarkdownテキストから見出し（H1-H3）を抽出します。
        /// </summary>
        /// <param name="markdown">Markdown文字列。</param>
        /// <returns>抽出された見出しリスト。</returns>
        System.Collections.Generic.List<PageLeaf.Models.TocItem> ExtractHeaders(string markdown);
    }
}
