using Microsoft.Extensions.Logging;
using PageLeaf.Models;
using PageLeaf.Services;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace PageLeaf.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IFileService _fileService;
        private readonly ILogger<MainViewModel> _logger;
        private readonly IDialogService _dialogService;
        private readonly IMarkdownService _markdownService; // 追加
        private FileTreeNode? _rootNode;
        private ObservableCollection<DisplayMode> _availableModes = null!;
        private DisplayMode _selectedMode;
        private ObservableCollection<string> _availableCssFiles = null!;
        private string _selectedCssFile = null!;
        private MarkdownDocument _currentDocument = null!;
        private bool _isMarkdownEditorVisible;
        private bool _isViewerVisible;
        private string _htmlContent = string.Empty; // 追加

        public FileTreeNode? RootNode
        {
            get => _rootNode;
            private set
            {
                _rootNode = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<DisplayMode> AvailableModes
        {
            get => _availableModes;
            set
            {
                _availableModes = value;
                OnPropertyChanged();
            }
        }

        public DisplayMode SelectedMode
        {
            get => _selectedMode;
            set
            {
                if (_selectedMode != value)
                {
                    _selectedMode = value;
                    OnPropertyChanged();
                    UpdateVisibility();
                    UpdateHtmlContent(); // モード変更時にもHTMLを更新
                }
            }
        }

        public bool IsMarkdownEditorVisible
        {
            get => _isMarkdownEditorVisible;
            set
            {
                if (_isMarkdownEditorVisible != value)
                {
                    _isMarkdownEditorVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsViewerVisible
        {
            get => _isViewerVisible;
            set
            {
                if (_isViewerVisible != value)
                {
                    _isViewerVisible = value;
                    OnPropertyChanged();
                }
            }
        }

        public string HtmlContent
        {
            get => _htmlContent;
            set
            {
                if (_htmlContent != value)
                {
                    _htmlContent = value;
                    OnPropertyChanged();
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
                _selectedCssFile = value;
                OnPropertyChanged();
            }
        }

        public MarkdownDocument CurrentDocument
        {
            get => _currentDocument;
            set
            {
                _currentDocument = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EditorText)); // CurrentDocumentが変更されたらEditorTextも更新
                UpdateHtmlContent(); // ドキュメント変更時にもHTMLを更新
            }
        }

        public string EditorText
        {
            get => _currentDocument?.Content ?? string.Empty;
            set
            {
                if (_currentDocument != null)
                {
                    _currentDocument.Content = value;
                }
                OnPropertyChanged(); // CurrentDocumentがnullの場合でも変更通知を発生させる
            }
        }

        public ICommand OpenFolderCommand { get; }
        public ICommand OpenFileCommand { get; }
        public ICommand SaveFileCommand { get; }
        public ICommand SaveAsFileCommand { get; } // 追加

        public MainViewModel(IFileService fileService, ILogger<MainViewModel> logger, IDialogService dialogService, IMarkdownService markdownService) // IMarkdownService を追加
        {
            ArgumentNullException.ThrowIfNull(fileService);
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(dialogService);
            ArgumentNullException.ThrowIfNull(markdownService); // 追加

            _fileService = fileService;
            _logger = logger;
            _dialogService = dialogService;
            _markdownService = markdownService; // 追加
            OpenFolderCommand = new Utilities.DelegateCommand(OpenFolder);
            OpenFileCommand = new Utilities.DelegateCommand(ExecuteOpenFile);
            SaveFileCommand = new Utilities.DelegateCommand(ExecuteSaveFile);
            SaveAsFileCommand = new Utilities.DelegateCommand(ExecuteSaveAsFile); // 追加

            AvailableModes = new ObservableCollection<DisplayMode>(Enum.GetValues(typeof(DisplayMode)).Cast<DisplayMode>());
            SelectedMode = AvailableModes.FirstOrDefault();

            AvailableCssFiles = new ObservableCollection<string> { "github.css", "solarized-light.css", "solarized-dark.css" };
            SelectedCssFile = AvailableCssFiles[0];

            CurrentDocument = new MarkdownDocument { Content = "# Hello, PageLeaf!" };

            UpdateVisibility(); // 初期表示を設定
        }

        private void UpdateVisibility()
        {
            IsMarkdownEditorVisible = SelectedMode == DisplayMode.Markdown;
            IsViewerVisible = SelectedMode == DisplayMode.Viewer;
            // RealTimeモードは今回は未実装のため、ここでは考慮しない
        }

        private void UpdateHtmlContent()
        {
            if (SelectedMode == DisplayMode.Viewer && CurrentDocument != null)
            {
                HtmlContent = _markdownService.ConvertToHtml(CurrentDocument.Content);
            }
            else
            {
                HtmlContent = string.Empty; // Viewerモードでない場合はHTMLをクリア
            }
        }

        private void OpenFolder(object? parameter)
        {
            try
            {
                if (parameter is string path)
                {
                    var children = _fileService.OpenFolder(path);
                    RootNode = new FileTreeNode
                    {
                        Name = System.IO.Path.GetFileName(path),
                        FilePath = path,
                        IsDirectory = true,
                        Children = new System.Collections.ObjectModel.ObservableCollection<FileTreeNode>(children)
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to open folder: {Path}", parameter);
            }
        }

        private void ExecuteOpenFile(object? parameter)
        {
            _logger.LogInformation("OpenFileCommand executed.");

            string? filePath = _dialogService.ShowOpenFileDialog(
                "Markdownファイルを開く",
                "Markdown files (*.md;*.markdown)|*.md;*.markdown|All files (*.*)|*.*" );

            _logger.LogInformation("ShowOpenFileDialog returned: {FilePath}", filePath ?? "null"); // Log filePath

            if (!string.IsNullOrEmpty(filePath))
            {
                try
                {
                    _logger.LogInformation("Attempting to open file: {FilePath}", filePath); // Log before opening
                    MarkdownDocument document = _fileService.Open(filePath);
                    _logger.LogInformation("File opened successfully. Document content length: {Length}", document.Content?.Length ?? 0); // Log after opening
                    CurrentDocument = document;
                    _logger.LogInformation("EditorText updated with content from {FilePath}", filePath); // Log after update
                    // TODO: document.FilePath も保持する必要があるが、これはフェーズ3でMarkdownDocumentモデルを導入する際に対応
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to open file: {FilePath}", filePath);
                    // TODO: エラーメッセージをユーザーに表示
                }
            }
            else
            {
                _logger.LogInformation("File open dialog was cancelled or returned empty path.");
            }
        }

        private void ExecuteSaveFile(object? parameter)
        {
            _logger.LogInformation("ExecuteSaveFile command triggered.");

            if (CurrentDocument == null)
            {
                _logger.LogWarning("CurrentDocument is null. Save command cannot execute.");
                return;
            }

            try
            {
                _fileService.Save(CurrentDocument);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while saving the file for {FilePath}.", CurrentDocument.FilePath);
            }
        }

        private void ExecuteSaveAsFile(object? parameter) // 追加
        {
            _logger.LogInformation("ExecuteSaveAsFile command triggered.");

            if (CurrentDocument == null)
            {
                _logger.LogWarning("CurrentDocument is null. Save As command cannot execute.");
                return;
            }

            string? newFilePath = _dialogService.ShowSaveFileDialog(
                "名前を付けて保存",
                "Markdown files (*.md;*.markdown)|*.md;*.markdown|All files (*.*)|*.*",
                CurrentDocument.FilePath // 既存のファイルパスを初期値として渡す
            );

            if (!string.IsNullOrEmpty(newFilePath))
            {
                try
                {
                    // 新しいファイルパスでドキュメントを更新し、保存
                    CurrentDocument.FilePath = newFilePath;
                    _fileService.Save(CurrentDocument);
                    _logger.LogInformation("File saved as: {FilePath}", newFilePath);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred while saving the file as {FilePath}.", newFilePath);
                }
            }
            else
            {
                _logger.LogInformation("Save As dialog was cancelled.");
            }
        }
    }
}
