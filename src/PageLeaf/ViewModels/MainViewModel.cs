using Microsoft.Extensions.Logging;
using PageLeaf.Models;
using PageLeaf.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using System.Windows; // Added

namespace PageLeaf.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IFileService _fileService;
        private readonly ILogger<MainViewModel> _logger;
        private readonly IDialogService _dialogService;
        private readonly ICssService _cssService;
        private readonly ISettingsService _settingsService;
        private readonly ICssEditorService _cssEditorService;

        private bool _isCssEditorVisible;
        private ObservableCollection<string> _availableCssFiles = null!;
        private string _selectedCssFile = null!;
        private bool _isWebView2Initialized;
        private GridLength _lastKnownCssEditorWidth; // Added

        public IEditorService Editor { get; }
        public CssEditorViewModel CssEditorViewModel { get; }
        public ObservableCollection<DisplayMode> AvailableModes { get; }

        public bool IsCssEditorVisible
        {
            get => _isCssEditorVisible;
            set
            {
                if (_isCssEditorVisible != value)
                {
                    _isCssEditorVisible = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CssEditorColumnWidth)); // Notify CssEditorColumnWidth change
                }
            }
        }

        public ObservableCollection<string> AvailableCssFiles
        {
            get => _availableCssFiles;
            set
            {
                _availableCssFiles = value;
                OnPropertyChanged();
            }
        }

        public string SelectedCssFile
        {
            get => _selectedCssFile;
            set
            {
                if (_selectedCssFile != value)
                {
                    _selectedCssFile = value;
                    OnPropertyChanged();
                    // 選択されたCSSファイルが変更された場合、設定を保存し、エディタビューに新しいCSSを適用する
                    _settingsService.CurrentSettings.SelectedCss = value;
                    _settingsService.SaveSettings(_settingsService.CurrentSettings);
                    Editor.ApplyCss(value);

                    try
                    {
                        var cssPath = _cssService.GetCssPath(value);
                        if (!string.IsNullOrEmpty(cssPath) && _fileService.FileExists(cssPath))
                        {
                            // CSSエディタのターゲットパスを更新
                            CssEditorViewModel.TargetCssPath = cssPath;

                            var cssContent = _fileService.ReadAllText(cssPath);
                            var parsedStyles = _cssEditorService.ParseCss(cssContent);

                            // CssEditorViewModelのLoadStylesメソッドを呼び出してスタイルを反映
                            CssEditorViewModel.LoadStyles(parsedStyles);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to read and parse CSS file: {CssFile}", value);
                    }
                }
            }
        }

        public bool IsWebView2Initialized
        {
            get => _isWebView2Initialized;
            private set
            {
                if (_isWebView2Initialized != value)
                {
                    _isWebView2Initialized = value;
                    OnPropertyChanged();
                }
            }
        }

        public void SetWebView2Initialized()
        {
            IsWebView2Initialized = true;
        }

        public GridLength CssEditorColumnWidth
        {
            get => IsCssEditorVisible ? _lastKnownCssEditorWidth : new GridLength(0);
            set
            {
                if (_lastKnownCssEditorWidth != value)
                {
                    _lastKnownCssEditorWidth = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand OpenFileCommand { get; }
        public ICommand SaveFileCommand { get; }
        public ICommand SaveAsFileCommand { get; }
        public ICommand NewDocumentCommand { get; }
        public ICommand ToggleCssEditorCommand { get; }


        public MainViewModel(IFileService fileService, ILogger<MainViewModel> logger, IDialogService dialogService, IEditorService editorService, ICssService cssService, ISettingsService settingsService, ICssEditorService cssEditorService)
        {
            ArgumentNullException.ThrowIfNull(fileService);
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(dialogService);
            ArgumentNullException.ThrowIfNull(editorService);
            ArgumentNullException.ThrowIfNull(cssService);
            ArgumentNullException.ThrowIfNull(settingsService);
            ArgumentNullException.ThrowIfNull(cssEditorService);

            _fileService = fileService;
            _logger = logger;
            _dialogService = dialogService;
            Editor = editorService;
            _cssService = cssService;
            _settingsService = settingsService;
            _cssEditorService = cssEditorService;

            CssEditorViewModel = new CssEditorViewModel(_fileService, _cssEditorService);
            CssEditorViewModel.CssSaved += OnCssSaved;

            OpenFileCommand = new Utilities.DelegateCommand(ExecuteOpenFile);
            SaveFileCommand = new Utilities.DelegateCommand(ExecuteSaveFile);
            SaveAsFileCommand = new Utilities.DelegateCommand(ExecuteSaveAsFile);
            NewDocumentCommand = new Utilities.DelegateCommand(ExecuteNewDocument);
            ToggleCssEditorCommand = new Utilities.DelegateCommand(ExecuteToggleCssEditor);

            AvailableModes = new ObservableCollection<DisplayMode>(
                Enum.GetValues(typeof(DisplayMode)).Cast<DisplayMode>()
            );

            AvailableCssFiles = new ObservableCollection<string>(_cssService.GetAvailableCssFileNames());

            // 設定から選択されたCSSを読み込む
            var loadedCss = _settingsService.CurrentSettings.SelectedCss;
            if (!string.IsNullOrEmpty(loadedCss) && AvailableCssFiles.Contains(loadedCss))
            {
                SelectedCssFile = loadedCss;
            }
            else
            {
                SelectedCssFile = AvailableCssFiles.FirstOrDefault() ?? "github.css";
            }

            Editor.ApplyCss(SelectedCssFile);

            _lastKnownCssEditorWidth = new GridLength(300); // Initialize default width
        }

        private void ExecuteNewDocument(object? parameter)
        {
            _logger.LogInformation("NewDocumentCommand executed.");

            SaveConfirmationResult result = Editor.PromptForSaveIfDirty();

            if (result == SaveConfirmationResult.Cancel)
            {
                return;
            }
            if (result == SaveConfirmationResult.Save)
            {
                ExecuteSaveFile(null);
            }

            Editor.NewDocument();
        }

        private void ExecuteOpenFile(object? parameter)
        {
            _logger.LogInformation("OpenFileCommand executed.");

            SaveConfirmationResult result = Editor.PromptForSaveIfDirty();

            if (result == SaveConfirmationResult.Cancel)
            {
                return;
            }
            if (result == SaveConfirmationResult.Save)
            {
                ExecuteSaveFile(null);
            }

            string? filePath = _dialogService.ShowOpenFileDialog(
                "Markdownファイルを開く",
                "Markdown files (*.md;*.markdown)|*.md;*.markdown|All files (*.*)|*.*");

            if (!string.IsNullOrEmpty(filePath))
            {
                try
                {
                    MarkdownDocument document = _fileService.Open(filePath);
                    Editor.LoadDocument(document);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to open file: {FilePath}", filePath);
                }
            }
        }

        private void ExecuteSaveFile(object? parameter)
        {
            _logger.LogInformation("ExecuteSaveFile command triggered.");

            if (Editor.CurrentDocument == null)
            {
                _logger.LogWarning("CurrentDocument is null. Save command cannot execute.");
                return;
            }

            // ファイルパスが設定されていない、またはファイルが存在しない場合は「名前を付けて保存」に切り替える
            if (string.IsNullOrEmpty(Editor.CurrentDocument.FilePath) || !_fileService.FileExists(Editor.CurrentDocument.FilePath))
            {
                _logger.LogInformation("File does not exist or has no path. Switching to Save As...");
                ExecuteSaveAsFile(parameter);
                return;
            }

            try
            {
                _fileService.Save(Editor.CurrentDocument);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while saving the file for {FilePath}.", Editor.CurrentDocument.FilePath);
            }
        }

        private void ExecuteSaveAsFile(object? parameter)
        {
            _logger.LogInformation("ExecuteSaveAsFile command triggered.");

            if (Editor.CurrentDocument == null)
            {
                _logger.LogWarning("CurrentDocument is null. Save As command cannot execute.");
                return;
            }

            string? newFilePath = _dialogService.ShowSaveFileDialog(
                "名前を付けて保存",
                "Markdown files (*.md;*.markdown)|*.md;*.markdown|All files (*.*)|*.*",
                Editor.CurrentDocument.FilePath
            );

            if (!string.IsNullOrEmpty(newFilePath))
            {
                try
                {
                    Editor.CurrentDocument.FilePath = newFilePath;
                    _fileService.Save(Editor.CurrentDocument);
                    _logger.LogInformation("File saved as: {FilePath}", newFilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while saving the file as {FilePath}.", newFilePath);
                }
            }
        }

        private void ExecuteToggleCssEditor(object? obj)
        {
            IsCssEditorVisible = !IsCssEditorVisible;
        }

        private void OnCssSaved(object? sender, EventArgs e)
        {
            Editor.ApplyCss(SelectedCssFile);
        }
    }
}
