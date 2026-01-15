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
        private const string NewStylePlaceholder = "(新規作成...)";
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
        private readonly IMarkdownService _markdownService;

        private bool _isCssEditorVisible;
        private ObservableCollection<string> _availableCssFiles = null!;
        private string _selectedCssFile = null!;
        private bool _isWebView2Initialized;
        private double _cssEditorColumnWidth = 230.0;
        private bool _isTocOpen; // 目次が開いているかどうか
        private ObservableCollection<TocItem> _tocItems = new ObservableCollection<TocItem>(); // 目次アイテム

        public IEditorService Editor { get; }
        public CssEditorViewModel CssEditorViewModel { get; }
        public FrontMatterViewModel FrontMatterViewModel { get; }
        public ObservableCollection<DisplayMode> AvailableModes { get; }

        /// <summary>
        /// 目次ポップアップが開いているかどうかを取得または設定します。
        /// </summary>
        public bool IsTocOpen
        {
            get => _isTocOpen;
            set
            {
                if (_isTocOpen != value)
                {
                    _isTocOpen = value;
                    OnPropertyChanged();
                    if (_isTocOpen)
                    {
                        LoadToc();
                    }
                }
            }
        }

        /// <summary>
        /// 目次アイテムのリスト。
        /// </summary>
        public ObservableCollection<TocItem> TocItems
        {
            get => _tocItems;
            set
            {
                _tocItems = value;
                OnPropertyChanged();
            }
        }

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
                    if (value == NewStylePlaceholder)
                    {
                        // 新規作成処理
                        CreateNewStyle();
                        return;
                    }

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
        /// <summary>表示モード（エディタ/プレビュー）を切り替えるコマンド。</summary>
        public ICommand ToggleDisplayModeCommand { get; }
        /// <summary>目次の表示/非表示を切り替えるコマンド。</summary>
        public ICommand ToggleTocCommand { get; }
        /// <summary>目次から特定の見出しへナビゲートするコマンド。</summary>
        public ICommand NavigateToHeaderCommand { get; }


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
            IPasteImageUseCase pasteImageUseCase,
            IMarkdownService markdownService)
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
            ArgumentNullException.ThrowIfNull(markdownService);

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
            _markdownService = markdownService;

            FrontMatterViewModel = new FrontMatterViewModel(editorService);

            // イベント購読
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
            ToggleDisplayModeCommand = new Utilities.DelegateCommand(ExecuteToggleDisplayMode);
            ToggleTocCommand = new Utilities.DelegateCommand(ExecuteToggleToc);
            NavigateToHeaderCommand = new Utilities.DelegateCommand(ExecuteNavigateToHeader);

            AvailableModes = new ObservableCollection<DisplayMode>(
                Enum.GetValues(typeof(DisplayMode)).Cast<DisplayMode>()
            );

            AvailableCssFiles = new ObservableCollection<string>(_cssManagementService.GetAvailableCssFileNames());
            AvailableCssFiles.Add(NewStylePlaceholder);

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
            ApplyDocumentMetadata();
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
                ApplyDocumentMetadata();
            }
        }

        private void ApplyDocumentMetadata()
        {
            var doc = Editor.CurrentDocument;
            if (doc != null && !string.IsNullOrEmpty(doc.SuggestedCss))
            {
                ApplySuggestedCss(doc.SuggestedCss);
            }
        }

        private void ApplySuggestedCss(string cssFileName)
        {
            if (!cssFileName.EndsWith(".css", StringComparison.OrdinalIgnoreCase))
            {
                cssFileName += ".css";
            }

            var matchedFile = AvailableCssFiles.FirstOrDefault(f => string.Equals(f, cssFileName, StringComparison.OrdinalIgnoreCase));

            if (matchedFile != null)
            {
                SelectedCssFile = matchedFile;
            }
            else
            {
                _logger.LogWarning("Suggested CSS file '{CssFile}' not found in available list.", cssFileName);
            }
        }

        private void ExecuteToggleDisplayMode(object? parameter)
        {
            if (Editor.SelectedMode == DisplayMode.Markdown)
            {
                Editor.SelectedMode = DisplayMode.Viewer;
            }
            else
            {
                Editor.SelectedMode = DisplayMode.Markdown;
            }
            Editor.RequestFocus(Editor.SelectedMode);
        }

        private void ExecuteToggleToc(object? parameter)
        {
            IsTocOpen = !IsTocOpen;
        }

        private void ExecuteNavigateToHeader(object? parameter)
        {
            if (parameter is TocItem item)
            {
                // ビュー側でスクロール処理を行うためにイベントを発行
                Editor.RequestScrollToHeader(item);
                IsTocOpen = false; // ナビゲート後は目次を閉じる
            }
        }

        /// <summary>現在のドキュメントから目次をロードします。</summary>
        private void LoadToc()
        {
            var headers = _markdownService.ExtractHeaders(Editor.EditorText);
            TocItems.Clear();
            foreach (var header in headers)
            {
                TocItems.Add(header);
            }
        }

        private void OnCssSaved(object? sender, EventArgs e)
        {
            Editor.ApplyCss(SelectedCssFile);
        }

        private void CreateNewStyle()
        {
            var newName = _dialogService.ShowInputDialog("新規CSS作成", "作成するスタイル名（ファイル名）を入力してください：", "new-style");
            if (string.IsNullOrWhiteSpace(newName))
            {
                // キャンセルされた場合は、元の選択状態に戻す必要があるが、
                // ComboBoxの仕様上、SelectedItemが変更された後にここで止まると表示がズレる可能性がある。
                // NotifyPropertyChanged を強制的に呼び出すことで、View側の表示を現在の _selectedCssFile に戻す。
                OnPropertyChanged(nameof(SelectedCssFile));
                return;
            }

            try
            {
                var createdFileName = _cssManagementService.CreateNewStyle(newName);

                // リストを更新（プレースホルダーを除去して再取得し、再度プレースホルダーを追加）
                var names = _cssManagementService.GetAvailableCssFileNames().ToList();
                AvailableCssFiles.Clear();
                foreach (var name in names)
                {
                    AvailableCssFiles.Add(name);
                }
                AvailableCssFiles.Add(NewStylePlaceholder);

                // 作成したファイルを選択状態にする
                SelectedCssFile = createdFileName;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create new CSS file: {StyleName}", newName);
                _dialogService.ShowMessage($"ファイルの作成に失敗しました：{ex.Message}", "エラー");
                OnPropertyChanged(nameof(SelectedCssFile));
            }
        }
    }
}
