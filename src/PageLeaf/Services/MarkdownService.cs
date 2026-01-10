using Markdig;
using System.Text;
using System;
using System.IO;
using System.Reflection;
using PageLeaf.Utilities.MarkdownExtensions;
using System.Collections.Generic;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Markdig.Syntax;
using Markdig.Renderers.Html;

namespace PageLeaf.Services
{
    public class MarkdownService : IMarkdownService
    {
        private readonly ISettingsService _settingsService;

        public MarkdownService(ISettingsService settingsService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        }

        public string ConvertToHtml(string markdown, string? cssPath, string? baseDirectory = null)
        {
            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UseYamlFrontMatter()
                .UseCodeBlockHeader()
                .Build();
            var htmlBody = Markdown.ToHtml(markdown ?? string.Empty, pipeline);

            var headBuilder = new StringBuilder();
            headBuilder.AppendLine("<meta charset=\"UTF-8\">");

            // ベースディレクトリの設定（画像等の相対パス解決用）
            if (!string.IsNullOrEmpty(baseDirectory))
            {
                try
                {
                    // ディレクトリパスをURIに変換し、末尾にスラッシュを保証
                    var baseUri = new Uri(baseDirectory).AbsoluteUri;
                    if (!baseUri.EndsWith("/")) baseUri += "/";
                    headBuilder.AppendLine($"<base href=\"{baseUri}\" />");
                }
                catch (Exception)
                {
                    // URI変換エラー時はbaseタグを追加しない
                }
            }

            // 拡張機能用のベーススタイルを追加
            var extensionsCssPath = Path.Combine(App.BaseDirectory, "css", "extensions.css");
            var extensionsCssUri = new Uri(extensionsCssPath).AbsoluteUri;
            headBuilder.AppendLine($"<link rel=\"stylesheet\" href=\"{extensionsCssUri}\">");

            if (!string.IsNullOrEmpty(cssPath))
            {
                var timestamp = DateTime.Now.Ticks; // タイムスタンプを取得
                headBuilder.AppendLine($"<link rel=\"stylesheet\" href=\"{cssPath}?v={timestamp}\">");
            }

            headBuilder.AppendLine("<style id=\"dynamic-style\"></style>");

            // highlight.jsのCSSへのリンクを追加 (設定から取得)
            var themeName = _settingsService.CurrentSettings.CodeBlockTheme;
            if (string.IsNullOrEmpty(themeName)) themeName = "github.css";

            var cssFilePath = Path.Combine(App.BaseDirectory, "highlight", "styles", themeName);
            var cssFileUri = new Uri(cssFilePath).AbsoluteUri;
            headBuilder.AppendLine($"<link rel=\"stylesheet\" href=\"{cssFileUri}\">");

            var htmlBuilder = new StringBuilder();
            htmlBuilder.AppendLine("<!DOCTYPE html>");
            htmlBuilder.AppendLine("<html>");
            htmlBuilder.AppendLine("<head>");
            htmlBuilder.Append(headBuilder);
            htmlBuilder.AppendLine("</head>");
            htmlBuilder.AppendLine("<body>");
            htmlBuilder.Append(htmlBody);

            // highlight.jsライブラリへのリンクを絶対パスで指定
            var scriptFilePath = Path.Combine(App.BaseDirectory, "highlight", "highlight.min.js");
            var extensionScriptPath = Path.Combine(App.BaseDirectory, "highlight", "pageleaf-extensions.js");
            var mermaidScriptPath = Path.Combine(App.BaseDirectory, "mermaid", "mermaid.min.js");

            // file:// スキームのURIに変換
            var scriptFileUri = new Uri(scriptFilePath).AbsoluteUri;
            var extensionScriptUri = new Uri(extensionScriptPath).AbsoluteUri;
            var mermaidScriptUri = new Uri(mermaidScriptPath).AbsoluteUri;

            htmlBuilder.AppendLine($"<script src=\"{scriptFileUri}\"></script>");
            htmlBuilder.AppendLine($"<script src=\"{extensionScriptUri}\"></script>");
            htmlBuilder.AppendLine($"<script src=\"{mermaidScriptUri}\"></script>");
            htmlBuilder.AppendLine("<script>hljs.highlightAll();</script>");
            htmlBuilder.AppendLine("<script>");
            htmlBuilder.AppendLine("  mermaid.initialize({ startOnLoad: true, theme: 'default' });");
            htmlBuilder.AppendLine("  mermaid.contentLoaded();");
            htmlBuilder.AppendLine("</script>");

            htmlBuilder.AppendLine("</body>");
            htmlBuilder.AppendLine("</html>");

            return htmlBuilder.ToString();
        }

        public Dictionary<string, object> ParseFrontMatter(string markdown)
        {
            if (string.IsNullOrEmpty(markdown) || !markdown.TrimStart().StartsWith("---"))
            {
                return new Dictionary<string, object>();
            }

            using (var reader = new StringReader(markdown.TrimStart()))
            {
                var line = reader.ReadLine(); // First ---
                if (line?.TrimEnd() != "---") return new Dictionary<string, object>();

                var yamlContent = new StringBuilder();
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.TrimEnd() == "---")
                    {
                        break;
                    }
                    yamlContent.AppendLine(line);
                }

                if (yamlContent.Length == 0) return new Dictionary<string, object>();

                try
                {
                    var deserializer = new DeserializerBuilder()
                        .WithNamingConvention(NullNamingConvention.Instance)
                        .Build();
                    var result = deserializer.Deserialize<Dictionary<string, object>>(yamlContent.ToString());
                    return result ?? new Dictionary<string, object>();
                }
                catch
                {
                    return new Dictionary<string, object>();
                }
            }
        }

        public (Dictionary<string, object> FrontMatter, string Body) Split(string markdown)
        {
            if (string.IsNullOrEmpty(markdown))
            {
                return (new Dictionary<string, object>(), string.Empty);
            }

            var frontMatter = ParseFrontMatter(markdown);
            var body = markdown;

            if (markdown.TrimStart().StartsWith("---"))
            {
                var firstDash = markdown.IndexOf("---");
                var secondDash = markdown.IndexOf("---", firstDash + 3);
                if (secondDash != -1)
                {
                    var endOfDash = secondDash + 3;
                    // 終了 "---" 直後の改行コードを1つだけスキップする
                    if (markdown.Length > endOfDash)
                    {
                        if (markdown.Substring(endOfDash).StartsWith("\r\n")) endOfDash += 2;
                        else if (markdown.Substring(endOfDash).StartsWith("\n")) endOfDash += 1;
                    }
                    body = markdown.Substring(endOfDash);
                }
            }

            return (frontMatter, body);
        }

        public string Join(Dictionary<string, object> frontMatter, string body)
        {
            if (frontMatter == null || frontMatter.Count == 0)
            {
                return body;
            }

            var serializer = new SerializerBuilder()
                .WithNamingConvention(NullNamingConvention.Instance)
                .Build();
            var yaml = serializer.Serialize(frontMatter);
            var nl = Environment.NewLine;

            return "---" + nl + yaml + "---" + nl + body;
        }

        public List<PageLeaf.Models.TocItem> ExtractHeaders(string markdown)
        {
            var list = new List<PageLeaf.Models.TocItem>();
            if (string.IsNullOrEmpty(markdown)) return list;

            var pipeline = new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UseYamlFrontMatter()
                .UseCodeBlockHeader() // ID生成の一貫性を保つため
                .Build();

            var document = Markdown.Parse(markdown, pipeline);

            foreach (var block in document)
            {
                if (block is Markdig.Syntax.HeadingBlock headingBlock)
                {
                    if (headingBlock.Level <= 3)
                    {
                        var text = GetInlineText(headingBlock.Inline);
                        var id = headingBlock.GetAttributes().Id;

                        // AdvancedExtensions には AutoIdentifiers が含まれているため、
                        // 基本的には ID が自動生成されるはずです。

                        list.Add(new PageLeaf.Models.TocItem
                        {
                            Level = headingBlock.Level,
                            Text = text,
                            Id = id ?? string.Empty,
                            LineNumber = headingBlock.Line
                        });
                    }
                }
            }
            return list;
        }

        /// <summary>インライン要素からテキストのみを抽出します。</summary>
        private string GetInlineText(Markdig.Syntax.Inlines.ContainerInline? inline)
        {
            if (inline == null) return string.Empty;
            var sb = new StringBuilder();
            foreach (var child in inline)
            {
                if (child is Markdig.Syntax.Inlines.LiteralInline literal)
                {
                    sb.Append(literal.Content);
                }
                else if (child is Markdig.Syntax.Inlines.CodeInline code)
                {
                    sb.Append(code.Content);
                }
                else if (child is Markdig.Syntax.Inlines.ContainerInline container)
                {
                    sb.Append(GetInlineText(container));
                }
                // 必要に応じて他のインライン型（強調、リンク等）の処理を追加
            }
            return sb.ToString();
        }
    }
}
