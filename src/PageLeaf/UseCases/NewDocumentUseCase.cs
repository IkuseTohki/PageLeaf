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
        private readonly IMarkdownService _markdownService;

        /// <summary>
        /// <see cref="NewDocumentUseCase"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="editorService">エディタサービス。</param>
        /// <param name="saveDocumentUseCase">保存ユースケース。</param>
        /// <param name="markdownService">Markdownサービス。</param>
        public NewDocumentUseCase(IEditorService editorService, ISaveDocumentUseCase saveDocumentUseCase, IMarkdownService markdownService)
        {
            _editorService = editorService;
            _saveDocumentUseCase = saveDocumentUseCase;
            _markdownService = markdownService;
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

            // テンプレート適用 (フロントマターの自動挿入)
            var now = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            var initialFrontMatter = new System.Collections.Generic.Dictionary<string, object>
            {
                { "title", "Untitled" },
                { "created", now },
                { "updated", now }
            };

            var contentWithTemplate = _markdownService.UpdateFrontMatter("", initialFrontMatter);
            _editorService.EditorText = contentWithTemplate;

            // テンプレートが適用された状態なので、変更あり(IsDirty=true)の状態になる。
            // これにより、即座に閉じようとした場合に保存確認が表示される。
        }
    }
}
