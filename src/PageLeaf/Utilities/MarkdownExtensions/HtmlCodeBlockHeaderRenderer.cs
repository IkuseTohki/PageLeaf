using Markdig.Renderers;
using Markdig.Renderers.Html;
using Markdig.Syntax;
using System.Linq;

namespace PageLeaf.Utilities.MarkdownExtensions
{
    /// <summary>
    /// コードブロックにファイル名ヘッダーを追加するHTMLレンダラーです。
    /// </summary>
    public class HtmlCodeBlockHeaderRenderer : HtmlObjectRenderer<FencedCodeBlock>
    {
        private readonly CodeBlockRenderer _baseRenderer;

        public HtmlCodeBlockHeaderRenderer(CodeBlockRenderer baseRenderer)
        {
            _baseRenderer = baseRenderer ?? new CodeBlockRenderer();
        }

        protected override void Write(HtmlRenderer renderer, FencedCodeBlock obj)
        {
            var info = obj.Info;
            var language = string.Empty;
            var fileName = string.Empty;

            if (!string.IsNullOrEmpty(info))
            {
                var parts = info.Split(':');
                language = parts[0];
                if (parts.Length >= 2)
                {
                    fileName = parts[1];
                }
            }

            // Mermaid ブロックの場合は特別な処理
            if (language == "mermaid")
            {
                renderer.Write("<div class=\"mermaid\">");
                renderer.WriteLeafRawLines(obj, false, false); // エスケープせず、生テキストを出力
                renderer.Write("</div>");
                return;
            }
            if (!string.IsNullOrEmpty(info))
            {
                // 元の情報を書き換えて、languageのみがclass属性に使われるようにする
                obj.Info = language;
            }

            // コンテナを出力
            renderer.Write("<div class=\"code-block-container\">");

            // ヘッダーを出力
            renderer.Write("<div class=\"code-block-header\">");

            // ファイル名があれば出力
            if (!string.IsNullOrEmpty(fileName))
            {
                renderer.Write("<span class=\"code-block-filename\">").WriteEscape(fileName).Write("</span>");
            }
            else
            {
                // ファイル名がない場合でも位置保持のための空の要素
                renderer.Write("<span class=\"code-block-filename\"></span>");
            }

            // コピーボタンを出力 (SVGアイコン)
            renderer.Write("<button class=\"code-block-copy-button\" onclick=\"copyCode(this)\" title=\"Copy to clipboard\">");
            // Copy Icon (GitHub style)
            renderer.Write("<svg class=\"icon-copy\" viewBox=\"0 0 16 16\" width=\"12\" height=\"12\"><path d=\"M0 6.75C0 5.784.784 5 1.75 5h1.5a.75.75 0 0 1 0 1.5h-1.5a.25.25 0 0 0-.25.25v7.5c0 .138.112.25.25.25h7.5a.25.25 0 0 0 .25-.25v-1.5a.75.75 0 0 1 1.5 0v1.5A1.75 1.75 0 0 1 9.25 16h-7.5A1.75 1.75 0 0 1 0 14.25Z M5 1.75C5 .784 5.784 0 6.75 0h7.5C15.216 0 16 .784 16 1.75v7.5A1.75 1.75 0 0 1 14.25 11h-7.5A1.75 1.75 0 0 1 5 9.25Zm1.75-.25a.25.25 0 0 0-.25.25v7.5c0 .138.112.25.25.25h7.5a.25.25 0 0 0 .25-.25v-7.5a.25.25 0 0 0-.25-.25Z\"></path></svg>");
            // Check Icon
            renderer.Write("<svg class=\"icon-check\" viewBox=\"0 0 16 16\" width=\"12\" height=\"12\" style=\"display:none;\"><path d=\"M13.78 4.22a.75.75 0 0 1 0 1.06l-7.25 7.25a.75.75 0 0 1-1.06 0L2.22 9.28a.751.751 0 0 1 .018-1.042.751.751 0 0 1 1.042-.018L6 10.94l6.72-6.72a.75.75 0 0 1 1.06 0Z\"></path></svg>");
            renderer.Write("</button>");
            renderer.Write("</div>");
            // 本体のコードブロックを出力
            _baseRenderer.Write(renderer, obj);

            renderer.Write("</div>");

            // Infoを元に戻す
            if (!string.IsNullOrEmpty(info))
            {
                obj.Info = info;
            }
        }
    }
}
