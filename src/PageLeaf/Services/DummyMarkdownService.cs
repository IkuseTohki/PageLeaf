using System;

namespace PageLeaf.Services
{
    public class DummyMarkdownService : IMarkdownService
    {
        public string ConvertToHtml(string markdown)
        {
            // ダミー実装: 受け取ったMarkdownをそのままHTMLとして返す
            return $"<p>Dummy HTML for: {markdown}</p>";
        }
    }
}