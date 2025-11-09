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

        /// <summary>
        /// 指定されたファイルパスのファイルが存在するかどうかを判断します。
        /// </summary>
        /// <param name="filePath">ファイルパス。</param>
        /// <returns>ファイルが存在する場合は true、それ以外の場合は false。</returns>
        public bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        /// <summary>
        /// 指定されたフォルダパスから、指定された検索パターンに一致するファイルのフルパスのリストを取得します。
        /// </summary>
        /// <param name="folderPath">検索対象のフォルダパス。</param>
        /// <param name="searchPattern">検索するファイルパターン（例: "*.css", "*.txt"）。</param>
        /// <returns>一致するファイルのフルパスのリスト。フォルダが存在しない場合やファイルが見つからない場合は空のリストを返します。</returns>
        public IEnumerable<string> GetFiles(string folderPath, string searchPattern)
        {
            if (string.IsNullOrEmpty(folderPath))
            {
                _logger.LogWarning("GetFiles called with null or empty folderPath.");
                return Enumerable.Empty<string>();
            }

            if (!Directory.Exists(folderPath))
            {
                _logger.LogWarning("Folder not found for GetFiles: {FolderPath}", folderPath);
                return Enumerable.Empty<string>();
            }

            try
            {
                return Directory.GetFiles(folderPath, searchPattern, SearchOption.TopDirectoryOnly);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting files from {FolderPath} with pattern {SearchPattern}.", folderPath, searchPattern);
                return Enumerable.Empty<string>();
            }
        }

        public string ReadAllText(string filePath)
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
                return File.ReadAllText(filePath, encoding);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading file content from {FilePath}.", filePath);
                throw; // or return string.Empty; or handle appropriately
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

        public void WriteAllText(string filePath, string content)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
            }

            try
            {
                File.WriteAllText(filePath, content, Encoding.UTF8);
                _logger.LogInformation("Successfully wrote text to file {FilePath}.", filePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error writing text to file {FilePath}.", filePath);
                throw;
            }
        }
    }
}
