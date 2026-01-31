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
        Code,
        Footnote
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
        private readonly HashSet<CssEditorTab> _dirtyTabs = new HashSet<CssEditorTab>();

        private static readonly string[] StylePropertyNames = new[]
        {
            "BodyTextColor", "BodyBackgroundColor", "BodyFontSize",
            "ParagraphLineHeight", "ParagraphMarginBottom", "ParagraphTextIndent",
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
        /// 指定されたタブに変更があるかどうかを取得します。
        /// </summary>
        public bool IsTabDirty(CssEditorTab tab) => _dirtyTabs.Contains(tab);

        public bool IsTitleTabDirty => IsTabDirty(CssEditorTab.Title);
        public bool IsGeneralTabDirty => IsTabDirty(CssEditorTab.General);
        public bool IsHeadingsTabDirty => IsTabDirty(CssEditorTab.Headings);
        public bool IsQuoteTabDirty => IsTabDirty(CssEditorTab.Quote);
        public bool IsListTabDirty => IsTabDirty(CssEditorTab.List);
        public bool IsTableTabDirty => IsTabDirty(CssEditorTab.Table);
        public bool IsCodeTabDirty => IsTabDirty(CssEditorTab.Code);
        public bool IsFootnoteTabDirty => IsTabDirty(CssEditorTab.Footnote);

        private void MarkTabDirty(CssEditorTab tab)
        {
            if (_dirtyTabs.Add(tab))
            {
                OnPropertyChanged(nameof(IsTabDirty));
                switch (tab)
                {
                    case CssEditorTab.Title: OnPropertyChanged(nameof(IsTitleTabDirty)); break;
                    case CssEditorTab.General: OnPropertyChanged(nameof(IsGeneralTabDirty)); break;
                    case CssEditorTab.Headings: OnPropertyChanged(nameof(IsHeadingsTabDirty)); break;
                    case CssEditorTab.Quote: OnPropertyChanged(nameof(IsQuoteTabDirty)); break;
                    case CssEditorTab.List: OnPropertyChanged(nameof(IsListTabDirty)); break;
                    case CssEditorTab.Table: OnPropertyChanged(nameof(IsTableTabDirty)); break;
                    case CssEditorTab.Code: OnPropertyChanged(nameof(IsCodeTabDirty)); break;
                    case CssEditorTab.Footnote: OnPropertyChanged(nameof(IsFootnoteTabDirty)); break;
                }
            }
        }

        private void ClearDirtyTabs()
        {
            _dirtyTabs.Clear();
            OnPropertyChanged(nameof(IsTabDirty));
            OnPropertyChanged(nameof(IsTitleTabDirty));
            OnPropertyChanged(nameof(IsGeneralTabDirty));
            OnPropertyChanged(nameof(IsHeadingsTabDirty));
            OnPropertyChanged(nameof(IsQuoteTabDirty));
            OnPropertyChanged(nameof(IsListTabDirty));
            OnPropertyChanged(nameof(IsTableTabDirty));
            OnPropertyChanged(nameof(IsCodeTabDirty));
            OnPropertyChanged(nameof(IsFootnoteTabDirty));
        }

        private CssEditorTab GetTabFromPropertyName(string propertyName)
        {
            if (propertyName.StartsWith("Body") || propertyName.StartsWith("Paragraph")) return CssEditorTab.General;
            if (propertyName.StartsWith("Title") || propertyName.StartsWith("IsTitle")) return CssEditorTab.Title;
            if (propertyName.StartsWith("h") && propertyName.Length >= 2 && char.IsDigit(propertyName[1])) return CssEditorTab.Headings;
            if (propertyName.Contains("Heading") || propertyName.Contains("IsHeading")) return CssEditorTab.Headings;
            if (propertyName.StartsWith("Quote")) return CssEditorTab.Quote;
            if (propertyName.StartsWith("List") || propertyName.StartsWith("NumberedList")) return CssEditorTab.List;
            if (propertyName.StartsWith("Table")) return CssEditorTab.Table;
            if (propertyName.Contains("Code")) return CssEditorTab.Code;
            if (propertyName.Contains("Footnote")) return CssEditorTab.Footnote;
            return CssEditorTab.General;
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

            // 脚注のスタイルをロード
            _styles[nameof(FootnoteMarkerTextColor)] = styleInfo.Footnote.MarkerTextColor;
            _flags["Footnote.IsMarkerBold"] = styleInfo.Footnote.IsMarkerBold;
            _flags["Footnote.HasMarkerBrackets"] = styleInfo.Footnote.HasMarkerBrackets;
            _styles[nameof(FootnoteAreaFontSize)] = styleInfo.Footnote.AreaFontSize;
            _styles[nameof(FootnoteAreaTextColor)] = styleInfo.Footnote.AreaTextColor;
            _styles[nameof(FootnoteAreaMarginTop)] = styleInfo.Footnote.AreaMarginTop;
            _styles[nameof(FootnoteAreaBorderTopColor)] = styleInfo.Footnote.AreaBorderTopColor;
            _styles[nameof(FootnoteAreaBorderTopWidth)] = styleInfo.Footnote.AreaBorderTopWidth;
            _styles[nameof(FootnoteAreaBorderTopStyle)] = styleInfo.Footnote.AreaBorderTopStyle;
            _styles[nameof(FootnoteListItemLineHeight)] = styleInfo.Footnote.ListItemLineHeight;
            _flags["Footnote.IsBackLinkVisible"] = styleInfo.Footnote.IsBackLinkVisible;

            IsDirty = false;
            ClearDirtyTabs();
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
                MarkTabDirty(GetTabFromPropertyName(key));
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

        public string? ParagraphLineHeight { get => this[nameof(ParagraphLineHeight)]; set => this[nameof(ParagraphLineHeight)] = value; }
        public string? ParagraphMarginBottom { get => this[nameof(ParagraphMarginBottom)]; set => this[nameof(ParagraphMarginBottom)] = value; }
        public string? ParagraphTextIndent { get => this[nameof(ParagraphTextIndent)]; set => this[nameof(ParagraphTextIndent)] = value; }

        public string? TitleTextColor { get => this[nameof(TitleTextColor)]; set => this[nameof(TitleTextColor)] = value; }
        public string? TitleFontSize { get => this[nameof(TitleFontSize)]; set => this[nameof(TitleFontSize)] = value; }
        public string? TitleFontFamily { get => this[nameof(TitleFontFamily)]; set => this[nameof(TitleFontFamily)] = value; }
        public string? TitleAlignment { get => this[nameof(TitleAlignment)]; set => this[nameof(TitleAlignment)] = value; }
        public string? TitleMarginBottom { get => this[nameof(TitleMarginBottom)]; set => this[nameof(TitleMarginBottom)] = value; }
        public bool IsTitleBold { get => GetTitleFlag("IsBold"); set => SetTitleFlag("IsBold", value); }
        public bool IsTitleItalic { get => GetTitleFlag("IsItalic"); set => SetTitleFlag("IsItalic", value); }
        public bool IsTitleUnderline { get => GetTitleFlag("IsUnderline"); set => SetTitleFlag("IsUnderline", value); }

        private bool GetTitleFlag(string attr) => _flags.TryGetValue($"Title.{attr}", out var b) && b;
        private void SetTitleFlag(string attr, bool value) { var key = $"Title.{attr}"; if (!_flags.TryGetValue(key, out var current) || current != value) { _flags[key] = value; IsDirty = true; MarkTabDirty(CssEditorTab.Title); UpdatePreview(); OnPropertyChanged($"IsTitle{attr}"); } }

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

        public string? FootnoteMarkerTextColor { get => this[nameof(FootnoteMarkerTextColor)]; set => this[nameof(FootnoteMarkerTextColor)] = value; }
        public bool IsFootnoteMarkerBold { get => GetFootnoteFlag(nameof(CssFootnoteInfo.IsMarkerBold)); set => SetFootnoteFlag(nameof(CssFootnoteInfo.IsMarkerBold), value); }
        public bool HasFootnoteMarkerBrackets { get => GetFootnoteFlag(nameof(CssFootnoteInfo.HasMarkerBrackets)); set => SetFootnoteFlag(nameof(CssFootnoteInfo.HasMarkerBrackets), value); }
        public string? FootnoteAreaFontSize { get => this[nameof(FootnoteAreaFontSize)]; set => this[nameof(FootnoteAreaFontSize)] = value; }
        public string? FootnoteAreaTextColor { get => this[nameof(FootnoteAreaTextColor)]; set => this[nameof(FootnoteAreaTextColor)] = value; }
        public string? FootnoteAreaMarginTop { get => this[nameof(FootnoteAreaMarginTop)]; set => this[nameof(FootnoteAreaMarginTop)] = value; }
        public string? FootnoteAreaBorderTopColor { get => this[nameof(FootnoteAreaBorderTopColor)]; set => this[nameof(FootnoteAreaBorderTopColor)] = value; }
        public string? FootnoteAreaBorderTopWidth { get => this[nameof(FootnoteAreaBorderTopWidth)]; set => this[nameof(FootnoteAreaBorderTopWidth)] = value; }
        public string? FootnoteAreaBorderTopStyle { get => this[nameof(FootnoteAreaBorderTopStyle)]; set => this[nameof(FootnoteAreaBorderTopStyle)] = value; }
        public string? FootnoteListItemLineHeight { get => this[nameof(FootnoteListItemLineHeight)]; set => this[nameof(FootnoteListItemLineHeight)] = value; }
        public bool IsFootnoteBackLinkVisible { get => GetFootnoteFlag(nameof(CssFootnoteInfo.IsBackLinkVisible)); set => SetFootnoteFlag(nameof(CssFootnoteInfo.IsBackLinkVisible), value); }

        private bool GetFootnoteFlag(string attr) => _flags.TryGetValue($"Footnote.{attr}", out var b) && b;
        private void SetFootnoteFlag(string attr, bool value) { var key = $"Footnote.{attr}"; if (!_flags.TryGetValue(key, out var current) || current != value) { _flags[key] = value; IsDirty = true; MarkTabDirty(CssEditorTab.Footnote); UpdatePreview(); OnPropertyChanged(key.Replace(".", "")); OnPropertyChanged(nameof(IsFootnoteMarkerBold)); OnPropertyChanged(nameof(HasFootnoteMarkerBrackets)); OnPropertyChanged(nameof(IsFootnoteBackLinkVisible)); } }

        public string? HeadingTextColor { get => this[$"{_selectedHeadingLevel}.TextColor"]; set => this[$"{_selectedHeadingLevel}.TextColor"] = value; }
        public string? HeadingFontSize { get => this[$"{_selectedHeadingLevel}.FontSize"]; set => this[$"{_selectedHeadingLevel}.FontSize"] = value; }
        public string? HeadingFontFamily { get => this[$"{_selectedHeadingLevel}.FontFamily"]; set => this[$"{_selectedHeadingLevel}.FontFamily"] = value; }
        public string? HeadingAlignment { get => this[$"{_selectedHeadingLevel}.Alignment"]; set => this[$"{_selectedHeadingLevel}.Alignment"] = value; }
        public bool IsHeadingBold { get => GetFlag("IsBold"); set => SetFlag("IsBold", value); }
        public bool IsHeadingItalic { get => GetFlag("IsItalic"); set => SetFlag("IsItalic", value); }
        public bool IsHeadingUnderline { get => GetFlag("IsUnderline"); set => SetFlag("IsUnderline", value); }
        public bool IsHeadingNumberingEnabled { get => GetFlag("IsNumberingEnabled"); set => SetFlag("IsNumberingEnabled", value); }

        private bool GetFlag(string attr) => _selectedHeadingLevel != null && _flags.TryGetValue($"{_selectedHeadingLevel}.{attr}", out var b) && b;
        private void SetFlag(string attr, bool value) { if (_selectedHeadingLevel == null) return; var key = $"{_selectedHeadingLevel}.{attr}"; if (!_flags.TryGetValue(key, out var current) || current != value) { _flags[key] = value; IsDirty = true; MarkTabDirty(CssEditorTab.Headings); UpdatePreview(); OnPropertyChanged($"IsHeading{attr}"); } }
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
            ClearDirtyTabs();
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

            styleInfo.Footnote = new CssFootnoteInfo
            {
                MarkerTextColor = this[nameof(FootnoteMarkerTextColor)],
                IsMarkerBold = GetFootnoteFlag("IsMarkerBold"),
                HasMarkerBrackets = GetFootnoteFlag("HasMarkerBrackets"),
                AreaFontSize = this[nameof(FootnoteAreaFontSize)],
                AreaTextColor = this[nameof(FootnoteAreaTextColor)],
                AreaMarginTop = this[nameof(FootnoteAreaMarginTop)],
                AreaBorderTopColor = this[nameof(FootnoteAreaBorderTopColor)],
                AreaBorderTopWidth = this[nameof(FootnoteAreaBorderTopWidth)],
                AreaBorderTopStyle = this[nameof(FootnoteAreaBorderTopStyle)],
                ListItemLineHeight = this[nameof(FootnoteListItemLineHeight)],
                IsBackLinkVisible = GetFootnoteFlag("IsBackLinkVisible")
            };

            return styleInfo;
        }
    }
}
