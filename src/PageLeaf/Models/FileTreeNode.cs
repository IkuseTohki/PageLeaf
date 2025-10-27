
using System.Collections.Generic;
using System.Linq;

namespace PageLeaf.Models
{
    public class FileTreeNode
    {
        public string Name { get; set; } = "";
        public string FilePath { get; set; } = "";
        public bool IsDirectory { get; set; }
        public IEnumerable<FileTreeNode> Children { get; set; } = Enumerable.Empty<FileTreeNode>();
    }
}
