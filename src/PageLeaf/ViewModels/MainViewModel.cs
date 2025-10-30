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
        private FileTreeNode _rootNode;
        private ObservableCollection<DisplayMode> _availableModes;
        private DisplayMode _selectedMode;
        private ObservableCollection<string> _availableCssFiles;
        private string _selectedCssFile;

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

        public ICommand OpenFolderCommand { get; }

        public MainViewModel(IFileService fileService, ILogger<MainViewModel> logger)
        {
            _fileService = fileService;
            _logger = logger;
            OpenFolderCommand = new Utilities.DelegateCommand(OpenFolder);

            AvailableModes = new ObservableCollection<DisplayMode>(Enum.GetValues(typeof(DisplayMode)).Cast<DisplayMode>());
            SelectedMode = AvailableModes.FirstOrDefault();

            AvailableCssFiles = new ObservableCollection<string> { "github.css", "solarized-light.css", "solarized-dark.css" };
            SelectedCssFile = AvailableCssFiles[0];
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
