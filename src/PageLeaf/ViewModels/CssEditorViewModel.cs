using System.Windows.Input;
using PageLeaf.Utilities;
using PageLeaf.Services;
using System;

namespace PageLeaf.ViewModels
{
    public class CssEditorViewModel : ViewModelBase
    {
        private readonly IFileService _fileService;
        private readonly ICssEditorService _cssEditorService;

        private string? _bodyTextColor;
        private string? _bodyBackgroundColor;

        public event EventHandler? CssSaved;

        public ICommand SaveCssCommand { get; }

        public CssEditorViewModel(IFileService fileService, ICssEditorService cssEditorService)
        {
            ArgumentNullException.ThrowIfNull(fileService);
            ArgumentNullException.ThrowIfNull(cssEditorService);

            _fileService = fileService;
            _cssEditorService = cssEditorService;
            SaveCssCommand = new DelegateCommand(ExecuteSaveCss);
        }

        public string? BodyTextColor
        {
            get => _bodyTextColor;
            set
            {
                if (_bodyTextColor != value)
                {
                    _bodyTextColor = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? BodyBackgroundColor
        {
            get => _bodyBackgroundColor;
            set
            {
                if (_bodyBackgroundColor != value)
                {
                    _bodyBackgroundColor = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? TargetCssPath { get; set; }

        private void ExecuteSaveCss(object? parameter)
        {
            if (string.IsNullOrEmpty(TargetCssPath))
            {
                // TODO: パスがない場合のエラーハンドリングを検討
                return;
            }

            // 1. ファイルを読み込む
            var existingCss = _fileService.ReadAllText(TargetCssPath);

            // 2. 更新用のスタイル情報を作成
            var styleInfo = new Models.CssStyleInfo
            {
                BodyTextColor = this.BodyTextColor,
                BodyBackgroundColor = this.BodyBackgroundColor
            };

            // 3. CSSコンテンツを更新
            var updatedCss = _cssEditorService.UpdateCssContent(existingCss, styleInfo);

            // 4. ファイルに書き込む
            _fileService.WriteAllText(TargetCssPath, updatedCss);

            CssSaved?.Invoke(this, EventArgs.Empty);
        }
    }
}

