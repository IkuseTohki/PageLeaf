using Microsoft.Extensions.Logging;
using PageLeaf.Models;
using System;
using System.Collections.Generic;

namespace PageLeaf.Services
{
    public class CssManagementService : ICssManagementService
    {
        private readonly ICssService _cssService;
        private readonly ICssEditorService _cssEditorService;
        private readonly IFileService _fileService;
        private readonly ILogger<CssManagementService> _logger;

        public CssManagementService(ICssService cssService, ICssEditorService cssEditorService, IFileService fileService, ILogger<CssManagementService> logger)
        {
            ArgumentNullException.ThrowIfNull(cssService);
            ArgumentNullException.ThrowIfNull(cssEditorService);
            ArgumentNullException.ThrowIfNull(fileService);
            ArgumentNullException.ThrowIfNull(logger);

            _cssService = cssService;
            _cssEditorService = cssEditorService;
            _fileService = fileService;
            _logger = logger;
        }

        public IEnumerable<string> GetAvailableCssFileNames()
        {
            return _cssService.GetAvailableCssFileNames();
        }

        public string GetCssPath(string cssFileName)
        {
            return _cssService.GetCssPath(cssFileName);
        }

        public CssStyleInfo LoadStyle(string cssFileName)
        {
            try
            {
                var path = _cssService.GetCssPath(cssFileName);
                if (!_fileService.FileExists(path))
                {
                    _logger.LogWarning("CSS file not found: {FileName} at {Path}", cssFileName, path);
                    return new CssStyleInfo(); // Return empty/default styles
                }

                var content = _fileService.ReadAllText(path);
                return _cssEditorService.ParseCss(content);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load style for {FileName}", cssFileName);
                throw;
            }
        }

        public void SaveStyle(string cssFileName, CssStyleInfo styleInfo)
        {
            try
            {
                var path = _cssService.GetCssPath(cssFileName);
                // Read existing content to preserve other styles
                var existingContent = _fileService.FileExists(path) ? _fileService.ReadAllText(path) : string.Empty;

                var newContent = _cssEditorService.UpdateCssContent(existingContent, styleInfo);

                _fileService.WriteAllText(path, newContent);
                _logger.LogInformation("Successfully saved style for {FileName}", cssFileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to save style for {FileName}", cssFileName);
                throw;
            }
        }
    }
}
