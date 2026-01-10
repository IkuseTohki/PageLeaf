using PageLeaf.Models;
using PageLeaf.ViewModels;
using System;
using System.ComponentModel;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic; // Added
using System.Linq; // Added

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
        private string _htmlFilePath = string.Empty;
        private readonly List<string> _tempHtmlFiles = new List<string>(); // Added

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

        public string HtmlFilePath
        {
            get => _htmlFilePath;
            private set
            {
                if (_htmlFilePath != value)
                {
                    // Clean up the old temporary file before setting a new one
                    CleanupOldTempFile(_htmlFilePath);
                    _htmlFilePath = value;
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
                    CurrentDocument.Content = value; // ここで MarkdownDocument.IsDirty が true になり、PropertyChangedイベント経由でUpdateHtmlContentが呼ばれる
                    OnPropertyChanged();
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

            SelectedMode = DisplayMode.Viewer; // 初期モード

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
            else if (e.PropertyName == nameof(MarkdownDocument.Content))
            {
                OnPropertyChanged(nameof(EditorText)); // EditorText の変更を通知
                UpdateHtmlContent(); // プレビューも更新
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
            CleanupTempFiles(); // New document means old temp file is no longer relevant
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

        public void UpdatePreview()
        {
            UpdateHtmlContent();
        }

        public void RequestInsertText(string text)
        {
            TextInsertionRequested?.Invoke(this, text);
        }

        public event EventHandler<string>? TextInsertionRequested;

        private void UpdateVisibility()
        {
            IsMarkdownEditorVisible = SelectedMode == DisplayMode.Markdown;
            IsViewerVisible = SelectedMode == DisplayMode.Viewer;
        }

        private void UpdateHtmlContent()
        {
            if (SelectedMode == DisplayMode.Viewer)
            {
                if (string.IsNullOrEmpty(CurrentDocument.Content))
                {
                    HtmlFilePath = string.Empty; // No content, no file path
                    return;
                }
#pragma warning disable CS8625 // null リテラルまたは考えられる null 値を null 非許容参照型に変換しています。
                var baseDir = !string.IsNullOrEmpty(CurrentDocument.FilePath)
                    ? Path.GetDirectoryName(CurrentDocument.FilePath)
                    : null;

                // 本文とフロントマターを結合して渡す
                string fullMarkdown = _markdownService.Join(CurrentDocument.FrontMatter, CurrentDocument.Content);
                string html = _markdownService.ConvertToHtml(fullMarkdown, _currentCssPath ?? string.Empty, baseDir);

                HtmlFilePath = SaveHtmlToTempFile(html);
#pragma warning restore CS8625 // null リテラルまたは考えられる null 値を null 非許容参照型に変換しています。
            }
            else
            {
                HtmlFilePath = string.Empty; // Not in viewer mode, no file path
            }
        }

        /// <summary>
        /// HTMLコンテンツを一時ファイルに保存し、そのパスを返します。
        /// </summary>
        /// <param name="htmlContent">保存するHTMLコンテンツ。</param>
        /// <returns>保存された一時ファイルのフルパス。</returns>
        private string SaveHtmlToTempFile(string htmlContent)
        {
            string tempDirectory = Path.GetTempPath();
            string fileName = $"PageLeaf-{Guid.NewGuid()}.html";
            string filePath = Path.Combine(tempDirectory, fileName);

            File.WriteAllText(filePath, htmlContent);
            _tempHtmlFiles.Add(filePath); // Track the created file
            return filePath;
        }

        /// <summary>
        /// 指定された一時ファイルを削除します。
        /// </summary>
        /// <param name="filePath">削除する一時ファイルのパス。</param>
        private void CleanupOldTempFile(string? filePath)
        {
            if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    _tempHtmlFiles.Remove(filePath);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Failed to delete temporary HTML file: {filePath}. Error: {ex.Message}");
                    // Log the error, but don't rethrow as it shouldn't stop the app.
                }
            }
        }

        /// <summary>
        /// 作成されたすべての一時HTMLファイルを削除します。
        /// </summary>
        public void CleanupTempFiles()
        {
            foreach (var filePath in _tempHtmlFiles.ToList()) // ToList() to avoid modification during iteration
            {
                CleanupOldTempFile(filePath);
            }
        }
    }
}
