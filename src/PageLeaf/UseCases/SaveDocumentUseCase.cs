using PageLeaf.Models;
using PageLeaf.Services;
using System;

namespace PageLeaf.UseCases
{
    /// <summary>
    /// ドキュメントを上書き保存するユースケースの実装クラスです。
    /// </summary>
    public class SaveDocumentUseCase : ISaveDocumentUseCase
    {
        private readonly IEditorService _editorService;
        private readonly IFileService _fileService;
        private readonly ISaveAsDocumentUseCase _saveAsDocumentUseCase;

        /// <summary>
        /// <see cref="SaveDocumentUseCase"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="editorService">エディタサービス。</param>
        /// <param name="fileService">ファイルサービス。</param>
        /// <param name="saveAsDocumentUseCase">名前を付けて保存ユースケース。</param>
        public SaveDocumentUseCase(IEditorService editorService, IFileService fileService, ISaveAsDocumentUseCase saveAsDocumentUseCase)
        {
            _editorService = editorService;
            _fileService = fileService;
            _saveAsDocumentUseCase = saveAsDocumentUseCase;
        }

        /// <inheritdoc />
        public bool Execute()
        {
            var document = _editorService.CurrentDocument;
            if (document == null)
            {
                return false;
            }

            // ファイルパスが設定されていない、またはファイルが存在しない場合は「名前を付けて保存」に切り替える
            if (string.IsNullOrEmpty(document.FilePath) || !_fileService.FileExists(document.FilePath))
            {
                return _saveAsDocumentUseCase.Execute();
            }

            try
            {
                _fileService.Save(document);
                document.IsDirty = false;
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}
