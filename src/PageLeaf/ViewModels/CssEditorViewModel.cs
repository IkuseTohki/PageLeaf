using System.Windows.Input;
using PageLeaf.Utilities;
using PageLeaf.Services;
using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using PageLeaf.Models;
using System.Linq;
using System.Reflection;

namespace PageLeaf.ViewModels
{
    public class CssEditorViewModel : ViewModelBase
    {
        private readonly ICssManagementService _cssManagementService;
        private readonly Dictionary<string, string?> _styles = new Dictionary<string, string?>();
        private readonly Dictionary<string, bool> _flags = new Dictionary<string, bool>();

        private static readonly string[] StylePropertyNames = new[]
        {
            "BodyTextColor", "BodyBackgroundColor", "BodyFontSize",
            "QuoteTextColor", "QuoteBackgroundColor", "QuoteBorderColor",
            "QuoteBorderWidth", "QuoteBorderStyle",
            "TableBorderColor", "TableHeaderBackgroundColor", "TableBorderWidth", "TableCellPadding",
            "CodeTextColor", "CodeBackgroundColor", "CodeFontFamily",
            "ListMarkerType", "ListIndent"
        };

        // 自動変換（GlobalUnit連動）の対象
        private static readonly string[] AutoConvertPropertyNames = new[] { "BodyFontSize", "HeadingFontSize" };

        // 常に px 固定の対象
        private static readonly string[] PxFixedPropertyNames = new[] { "QuoteBorderWidth", "TableBorderWidth", "TableCellPadding", "ListIndent" };

        private string _globalUnit = "px";
        private string? _selectedHeadingLevel;

        public event EventHandler? CssSaved;
        public ICommand SaveCssCommand { get; }
        public ICommand ResetCommand { get; }
        public ObservableCollection<string> AvailableHeadingLevels { get; }
        public ObservableCollection<string> AvailableUnits { get; }
        public string? TargetCssFileName { get; private set; }

        public string GlobalUnit
        {
            get => _globalUnit;
            set { if (_globalUnit != value) { var old = _globalUnit; _globalUnit = value; ConvertAutoFontSizes(old, value); OnPropertyChanged(); } }
        }

        public string? this[string key] { get => GetStyleValue(key); set => SetStyleValue(key, value); }

        public CssEditorViewModel(ICssManagementService cssManagementService)
        {
            ArgumentNullException.ThrowIfNull(cssManagementService);
            _cssManagementService = cssManagementService;
            SaveCssCommand = new DelegateCommand(ExecuteSaveCss);
            ResetCommand = new DelegateCommand(ExecuteReset);
            AvailableHeadingLevels = new ObservableCollection<string>(Enumerable.Range(1, 6).Select(i => $"h{i}"));
            SelectedHeadingLevel = AvailableHeadingLevels.FirstOrDefault();
            AvailableUnits = new ObservableCollection<string> { "px", "em", "%" };
        }

        public void Load(string cssFileName)
        {
            TargetCssFileName = cssFileName;
            var styleInfo = _cssManagementService.LoadStyle(cssFileName);
            LoadStyles(styleInfo);
        }

        private void ExecuteReset(object? parameter)
        {
            if (!string.IsNullOrEmpty(TargetCssFileName))
            {
                Load(TargetCssFileName);
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
                    var raw = prop.GetValue(styleInfo) as string;
                    if (AutoConvertPropertyNames.Contains(name))
                        _styles[name] = UnitConversionHelper.ParseAndConvert(raw, GlobalUnit, GetDefaultSize(name));
                    else if (PxFixedPropertyNames.Contains(name))
                        _styles[name] = UnitConversionHelper.ParseAndConvert(raw, "px", GetDefaultSize(name));
                    else
                        _styles[name] = raw;
                }
            }

            foreach (var level in AvailableHeadingLevels)
            {
                _styles[$"{level}.TextColor"] = styleInfo.HeadingTextColors.TryGetValue(level, out var c) ? c : null;
                var rawSize = styleInfo.HeadingFontSizes.TryGetValue(level, out var s) ? s : null;
                _styles[$"{level}.FontSize"] = UnitConversionHelper.ParseAndConvert(rawSize, GlobalUnit, GetDefaultHeadingSize(level));
                _styles[$"{level}.FontFamily"] = styleInfo.HeadingFontFamilies.TryGetValue(level, out var f) ? f : null;

                if (styleInfo.HeadingStyleFlags.TryGetValue(level, out var flags))
                {
                    _flags[$"{level}.IsBold"] = flags.IsBold; _flags[$"{level}.IsItalic"] = flags.IsItalic;
                    _flags[$"{level}.IsUnderline"] = flags.IsUnderline; _flags[$"{level}.IsStrikethrough"] = flags.IsStrikethrough;
                }
                _flags[$"{level}.IsNumberingEnabled"] = styleInfo.HeadingNumberingStates.TryGetValue(level, out var n) && n;
            }
            OnPropertyChanged(string.Empty);
            UpdateHeadingProperties();
        }

        private string GetDefaultSize(string name) => name switch { "BodyFontSize" => "16", "QuoteBorderWidth" => "4", "TableBorderWidth" => "1", "TableCellPadding" => "6", "ListIndent" => "20", _ => "16" };
        private string GetDefaultHeadingSize(string lv) => lv switch { "h1" => "32", "h2" => "24", "h3" => "18.72", "h4" => "16", "h5" => "13.28", "h6" => "10.72", _ => "16" };

        private void ConvertAutoFontSizes(string fromUnit, string toUnit)
        {
            // 単一プロパティの変換と通知
            foreach (var name in AutoConvertPropertyNames)
            {
                if (_styles.TryGetValue(name, out var val))
                {
                    _styles[name] = ConvertValue(val, fromUnit, toUnit);
                    OnPropertyChanged(name); // UI更新のために個別に通知
                }
            }

            // 各見出しレベルの数値を変換
            foreach (var lv in AvailableHeadingLevels)
            {
                var key = $"{lv}.FontSize";
                if (_styles.TryGetValue(key, out var v))
                {
                    _styles[key] = ConvertValue(v, fromUnit, toUnit);
                }
            }

            OnPropertyChanged("Item[]"); // インデクサを使用している要素への通知
            UpdateHeadingProperties();   // 現在選択中の見出しプロパティの通知
        }

        private string? ConvertValue(string? value, string from, string to)
        {
            if (!double.TryParse(value, out var d)) return value;
            double px = from switch { "em" => UnitConversionHelper.EmToPx(d), "%" => UnitConversionHelper.PercentToPx(d), _ => d };
            double result = to switch { "em" => UnitConversionHelper.PxToEm(px), "%" => UnitConversionHelper.PxToPercent(px), _ => px };
            return UnitConversionHelper.Round(result).ToString();
        }

        private string? GetStyleValue(string key) => _styles.TryGetValue(key, out var value) ? value : null;

        private void SetStyleValue(string key, string? value)
        {
            if (!_styles.TryGetValue(key, out var current) || current != value)
            {
                _styles[key] = value;
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
        public string? QuoteTextColor { get => this[nameof(QuoteTextColor)]; set => this[nameof(QuoteTextColor)] = value; }
        public string? QuoteBackgroundColor { get => this[nameof(QuoteBackgroundColor)]; set => this[nameof(QuoteBackgroundColor)] = value; }
        public string? QuoteBorderColor { get => this[nameof(QuoteBorderColor)]; set => this[nameof(QuoteBorderColor)] = value; }
        public string? QuoteBorderWidth { get => this[nameof(QuoteBorderWidth)]; set => this[nameof(QuoteBorderWidth)] = value; }
        public string? QuoteBorderStyle { get => this[nameof(QuoteBorderStyle)]; set => this[nameof(QuoteBorderStyle)] = value; }
        public string? TableBorderColor { get => this[nameof(TableBorderColor)]; set => this[nameof(TableBorderColor)] = value; }
        public string? TableHeaderBackgroundColor { get => this[nameof(TableHeaderBackgroundColor)]; set => this[nameof(TableHeaderBackgroundColor)] = value; }
        public string? TableBorderWidth { get => this[nameof(TableBorderWidth)]; set => this[nameof(TableBorderWidth)] = value; }
        public string? TableCellPadding { get => this[nameof(TableCellPadding)]; set => this[nameof(TableCellPadding)] = value; }
        public string? CodeTextColor { get => this[nameof(CodeTextColor)]; set => this[nameof(CodeTextColor)] = value; }
        public string? CodeBackgroundColor { get => this[nameof(CodeBackgroundColor)]; set => this[nameof(CodeBackgroundColor)] = value; }
        public string? CodeFontFamily { get => this[nameof(CodeFontFamily)]; set => this[nameof(CodeFontFamily)] = value; }
        public string? ListMarkerType { get => this[nameof(ListMarkerType)]; set => this[nameof(ListMarkerType)] = value; }
        public string? ListIndent { get => this[nameof(ListIndent)]; set => this[nameof(ListIndent)] = value; }
        public string? HeadingTextColor { get => this[$"{_selectedHeadingLevel}.TextColor"]; set => this[$"{_selectedHeadingLevel}.TextColor"] = value; }
        public string? HeadingFontSize { get => this[$"{_selectedHeadingLevel}.FontSize"]; set => this[$"{_selectedHeadingLevel}.FontSize"] = value; }
        public string? HeadingFontFamily { get => this[$"{_selectedHeadingLevel}.FontFamily"]; set => this[$"{_selectedHeadingLevel}.FontFamily"] = value; }
        public bool IsHeadingBold { get => GetFlag("IsBold"); set => SetFlag("IsBold", value); }
        public bool IsHeadingItalic { get => GetFlag("IsItalic"); set => SetFlag("IsItalic", value); }
        public bool IsHeadingUnderline { get => GetFlag("IsUnderline"); set => SetFlag("IsUnderline", value); }
        public bool IsHeadingStrikethrough { get => GetFlag("IsStrikethrough"); set => SetFlag("IsStrikethrough", value); }
        public bool IsHeadingNumberingEnabled { get => GetFlag("IsNumberingEnabled"); set => SetFlag("IsNumberingEnabled", value); }

        private bool GetFlag(string attr) => _selectedHeadingLevel != null && _flags.TryGetValue($"{_selectedHeadingLevel}.{attr}", out var b) && b;
        private void SetFlag(string attr, bool value) { if (_selectedHeadingLevel == null) return; var key = $"{_selectedHeadingLevel}.{attr}"; if (!_flags.TryGetValue(key, out var current) || current != value) { _flags[key] = value; OnPropertyChanged($"IsHeading{attr}"); } }
        public string? SelectedHeadingLevel { get => _selectedHeadingLevel; set { if (_selectedHeadingLevel != value) { _selectedHeadingLevel = value; OnPropertyChanged(); UpdateHeadingProperties(); } } }
        private void UpdateHeadingProperties() { OnPropertyChanged(nameof(HeadingTextColor)); OnPropertyChanged(nameof(HeadingFontSize)); OnPropertyChanged(nameof(HeadingFontFamily)); OnPropertyChanged(nameof(IsHeadingBold)); OnPropertyChanged(nameof(IsHeadingItalic)); OnPropertyChanged(nameof(IsHeadingUnderline)); OnPropertyChanged(nameof(IsHeadingStrikethrough)); OnPropertyChanged(nameof(IsHeadingNumberingEnabled)); }

        private void ExecuteSaveCss(object? parameter)
        {
            if (string.IsNullOrEmpty(TargetCssFileName)) return;
            var styleInfo = new CssStyleInfo();
            var type = typeof(CssStyleInfo);
            foreach (var name in StylePropertyNames)
            {
                var prop = type.GetProperty(name);
                if (prop != null && _styles.TryGetValue(name, out var val))
                    prop.SetValue(styleInfo, AddUnit(name, val));
            }
            foreach (var lv in AvailableHeadingLevels)
            {
                if (_styles.TryGetValue($"{lv}.TextColor", out var c) && c != null) styleInfo.HeadingTextColors[lv] = c;
                if (_styles.TryGetValue($"{lv}.FontSize", out var s) && s != null) styleInfo.HeadingFontSizes[lv] = s + GlobalUnit;
                if (_styles.TryGetValue($"{lv}.FontFamily", out var f) && f != null) styleInfo.HeadingFontFamilies[lv] = f;
                styleInfo.HeadingStyleFlags[lv] = new HeadingStyleFlags { IsBold = _flags.TryGetValue($"{lv}.IsBold", out var b) && b, IsItalic = _flags.TryGetValue($"{lv}.IsItalic", out var i) && i, IsUnderline = _flags.TryGetValue($"{lv}.IsUnderline", out var u) && u, IsStrikethrough = _flags.TryGetValue($"{lv}.IsStrikethrough", out var st) && st };
                if (_flags.TryGetValue($"{lv}.IsNumberingEnabled", out var n)) styleInfo.HeadingNumberingStates[lv] = n;
            }
            _cssManagementService.SaveStyle(TargetCssFileName, styleInfo);
            CssSaved?.Invoke(this, EventArgs.Empty);
        }

        private string? AddUnit(string propertyName, string? value)
        {
            if (string.IsNullOrEmpty(value)) return value;
            if (AutoConvertPropertyNames.Contains(propertyName)) return value + GlobalUnit;
            if (PxFixedPropertyNames.Contains(propertyName)) return value + "px";
            return value;
        }
    }
}
