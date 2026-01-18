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
using PageLeaf.Models;

namespace PageLeaf.Services
{
    public class MarkdownService : IMarkdownService
    {
        private const string DefaultTheme = "github.css";
        // jsDelivrを使用してメジャーバージョン11の最新を取得
        private const string HighlightCdnBase = "https://cdn.jsdelivr.net/gh/highlightjs/cdn-release@11/build/";
        // npmの最新版（latest）を指すように修正
        private const string MermaidCdnUrl = "https://cdn.jsdelivr.net/npm/mermaid/dist/mermaid.min.js";

        private readonly ISettingsService _settingsService;

        public MarkdownService(ISettingsService settingsService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        }

        private struct EffectiveSettings
        {
            public string ThemeName { get; set; }
            public ResourceSource ResourceSource { get; set; }
        }

        public string ConvertToHtml(string markdown, string? cssPath, string? baseDirectory = null)
        {
            var frontMatter = ParseFrontMatter(markdown ?? string.Empty);
            var settings = GetEffectiveSettings(frontMatter);
            var appSettings = _settingsService.CurrentSettings;

            var pipeline = CreatePipeline();
            var htmlBody = Markdown.ToHtml(markdown ?? string.Empty, pipeline);

            var htmlBuilder = new StringBuilder();
            htmlBuilder.AppendLine("<!DOCTYPE html>");
            htmlBuilder.AppendLine("<html>");
            htmlBuilder.AppendLine("<head>");
            htmlBuilder.Append(BuildHead(cssPath, baseDirectory, settings));
            htmlBuilder.AppendLine("</head>");
            htmlBuilder.AppendLine("<body>");

            // タイトルの挿入
            if (appSettings.ShowTitleInPreview && frontMatter.TryGetValue("title", out var titleObj) && titleObj is string title && !string.IsNullOrWhiteSpace(title))
            {
                htmlBuilder.AppendLine($"<header id=\"page-title\" class=\"title-element\">{System.Net.WebUtility.HtmlEncode(title)}</header>");
            }

            htmlBuilder.Append(htmlBody);
            htmlBuilder.Append(BuildScripts(settings));
            htmlBuilder.AppendLine("</body>");
            htmlBuilder.AppendLine("</html>");

            return htmlBuilder.ToString();
        }

        private MarkdownPipeline CreatePipeline()
        {
            return new MarkdownPipelineBuilder()
                .UseAdvancedExtensions()
                .UseYamlFrontMatter()
                .UseCodeBlockHeader()
                .Build();
        }

        private EffectiveSettings GetEffectiveSettings(Dictionary<string, object> frontMatter)
        {
            var appSettings = _settingsService.CurrentSettings;
            var settings = new EffectiveSettings
            {
                ThemeName = !string.IsNullOrEmpty(appSettings.CodeBlockTheme) ? appSettings.CodeBlockTheme : DefaultTheme,
                ResourceSource = appSettings.LibraryResourceSource
            };

            // フロントマターによる上書き (テーマ)
            if (frontMatter.TryGetValue("syntax_highlight", out var fmThemeObj) && fmThemeObj is string fmTheme && !string.IsNullOrWhiteSpace(fmTheme))
            {
                var candidateTheme = fmTheme.EndsWith(".css", StringComparison.OrdinalIgnoreCase) ? fmTheme : fmTheme + ".css";
                if (File.Exists(Path.Combine(App.BaseDirectory, "highlight", "styles", candidateTheme)))
                {
                    settings.ThemeName = candidateTheme;
                }
            }

            return settings;
        }

        private string BuildHead(string? cssPath, string? baseDirectory, EffectiveSettings settings)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<meta charset=\"UTF-8\">");

            // ベースディレクトリの設定（画像等の相対パス解決用）
            if (!string.IsNullOrEmpty(baseDirectory))
            {
                try
                {
                    // ディレクトリパスをURIに変換し、末尾にスラッシュを保証
                    var baseUri = new Uri(baseDirectory).AbsoluteUri;
                    if (!baseUri.EndsWith("/")) baseUri += "/";
                    sb.AppendLine($"<base href=\"{baseUri}\" />");
                }
                catch (Exception)
                {
                    // URI変換エラー時はbaseタグを追加しない
                }
            }

            // 拡張機能用のベーススタイルを追加
            var extensionsCssUri = new Uri(Path.Combine(App.BaseDirectory, "css", "extensions.css")).AbsoluteUri;
            sb.AppendLine($"<link rel=\"stylesheet\" href=\"{extensionsCssUri}\">");

            if (!string.IsNullOrEmpty(cssPath))
            {
                sb.AppendLine($"<link rel=\"stylesheet\" href=\"{cssPath}?v={DateTime.Now.Ticks}\">");
            }

            sb.AppendLine("<style id=\"dynamic-style\"></style>");

            string themeUri = settings.ResourceSource == ResourceSource.Cdn
                ? $"{HighlightCdnBase}styles/{settings.ThemeName}"
                : new Uri(Path.Combine(App.BaseDirectory, "highlight", "styles", settings.ThemeName)).AbsoluteUri;

            sb.AppendLine($"<link rel=\"stylesheet\" href=\"{themeUri}\">");

            return sb.ToString();
        }

        private string BuildScripts(EffectiveSettings settings)
        {
            string highlightUri;
            string mermaidUri;
            string extensionUri = new Uri(Path.Combine(App.BaseDirectory, "highlight", "pageleaf-extensions.js")).AbsoluteUri;

            if (settings.ResourceSource == ResourceSource.Cdn)
            {
                highlightUri = $"{HighlightCdnBase}highlight.min.js";
                mermaidUri = MermaidCdnUrl;
            }
            else
            {
                highlightUri = new Uri(Path.Combine(App.BaseDirectory, "highlight", "highlight.min.js")).AbsoluteUri;
                mermaidUri = new Uri(Path.Combine(App.BaseDirectory, "mermaid", "mermaid.min.js")).AbsoluteUri;
            }

            var sb = new StringBuilder();
            sb.AppendLine($"<script src=\"{highlightUri}\"></script>");
            sb.AppendLine($"<script src=\"{extensionUri}\"></script>");
            sb.AppendLine($"<script src=\"{mermaidUri}\"></script>");
            sb.AppendLine("<script>hljs.highlightAll();</script>");
            sb.AppendLine("<script>");
            sb.AppendLine("  mermaid.initialize({ startOnLoad: true, theme: 'default' });");
            sb.AppendLine("  mermaid.contentLoaded();");
            sb.AppendLine("</script>");

            return sb.ToString();
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
