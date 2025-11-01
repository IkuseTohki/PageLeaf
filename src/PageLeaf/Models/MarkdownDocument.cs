
using System.Text;

namespace PageLeaf.Models
{
    public class MarkdownDocument
    {
        public string Content { get; set; } = "";
        public string FilePath { get; set; } = "";
        public Encoding? Encoding { get; set; } // 追加
    }
}
