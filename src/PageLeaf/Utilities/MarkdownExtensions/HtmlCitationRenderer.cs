using Markdig.Renderers;
using Markdig.Renderers.Html;

namespace PageLeaf.Utilities.MarkdownExtensions
{
    /// <summary>
    /// <see cref="CitationInline"/> を HTML の &lt;cite&gt; タグとしてレンダリングするクラスです。
    /// </summary>
    public class HtmlCitationRenderer : HtmlObjectRenderer<CitationInline>
    {
        protected override void Write(HtmlRenderer renderer, CitationInline obj)
        {
            if (renderer.EnableHtmlForInline)
            {
                renderer.Write("<cite>");
            }

            renderer.WriteChildren(obj);

            if (renderer.EnableHtmlForInline)
            {
                renderer.Write("</cite>");
            }
        }
    }
}
