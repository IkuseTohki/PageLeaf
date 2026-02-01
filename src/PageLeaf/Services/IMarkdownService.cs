
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
        /// 指定されたMarkdownテキストから見出し（H1-H3）を抽出します。
        /// </summary>
        /// <param name="markdown">Markdown文字列。</param>
        /// <returns>抽出された見出しリスト。</returns>
        System.Collections.Generic.List<PageLeaf.Models.TocItem> ExtractHeaders(string markdown);
    }
}
