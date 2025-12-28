using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PageLeaf.Services
{
    /// <summary>
    /// CSSファイル関連の操作を提供するサービスです。
    /// </summary>
    public class CssService : ICssService
    {
        private readonly IFileService _fileService;
        private readonly ILogger<CssService> _logger;
        private readonly string _cssDirectoryPath;

        /// <summary>
        /// CssService クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="fileService">ファイル操作サービス。</param>
        /// <param name="logger">ロガー。</param>
        public CssService(IFileService fileService, ILogger<CssService> logger)
        {
            _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _cssDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "css");
            _logger.LogInformation("CSS directory path initialized to: {CssDirectoryPath}", _cssDirectoryPath);
        }

        /// <summary>
        /// 利用可能なCSSファイルのファイル名リストを取得します。
        /// </summary>
        /// <returns>利用可能なCSSファイルのファイル名（拡張子含む）のリスト。</returns>
        public IEnumerable<string> GetAvailableCssFileNames()
        {
            try
            {
                _logger.LogInformation("Attempting to get available CSS files from: {CssDirectoryPath}", _cssDirectoryPath);
                var fullPaths = _fileService.GetFiles(_cssDirectoryPath, "*.css");
                var fileNames = fullPaths.Select(Path.GetFileName).OfType<string>().ToList();
                _logger.LogInformation("Found {Count} CSS files.", fileNames.Count);
                return fileNames;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available CSS file names from {CssDirectoryPath}.", _cssDirectoryPath);
                return Enumerable.Empty<string>();
            }
        }

        /// <summary>
        /// 指定されたCSSファイル名の完全な絶対パスを取得します。
        /// </summary>
        /// <param name="cssFileName">パスを取得するCSSファイル名。</param>
        /// <returns>CSSファイルの絶対パス。ファイル名が空またはnullの場合は空文字列を返します。</returns>
        public string GetCssPath(string cssFileName)
        {
            if (string.IsNullOrEmpty(cssFileName))
            {
                _logger.LogWarning("GetCssPath called with empty file name.");
                return string.Empty;
            }

            return Path.Combine(_cssDirectoryPath, cssFileName);
        }
    }
}
