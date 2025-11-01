using Markdig;

namespace PageLeaf.Services
{
    public class MarkdownService : IMarkdownService
    {
        private const string HtmlTemplate = @"<!DOCTYPE html>
<html>
<head>
<meta charset=""UTF-8"">
</head>
<body>
{0}</body>
</html>
";

        public string ConvertToHtml(string markdown)
        {
            var htmlBody = Markdown.ToHtml(markdown ?? string.Empty);

            // 完全なHTMLドキュメントとして組み立てる
            return string.Format(HtmlTemplate, htmlBody);
        }
    }
}