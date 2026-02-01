
using PageLeaf.Models;
using PageLeaf.Models.Markdown;
using PageLeaf.Models.Css;
using PageLeaf.Models.Css.Elements;
using PageLeaf.Models.Settings;

namespace PageLeaf.Services
{
    public interface IExportService
    {
        void Export(MarkdownDocument document, ExportFormat format);
    }
}
