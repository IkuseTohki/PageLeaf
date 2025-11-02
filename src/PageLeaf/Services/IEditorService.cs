using PageLeaf.Models;
using System.ComponentModel;

namespace PageLeaf.Services
{
    public interface IEditorService : INotifyPropertyChanged
    {
        MarkdownDocument CurrentDocument { get; }
        string EditorText { get; set; }
        string HtmlContent { get; }
        DisplayMode SelectedMode { get; set; }
        bool IsMarkdownEditorVisible { get; }
        bool IsViewerVisible { get; }

        void LoadDocument(MarkdownDocument document);
    }
}