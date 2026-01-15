using PageLeaf.Models;
using PageLeaf.Services;
using System;

namespace PageLeaf.UseCases
{
    /// <summary>
    /// 既存のMarkdownファイルを開くユースケースの実装クラスです。
    /// </summary>
    public class OpenDocumentUseCase : IOpenDocumentUseCase
    {
        private readonly IEditorService _editorService;
        private readonly IFileService _fileService;
        private readonly IDialogService _dialogService;
        private readonly ISaveDocumentUseCase _saveDocumentUseCase;
        private readonly IMarkdownService _markdownService;

        /// <summary>
        /// <see cref="OpenDocumentUseCase"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="editorService">エディタサービス。</param>
        /// <param name="fileService">ファイルサービス。</param>
        /// <param name="dialogService">ダイアログサービス。</param>
        /// <param name="saveDocumentUseCase">保存ユースケース。</param>
        /// <param name="markdownService">Markdownサービス。</param>
        public OpenDocumentUseCase(
            IEditorService editorService,
            IFileService fileService,
            IDialogService dialogService,
            ISaveDocumentUseCase saveDocumentUseCase,
            IMarkdownService markdownService)
        {
            _editorService = editorService;
            _fileService = fileService;
            _dialogService = dialogService;
            _saveDocumentUseCase = saveDocumentUseCase;
            _markdownService = markdownService;
        }

        /// <inheritdoc />
        public void Execute()
        {
            if (!ConfirmSaveIfDirty()) return;

            string? filePath = _dialogService.ShowOpenFileDialog(
                "Markdownファイルを開く",
                "Markdown files (*.md;*.markdown)|*.md;*.markdown|All files (*.*)|*.*");

            if (!string.IsNullOrEmpty(filePath))
            {
                OpenInternal(filePath);
            }
        }

        /// <inheritdoc />
        public void OpenPath(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) return;
            if (!ConfirmSaveIfDirty()) return;

            OpenInternal(filePath);
        }

        private bool ConfirmSaveIfDirty()
        {
            var result = _editorService.PromptForSaveIfDirty();

            if (result == SaveConfirmationResult.Cancel)
            {
                return false;
            }

            if (result == SaveConfirmationResult.Save)
            {
                if (!_saveDocumentUseCase.Execute())
                {
                    return false;
                }
            }

            return true;
        }

        private void OpenInternal(string filePath)
        {
            try
            {
                MarkdownDocument document = _fileService.Open(filePath);

                // 生のContentを分離
                var (frontMatter, body) = _markdownService.Split(document.Content);
                document.FrontMatter = frontMatter;
                document.Content = body;

                _editorService.LoadDocument(document);
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"ファイルを開けませんでした。\n{ex.Message}", "エラー");
            }
        }
    }
}
