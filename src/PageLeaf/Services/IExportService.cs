
using PageLeaf.Models;

namespace PageLeaf.Services
{
    public interface IExportService
    {
        void Export(MarkdownDocument document, ExportFormat format);
    }
}
