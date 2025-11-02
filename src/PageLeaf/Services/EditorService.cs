using PageLeaf.Models;
using PageLeaf.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.Linq;

namespace PageLeaf.Services
{
    public class EditorService : ViewModelBase, IEditorService
    {
        private readonly IMarkdownService _markdownService;

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
            get
            {
                return _htmlContent;
            }
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
                _currentDocument = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(EditorText)); // CurrentDocumentが変更されたらEditorTextも更新
                UpdateHtmlContent(); // ドキュメント変更時にもHTMLを更新
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
                    OnPropertyChanged(); // CurrentDocumentがnullの場合でも変更通知を発生させる
                    UpdateHtmlContent();
                }
            }
        }

        public EditorService(IMarkdownService markdownService)
        {
            ArgumentNullException.ThrowIfNull(markdownService);

            _markdownService = markdownService;

            SelectedMode = DisplayMode.Markdown; // 初期モード

            UpdateVisibility(); // 初期表示を設定
        }

        public void LoadDocument(MarkdownDocument document)
        {
            CurrentDocument = document;
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
                HtmlContent = _markdownService.ConvertToHtml(CurrentDocument.Content);
            }
            else
            { 
                HtmlContent = string.Empty;
            }
        }
    }
}
