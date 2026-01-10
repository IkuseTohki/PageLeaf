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
                if (document.FrontMatter.Count > 0)
                {
                    // 参照を新しくすることで変更通知を飛ばす
                    var newFrontMatter = new System.Collections.Generic.Dictionary<string, object>(document.FrontMatter);
                    newFrontMatter["updated"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                    document.FrontMatter = newFrontMatter;
                }

                // 保存用に結合
                var fullContent = _markdownService.Join(document.FrontMatter, document.Content);

                // FileService.Save は document オブジェクトを受け取るため、
                // 一時的に全文を持たせる必要があるが、エディタ側の同期を避けるため
                // 別のインスタンスを作成するか、FileService側の引数を検討する。
                // ここではクローンに近いドキュメントオブジェクトを作成して保存に回す。
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
