using PageLeaf.Models;
using PageLeaf.Models.Markdown;
using PageLeaf.Models.Css;
using PageLeaf.Models.Css.Elements;
using PageLeaf.Models.Settings;
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
        private readonly ISettingsService _settingsService;

        /// <summary>
        /// <see cref="SaveDocumentUseCase"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="editorService">エディタサービス。</param>
        /// <param name="fileService">ファイルサービス。</param>
        /// <param name="saveAsDocumentUseCase">名前を付けて保存ユースケース。</param>
        /// <param name="settingsService">設定サービス。</param>
        public SaveDocumentUseCase(IEditorService editorService, IFileService fileService, ISaveAsDocumentUseCase saveAsDocumentUseCase, ISettingsService settingsService)
        {
            _editorService = editorService;
            _fileService = fileService;
            _saveAsDocumentUseCase = saveAsDocumentUseCase;
            _settingsService = settingsService;
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
                // ドメインモデルの振る舞いを使用して保存準備を行う
                document.UpdateTimestamp();

                if (_settingsService.CurrentSettings.Editor.RenumberFootnotesOnSave)
                {
                    document.RenumberFootnotes();
                }

                // 保存用に結合されたテキストを取得
                var fullContent = document.ToFullString();

                // FileService.Save は document オブジェクトを受け取る設計のため、
                // 一時的に保存用のクローンを作成して渡す
                var saveTarget = new MarkdownDocument
                {
                    FilePath = document.FilePath,
                    Content = fullContent,
                    Encoding = document.Encoding
                };

                _fileService.Save(saveTarget);
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
