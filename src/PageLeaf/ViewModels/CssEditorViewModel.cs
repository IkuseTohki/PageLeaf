using System.Windows.Input;
using PageLeaf.Utilities;
using PageLeaf.Services;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using PageLeaf.Models;
using PageLeaf.UseCases;
using System.Linq;
using System.Reflection;

namespace PageLeaf.ViewModels
{
    /// <summary>
    /// CSSエディタのタブカテゴリを定義します。
    /// </summary>
    public enum CssEditorTab
    {
        Title,
        General,
        Headings,
        Quote,
        List,
        Table,
        Code
    }

    public class CssEditorViewModel : ViewModelBase
    {
        private readonly ICssManagementService _cssManagementService;
        private readonly ILoadCssUseCase _loadCssUseCase;
        private readonly ISaveCssUseCase _saveCssUseCase;
        private readonly IDialogService _dialogService;
        private readonly ISettingsService _settingsService;
        private readonly Dictionary<string, string?> _styles = new Dictionary<string, string?>();
        private readonly Dictionary<string, bool> _flags = new Dictionary<string, bool>();

        private static readonly string[] StylePropertyNames = new[]
        {
            "BodyTextColor", "BodyBackgroundColor", "BodyFontSize",
            "TitleTextColor", "TitleFontSize", "TitleFontFamily", "TitleAlignment", "TitleMarginBottom",
            "QuoteTextColor", "QuoteBackgroundColor", "QuoteBorderColor",
            "QuoteBorderWidth", "QuoteBorderStyle",
            "TableBorderColor", "TableHeaderBackgroundColor", "TableHeaderTextColor", "TableHeaderFontSize", "TableBorderWidth", "TableBorderStyle", "TableHeaderAlignment", "TableCellPadding",
            "CodeTextColor", "CodeBackgroundColor", "CodeFontFamily",
            "InlineCodeTextColor", "InlineCodeBackgroundColor",
            "BlockCodeTextColor", "BlockCodeBackgroundColor",
            "ListMarkerType", "NumberedListMarkerType", "ListMarkerSize", "ListIndent"
        };

        private string? _selectedHeadingLevel;
        private bool _isDirty;
        private string _originalCssContent = string.Empty;
        private string _previewCss = string.Empty;
        private CssEditorTab _selectedTab = CssEditorTab.General;

        public event EventHandler? CssSaved;
        public ICommand SaveCssCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand SelectColorCommand { get; }
        public ObservableCollection<string> AvailableHeadingLevels { get; }
        public ObservableCollection<string> AvailableUnits { get; }
        public ObservableCollection<string> AvailableAlignments { get; }
        public ObservableCollection<string> AvailableListMarkerTypes { get; }
        public ObservableCollection<string> AvailableNumberedListMarkerTypes { get; }
        public string? TargetCssFileName { get; private set; }

        public string PreviewCss
        {
            get => _previewCss;
            private set { if (_previewCss != value) { _previewCss = value; OnPropertyChanged(); } }
        }

        public bool IsDirty
        {
            get => _isDirty;
            set { if (_isDirty != value) { _isDirty = value; OnPropertyChanged(); } }
        }

        /// <summary>
        /// タイトルタブを表示すべきかどうか。
        /// </summary>
        public bool IsTitleTabVisible => _settingsService.CurrentSettings.ShowTitleInPreview;

        /// <summary>
        /// 現在選択されているタブ。
        /// </summary>
        public CssEditorTab SelectedTab
        {
            get => _selectedTab;
            set { if (_selectedTab != value) { _selectedTab = value; OnPropertyChanged(); } }
        }

        public string? this[string key] { get => GetStyleValue(key); set => SetStyleValue(key, value); }

        public CssEditorViewModel(
            ICssManagementService cssManagementService,
            ILoadCssUseCase loadCssUseCase,
            ISaveCssUseCase saveCssUseCase,
            IDialogService dialogService,
            ISettingsService settingsService)
        {
            ArgumentNullException.ThrowIfNull(cssManagementService);
            ArgumentNullException.ThrowIfNull(loadCssUseCase);
            ArgumentNullException.ThrowIfNull(saveCssUseCase);
            ArgumentNullException.ThrowIfNull(dialogService);
            ArgumentNullException.ThrowIfNull(settingsService);

            _cssManagementService = cssManagementService;
            _loadCssUseCase = loadCssUseCase;
            _saveCssUseCase = saveCssUseCase;
            _dialogService = dialogService;
            _settingsService = settingsService;

            SaveCssCommand = new DelegateCommand(ExecuteSaveCss, CanExecuteSaveCss);
            ResetCommand = new DelegateCommand(ExecuteReset, CanExecuteReset);
            SelectColorCommand = new DelegateCommand(ExecuteSelectColor);
            AvailableHeadingLevels = new ObservableCollection<string>(Enumerable.Range(1, 6).Select(i => $"h{i}"));
            SelectedHeadingLevel = AvailableHeadingLevels.FirstOrDefault();
            AvailableUnits = new ObservableCollection<string> { "px", "em", "%" };
            AvailableAlignments = new ObservableCollection<string> { "left", "center", "right" };
            AvailableListMarkerTypes = new ObservableCollection<string> { "disc", "circle", "square", "none" };
            AvailableNumberedListMarkerTypes = new ObservableCollection<string> { "decimal", "decimal-leading-zero", "lower-alpha", "upper-alpha", "lower-roman", "upper-roman", "decimal-nested" };
        }

        private void ExecuteSelectColor(object? parameter)
        {
            if (parameter is string key)
            {
                // 文字色のキー変換
                string actualKey = key switch
                {
                    nameof(HeadingTextColor) => $"{_selectedHeadingLevel}.TextColor",
                    nameof(TitleTextColor) => nameof(TitleTextColor),
                    _ => key
                };

                var currentColor = GetStyleValue(actualKey);
                var newColor = _dialogService.ShowColorPickerDialog(currentColor);
                if (newColor != null)
                {
                    SetStyleValue(actualKey, newColor);
                }
            }
        }

        public void Load(string cssFileName)
        {
            TargetCssFileName = cssFileName;
            var (content, styleInfo) = _loadCssUseCase.Execute(cssFileName);
            _originalCssContent = content;
            LoadStyles(styleInfo);
        }

        private void UpdatePreview()
        {
            if (string.IsNullOrEmpty(TargetCssFileName)) return;
            var styleInfo = CreateStyleInfo();
            PreviewCss = _cssManagementService.GenerateCss(_originalCssContent, styleInfo);
        }

        private bool CanExecuteReset(object? parameter) => IsDirty && !string.IsNullOrEmpty(TargetCssFileName);

        private void ExecuteReset(object? parameter)
        {
            if (CanExecuteReset(parameter))
            {
                Load(TargetCssFileName!);
            }
        }

        private void LoadStyles(CssStyleInfo styleInfo)
        {
            var type = typeof(CssStyleInfo);
            foreach (var name in StylePropertyNames)
            {
                var prop = type.GetProperty(name);
                if (prop != null)
                {
                    _styles[name] = prop.GetValue(styleInfo) as string;
                }
            }

            // タイトルのフラグをロード
            if (styleInfo.TitleStyleFlags != null)
            {
                _flags["Title.IsBold"] = styleInfo.TitleStyleFlags.IsBold;
                _flags["Title.IsItalic"] = styleInfo.TitleStyleFlags.IsItalic;
                _flags["Title.IsUnderline"] = styleInfo.TitleStyleFlags.IsUnderline;
            }

            foreach (var level in AvailableHeadingLevels)
            {
                _styles[$"{level}.TextColor"] = styleInfo.HeadingTextColors.TryGetValue(level, out var c) ? c : null;
                _styles[$"{level}.FontSize"] = styleInfo.HeadingFontSizes.TryGetValue(level, out var s) ? s : null;
                _styles[$"{level}.FontFamily"] = styleInfo.HeadingFontFamilies.TryGetValue(level, out var f) ? f : null;
                _styles[$"{level}.Alignment"] = styleInfo.HeadingAlignments.TryGetValue(level, out var a) ? a : null;

                if (styleInfo.HeadingStyleFlags.TryGetValue(level, out var flags))
                {
                    _flags[$"{level}.IsBold"] = flags.IsBold; _flags[$"{level}.IsItalic"] = flags.IsItalic;
                    _flags[$"{level}.IsUnderline"] = flags.IsUnderline;
                }
                _flags[$"{level}.IsNumberingEnabled"] = styleInfo.HeadingNumberingStates.TryGetValue(level, out var n) && n;
            }
            IsDirty = false;
            OnPropertyChanged(string.Empty);
            UpdateHeadingProperties();
            UpdatePreview();
        }

        private string? GetStyleValue(string key) => _styles.TryGetValue(key, out var value) ? value : null;

        private void SetStyleValue(string key, string? value)
        {
            // 枠線の種類が解除された場合はデフォルトの solid を適用する
            if (string.IsNullOrEmpty(value) && (key == nameof(TableBorderStyle) || key == nameof(QuoteBorderStyle)))
            {
                value = "solid";
            }

            if (!_styles.TryGetValue(key, out var current) || current != value)
            {
                _styles[key] = value;
                IsDirty = true;
                UpdatePreview();
                OnPropertyChanged("Item[]");
                if (_selectedHeadingLevel != null && key.StartsWith(_selectedHeadingLevel))
                {
                    var attr = key.Substring(_selectedHeadingLevel.Length + 1);
                    OnPropertyChanged($"Heading{attr}");
                }
                else OnPropertyChanged(key);
            }
        }

        // 基本プロパティ
        public string? BodyTextColor { get => this[nameof(BodyTextColor)]; set => this[nameof(BodyTextColor)] = value; }
        public string? BodyBackgroundColor { get => this[nameof(BodyBackgroundColor)]; set => this[nameof(BodyBackgroundColor)] = value; }
        public string? BodyFontSize { get => this[nameof(BodyFontSize)]; set => this[nameof(BodyFontSize)] = value; }

        public string? TitleTextColor { get => this[nameof(TitleTextColor)]; set => this[nameof(TitleTextColor)] = value; }
        public string? TitleFontSize { get => this[nameof(TitleFontSize)]; set => this[nameof(TitleFontSize)] = value; }
        public string? TitleFontFamily { get => this[nameof(TitleFontFamily)]; set => this[nameof(TitleFontFamily)] = value; }
        public string? TitleAlignment { get => this[nameof(TitleAlignment)]; set => this[nameof(TitleAlignment)] = value; }
        public string? TitleMarginBottom { get => this[nameof(TitleMarginBottom)]; set => this[nameof(TitleMarginBottom)] = value; }
        public bool IsTitleBold { get => GetTitleFlag("IsBold"); set => SetTitleFlag("IsBold", value); }
        public bool IsTitleItalic { get => GetTitleFlag("IsItalic"); set => SetTitleFlag("IsItalic", value); }
        public bool IsTitleUnderline { get => GetTitleFlag("IsUnderline"); set => SetTitleFlag("IsUnderline", value); }

        private bool GetTitleFlag(string attr) => _flags.TryGetValue($"Title.{attr}", out var b) && b;
        private void SetTitleFlag(string attr, bool value) { var key = $"Title.{attr}"; if (!_flags.TryGetValue(key, out var current) || current != value) { _flags[key] = value; IsDirty = true; UpdatePreview(); OnPropertyChanged($"IsTitle{attr}"); } }

        public string? QuoteTextColor { get => this[nameof(QuoteTextColor)]; set => this[nameof(QuoteTextColor)] = value; }
        public string? QuoteBackgroundColor { get => this[nameof(QuoteBackgroundColor)]; set => this[nameof(QuoteBackgroundColor)] = value; }
        public string? QuoteBorderColor { get => this[nameof(QuoteBorderColor)]; set => this[nameof(QuoteBorderColor)] = value; }
        public string? QuoteBorderWidth { get => this[nameof(QuoteBorderWidth)]; set => this[nameof(QuoteBorderWidth)] = value; }
        public string? QuoteBorderStyle { get => this[nameof(QuoteBorderStyle)]; set => this[nameof(QuoteBorderStyle)] = value; }
        public string? TableBorderColor { get => this[nameof(TableBorderColor)]; set => this[nameof(TableBorderColor)] = value; }
        public string? TableHeaderBackgroundColor { get => this[nameof(TableHeaderBackgroundColor)]; set => this[nameof(TableHeaderBackgroundColor)] = value; }
        public string? TableHeaderTextColor { get => this[nameof(TableHeaderTextColor)]; set => this[nameof(TableHeaderTextColor)] = value; }
        public string? TableHeaderFontSize { get => this[nameof(TableHeaderFontSize)]; set => this[nameof(TableHeaderFontSize)] = value; }
        public string? TableBorderWidth { get => this[nameof(TableBorderWidth)]; set => this[nameof(TableBorderWidth)] = value; }
        public string? TableBorderStyle { get => this[nameof(TableBorderStyle)]; set => this[nameof(TableBorderStyle)] = value; }
        public string? TableHeaderAlignment { get => this[nameof(TableHeaderAlignment)]; set => this[nameof(TableHeaderAlignment)] = value; }
        public string? TableCellPadding { get => this[nameof(TableCellPadding)]; set => this[nameof(TableCellPadding)] = value; }
        public string? CodeTextColor { get => this[nameof(CodeTextColor)]; set => this[nameof(CodeTextColor)] = value; }
        public string? CodeBackgroundColor { get => this[nameof(CodeBackgroundColor)]; set => this[nameof(CodeBackgroundColor)] = value; }
        public string? InlineCodeTextColor { get => this[nameof(InlineCodeTextColor)]; set => this[nameof(InlineCodeTextColor)] = value; }
        public string? InlineCodeBackgroundColor { get => this[nameof(InlineCodeBackgroundColor)]; set => this[nameof(InlineCodeBackgroundColor)] = value; }
        public string? BlockCodeTextColor { get => this[nameof(BlockCodeTextColor)]; set => this[nameof(BlockCodeTextColor)] = value; }
        public string? BlockCodeBackgroundColor { get => this[nameof(BlockCodeBackgroundColor)]; set => this[nameof(BlockCodeBackgroundColor)] = value; }
        public bool IsCodeBlockOverrideEnabled => _settingsService.CurrentSettings.UseCustomCodeBlockStyle;
        public string? CodeFontFamily { get => this[nameof(CodeFontFamily)]; set => this[nameof(CodeFontFamily)] = value; }
        public string? ListMarkerType { get => this[nameof(ListMarkerType)]; set => this[nameof(ListMarkerType)] = value; }
        public string? NumberedListMarkerType { get => this[nameof(NumberedListMarkerType)]; set => this[nameof(NumberedListMarkerType)] = value; }
        public string? ListMarkerSize { get => this[nameof(ListMarkerSize)]; set => this[nameof(ListMarkerSize)] = value; }
        public string? ListIndent { get => this[nameof(ListIndent)]; set => this[nameof(ListIndent)] = value; }
        public string? HeadingTextColor { get => this[$"{_selectedHeadingLevel}.TextColor"]; set => this[$"{_selectedHeadingLevel}.TextColor"] = value; }
        public string? HeadingFontSize { get => this[$"{_selectedHeadingLevel}.FontSize"]; set => this[$"{_selectedHeadingLevel}.FontSize"] = value; }
        public string? HeadingFontFamily { get => this[$"{_selectedHeadingLevel}.FontFamily"]; set => this[$"{_selectedHeadingLevel}.FontFamily"] = value; }
        public string? HeadingAlignment { get => this[$"{_selectedHeadingLevel}.Alignment"]; set => this[$"{_selectedHeadingLevel}.Alignment"] = value; }
        public bool IsHeadingBold { get => GetFlag("IsBold"); set => SetFlag("IsBold", value); }
        public bool IsHeadingItalic { get => GetFlag("IsItalic"); set => SetFlag("IsItalic", value); }
        public bool IsHeadingUnderline { get => GetFlag("IsUnderline"); set => SetFlag("IsUnderline", value); }
        public bool IsHeadingNumberingEnabled { get => GetFlag("IsNumberingEnabled"); set => SetFlag("IsNumberingEnabled", value); }

        private bool GetFlag(string attr) => _selectedHeadingLevel != null && _flags.TryGetValue($"{_selectedHeadingLevel}.{attr}", out var b) && b;
        private void SetFlag(string attr, bool value) { if (_selectedHeadingLevel == null) return; var key = $"{_selectedHeadingLevel}.{attr}"; if (!_flags.TryGetValue(key, out var current) || current != value) { _flags[key] = value; IsDirty = true; UpdatePreview(); OnPropertyChanged($"IsHeading{attr}"); } }
        public string? SelectedHeadingLevel { get => _selectedHeadingLevel; set { if (_selectedHeadingLevel != value) { _selectedHeadingLevel = value; OnPropertyChanged(); UpdateHeadingProperties(); } } }
        private void UpdateHeadingProperties() { OnPropertyChanged(nameof(HeadingTextColor)); OnPropertyChanged(nameof(HeadingFontSize)); OnPropertyChanged(nameof(HeadingFontFamily)); OnPropertyChanged(nameof(HeadingAlignment)); OnPropertyChanged(nameof(IsHeadingBold)); OnPropertyChanged(nameof(IsHeadingItalic)); OnPropertyChanged(nameof(IsHeadingUnderline)); OnPropertyChanged(nameof(IsHeadingNumberingEnabled)); }

        public void NotifySettingsChanged()
        {
            OnPropertyChanged(nameof(IsCodeBlockOverrideEnabled));
            OnPropertyChanged(nameof(IsTitleTabVisible));

            // タイトル表示がオフになり、かつ現在タイトルタブが選択されている場合は「全体」へ切り替える
            if (!IsTitleTabVisible && SelectedTab == CssEditorTab.Title)
            {
                SelectedTab = CssEditorTab.General;
            }

            UpdatePreview();
        }

        private bool CanExecuteSaveCss(object? parameter) => IsDirty && !string.IsNullOrEmpty(TargetCssFileName);

        private void ExecuteSaveCss(object? parameter)
        {
            if (!CanExecuteSaveCss(parameter)) return;
            var styleInfo = CreateStyleInfo();
            _saveCssUseCase.Execute(TargetCssFileName!, styleInfo);
            IsDirty = false;
            CssSaved?.Invoke(this, EventArgs.Empty);
        }

        private CssStyleInfo CreateStyleInfo()
        {
            var styleInfo = new CssStyleInfo();
            var type = typeof(CssStyleInfo);
            foreach (var name in StylePropertyNames)
            {
                var prop = type.GetProperty(name);
                if (prop != null && _styles.TryGetValue(name, out var val))
                    prop.SetValue(styleInfo, val);
            }
            styleInfo.IsCodeBlockOverrideEnabled = IsCodeBlockOverrideEnabled;

            // タイトルのフラグを保存
            styleInfo.TitleStyleFlags = new HeadingStyleFlags
            {
                IsBold = GetTitleFlag("IsBold"),
                IsItalic = GetTitleFlag("IsItalic"),
                IsUnderline = GetTitleFlag("IsUnderline")
            };

            foreach (var lv in AvailableHeadingLevels)
            {
                if (_styles.TryGetValue($"{lv}.TextColor", out var c) && c != null) styleInfo.HeadingTextColors[lv] = c;
                if (_styles.TryGetValue($"{lv}.FontSize", out var s) && s != null) styleInfo.HeadingFontSizes[lv] = s;
                if (_styles.TryGetValue($"{lv}.FontFamily", out var f) && f != null) styleInfo.HeadingFontFamilies[lv] = f;
                if (_styles.TryGetValue($"{lv}.Alignment", out var a)) styleInfo.HeadingAlignments[lv] = a;
                styleInfo.HeadingStyleFlags[lv] = new HeadingStyleFlags
                {
                    IsBold = _flags.TryGetValue($"{lv}.IsBold", out var b) && b,
                    IsItalic = _flags.TryGetValue($"{lv}.IsItalic", out var i) && i,
                    IsUnderline = _flags.TryGetValue($"{lv}.IsUnderline", out var u) && u
                };
                if (_flags.TryGetValue($"{lv}.IsNumberingEnabled", out var n)) styleInfo.HeadingNumberingStates[lv] = n;
            }
            return styleInfo;
        }
    }
}
