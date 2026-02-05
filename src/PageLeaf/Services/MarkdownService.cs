using Markdig;
using System.Text;
using System;
using System.IO;
using PageLeaf.Utilities.MarkdownExtensions;
using System.Collections.Generic;
using Markdig.Syntax;
using Markdig.Renderers.Html;
using PageLeaf.Models;
using PageLeaf.Models.Markdown;
using PageLeaf.Models.Css;
using PageLeaf.Models.Css.Elements;
using PageLeaf.Models.Settings;

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
            var frontMatter = ParseFrontMatterInternal(markdown ?? string.Empty);
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
            if (appSettings.View.ShowTitleInPreview && frontMatter.TryGetValue("title", out var titleObj) && titleObj is string title && !string.IsNullOrWhiteSpace(title))
            {
                htmlBuilder.AppendLine($"<header id=\"page-title\" class=\"title-element\">{System.Net.WebUtility.HtmlEncode(title)}</header>");
            }

            htmlBuilder.Append(htmlBody);
            htmlBuilder.Append(BuildScripts(settings));
            htmlBuilder.AppendLine("</body>");
            htmlBuilder.AppendLine("</html>");

            return htmlBuilder.ToString();
        }

        /// <summary>
        /// リソース（CSS/JS）の場所を特定し、URI形式で返します。
        /// 1. BaseDirectory（exeフォルダ）にあればそれを優先（デバッグ時用）
        /// 2. なければ AppInternalTempDirectory（一時フォルダ）を参照
        /// </summary>
        private string GetResourceUri(string subPath)
        {
            var localPath = Path.Combine(App.BaseDirectory, subPath);
            if (File.Exists(localPath)) return new Uri(localPath).ToString();

            return new Uri(Path.Combine(App.AppInternalTempDirectory, subPath)).ToString();
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
                ThemeName = !string.IsNullOrEmpty(appSettings.View.CodeBlockTheme) ? appSettings.View.CodeBlockTheme : DefaultTheme,
                ResourceSource = appSettings.Appearance.LibraryResourceSource
            };

            // フロントマターによる上書き (テーマ)
            if (frontMatter.TryGetValue("syntax_highlight", out var fmThemeObj) && fmThemeObj is string fmTheme && !string.IsNullOrWhiteSpace(fmTheme))
            {
                var candidateTheme = fmTheme.EndsWith(".css", StringComparison.OrdinalIgnoreCase) ? fmTheme : fmTheme + ".css";
                var themeSubPath = Path.Combine("highlight", "styles", candidateTheme);

                // 存在チェック（Base または Temp）
                if (File.Exists(Path.Combine(App.BaseDirectory, themeSubPath)) ||
                    File.Exists(Path.Combine(App.AppInternalTempDirectory, themeSubPath)))
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
                    var baseUri = new Uri(baseDirectory).ToString();
                    if (!baseUri.EndsWith("/")) baseUri += "/";
                    sb.AppendLine($"<base href=\"{baseUri}\" />");
                }
                catch (Exception)
                {
                    // URI変換エラー時はbaseタグを追加しない
                }
            }

            // 拡張機能用のベーススタイルを追加
            // HTMLの <base> タグの影響を受けないよう、アプリ内リソースは絶対URIで指定する
            var extensionsCssUri = GetResourceUri("css/extensions.css");
            sb.AppendLine($"<link rel=\"stylesheet\" href=\"{extensionsCssUri}\">");

            if (!string.IsNullOrEmpty(cssPath))
            {
                // ユーザーCSSも絶対URIに変換（エスケープを避けるためToStringを使用）
                var userCssUri = new Uri(cssPath).ToString();
                sb.AppendLine($"<link rel=\"stylesheet\" href=\"{userCssUri}\">");
            }

            sb.AppendLine("<style id=\"dynamic-style\"></style>");

            string themeUri = settings.ResourceSource == ResourceSource.Cdn
                ? $"{HighlightCdnBase}styles/{settings.ThemeName}"
                : GetResourceUri(Path.Combine("highlight", "styles", settings.ThemeName));

            sb.AppendLine($"<link rel=\"stylesheet\" href=\"{themeUri}\">");

            return sb.ToString();
        }

        private string BuildScripts(EffectiveSettings settings)
        {
            string highlightUri;
            string mermaidUri;
            string extensionUri = GetResourceUri("highlight/pageleaf-extensions.js");
            string previewExtensionUri = GetResourceUri("js/preview-extensions.js");

            if (settings.ResourceSource == ResourceSource.Cdn)
            {
                highlightUri = $"{HighlightCdnBase}highlight.min.js";
                mermaidUri = MermaidCdnUrl;
            }
            else
            {
                highlightUri = GetResourceUri("highlight/highlight.min.js");
                mermaidUri = GetResourceUri("mermaid/mermaid.min.js");
            }

            var sb = new StringBuilder();
            sb.AppendLine($"<script src=\"{highlightUri}\"></script>");
            sb.AppendLine($"<script src=\"{extensionUri}\"></script>");
            sb.AppendLine($"<script src=\"{mermaidUri}\"></script>");
            sb.AppendLine($"<script src=\"{previewExtensionUri}\"></script>");
            sb.AppendLine("<script>hljs.highlightAll();</script>");

            return sb.ToString();
        }

        /// <summary>
        /// 内部的なフロントマター解析（HTML変換時のタイトル表示用）。
        /// 実際のドキュメント構築用は MarkdownDocument.Load を使用すること。
        /// </summary>
        private Dictionary<string, object> ParseFrontMatterInternal(string markdown)
        {
            // 簡易的な解析（Markdigの拡張を使わず、Html変換前のタイトル表示のためにのみ使用）
            if (string.IsNullOrEmpty(markdown) || !markdown.TrimStart().StartsWith("---"))
            {
                return new Dictionary<string, object>();
            }

            using (var reader = new StringReader(markdown.TrimStart()))
            {
                var line = reader.ReadLine();
                if (line?.TrimEnd() != "---") return new Dictionary<string, object>();

                var yamlContent = new StringBuilder();
                while ((line = reader.ReadLine()) != null)
                {
                    if (line.TrimEnd() == "---") break;
                    yamlContent.AppendLine(line);
                }

                try
                {
                    var deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
                        .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.NullNamingConvention.Instance)
                        .Build();
                    return deserializer.Deserialize<Dictionary<string, object>>(yamlContent.ToString()) ?? new Dictionary<string, object>();
                }
                catch
                {
                    return new Dictionary<string, object>();
                }
            }
        }

        public List<TocItem> ExtractHeaders(string markdown)
        {
            var list = new List<TocItem>();
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

                        list.Add(new TocItem(
                            headingBlock.Level,
                            text,
                            id ?? string.Empty,
                            headingBlock.Line
                        ));
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
