
using PageLeaf.Models;

namespace PageLeaf.Services
{
    public enum ExportFormat
    {
        Html,
        Pdf,
        Word,
        Png
    }

    public interface IExportService
    {
        void Export(MarkdownDocument document, ExportFormat format);
    }
}
