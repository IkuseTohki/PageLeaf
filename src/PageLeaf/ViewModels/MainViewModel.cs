using Microsoft.Extensions.Logging;
using PageLeaf.Models;
using PageLeaf.Services;
using PageLeaf.UseCases;
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
        private readonly ICssManagementService _cssManagementService;
        private readonly ISettingsService _settingsService;
        private readonly INewDocumentUseCase _newDocumentUseCase;
        private readonly IOpenDocumentUseCase _openDocumentUseCase;
        private readonly ISaveDocumentUseCase _saveDocumentUseCase;
        private readonly ISaveAsDocumentUseCase _saveAsDocumentUseCase;
        private readonly IPasteImageUseCase _pasteImageUseCase;

        private bool _isCssEditorVisible;
        private ObservableCollection<string> _availableCssFiles = null!;
        private string _selectedCssFile = null!;
        private bool _isWebView2Initialized;
        private double _cssEditorColumnWidth = 230.0;

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

                    // 設定を保存
                    _settingsService.CurrentSettings.SelectedCss = value;
                    _settingsService.SaveSettings(_settingsService.CurrentSettings);

                    // エディタ（WebView）に適用
                    Editor.ApplyCss(value);

                    try
                    {
                        // CSSエディタViewModelにロードさせる
                        CssEditorViewModel.Load(value);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to load CSS style: {CssFile}", value);
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

        public double CssEditorColumnWidth
        {
            get => _cssEditorColumnWidth;
            set
            {
                if (_cssEditorColumnWidth != value)
                {
                    _cssEditorColumnWidth = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand OpenFileCommand { get; }
        public ICommand SaveFileCommand { get; }
        public ICommand SaveAsFileCommand { get; }
        public ICommand NewDocumentCommand { get; }
        public ICommand ToggleCssEditorCommand { get; }
        public ICommand ShowSettingsCommand { get; }
        public ICommand ShowAboutCommand { get; }
        public ICommand PasteImageCommand { get; }
        public ICommand OpenFileByPathCommand { get; }


        public MainViewModel(
            IFileService fileService,
            ILogger<MainViewModel> logger,
            IDialogService dialogService,
            IEditorService editorService,
            ISettingsService settingsService,
            ICssManagementService cssManagementService,
            CssEditorViewModel cssEditorViewModel,
            INewDocumentUseCase newDocumentUseCase,
            IOpenDocumentUseCase openDocumentUseCase,
            ISaveDocumentUseCase saveDocumentUseCase,
            ISaveAsDocumentUseCase saveAsDocumentUseCase,
            IPasteImageUseCase pasteImageUseCase)
        {
            ArgumentNullException.ThrowIfNull(fileService);
            ArgumentNullException.ThrowIfNull(logger);
            ArgumentNullException.ThrowIfNull(dialogService);
            ArgumentNullException.ThrowIfNull(editorService);
            ArgumentNullException.ThrowIfNull(settingsService);
            ArgumentNullException.ThrowIfNull(cssManagementService);
            ArgumentNullException.ThrowIfNull(cssEditorViewModel);
            ArgumentNullException.ThrowIfNull(newDocumentUseCase);
            ArgumentNullException.ThrowIfNull(openDocumentUseCase);
            ArgumentNullException.ThrowIfNull(saveDocumentUseCase);
            ArgumentNullException.ThrowIfNull(saveAsDocumentUseCase);
            ArgumentNullException.ThrowIfNull(pasteImageUseCase);

            _fileService = fileService;
            _logger = logger;
            _dialogService = dialogService;
            Editor = editorService;
            _settingsService = settingsService;
            _cssManagementService = cssManagementService;
            CssEditorViewModel = cssEditorViewModel;
            _newDocumentUseCase = newDocumentUseCase;
            _openDocumentUseCase = openDocumentUseCase;
            _saveDocumentUseCase = saveDocumentUseCase;
            _saveAsDocumentUseCase = saveAsDocumentUseCase;
            _pasteImageUseCase = pasteImageUseCase;

            // Subscribe to event
            CssEditorViewModel.CssSaved += OnCssSaved;

            OpenFileCommand = new Utilities.DelegateCommand(ExecuteOpenFile);
            SaveFileCommand = new Utilities.DelegateCommand(ExecuteSaveFile);
            SaveAsFileCommand = new Utilities.DelegateCommand(ExecuteSaveAsFile);
            NewDocumentCommand = new Utilities.DelegateCommand(ExecuteNewDocument);
            ToggleCssEditorCommand = new Utilities.DelegateCommand(ExecuteToggleCssEditor);
            ShowSettingsCommand = new Utilities.DelegateCommand(ExecuteShowSettings);
            ShowAboutCommand = new Utilities.DelegateCommand(ExecuteShowAbout);
            PasteImageCommand = new Utilities.DelegateCommand(ExecutePasteImage);
            OpenFileByPathCommand = new Utilities.DelegateCommand(ExecuteOpenFileByPath);

            AvailableModes = new ObservableCollection<DisplayMode>(
                Enum.GetValues(typeof(DisplayMode)).Cast<DisplayMode>()
            );

            AvailableCssFiles = new ObservableCollection<string>(_cssManagementService.GetAvailableCssFileNames());

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

            // SelectedCssFileのセッターで Load が呼ばれるので、ここでは明示的に呼ばなくて良いが、
            // 初期化タイミングによってはセッターロジックが期待通り動かない場合もあるので確認が必要。
            // ObservableCollectionの初期化後なので大丈夫なはず。
            //念のため、確実に適用しておく
            Editor.ApplyCss(SelectedCssFile);
        }

        private void ExecuteNewDocument(object? parameter)
        {
            _logger.LogInformation("NewDocumentCommand executed.");
            _newDocumentUseCase.Execute();
        }

        private void ExecuteOpenFile(object? parameter)
        {
            _logger.LogInformation("OpenFileCommand executed.");
            _openDocumentUseCase.Execute();
        }

        private void ExecuteSaveFile(object? parameter)
        {
            _logger.LogInformation("ExecuteSaveFile command triggered.");
            _saveDocumentUseCase.Execute();
        }

        private void ExecuteSaveAsFile(object? parameter)
        {
            _logger.LogInformation("ExecuteSaveAsFile command triggered.");
            _saveAsDocumentUseCase.Execute();
        }

        private void ExecuteToggleCssEditor(object? obj)
        {
            IsCssEditorVisible = !IsCssEditorVisible;
        }

        private void ExecuteShowSettings(object? parameter)
        {
            _dialogService.ShowSettingsDialog();

            // 設定が変更された可能性があるため、各所に通知・反映
            CssEditorViewModel.NotifySettingsChanged();

            // WebViewの内容を更新（テーマ変更の反映）
            Editor.UpdatePreview();
        }

        private void ExecuteShowAbout(object? parameter)
        {
            _dialogService.ShowAboutDialog();
        }

        private async void ExecutePasteImage(object? parameter)
        {
            _logger.LogInformation("PasteImageCommand executed.");
            var filePath = Editor.CurrentDocument.FilePath ?? string.Empty;
            await _pasteImageUseCase.ExecuteAsync(filePath);
        }

        private void ExecuteOpenFileByPath(object? parameter)
        {
            if (parameter is string filePath)
            {
                _logger.LogInformation("OpenFileByPathCommand executed for: {FilePath}", filePath);
                _openDocumentUseCase.OpenPath(filePath);
            }
        }

        private void OnCssSaved(object? sender, EventArgs e)
        {
            Editor.ApplyCss(SelectedCssFile);
        }
    }
}
