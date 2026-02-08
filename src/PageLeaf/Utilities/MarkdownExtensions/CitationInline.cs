using Markdig.Syntax.Inlines;

namespace PageLeaf.Utilities.MarkdownExtensions
{
    /// <summary>
    /// Markdown の解析結果（抽象構文木：AST）において、「引用元」要素を表現するデータモデルです。
    /// </summary>
    /// <remarks>
    /// このクラスは以下の責務を担います：
    /// 1. 構文木（AST）上のノードとしての表現：
    ///    [cite: ...] 形式の記述が、単なる文字列ではなく独立した「引用元」要素であることを識別可能にします。
    /// 2. 解析（Parser）と描画（Renderer）の仲介：
    ///    CitationInlineParser によって生成され、HtmlCitationRenderer によって &lt;cite&gt; タグへ変換される際の橋渡しとなります。
    /// 3. 子要素の保持：
    ///    ContainerInline を継承することで、引用元テキスト内の強調やリンクなどの他のインライン要素を構造的に保持できます。
    /// </remarks>
    public class CitationInline : ContainerInline
    {
    }
}
