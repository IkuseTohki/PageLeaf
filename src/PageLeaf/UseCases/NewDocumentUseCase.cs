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
        private readonly ISettingsService _settingsService;

        /// <summary>
        /// <see cref="NewDocumentUseCase"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="editorService">エディタサービス。</param>
        /// <param name="saveDocumentUseCase">保存ユースケース。</param>
        /// <param name="markdownService">Markdownサービス。</param>
        /// <param name="settingsService">設定サービス。</param>
        public NewDocumentUseCase(
            IEditorService editorService,
            ISaveDocumentUseCase saveDocumentUseCase,
            IMarkdownService markdownService,
            ISettingsService settingsService)
        {
            _editorService = editorService;
            _saveDocumentUseCase = saveDocumentUseCase;
            _markdownService = markdownService;
            _settingsService = settingsService;
        }

        /// <inheritdoc />
        public void Execute()
        {
            if (!HandleSaveConfirmation())
            {
                return;
            }

            _editorService.NewDocument();

            if (_settingsService.CurrentSettings.AutoInsertFrontMatter)
            {
                ApplyDefaultTemplate();
            }
        }

        /// <summary>
        /// 必要に応じて保存確認を行い、処理を継続してよいかどうかを判断します。
        /// </summary>
        /// <returns>処理を継続する場合は true、中断する場合は false。</returns>
        private bool HandleSaveConfirmation()
        {
            var result = _editorService.PromptForSaveIfDirty();

            if (result == SaveConfirmationResult.Cancel)
            {
                return false;
            }

            if (result == SaveConfirmationResult.Save)
            {
                return _saveDocumentUseCase.Execute();
            }

            return true;
        }

        /// <summary>
        /// 新規ドキュメントにデフォルトのテンプレート（フロントマターとタイトル）を適用します。
        /// </summary>
        private void ApplyDefaultTemplate()
        {
            var doc = _editorService.CurrentDocument;
            if (doc == null)
            {
                return;
            }

            var now = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            doc.FrontMatter = new System.Collections.Generic.Dictionary<string, object>
            {
                { "title", "Untitled" },
                { "created", now },
                { "updated", now }
            };

            doc.Content = "# Untitled" + System.Environment.NewLine;
        }
    }
}
