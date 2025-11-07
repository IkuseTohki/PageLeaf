using PageLeaf.Models;

namespace PageLeaf.Services
{
    public interface ICssEditorService
    {
        CssStyleInfo ParseCss(string cssContent);
    }
}
