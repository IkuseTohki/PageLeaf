
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
        /// <returns>生成されたHTML文字列。</returns>
        string ConvertToHtml(string markdown, string? cssPath);
    }
}
