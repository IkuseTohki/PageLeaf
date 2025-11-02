
namespace PageLeaf.Services
{
    public interface IMarkdownService
    {
        string ConvertToHtml(string markdown, string? cssPath);
    }
}
