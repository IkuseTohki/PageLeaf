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
        private readonly IMarkdownService _markdownService;

        /// <summary>
        /// <see cref="SaveDocumentUseCase"/> クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="editorService">エディタサービス。</param>
        /// <param name="fileService">ファイルサービス。</param>
        /// <param name="saveAsDocumentUseCase">名前を付けて保存ユースケース。</param>
        /// <param name="markdownService">Markdownサービス。</param>
        public SaveDocumentUseCase(IEditorService editorService, IFileService fileService, ISaveAsDocumentUseCase saveAsDocumentUseCase, IMarkdownService markdownService)
        {
            _editorService = editorService;
            _fileService = fileService;
            _saveAsDocumentUseCase = saveAsDocumentUseCase;
            _markdownService = markdownService;
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
                // フロントマターの更新 (updated)
                // 既にフロントマターがある場合のみ更新する。
                // フロントマターがないファイルに対して自動的に追加されることは避ける。

                var currentFrontMatter = _markdownService.ParseFrontMatter(document.Content);
                if (currentFrontMatter.Count > 0)
                {
                    var updatedContent = _markdownService.UpdateFrontMatter(document.Content, new System.Collections.Generic.Dictionary<string, object>
                    {
                        { "updated", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") }
                    });
                    document.Content = updatedContent;
                }

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
