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
    /// <summary>
    /// CSSスタイルの編集を管理するViewModel。
    /// プロパティへの直接アクセスに加え、インデクサによる動的なアクセスをサポートします。
    /// </summary>
    public class CssEditorViewModel : ViewModelBase
    {
        private readonly ICssManagementService _cssManagementService;

        // 文字列値（Color, FontSize, FontFamily等）を保持する辞書
        private readonly Dictionary<string, string?> _styles = new Dictionary<string, string?>();

        // フラグ値（Bold, Italic等）を保持する辞書。キーは "h1.IsBold" の形式。
        private readonly Dictionary<string, bool> _flags = new Dictionary<string, bool>();

        // スタイル属性のメタデータ（単一文字列プロパティ）
        private static readonly string[] StylePropertyNames = new[]
        {
            "BodyTextColor", "BodyBackgroundColor", "BodyFontSize",
            "QuoteTextColor", "QuoteBackgroundColor", "QuoteBorderColor",
            "QuoteBorderWidth", "QuoteBorderStyle",
            "TableBorderColor", "TableHeaderBackgroundColor", "TableBorderWidth", "TableCellPadding",
            "CodeTextColor", "CodeBackgroundColor", "CodeFontFamily",
            "ListMarkerType", "ListIndent"
        };

        // 見出し用の属性名
        private static readonly string[] HeadingAttributes = new[] { "TextColor", "FontSize", "FontFamily" };
        private static readonly string[] HeadingFlags = new[] { "IsBold", "IsItalic", "IsUnderline", "IsStrikethrough", "IsNumberingEnabled" };

        private string? _selectedHeadingLevel;
        public event EventHandler? CssSaved;
        public ICommand SaveCssCommand { get; }
        public ObservableCollection<string> AvailableHeadingLevels { get; }
        public string? TargetCssFileName { get; private set; }

        /// <summary>
        /// 文字列キーでスタイル値を取得または設定します。
        /// キー形式: "BodyTextColor" または "h1.TextColor"
        /// </summary>
        public string? this[string key]
        {
            get => GetStyleValue(key);
            set => SetStyleValue(key, value);
        }

        public CssEditorViewModel(ICssManagementService cssManagementService)
        {
            ArgumentNullException.ThrowIfNull(cssManagementService);
            _cssManagementService = cssManagementService;
            SaveCssCommand = new DelegateCommand(ExecuteSaveCss);

            AvailableHeadingLevels = new ObservableCollection<string>(Enumerable.Range(1, 6).Select(i => $"h{i}"));
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
            var type = typeof(CssStyleInfo);
            // 単一プロパティのロード
            foreach (var name in StylePropertyNames)
            {
                var prop = type.GetProperty(name);
                if (prop != null) _styles[name] = prop.GetValue(styleInfo) as string;
            }

            // 見出し辞書の展開
            foreach (var level in AvailableHeadingLevels)
            {
                _styles[$"{level}.TextColor"] = styleInfo.HeadingTextColors.TryGetValue(level, out var c) ? c : null;
                _styles[$"{level}.FontSize"] = styleInfo.HeadingFontSizes.TryGetValue(level, out var s) ? s : null;
                _styles[$"{level}.FontFamily"] = styleInfo.HeadingFontFamilies.TryGetValue(level, out var f) ? f : null;

                if (styleInfo.HeadingStyleFlags.TryGetValue(level, out var flags))
                {
                    _flags[$"{level}.IsBold"] = flags.IsBold;
                    _flags[$"{level}.IsItalic"] = flags.IsItalic;
                    _flags[$"{level}.IsUnderline"] = flags.IsUnderline;
                    _flags[$"{level}.IsStrikethrough"] = flags.IsStrikethrough;
                }
                _flags[$"{level}.IsNumberingEnabled"] = styleInfo.HeadingNumberingStates.TryGetValue(level, out var n) && n;
            }

            UpdateHeadingProperties();
            OnPropertyChanged(string.Empty); // 全プロパティの更新通知
        }

        private string? GetStyleValue(string key) => _styles.TryGetValue(key, out var value) ? value : null;

        private void SetStyleValue(string key, string? value)
        {
            if (!_styles.TryGetValue(key, out var current) || current != value)
            {
                _styles[key] = value;
                OnPropertyChanged("Item[]"); // インデクサ全体の変更通知

                // 現在選択中の見出しレベルと同期している場合の通知
                if (_selectedHeadingLevel != null && key.StartsWith(_selectedHeadingLevel))
                {
                    var attr = key.Substring(_selectedHeadingLevel.Length + 1);
                    OnPropertyChanged($"Heading{attr}");
                }
                else
                {
                    OnPropertyChanged(key);
                }
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

        // 現在選択中の見出しレベルに対するエイリアスプロパティ
        public string? HeadingTextColor { get => this[$"{_selectedHeadingLevel}.TextColor"]; set => this[$"{_selectedHeadingLevel}.TextColor"] = value; }
        public string? HeadingFontSize { get => this[$"{_selectedHeadingLevel}.FontSize"]; set => this[$"{_selectedHeadingLevel}.FontSize"] = value; }
        public string? HeadingFontFamily { get => this[$"{_selectedHeadingLevel}.FontFamily"]; set => this[$"{_selectedHeadingLevel}.FontFamily"] = value; }

        public bool IsHeadingBold { get => GetFlag("IsBold"); set => SetFlag("IsBold", value); }
        public bool IsHeadingItalic { get => GetFlag("IsItalic"); set => SetFlag("IsItalic", value); }
        public bool IsHeadingUnderline { get => GetFlag("IsUnderline"); set => SetFlag("IsUnderline", value); }
        public bool IsHeadingStrikethrough { get => GetFlag("IsStrikethrough"); set => SetFlag("IsStrikethrough", value); }
        public bool IsHeadingNumberingEnabled { get => GetFlag("IsNumberingEnabled"); set => SetFlag("IsNumberingEnabled", value); }

        private bool GetFlag(string attr) => _selectedHeadingLevel != null && _flags.TryGetValue($"{_selectedHeadingLevel}.{attr}", out var b) && b;
        private void SetFlag(string attr, bool value)
        {
            if (_selectedHeadingLevel == null) return;
            var key = $"{_selectedHeadingLevel}.{attr}";
            if (!_flags.TryGetValue(key, out var current) || current != value)
            {
                _flags[key] = value;
                OnPropertyChanged($"IsHeading{attr}");
            }
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
            OnPropertyChanged(nameof(HeadingTextColor));
            OnPropertyChanged(nameof(HeadingFontSize));
            OnPropertyChanged(nameof(HeadingFontFamily));
            OnPropertyChanged(nameof(IsHeadingBold));
            OnPropertyChanged(nameof(IsHeadingItalic));
            OnPropertyChanged(nameof(IsHeadingUnderline));
            OnPropertyChanged(nameof(IsHeadingStrikethrough));
            OnPropertyChanged(nameof(IsHeadingNumberingEnabled));
        }

        private void ExecuteSaveCss(object? parameter)
        {
            if (string.IsNullOrEmpty(TargetCssFileName)) return;
            var styleInfo = new CssStyleInfo();
            var type = typeof(CssStyleInfo);

            foreach (var name in StylePropertyNames)
            {
                var prop = type.GetProperty(name);
                if (prop != null && _styles.TryGetValue(name, out var val)) prop.SetValue(styleInfo, val);
            }

            foreach (var lv in AvailableHeadingLevels)
            {
                if (_styles.TryGetValue($"{lv}.TextColor", out var c) && c != null) styleInfo.HeadingTextColors[lv] = c;
                if (_styles.TryGetValue($"{lv}.FontSize", out var s) && s != null) styleInfo.HeadingFontSizes[lv] = s;
                if (_styles.TryGetValue($"{lv}.FontFamily", out var f) && f != null) styleInfo.HeadingFontFamilies[lv] = f;

                styleInfo.HeadingStyleFlags[lv] = new HeadingStyleFlags
                {
                    IsBold = _flags.TryGetValue($"{lv}.IsBold", out var b) && b,
                    IsItalic = _flags.TryGetValue($"{lv}.IsItalic", out var i) && i,
                    IsUnderline = _flags.TryGetValue($"{lv}.IsUnderline", out var u) && u,
                    IsStrikethrough = _flags.TryGetValue($"{lv}.IsStrikethrough", out var st) && st
                };
                if (_flags.TryGetValue($"{lv}.IsNumberingEnabled", out var n)) styleInfo.HeadingNumberingStates[lv] = n;
            }

            _cssManagementService.SaveStyle(TargetCssFileName, styleInfo);
            CssSaved?.Invoke(this, EventArgs.Empty);
        }
    }
}
