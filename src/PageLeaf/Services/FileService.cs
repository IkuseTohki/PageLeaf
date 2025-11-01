using Microsoft.Extensions.Logging;
using PageLeaf.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Ude;

namespace PageLeaf.Services
{
    public class FileService : IFileService
    {
        private readonly ILogger<FileService> _logger;

        public FileService(ILogger<FileService> logger)
        {
            _logger = logger;
        }

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
                var encoding = DetectEncoding(filePath);
                string content = File.ReadAllText(filePath, encoding);
                return new MarkdownDocument
                {
                    FilePath = filePath,
                    Content = content,
                    Encoding = encoding // 検出したエンコーディングを保存
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
            if (document == null)
            {
                _logger.LogError("Save method called with a null document.");
                throw new ArgumentNullException(nameof(document));
            }

            if (string.IsNullOrWhiteSpace(document.FilePath))
            {
                _logger.LogError("Save method called with a document that has a null, empty, or whitespace FilePath.");
                throw new ArgumentException("File path cannot be null, empty, or whitespace.", nameof(document.FilePath));
            }

            _logger.LogInformation("Attempting to save file to {FilePath}.", document.FilePath);

            try
            {
                // ドキュメントに保存されているエンコーディングを使用。なければUTF8をデフォルトとする。
                var encodingToUse = document.Encoding ?? Encoding.UTF8;
                File.WriteAllText(document.FilePath, document.Content ?? string.Empty, encodingToUse);
                _logger.LogInformation("Successfully saved file to {FilePath} with encoding {EncodingName}.", document.FilePath, encodingToUse.WebName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving file to {FilePath}.", document.FilePath);
                throw new IOException($"Error saving file: {document.FilePath}", ex);
            }
        }

        private Encoding DetectEncoding(string filePath)
        {
            // .NET Core では、CodePagesEncodingProvider の登録が必要
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                var charsetDetector = new CharsetDetector();
                charsetDetector.Feed(fileStream);
                charsetDetector.DataEnd();

                if (charsetDetector.Charset != null)
                {
                    _logger.LogInformation("Detected charset: {Charset}, Confidence: {Confidence}", charsetDetector.Charset, charsetDetector.Confidence);
                    try
                    {
                        return Encoding.GetEncoding(charsetDetector.Charset);
                    }
                    catch (ArgumentException)
                    {
                        _logger.LogWarning("Could not get encoding for detected charset '{Charset}'. Falling back to UTF-8.", charsetDetector.Charset);
                        // 不明なエンコーディングの場合はデフォルト（UTF-8）にフォールバック
                        return Encoding.UTF8;
                    }
                }
                else
                {
                    _logger.LogInformation("Charset detection failed. Falling back to UTF-8.");
                    // 判別できなかった場合もデフォルト（UTF-8）にフォールバック
                    return Encoding.UTF8;
                }
            }
        }
    }
}
