using Markdig;
using System.Text;
using System;
using System.IO;
using System.Reflection;
using PageLeaf.Utilities.MarkdownExtensions;

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
                    headBuilder.AppendLine($@"<base href=""{baseUri}"" />");
                }
                catch (Exception)
                {
                    // URI変換エラー時はbaseタグを追加しない
                }
            }

            // 拡張機能用のベーススタイルを追加
            var extensionsCssPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "css", "extensions.css");
            var extensionsCssUri = new Uri(extensionsCssPath).AbsoluteUri;
            headBuilder.AppendLine($@"<link rel=""stylesheet"" href=""{extensionsCssUri}"">");

            if (!string.IsNullOrEmpty(cssPath))
            {
                var timestamp = DateTime.Now.Ticks; // タイムスタンプを取得
                headBuilder.AppendLine($"<link rel=\"stylesheet\" href=\"{cssPath}?v={timestamp}\">");
            }

            headBuilder.AppendLine("<style id=\"dynamic-style\"></style>");

            // highlight.jsのCSSへのリンクを追加 (設定から取得)
            var themeName = _settingsService.CurrentSettings.CodeBlockTheme;
            if (string.IsNullOrEmpty(themeName)) themeName = "github.css";

            var cssFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "highlight", "styles", themeName);
            var cssFileUri = new Uri(cssFilePath).AbsoluteUri;
            headBuilder.AppendLine($@"<link rel=""stylesheet"" href=""{cssFileUri}"">");

            var htmlBuilder = new StringBuilder();
            htmlBuilder.AppendLine("<!DOCTYPE html>");
            htmlBuilder.AppendLine("<html>");
            htmlBuilder.AppendLine("<head>");
            htmlBuilder.Append(headBuilder);
            htmlBuilder.AppendLine("</head>");
            htmlBuilder.AppendLine("<body>");
            htmlBuilder.Append(htmlBody);

            // highlight.jsライブラリへのリンクを絶対パスで指定
            var scriptFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "highlight", "highlight.min.js");
            var extensionScriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "highlight", "pageleaf-extensions.js");
            var mermaidScriptPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "mermaid", "mermaid.min.js");

            // file:// スキームのURIに変換
            var scriptFileUri = new Uri(scriptFilePath).AbsoluteUri;
            var extensionScriptUri = new Uri(extensionScriptPath).AbsoluteUri;
            var mermaidScriptUri = new Uri(mermaidScriptPath).AbsoluteUri;

            htmlBuilder.AppendLine($@"<script src=""{scriptFileUri}""></script>");
            htmlBuilder.AppendLine($@"<script src=""{extensionScriptUri}""></script>");
            htmlBuilder.AppendLine($@"<script src=""{mermaidScriptUri}""></script>");
            htmlBuilder.AppendLine("<script>hljs.highlightAll();</script>");
            htmlBuilder.AppendLine("<script>");
            htmlBuilder.AppendLine("  mermaid.initialize({ startOnLoad: true, theme: 'default' });");
            htmlBuilder.AppendLine("  mermaid.contentLoaded();");
            htmlBuilder.AppendLine("</script>");

            htmlBuilder.AppendLine("</body>");
            htmlBuilder.AppendLine("</html>");

            return htmlBuilder.ToString();
        }
    }
}
