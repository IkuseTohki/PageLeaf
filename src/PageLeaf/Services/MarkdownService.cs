using Markdig;
using System.Text;
using System;

namespace PageLeaf.Services
{
    public class MarkdownService : IMarkdownService
    {
        public string ConvertToHtml(string markdown, string? cssPath)
        {
            var htmlBody = Markdown.ToHtml(markdown ?? string.Empty);

            var headBuilder = new StringBuilder();
            headBuilder.AppendLine("<meta charset=\"UTF-8\">");
            if (!string.IsNullOrEmpty(cssPath))
            {
                var timestamp = DateTime.Now.Ticks; // タイムスタンプを取得
                headBuilder.AppendLine($"<link rel=\"stylesheet\" href=\"{cssPath}?v={timestamp}\">");
            }

            var htmlBuilder = new StringBuilder();
            htmlBuilder.AppendLine("<!DOCTYPE html>");
            htmlBuilder.AppendLine("<html>");
            htmlBuilder.AppendLine("<head>");
            htmlBuilder.Append(headBuilder.ToString());
            htmlBuilder.AppendLine("</head>");
            htmlBuilder.AppendLine("<body>");
            htmlBuilder.Append(htmlBody); // Changed to Append
            htmlBuilder.AppendLine("</body>");
            htmlBuilder.AppendLine("</html>");

            return htmlBuilder.ToString();
        }
    }
}