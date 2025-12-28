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
        private readonly ICssManagementService _cssManagementService;

        private string? _bodyTextColor;
        private string? _bodyBackgroundColor;
        private string? _bodyFontSize;

        private string? _headingTextColor;
        private string? _headingFontSize;
        private string? _headingFontFamily;
        private string? _quoteTextColor;
        private string? _quoteBackgroundColor;
        private string? _quoteBorderColor;
        private string? _quoteBorderWidth;
        private string? _quoteBorderStyle;
        private string? _tableBorderColor;
        private string? _tableHeaderBackgroundColor;
        private string? _tableBorderWidth;
        private string? _tableCellPadding;
        private string? _codeTextColor;
        private string? _codeBackgroundColor;
        private string? _codeFontFamily;
        private string? _listMarkerType;
        private string? _listIndent;
        private bool _isHeadingNumberingEnabled;

        private Dictionary<string, string> _allHeadingTextColors = new Dictionary<string, string>();
        private Dictionary<string, string> _allHeadingFontSizes = new Dictionary<string, string>();
        private Dictionary<string, string> _allHeadingFontFamilies = new Dictionary<string, string>();
        private Dictionary<string, HeadingStyleFlags> _allHeadingStyleFlags = new Dictionary<string, HeadingStyleFlags>();
        private Dictionary<string, bool> _allHeadingNumberingStates = new Dictionary<string, bool>();
        private string? _selectedHeadingLevel;

        public event EventHandler? CssSaved;

        public ICommand SaveCssCommand { get; }

        public ObservableCollection<string> AvailableHeadingLevels { get; }

        public string? TargetCssFileName { get; private set; }

        public CssEditorViewModel(ICssManagementService cssManagementService)
        {
            ArgumentNullException.ThrowIfNull(cssManagementService);

            _cssManagementService = cssManagementService;
            SaveCssCommand = new DelegateCommand(ExecuteSaveCss);

            AvailableHeadingLevels = new ObservableCollection<string>(
                Enumerable.Range(1, 6).Select(i => $"h{i}")
            );
            SelectedHeadingLevel = AvailableHeadingLevels.FirstOrDefault();
        }

        public void Load(string cssFileName)
        {
            TargetCssFileName = cssFileName;
            var styleInfo = _cssManagementService.LoadStyle(cssFileName);
            LoadStyles(styleInfo);
        }

        private void LoadStyles(CssStyleInfo styleInfo)
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

            // Heading Numbering states
            _allHeadingNumberingStates.Clear();
            foreach (var entry in styleInfo.HeadingNumberingStates)
            {
                _allHeadingNumberingStates[entry.Key] = entry.Value;
            }

            // Quote styles
            QuoteTextColor = styleInfo.QuoteTextColor;
            QuoteBackgroundColor = styleInfo.QuoteBackgroundColor;
            QuoteBorderColor = styleInfo.QuoteBorderColor;
            QuoteBorderWidth = styleInfo.QuoteBorderWidth;
            QuoteBorderStyle = styleInfo.QuoteBorderStyle;

            // List styles
            ListMarkerType = styleInfo.ListMarkerType;
            ListIndent = styleInfo.ListIndent;

            // Table styles
            TableBorderColor = styleInfo.TableBorderColor;
            TableHeaderBackgroundColor = styleInfo.TableHeaderBackgroundColor;
            TableBorderWidth = styleInfo.TableBorderWidth;
            TableCellPadding = styleInfo.TableCellPadding;

            // Code styles
            CodeTextColor = styleInfo.CodeTextColor;
            CodeBackgroundColor = styleInfo.CodeBackgroundColor;
            CodeFontFamily = styleInfo.CodeFontFamily;

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
                    HeadingTextColor = null;
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

                // Heading Numbering State
                if (_allHeadingNumberingStates.TryGetValue(_selectedHeadingLevel, out var isEnabled))
                {
                    IsHeadingNumberingEnabled = isEnabled;
                }
                else
                {
                    IsHeadingNumberingEnabled = false;
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
                IsHeadingNumberingEnabled = false;
            }
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
                    if (_selectedHeadingLevel != null)
                    {
                        _allHeadingTextColors[_selectedHeadingLevel] = value ?? string.Empty;
                    }
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
                    if (_selectedHeadingLevel != null)
                    {
                        _allHeadingFontSizes[_selectedHeadingLevel] = value ?? string.Empty;
                    }
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
                    if (_selectedHeadingLevel != null)
                    {
                        _allHeadingFontFamilies[_selectedHeadingLevel] = value ?? string.Empty;
                    }
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
                    if (_selectedHeadingLevel != null)
                    {
                        if (!_allHeadingStyleFlags.TryGetValue(_selectedHeadingLevel, out var flags))
                        {
                            flags = new HeadingStyleFlags();
                            _allHeadingStyleFlags[_selectedHeadingLevel] = flags;
                        }
                        flags.IsBold = value;
                    }
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
                    if (_selectedHeadingLevel != null)
                    {
                        if (!_allHeadingStyleFlags.TryGetValue(_selectedHeadingLevel, out var flags))
                        {
                            flags = new HeadingStyleFlags();
                            _allHeadingStyleFlags[_selectedHeadingLevel] = flags;
                        }
                        flags.IsItalic = value;
                    }
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
                    if (_selectedHeadingLevel != null)
                    {
                        if (!_allHeadingStyleFlags.TryGetValue(_selectedHeadingLevel, out var flags))
                        {
                            flags = new HeadingStyleFlags();
                            _allHeadingStyleFlags[_selectedHeadingLevel] = flags;
                        }
                        flags.IsUnderline = value;
                    }
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
                    if (_selectedHeadingLevel != null)
                    {
                        if (!_allHeadingStyleFlags.TryGetValue(_selectedHeadingLevel, out var flags))
                        {
                            flags = new HeadingStyleFlags();
                            _allHeadingStyleFlags[_selectedHeadingLevel] = flags;
                        }
                        flags.IsStrikethrough = value;
                    }
                }
            }
        }

        public bool IsHeadingNumberingEnabled
        {
            get => _isHeadingNumberingEnabled;
            set
            {
                if (_isHeadingNumberingEnabled != value)
                {
                    _isHeadingNumberingEnabled = value;
                    OnPropertyChanged();
                    if (_selectedHeadingLevel != null)
                    {
                        _allHeadingNumberingStates[_selectedHeadingLevel] = value;
                    }
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

        public string? QuoteBorderWidth
        {
            get => _quoteBorderWidth;
            set
            {
                if (_quoteBorderWidth != value)
                {
                    _quoteBorderWidth = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? QuoteBorderStyle
        {
            get => _quoteBorderStyle;
            set
            {
                if (_quoteBorderStyle != value)
                {
                    _quoteBorderStyle = value;
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

        public string? TableBorderWidth
        {
            get => _tableBorderWidth;
            set
            {
                if (_tableBorderWidth != value)
                {
                    _tableBorderWidth = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? TableCellPadding
        {
            get => _tableCellPadding;
            set
            {
                if (_tableCellPadding != value)
                {
                    _tableCellPadding = value;
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

        public string? CodeFontFamily
        {
            get => _codeFontFamily;
            set
            {
                if (_codeFontFamily != value)
                {
                    _codeFontFamily = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? ListMarkerType
        {
            get => _listMarkerType;
            set
            {
                if (_listMarkerType != value)
                {
                    _listMarkerType = value;
                    OnPropertyChanged();
                }
            }
        }

        public string? ListIndent
        {
            get => _listIndent;
            set
            {
                if (_listIndent != value)
                {
                    _listIndent = value;
                    OnPropertyChanged();
                }
            }
        }

        private void ExecuteSaveCss(object? parameter)
        {
            if (string.IsNullOrEmpty(TargetCssFileName))
            {
                return;
            }

            var styleInfo = new Models.CssStyleInfo
            {
                BodyTextColor = this.BodyTextColor,
                BodyBackgroundColor = this.BodyBackgroundColor,
                BodyFontSize = this.BodyFontSize,
                QuoteTextColor = this.QuoteTextColor,
                QuoteBackgroundColor = this.QuoteBackgroundColor,
                QuoteBorderColor = this.QuoteBorderColor,
                QuoteBorderWidth = this.QuoteBorderWidth,
                QuoteBorderStyle = this.QuoteBorderStyle,
                TableBorderColor = this.TableBorderColor,
                TableHeaderBackgroundColor = this.TableHeaderBackgroundColor,
                TableBorderWidth = this.TableBorderWidth,
                TableCellPadding = this.TableCellPadding,
                CodeTextColor = this.CodeTextColor,
                CodeBackgroundColor = this.CodeBackgroundColor,
                CodeFontFamily = this.CodeFontFamily,
                ListMarkerType = this.ListMarkerType,
                ListIndent = this.ListIndent
            };

            foreach (var entry in _allHeadingTextColors)
            {
                styleInfo.HeadingTextColors[entry.Key] = entry.Value;
            }
            foreach (var entry in _allHeadingFontSizes)
            {
                styleInfo.HeadingFontSizes[entry.Key] = entry.Value;
            }
            foreach (var entry in _allHeadingFontFamilies)
            {
                styleInfo.HeadingFontFamilies[entry.Key] = entry.Value;
            }
            foreach (var entry in _allHeadingStyleFlags)
            {
                styleInfo.HeadingStyleFlags[entry.Key] = entry.Value;
            }
            foreach (var entry in _allHeadingNumberingStates)
            {
                styleInfo.HeadingNumberingStates[entry.Key] = entry.Value;
            }

            _cssManagementService.SaveStyle(TargetCssFileName, styleInfo);

            CssSaved?.Invoke(this, EventArgs.Empty);
        }
    }
}
