using System.Windows.Input;
using PageLeaf.Utilities;
using PageLeaf.Services;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using PageLeaf.Models;
using System.Linq;

namespace PageLeaf.ViewModels
{
    public class CssEditorViewModel : ViewModelBase
    {
        private readonly IFileService _fileService;
        private readonly ICssEditorService _cssEditorService;

        private string? _bodyTextColor;
        private string? _bodyBackgroundColor;
        private string? _bodyFontSize;

        private string? _headingTextColor;
        private string? _headingFontSize;
        private string? _headingFontFamily;
        private string? _quoteTextColor;
        private string? _quoteBackgroundColor;
        private string? _quoteBorderColor;
        private string? _tableBorderColor;
        private string? _tableHeaderBackgroundColor;
        private string? _codeTextColor;
        private string? _codeBackgroundColor;

        private Dictionary<string, string> _allHeadingTextColors = new();
        private Dictionary<string, string> _allHeadingFontSizes = new();
        private Dictionary<string, string> _allHeadingFontFamilies = new();
        private Dictionary<string, HeadingStyleFlags> _allHeadingStyleFlags = new();
        private string? _selectedHeadingLevel;

        public event EventHandler? CssSaved;

        public ICommand SaveCssCommand { get; }

        public ObservableCollection<string> AvailableHeadingLevels { get; }

        public CssEditorViewModel(IFileService fileService, ICssEditorService cssEditorService)
        {
            ArgumentNullException.ThrowIfNull(fileService);
            ArgumentNullException.ThrowIfNull(cssEditorService);

            _fileService = fileService;
            _cssEditorService = cssEditorService;
            SaveCssCommand = new DelegateCommand(ExecuteSaveCss);

            AvailableHeadingLevels = new ObservableCollection<string>(
                Enumerable.Range(1, 6).Select(i => $"h{i}")
            );
            SelectedHeadingLevel = AvailableHeadingLevels.FirstOrDefault();
        }

        public void LoadStyles(CssStyleInfo styleInfo)
        {
            // Body styles
            BodyTextColor = styleInfo.BodyTextColor;
            BodyBackgroundColor = styleInfo.BodyBackgroundColor;
            BodyFontSize = styleInfo.BodyFontSize;

            // Heading styles
            _allHeadingTextColors.Clear();
            foreach (var entry in styleInfo.HeadingTextColors)
            {
                _allHeadingTextColors[entry.Key] = entry.Value;
            }

            _allHeadingFontSizes.Clear();
            foreach (var entry in styleInfo.HeadingFontSizes)
            {
                _allHeadingFontSizes[entry.Key] = entry.Value;
            }

            _allHeadingFontFamilies.Clear();
            foreach (var entry in styleInfo.HeadingFontFamilies)
            {
                _allHeadingFontFamilies[entry.Key] = entry.Value;
            }

            _allHeadingStyleFlags.Clear();
            foreach (var entry in styleInfo.HeadingStyleFlags)
            {
                _allHeadingStyleFlags[entry.Key] = entry.Value;
            }

            UpdateHeadingProperties();
        }

        public string? SelectedHeadingLevel
        {
            get => _selectedHeadingLevel;
            set
            {
                if (_selectedHeadingLevel != value)
                {
                    _selectedHeadingLevel = value;
                    OnPropertyChanged();
                    UpdateHeadingProperties();
                }
            }
        }

        private void UpdateHeadingProperties()
        {
            if (_selectedHeadingLevel != null)
            {
                // HeadingTextColor
                if (_allHeadingTextColors.TryGetValue(_selectedHeadingLevel, out var color))
                {
                    HeadingTextColor = color;
                }
                else
                {
                    HeadingTextColor = null; // 選択されたレベルの色がない場合
                }

                // HeadingFontSize
                if (_allHeadingFontSizes.TryGetValue(_selectedHeadingLevel, out var fontSize))
                {
                    HeadingFontSize = fontSize;
                }
                else
                {
                    HeadingFontSize = null;
                }

                // HeadingFontFamily
                if (_allHeadingFontFamilies.TryGetValue(_selectedHeadingLevel, out var fontFamily))
                {
                    HeadingFontFamily = fontFamily;
                }
                else
                {
                    HeadingFontFamily = null;
                }

                // HeadingStyleFlags
                if (_allHeadingStyleFlags.TryGetValue(_selectedHeadingLevel, out var flags))
                {
                    IsHeadingBold = flags.IsBold;
                    IsHeadingItalic = flags.IsItalic;
                    IsHeadingUnderline = flags.IsUnderline;
                    IsHeadingStrikethrough = flags.IsStrikethrough;
                }
                else
                {
                    IsHeadingBold = false;
                    IsHeadingItalic = false;
                    IsHeadingUnderline = false;
                    IsHeadingStrikethrough = false;
                }
            }
            else
            {
                HeadingTextColor = null;
                HeadingFontSize = null;
                HeadingFontFamily = null;
                IsHeadingBold = false;
                IsHeadingItalic = false;
                IsHeadingUnderline = false;
                IsHeadingStrikethrough = false;
            }
            // 他の見出し関連プロパティもここに追加する
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

        public string? BodyFontSize
        {
            get => _bodyFontSize;
            set
            {
                if (_bodyFontSize != value)
                {
                    _bodyFontSize = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? HeadingTextColor
        {
            get => _headingTextColor;
            set
            {
                if (_headingTextColor != value)
                {
                    _headingTextColor = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? HeadingFontSize
        {
            get => _headingFontSize;
            set
            {
                if (_headingFontSize != value)
                {
                    _headingFontSize = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? HeadingFontFamily
        {
            get => _headingFontFamily;
            set
            {
                if (_headingFontFamily != value)
                {
                    _headingFontFamily = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isHeadingBold;
        public bool IsHeadingBold
        {
            get => _isHeadingBold;
            set
            {
                if (_isHeadingBold != value)
                {
                    _isHeadingBold = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isHeadingItalic;
        public bool IsHeadingItalic
        {
            get => _isHeadingItalic;
            set
            {
                if (_isHeadingItalic != value)
                {
                    _isHeadingItalic = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isHeadingUnderline;
        public bool IsHeadingUnderline
        {
            get => _isHeadingUnderline;
            set
            {
                if (_isHeadingUnderline != value)
                {
                    _isHeadingUnderline = value;
                    OnPropertyChanged();
                }
            }
        }

        private bool _isHeadingStrikethrough;
        public bool IsHeadingStrikethrough
        {
            get => _isHeadingStrikethrough;
            set
            {
                if (_isHeadingStrikethrough != value)
                {
                    _isHeadingStrikethrough = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? QuoteTextColor
        {
            get => _quoteTextColor;
            set
            {
                if (_quoteTextColor != value)
                {
                    _quoteTextColor = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? QuoteBackgroundColor
        {
            get => _quoteBackgroundColor;
            set
            {
                if (_quoteBackgroundColor != value)
                {
                    _quoteBackgroundColor = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? QuoteBorderColor
        {
            get => _quoteBorderColor;
            set
            {
                if (_quoteBorderColor != value)
                {
                    _quoteBorderColor = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? TableBorderColor
        {
            get => _tableBorderColor;
            set
            {
                if (_tableBorderColor != value)
                {
                    _tableBorderColor = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? TableHeaderBackgroundColor
        {
            get => _tableHeaderBackgroundColor;
            set
            {
                if (_tableHeaderBackgroundColor != value)
                {
                    _tableHeaderBackgroundColor = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? CodeTextColor
        {
            get => _codeTextColor;
            set
            {
                if (_codeTextColor != value)
                {
                    _codeTextColor = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? CodeBackgroundColor
        {
            get => _codeBackgroundColor;
            set
            {
                if (_codeBackgroundColor != value)
                {
                    _codeBackgroundColor = value;
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
                BodyBackgroundColor = this.BodyBackgroundColor,
                BodyFontSize = this.BodyFontSize,
                HeadingTextColor = this.HeadingTextColor,
                QuoteTextColor = this.QuoteTextColor,
                QuoteBackgroundColor = this.QuoteBackgroundColor,
                QuoteBorderColor = this.QuoteBorderColor,
                TableBorderColor = this.TableBorderColor,
                TableHeaderBackgroundColor = this.TableHeaderBackgroundColor,
                CodeTextColor = this.CodeTextColor,
                CodeBackgroundColor = this.CodeBackgroundColor
            };

            // 3. CSSコンテンツを更新
            var updatedCss = _cssEditorService.UpdateCssContent(existingCss, styleInfo);

            // 4. ファイルに書き込む
            _fileService.WriteAllText(TargetCssPath, updatedCss);

            CssSaved?.Invoke(this, EventArgs.Empty);
        }
    }
}


