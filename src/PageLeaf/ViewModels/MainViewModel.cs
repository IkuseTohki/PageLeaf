using Microsoft.Extensions.Logging;
using PageLeaf.Models;
using PageLeaf.Services;
using System;
using System.Windows.Input;

namespace PageLeaf.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IFileService _fileService;
        private readonly ILogger<MainViewModel> _logger;
        private FileTreeNode _rootNode;

        public FileTreeNode RootNode
        {
            get => _rootNode;
            private set
            {
                _rootNode = value;
                OnPropertyChanged();
            }
        }

        public ICommand OpenFolderCommand { get; }

        public MainViewModel(IFileService fileService, ILogger<MainViewModel> logger)
        {
            _fileService = fileService;
            _logger = logger;
            OpenFolderCommand = new Utilities.DelegateCommand(OpenFolder);
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
    }
}
