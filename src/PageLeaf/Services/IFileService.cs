
using PageLeaf.Models;
using System.Collections.Generic;

namespace PageLeaf.Services
{
    public interface IFileService
    {
        MarkdownDocument Open(string filePath);
        void Save(MarkdownDocument document);
        IEnumerable<FileTreeNode> OpenFolder(string folderPath);
        bool FileExists(string filePath);
    }
}
