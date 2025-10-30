using PageLeaf.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PageLeaf.Services
{
    public class FileService : IFileService
    {
        /// <summary>
        /// 指定されたフォルダパスのファイルとサブフォルダの構造を再帰的に読み込み、ツリーノードのコレクションとして返します。
        /// </summary>
        /// <param name="folderPath">構造を読み込む対象のフォルダパス。</param>
        /// <returns>フォルダ構造を表すFileTreeNodeのIEnumerable。</returns>
        public IEnumerable<FileTreeNode> OpenFolder(string folderPath)
        {
            if (folderPath is null)
            {
                throw new ArgumentNullException(nameof(folderPath));
            }
            if (string.IsNullOrWhiteSpace(folderPath))
            {
                throw new ArgumentException("Folder path cannot be empty or whitespace.", nameof(folderPath));
            }

            if (!Directory.Exists(folderPath))
            {
                // フォルダが存在しない場合は例外をスローします。
                throw new DirectoryNotFoundException($"Directory not found: {folderPath}");
            }

            var nodes = new List<FileTreeNode>();
            var directoryInfo = new DirectoryInfo(folderPath);

            // サブフォルダを処理します。
            foreach (var directory in directoryInfo.GetDirectories())
            {
                nodes.Add(new FileTreeNode
                {
                    Name = directory.Name,
                    FilePath = directory.FullName,
                    IsDirectory = true,
                    // 再帰的に子要素を取得します。
                    Children = OpenFolder(directory.FullName)
                });
            }

            // ファイルを処理します。
            foreach (var file in directoryInfo.GetFiles())
            {
                nodes.Add(new FileTreeNode
                {
                    Name = file.Name,
                    FilePath = file.FullName,
                    IsDirectory = false,
                    // ファイルに子要素はありません。
                    Children = Enumerable.Empty<FileTreeNode>()
                });
            }

            return nodes;
        }

        public MarkdownDocument Open(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
            }

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            try
            {
                string content = File.ReadAllText(filePath);
                return new MarkdownDocument
                {
                    FilePath = filePath,
                    Content = content
                };
            }
            catch (IOException ex)
            {
                throw new IOException($"Error reading file: {filePath}", ex);
            }
            catch (UnauthorizedAccessException ex)
            {
                throw new UnauthorizedAccessException($"Access to file '{filePath}' is denied.", ex);
            }
            catch (Exception ex)
            {
                throw new Exception($"An unexpected error occurred while opening file: {filePath}", ex);
            }
        }

        public void Save(MarkdownDocument document)
        {
            // TODO: 未実装です。今後のタスクで実装します。
            throw new NotImplementedException();
        }
    }
}
