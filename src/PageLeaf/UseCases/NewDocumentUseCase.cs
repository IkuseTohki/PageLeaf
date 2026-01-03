using PageLeaf.Models;
using PageLeaf.Services;

namespace PageLeaf.UseCases
{
    /// <summary>
    /// 新規ドキュメントを作成するユースケースの実装クラスです。
    /// </summary>
    public class NewDocumentUseCase : INewDocumentUseCase
    {
        private readonly IEditorService _editorService;
        private readonly ISaveDocumentUseCase _saveDocumentUseCase;

        /// <summary>
        /// <see cref="NewDocumentUseCase"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="editorService">エディタサービス。</param>
        /// <param name="saveDocumentUseCase">保存ユースケース。</param>
        public NewDocumentUseCase(IEditorService editorService, ISaveDocumentUseCase saveDocumentUseCase)
        {
            _editorService = editorService;
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

            _editorService.NewDocument();
        }
    }
}