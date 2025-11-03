using PageLeaf.Models;
using PageLeaf.ViewModels;
using System;
using System.ComponentModel;

namespace PageLeaf.Services
{
    public class EditorService : ViewModelBase, IEditorService
    {
        private readonly IMarkdownService _markdownService;
        private readonly ICssService _cssService;
        private readonly IDialogService _dialogService;
        private string? _currentCssPath;

        private DisplayMode _selectedMode;
        private MarkdownDocument _currentDocument = new MarkdownDocument();
        private bool _isMarkdownEditorVisible;
        private bool _isViewerVisible;
        private string _htmlContent = string.Empty;

        // EditorService が公開する IsDirty プロパティ
        public bool IsDirty => CurrentDocument.IsDirty;

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
            private set
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
            private set
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
            private set
            {
                if (_htmlContent != value)
                {
                    _htmlContent = value;
                    OnPropertyChanged();
                }
            }
        }

        public MarkdownDocument CurrentDocument
        {
            get => _currentDocument;
            private set
            {
                if (_currentDocument != value)
                {
                    // 既存の購読を解除
                    if (_currentDocument != null)
                    {
                        _currentDocument.PropertyChanged -= CurrentDocument_PropertyChanged;
                    }

                    _currentDocument = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(EditorText)); // CurrentDocumentが変更されたらEditorTextも更新
                    UpdateHtmlContent(); // ドキュメント変更時にもHTMLを更新

                    // 新しいドキュメントの購読を開始
                    if (_currentDocument != null)
                    {
                        _currentDocument.PropertyChanged += CurrentDocument_PropertyChanged;
                    }
                    OnPropertyChanged(nameof(IsDirty)); // IsDirty の状態も更新される可能性があるため通知
                }
            }
        }

        public string EditorText
        {
            get => CurrentDocument.Content ?? string.Empty;
            set
            {
                if (CurrentDocument.Content != value)
                {
                    CurrentDocument.Content = value; // ここで MarkdownDocument.IsDirty が true になる
                    OnPropertyChanged();
                    UpdateHtmlContent();
                }
            }
        }

        public EditorService(IMarkdownService markdownService, ICssService cssService, IDialogService dialogService)
        {
            ArgumentNullException.ThrowIfNull(markdownService);
            ArgumentNullException.ThrowIfNull(cssService);
            ArgumentNullException.ThrowIfNull(dialogService); // IDialogService の null チェック

            _markdownService = markdownService;
            _cssService = cssService;
            _dialogService = dialogService; // IDialogService を設定

            SelectedMode = DisplayMode.Markdown; // 初期モード

            // CurrentDocument の PropertyChanged イベントを購読し、IsDirty の変更を検知
            _currentDocument.PropertyChanged += CurrentDocument_PropertyChanged;

            UpdateVisibility(); // 初期表示を設定
        }

        // CurrentDocument の PropertyChanged イベントハンドラ
        private void CurrentDocument_PropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(MarkdownDocument.IsDirty))
            {
                OnPropertyChanged(nameof(IsDirty)); // EditorService の IsDirty の変更を通知
            }
        }

        public void LoadDocument(MarkdownDocument document)
        {
            CurrentDocument = document;
            CurrentDocument.IsDirty = false; // ドキュメントをロードしたら、変更状態をリセット
        }

        public void ApplyCss(string cssFileName)
        {
            _currentCssPath = _cssService.GetCssPath(cssFileName);
            UpdateHtmlContent();
        }

        /// <summary>
        /// 新しいドキュメントを作成し、エディタの状態をリセットします。
        /// </summary>
        public void NewDocument()
        {
            CurrentDocument = new MarkdownDocument();
        }

        /// <summary>
        /// 未保存の変更がある場合、ユーザーに保存を促すダイアログを表示します。
        /// </summary>
        /// <returns>ユーザーの選択結果。</returns>
        public SaveConfirmationResult PromptForSaveIfDirty()
        {
            if (IsDirty)
            {
                return _dialogService.ShowSaveConfirmationDialog();
            }
            return SaveConfirmationResult.NoAction; // 変更がない場合は何もしない
        }

        private void UpdateVisibility()
        {
            IsMarkdownEditorVisible = SelectedMode == DisplayMode.Markdown;
            IsViewerVisible = SelectedMode == DisplayMode.Viewer;
        }

        private void UpdateHtmlContent()
        {
            if (SelectedMode == DisplayMode.Viewer)
            {
#pragma warning disable CS8625 // null リテラルまたは考えられる null 値を null 非許容参照型に変換しています。
                HtmlContent = _markdownService.ConvertToHtml(CurrentDocument.Content, _currentCssPath ?? string.Empty);
#pragma warning restore CS8625 // null リテラルまたは考えられる null 値を null 非許容参照型に変換しています。
            }
            else
            {
                HtmlContent = string.Empty;
            }
        }
    }
}
