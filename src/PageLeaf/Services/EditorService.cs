using PageLeaf.Models;
using PageLeaf.ViewModels;
using System;

namespace PageLeaf.Services
{
    public class EditorService : ViewModelBase, IEditorService
    {
        private readonly IMarkdownService _markdownService;
        private readonly ICssService _cssService;
        private string? _currentCssPath;

        private DisplayMode _selectedMode;
        private MarkdownDocument _currentDocument = new MarkdownDocument { Content = "# Hello, PageLeaf!" };
        private bool _isMarkdownEditorVisible;
        private bool _isViewerVisible;
        private string _htmlContent = string.Empty;

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
                    _currentDocument = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(EditorText)); // CurrentDocumentが変更されたらEditorTextも更新
                    UpdateHtmlContent(); // ドキュメント変更時にもHTMLを更新
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
                    CurrentDocument.Content = value;
                    OnPropertyChanged();
                    UpdateHtmlContent();
                }
            }
        }

        public EditorService(IMarkdownService markdownService, ICssService cssService)
        {
            ArgumentNullException.ThrowIfNull(markdownService);
            ArgumentNullException.ThrowIfNull(cssService);

            _markdownService = markdownService;
            _cssService = cssService;

            SelectedMode = DisplayMode.Markdown; // 初期モード

            UpdateVisibility(); // 初期表示を設定
        }

        public void LoadDocument(MarkdownDocument document)
        {
            CurrentDocument = document;
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
            CurrentDocument = new MarkdownDocument { Content = string.Empty, FilePath = null };
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
                HtmlContent = _markdownService.ConvertToHtml(CurrentDocument.Content, _currentCssPath ?? string.Empty);
            }
            else
            {
                HtmlContent = string.Empty;
            }
        }
    }
}
