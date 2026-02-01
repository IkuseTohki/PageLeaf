using PageLeaf.Models;
using PageLeaf.Models.Markdown;
using PageLeaf.Models.Css;
using PageLeaf.Models.Css.Elements;
using PageLeaf.Models.Settings;
using PageLeaf.Services;
using PageLeaf.Utilities;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PageLeaf.UseCases
{
    /// <summary>
    /// 画像貼り付けユースケースの実装です。
    /// </summary>
    public class PasteImageUseCase : IPasteImageUseCase
    {
        private readonly IImagePasteService _imagePasteService;
        private readonly ISettingsService _settingsService;
        private readonly IEditorService _editorService;
        private readonly IDialogService _dialogService;

        public PasteImageUseCase(
            IImagePasteService imagePasteService,
            ISettingsService settingsService,
            IEditorService editorService,
            IDialogService dialogService)
        {
            _imagePasteService = imagePasteService;
            _settingsService = settingsService;
            _editorService = editorService;
            _dialogService = dialogService;
        }

        public async Task ExecuteAsync(string currentMarkdownFilePath)
        {
            if (string.IsNullOrEmpty(currentMarkdownFilePath))
            {
                _dialogService.ShowMessage("画像を貼り付けるには、先にMarkdownファイルを保存してください。", "画像貼り付け");
                return;
            }

            var settings = _settingsService.CurrentSettings;
            var now = DateTime.Now;

            // ファイル名と保存先ディレクトリを決定
            var fileNameNoExt = ImageFileNameResolver.ResolveFileName(settings.ImageFileNameTemplate, currentMarkdownFilePath, now);

            // ResolveFullSavePathはファイルパスを返すため、ダミーファイル名を使ってディレクトリを取得
            var dummyFullPath = ImageFileNameResolver.ResolveFullSavePath(currentMarkdownFilePath, settings.ImageSaveDirectory, "dummy");
            var saveDir = Path.GetDirectoryName(dummyFullPath);

            if (string.IsNullOrEmpty(saveDir))
            {
                _dialogService.ShowMessage("画像の保存先ディレクトリを特定できませんでした。", "エラー");
                return;
            }

            var savedPath = await _imagePasteService.SaveClipboardImageAsync(saveDir, fileNameNoExt);

            if (savedPath != null)
            {
                // Markdownリンクの生成
                var markdownDir = Path.GetDirectoryName(currentMarkdownFilePath);

                string relativePath;
                if (!string.IsNullOrEmpty(markdownDir))
                {
                    relativePath = Path.GetRelativePath(markdownDir, savedPath);
                }
                else
                {
                    relativePath = savedPath;
                }

                // WindowsパスセパレータをMarkdown標準の/に変換
                relativePath = relativePath.Replace("\\", "/");

                // 画像リンクを作成
                var markdownLink = $"![Image]({relativePath})";

                // エディタに挿入
                _editorService.RequestInsertText(markdownLink);
            }
        }
    }
}
