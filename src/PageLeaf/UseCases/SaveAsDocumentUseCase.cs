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

            string filter = "Markdown files (*.md;*.markdown)|*.md;*.markdown|All files (*.*)|*.*";
            string? filePath = _dialogService.ShowSaveFileDialog("名前を付けて保存", filter, document.FilePath);

            if (string.IsNullOrEmpty(filePath)) return false;

            try
            {
                // 新しいパスを設定
                document.FilePath = filePath;

                // タイムスタンプ更新とシリアライズ
                document.UpdateTimestamp();
                var fullContent = document.ToFullString();

                var saveTarget = new MarkdownDocument
                {
                    FilePath = filePath,
                    Content = fullContent,
                    Encoding = document.Encoding
                };

                _fileService.Save(saveTarget);
                document.IsDirty = false;
                return true;
            }
            catch (Exception ex)
            {
                _dialogService.ShowMessage($"ファイルの保存に失敗しました：{ex.Message}", "エラー");
                return false;
            }
        }
    }
}
