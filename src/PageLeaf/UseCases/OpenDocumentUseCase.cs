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

        /// <summary>
        /// <see cref="OpenDocumentUseCase"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="editorService">エディタサービス。</param>
        /// <param name="fileService">ファイルサービス。</param>
        /// <param name="dialogService">ダイアログサービス。</param>
        /// <param name="saveDocumentUseCase">保存ユースケース。</param>
        public OpenDocumentUseCase(
            IEditorService editorService,
            IFileService fileService,
            IDialogService dialogService,
            ISaveDocumentUseCase saveDocumentUseCase)
        {
            _editorService = editorService;
            _fileService = fileService;
            _dialogService = dialogService;
            _saveDocumentUseCase = saveDocumentUseCase;
        }

        /// <inheritdoc />
        public void Execute()
        {
            var result = _editorService.PromptForSaveIfDirty();

            if (result == SaveConfirmationResult.Cancel)
            {
                return;
            }

            if (result == SaveConfirmationResult.Save)
            {
                _saveDocumentUseCase.Execute();
            }

            string? filePath = _dialogService.ShowOpenFileDialog(
                "Markdownファイルを開く",
                "Markdown files (*.md;*.markdown)|*.md;*.markdown|All files (*.*)|*.*");

            if (!string.IsNullOrEmpty(filePath))
            {
                try
                {
                    MarkdownDocument document = _fileService.Open(filePath);
                    _editorService.LoadDocument(document);
                }
                catch (Exception)
                {
                    // 必要に応じてエラー通知を検討
                }
            }
        }
    }
}
