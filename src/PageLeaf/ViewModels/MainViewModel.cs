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
        private FileTreeNode _rootNode;
        private ObservableCollection<DisplayMode> _availableModes;
        private DisplayMode _selectedMode;
        private ObservableCollection<string> _availableCssFiles;
        private string _selectedCssFile;
        private string _editorText;

        public FileTreeNode RootNode
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
                _selectedMode = value;
                OnPropertyChanged();
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

        public string EditorText
        {
            get => _editorText;
            set
            {
                _editorText = value;
                OnPropertyChanged();
            }
        }

        public ICommand OpenFolderCommand { get; }
        public ICommand OpenFileCommand { get; }

        public MainViewModel(IFileService fileService, ILogger<MainViewModel> logger, IDialogService dialogService) // MODIFIED: Constructor parameters
        {
            _fileService = fileService;
            _logger = logger;
            _dialogService = dialogService;
            OpenFolderCommand = new Utilities.DelegateCommand(OpenFolder);
            OpenFileCommand = new Utilities.DelegateCommand(ExecuteOpenFile);

            AvailableModes = new ObservableCollection<DisplayMode>(Enum.GetValues(typeof(DisplayMode)).Cast<DisplayMode>());
            SelectedMode = AvailableModes.FirstOrDefault();

            AvailableCssFiles = new ObservableCollection<string> { "github.css", "solarized-light.css", "solarized-dark.css" };
            SelectedCssFile = AvailableCssFiles[0];

            EditorText = "# Hello, PageLeaf!";
        }

        private void OpenFolder(object parameter)
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

        private void ExecuteOpenFile(object parameter)
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
                    EditorText = document.Content;
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
    }
}
