using Markdig;
using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace PageLeaf.Utilities.MarkdownExtensions
{
    /// <summary>
    /// コードブロックにヘッダーを追加するMarkdig拡張です。
    /// </summary>
    public class CodeBlockHeaderExtension : IMarkdownExtension
    {
        public void Setup(MarkdownPipelineBuilder pipeline)
        {
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            if (renderer is HtmlRenderer htmlRenderer)
            {
                var codeBlockRenderer = htmlRenderer.ObjectRenderers.Find<CodeBlockRenderer>();
                if (codeBlockRenderer != null)
                {
                    // 既存のレンダラーをラップする
                    htmlRenderer.ObjectRenderers.Replace<CodeBlockRenderer>(new HtmlCodeBlockHeaderRenderer(codeBlockRenderer));
                }
            }
        }
    }

    public static class MarkdownPipelineBuilderExtensions
    {
        /// <summary>
        /// コードブロックにヘッダーを追加する拡張を有効にします。
        /// </summary>
        public static MarkdownPipelineBuilder UseCodeBlockHeader(this MarkdownPipelineBuilder pipeline)
        {
            pipeline.Extensions.AddIfNotAlready<CodeBlockHeaderExtension>();
            return pipeline;
        }
    }
}
