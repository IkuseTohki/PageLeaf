using PageLeaf.Models;
using PageLeaf.Services;
using System;

namespace PageLeaf.UseCases
{
    /// <summary>
    /// ドキュメントに名前を付けて保存するユースケースの実装クラスです。
    /// </summary>
    public class SaveAsDocumentUseCase : ISaveAsDocumentUseCase
    {
        private readonly IEditorService _editorService;
        private readonly IFileService _fileService;
        private readonly IDialogService _dialogService;

        /// <summary>
        /// <see cref="SaveAsDocumentUseCase"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="editorService">エディタサービス。</param>
        /// <param name="fileService">ファイルサービス。</param>
        /// <param name="dialogService">ダイアログサービス。</param>
        public SaveAsDocumentUseCase(IEditorService editorService, IFileService fileService, IDialogService dialogService)
        {
            _editorService = editorService;
            _fileService = fileService;
            _dialogService = dialogService;
        }

        /// <inheritdoc />
        public bool Execute()
        {
            var document = _editorService.CurrentDocument;
            if (document == null)
            {
                return false;
            }

            string? newFilePath = _dialogService.ShowSaveFileDialog(
                "名前を付けて保存",
                "Markdown files (*.md;*.markdown)|*.md;*.markdown|All files (*.*)|*.*",
                document.FilePath
            );

            if (!string.IsNullOrEmpty(newFilePath))
            {
                try
                {
                    document.FilePath = newFilePath;
                    _fileService.Save(document);
                    return true;
                }
                catch (Exception)
                {
                    return false;
                }
            }

            return false;
        }
    }
}
