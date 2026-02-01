using PageLeaf.Models;
using PageLeaf.Models.Markdown;
using PageLeaf.Models.Css;
using PageLeaf.Models.Css.Elements;
using PageLeaf.Models.Settings;
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

            var settings = _settingsService.CurrentSettings;
            var resultFrontMatter = new System.Collections.Generic.Dictionary<string, object>();

            // 1. アプリ管理の標準プロパティを生成
            resultFrontMatter["title"] = "";
            resultFrontMatter["created"] = ReplacePlaceholders("{Now}");
            resultFrontMatter["updated"] = ReplacePlaceholders("{Now}");
            resultFrontMatter["css"] = "";
            resultFrontMatter["syntax_highlight"] = "";

            // 2. ユーザー定義の追加プロパティをマージ (標準プロパティの上書きも許可)
            if (settings.AdditionalFrontMatter != null)
            {
                foreach (var prop in settings.AdditionalFrontMatter)
                {
                    if (!string.IsNullOrWhiteSpace(prop.Key))
                    {
                        resultFrontMatter[prop.Key] = ReplacePlaceholders(prop.Value);
                    }
                }
            }

            doc.FrontMatter = resultFrontMatter;

            // title プロパティがあれば見出しとして使用、なければ Untitled
            string titleValue = resultFrontMatter.TryGetValue("title", out var t) ? t.ToString() ?? "" : "";
            string displayTitle = string.IsNullOrWhiteSpace(titleValue) ? "Untitled" : titleValue;
            doc.Content = $"# {displayTitle}" + System.Environment.NewLine;
        }

        /// <summary>
        /// 文字列内のプレースホルダーを現在の値に置換します。
        /// </summary>
        /// <param name="value">置換対象の文字列。</param>
        /// <returns>置換後の文字列。</returns>
        private string ReplacePlaceholders(string value)
        {
            if (string.IsNullOrEmpty(value)) return value;

            var now = System.DateTime.Now;
            return value
                .Replace("{Now}", now.ToString("yyyy-MM-dd HH:mm:ss"))
                .Replace("{Date}", now.ToString("yyyy-MM-dd"))
                .Replace("{Time}", now.ToString("HH:mm:ss"));
        }
    }
}
