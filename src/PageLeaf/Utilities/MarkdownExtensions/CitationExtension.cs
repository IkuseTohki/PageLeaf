using Markdig;
using Markdig.Renderers;

namespace PageLeaf.Utilities.MarkdownExtensions
{
    /// <summary>
    /// [cite: ...] 形式の引用元をサポートするMarkdig拡張です。
    /// </summary>
    public class CitationExtension : IMarkdownExtension
    {
        public void Setup(MarkdownPipelineBuilder pipeline)
        {
            if (!pipeline.InlineParsers.Contains<CitationInlineParser>())
            {
                pipeline.InlineParsers.Insert(0, new CitationInlineParser());
            }
        }

        public void Setup(MarkdownPipeline pipeline, IMarkdownRenderer renderer)
        {
            if (renderer is HtmlRenderer htmlRenderer)
            {
                if (!htmlRenderer.ObjectRenderers.Contains<HtmlCitationRenderer>())
                {
                    htmlRenderer.ObjectRenderers.AddIfNotAlready(new HtmlCitationRenderer());
                }
            }
        }
    }

    public static class CitationExtensionExtensions
    {
        /// <summary>
        /// 引用元の拡張を有効にします。
        /// </summary>
        public static MarkdownPipelineBuilder UseCitation(this MarkdownPipelineBuilder pipeline)
        {
            pipeline.Extensions.AddIfNotAlready<CitationExtension>();
            return pipeline;
        }
    }
}
