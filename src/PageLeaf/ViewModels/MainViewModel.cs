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
        private FileTreeNode? _rootNode;
        private ObservableCollection<string> _availableCssFiles = null!;
        private string _selectedCssFile = null!;

        public IEditorService Editor { get; } // EditorService をプロパティとして公開

        public ObservableCollection<DisplayMode> AvailableModes { get; }

        public FileTreeNode? RootNode
        {
            get => _rootNode;
            private set
            {
                _rootNode = value;
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

        public ICommand OpenFolderCommand { get; }
        public ICommand OpenFileCommand { get; }
        public ICommand SaveFileCommand { get; }
        public ICommand SaveAsFileCommand { get; }

        public MainViewModel(IFileService fileService, ILogger<MainViewModel> logger, IDialogService dialogService, IEditorService editorService)
        {
            ArgumentNullException.ThrowIfNull(fileService);
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(dialogService);
            ArgumentNullException.ThrowIfNull(editorService);

            _fileService = fileService;
            _logger = logger;
            _dialogService = dialogService;
            Editor = editorService;

            OpenFolderCommand = new Utilities.DelegateCommand(OpenFolder);
            OpenFileCommand = new Utilities.DelegateCommand(ExecuteOpenFile);
            SaveFileCommand = new Utilities.DelegateCommand(ExecuteSaveFile);
            SaveAsFileCommand = new Utilities.DelegateCommand(ExecuteSaveAsFile);

            AvailableModes = new ObservableCollection<DisplayMode>(
                Enum.GetValues(typeof(DisplayMode)).Cast<DisplayMode>()
            );


            AvailableCssFiles = new ObservableCollection<string> { "github.css", "solarized-light.css", "solarized-dark.css" };
            SelectedCssFile = AvailableCssFiles[0];
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
    }
}
