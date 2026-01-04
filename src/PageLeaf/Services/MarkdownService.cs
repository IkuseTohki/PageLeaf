using Markdig;
using System.Text;
using System;
using System.IO;
using System.Reflection;

namespace PageLeaf.Services
{
    public class MarkdownService : IMarkdownService
    {
        private readonly ISettingsService _settingsService;

        public MarkdownService(ISettingsService settingsService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        }

        public string ConvertToHtml(string markdown, string? cssPath)
        {
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            var htmlBody = Markdown.ToHtml(markdown ?? string.Empty, pipeline);

            var headBuilder = new StringBuilder();
            headBuilder.AppendLine("<meta charset=\"UTF-8\">");
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
            // file:// スキームのURIに変換
            var scriptFileUri = new Uri(scriptFilePath).AbsoluteUri;

            htmlBuilder.AppendLine($@"<script src=""{scriptFileUri}""></script>");
            htmlBuilder.AppendLine("<script>hljs.highlightAll();</script>");

            htmlBuilder.AppendLine("</body>");
            htmlBuilder.AppendLine("</html>");

            return htmlBuilder.ToString();
        }
    }
}
