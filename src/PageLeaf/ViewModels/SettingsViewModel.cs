using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using PageLeaf.Models;
using PageLeaf.Models.Markdown;
using PageLeaf.Models.Css;
using PageLeaf.Models.Css.Elements;
using PageLeaf.Models.Settings;
using PageLeaf.Services;
using PageLeaf.Utilities;

namespace PageLeaf.ViewModels
{
    /// <summary>
    /// 設定のカテゴリを定義します。
    /// </summary>
    public enum SettingsCategory
    {
        Appearance,
        Editor,
        Image,
        Code,
        Diagnostic
    }

    /// <summary>
    /// アプリケーション設定画面のビューモデルです。
    /// </summary>
    public class SettingsViewModel : ViewModelBase
    {
        private readonly ISettingsService _settingsService;
        private string _selectedCodeBlockTheme;
        private bool _useCustomCodeBlockStyle;
        private string _imageSaveDirectory = "images";
        private string _imageFileNameTemplate = "image_{Date}_{Time}";
        private int _indentSize = 4;
        private bool _useSpacesForIndent = true;
        private double _editorFontSize = 14.0;
        private bool _autoInsertFrontMatter = true;
        private bool _showTitleInPreview = true;
        private bool _renumberFootnotesOnSave = true;
        private ResourceSource _libraryResourceSource = ResourceSource.Local;
        private AppTheme _theme = AppTheme.System;
        private LogOutputLevel _minimumLogLevel = LogOutputLevel.Standard;
        private bool _enableFileLogging = true;
        private SettingsCategory _currentCategory = SettingsCategory.Appearance;

        /// <summary>
        /// 現在選択されているカテゴリ。
        /// </summary>
        public SettingsCategory CurrentCategory
        {
            get => _currentCategory;
            set { if (_currentCategory != value) { _currentCategory = value; OnPropertyChanged(); } }
        }

        /// <summary>
        /// アプリケーションの表示テーマ。
        /// </summary>
        public AppTheme Theme
        {
            get => _theme;
            set { if (_theme != value) { _theme = value; OnPropertyChanged(); } }
        }

        /// <summary>
        /// ログ出力レベル。
        /// </summary>
        public LogOutputLevel MinimumLogLevel
        {
            get => _minimumLogLevel;
            set { if (_minimumLogLevel != value) { _minimumLogLevel = value; OnPropertyChanged(); } }
        }

        /// <summary>
        /// ファイルへのログ出力を有効にするかどうか。
        /// </summary>
        public bool EnableFileLogging
        {
            get => _enableFileLogging;
            set { if (_enableFileLogging != value) { _enableFileLogging = value; OnPropertyChanged(); } }
        }

        /// <summary>
        /// 利用可能なログレベルのリスト。
        /// </summary>
        public ObservableCollection<LogOutputLevel> AvailableLogLevels { get; } = new ObservableCollection<LogOutputLevel>((LogOutputLevel[])Enum.GetValues(typeof(LogOutputLevel)));

        /// <summary>
        /// プレビューの最上部にフロントマターのタイトルを表示するかどうか。
        /// </summary>
        public bool ShowTitleInPreview
        {
            get => _showTitleInPreview;
            set { if (_showTitleInPreview != value) { _showTitleInPreview = value; OnPropertyChanged(); } }
        }

        /// <summary>
        /// 保存時に脚注番号を振り直すかどうか。
        /// </summary>
        public bool RenumberFootnotesOnSave
        {
            get => _renumberFootnotesOnSave;
            set { if (_renumberFootnotesOnSave != value) { _renumberFootnotesOnSave = value; OnPropertyChanged(); } }
        }

        /// <summary>
        /// エディタのフォントサイズ。
        /// </summary>
        public double EditorFontSize
        {
            get => _editorFontSize;
            set
            {
                var safeValue = Math.Clamp(value, 1.0, 100.0);
                if (_editorFontSize != safeValue) { _editorFontSize = safeValue; OnPropertyChanged(); }
            }
        }

        /// <summary>
        /// ライブラリ（Highlight.js, Mermaid）の読み込み先。
        /// </summary>
        public ResourceSource LibraryResourceSource
        {
            get => _libraryResourceSource;
            set
            {
                if (_libraryResourceSource != value)
                {
                    _libraryResourceSource = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(IsCdnEnabled));
                }
            }
        }

        /// <summary>
        /// CDNが有効かどうか（UIのスイッチ用）。
        /// </summary>
        public bool IsCdnEnabled
        {
            get => LibraryResourceSource == ResourceSource.Cdn;
            set => LibraryResourceSource = value ? ResourceSource.Cdn : ResourceSource.Local;
        }

        /// <summary>
        /// 利用可能なコードブロックテーマのリスト。
        /// </summary>
        public ObservableCollection<string> AvailableThemes { get; } = new ObservableCollection<string>();

        /// <summary>
        /// 選択されたコードブロックテーマ。
        /// </summary>
        public string SelectedCodeBlockTheme
        {
            get => _selectedCodeBlockTheme;
            set { if (_selectedCodeBlockTheme != value) { _selectedCodeBlockTheme = value; OnPropertyChanged(); } }
        }

        /// <summary>
        /// カスタムスタイルを使用してテーマを上書きするかどうか。
        /// </summary>
        public bool UseCustomCodeBlockStyle
        {
            get => _useCustomCodeBlockStyle;
            set { if (_useCustomCodeBlockStyle != value) { _useCustomCodeBlockStyle = value; OnPropertyChanged(); } }
        }

        /// <summary>
        /// 画像を保存するディレクトリ（Markdownファイルからの相対パス）。
        /// </summary>
        public string ImageSaveDirectory
        {
            get => _imageSaveDirectory;
            set { if (_imageSaveDirectory != value) { _imageSaveDirectory = value; OnPropertyChanged(); } }
        }

        /// <summary>
        /// 画像のファイル名テンプレート。
        /// </summary>
        public string ImageFileNameTemplate
        {
            get => _imageFileNameTemplate;
            set { if (_imageFileNameTemplate != value) { _imageFileNameTemplate = value; OnPropertyChanged(); } }
        }

        /// <summary>
        /// インデント幅（スペース数）。
        /// </summary>
        public int IndentSize
        {
            get => _indentSize;
            set
            {
                var safeValue = Math.Clamp(value, 1, 32);
                if (_indentSize != safeValue) { _indentSize = safeValue; OnPropertyChanged(); }
            }
        }

        /// <summary>
        /// タブの代わりにスペースを使用するかどうか。
        /// </summary>
        public bool UseSpacesForIndent
        {
            get => _useSpacesForIndent;
            set { if (_useSpacesForIndent != value) { _useSpacesForIndent = value; OnPropertyChanged(); } }
        }

        /// <summary>
        /// 新規作成時にフロントマターを自動挿入するかどうか。
        /// </summary>
        public bool AutoInsertFrontMatter
        {
            get => _autoInsertFrontMatter;
            set { if (_autoInsertFrontMatter != value) { _autoInsertFrontMatter = value; OnPropertyChanged(); } }
        }

        /// <summary>
        /// 自動挿入されるフロントマターのプロパティリスト。
        /// </summary>
        public ObservableCollection<FrontMatterProperty> DefaultFrontMatterProperties { get; } = new ObservableCollection<FrontMatterProperty>();

        /// <summary>
        /// 保存コマンド。
        /// </summary>
        public ICommand SaveCommand { get; }

        /// <summary>
        /// キャンセルコマンド。
        /// </summary>
        public ICommand CancelCommand { get; }

        /// <summary>
        /// フロントマタープロパティを追加するコマンド。
        /// </summary>
        public ICommand AddFrontMatterPropertyCommand { get; }

        /// <summary>
        /// フロントマタープロパティを削除するコマンド。
        /// </summary>
        public ICommand RemoveFrontMatterPropertyCommand { get; }

        /// <summary>
        /// 設定が保存されたときに発生するイベント。
        /// </summary>
        public event EventHandler? RequestClose;

        public SettingsViewModel(ISettingsService settingsService)
        {
            _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));

            // 現在の設定をロード
            var settings = _settingsService.CurrentSettings;
            _selectedCodeBlockTheme = settings.View.CodeBlockTheme;
            _useCustomCodeBlockStyle = settings.View.UseCustomCodeBlockStyle;
            _imageSaveDirectory = settings.Image.SaveDirectory;
            _imageFileNameTemplate = settings.Image.FileNameTemplate;
            _indentSize = Math.Clamp(settings.Editor.IndentSize, 1, 32);
            _useSpacesForIndent = settings.Editor.UseSpacesForIndent;
            _editorFontSize = Math.Clamp(settings.Editor.EditorFontSize, 1.0, 100.0);
            _autoInsertFrontMatter = settings.Editor.AutoInsertFrontMatter;
            _showTitleInPreview = settings.View.ShowTitleInPreview;
            _renumberFootnotesOnSave = settings.Editor.RenumberFootnotesOnSave;
            _libraryResourceSource = settings.Appearance.LibraryResourceSource;
            _theme = settings.Appearance.Theme;
            _minimumLogLevel = settings.Logging.MinimumLevel;
            _enableFileLogging = settings.Logging.EnableFileLogging;

            // 追加フロントマタープロパティのロード
            DefaultFrontMatterProperties.Clear();
            if (settings.Editor.AdditionalFrontMatter != null)
            {
                foreach (var prop in settings.Editor.AdditionalFrontMatter)
                {
                    DefaultFrontMatterProperties.Add(new FrontMatterProperty { Key = prop.Key, Value = prop.Value });
                }
            }

            // テーマ一覧の取得
            LoadAvailableThemes();

            SaveCommand = new DelegateCommand((o) => Save());
            CancelCommand = new DelegateCommand((o) => RequestClose?.Invoke(this, EventArgs.Empty));
            AddFrontMatterPropertyCommand = new DelegateCommand((o) => AddFrontMatterProperty());
            RemoveFrontMatterPropertyCommand = new DelegateCommand((o) => RemoveFrontMatterProperty(o));
        }

        private void AddFrontMatterProperty()
        {
            string baseKey = "new_property";
            string key = baseKey;
            int count = 1;
            while (DefaultFrontMatterProperties.Any(p => p.Key == key))
            {
                key = $"{baseKey}_{count++}";
            }
            DefaultFrontMatterProperties.Add(new FrontMatterProperty { Key = key, Value = "" });
        }

        private void RemoveFrontMatterProperty(object? parameter)
        {
            if (parameter is FrontMatterProperty property)
            {
                DefaultFrontMatterProperties.Remove(property);
            }
        }

        private void LoadAvailableThemes()
        {
            try
            {
                // 実行ディレクトリまたは一時フォルダからテーマを探す
                var baseDir = App.BaseDirectory;
                var tempDir = App.AppInternalTempDirectory;
                var themesDir = Path.Combine(baseDir, "highlight", "styles");

                // exeフォルダになければ一時フォルダを確認
                if (!Directory.Exists(themesDir))
                {
                    themesDir = Path.Combine(tempDir, "highlight", "styles");
                }

                if (!Directory.Exists(themesDir))
                {
                    // どこにもなければデフォルト
                    AvailableThemes.Clear();
                    AvailableThemes.Add("github.css");
                    SelectedCodeBlockTheme = "github.css";
                    return;
                }

                var files = Directory.GetFiles(themesDir, "*.css")
                    .Select(Path.GetFileName)
                    .OfType<string>()
                    .Where(f => !f.EndsWith(".min.css"))
                    .OrderBy(f => f);

                AvailableThemes.Clear();
                foreach (var file in files)
                {
                    AvailableThemes.Add(file);
                }

                // 現在の選択がリストにない場合はデフォルトに
                if (!AvailableThemes.Contains(SelectedCodeBlockTheme))
                {
                    if (AvailableThemes.Contains("github.css")) SelectedCodeBlockTheme = "github.css";
                    else if (AvailableThemes.Any()) SelectedCodeBlockTheme = AvailableThemes.First();
                }
            }
            catch (Exception)
            {
                // エラー時は最低限のリスト
                AvailableThemes.Add("github.css");
                SelectedCodeBlockTheme = "github.css";
            }
        }

        private void Save()
        {
            var settings = _settingsService.CurrentSettings;

            settings.View.CodeBlockTheme = SelectedCodeBlockTheme;
            settings.View.UseCustomCodeBlockStyle = UseCustomCodeBlockStyle;
            settings.Image.SaveDirectory = ImageSaveDirectory;
            settings.Image.FileNameTemplate = ImageFileNameTemplate;
            settings.Editor.IndentSize = IndentSize;
            settings.Editor.UseSpacesForIndent = UseSpacesForIndent;
            settings.Editor.EditorFontSize = EditorFontSize;
            settings.Editor.AutoInsertFrontMatter = AutoInsertFrontMatter;
            settings.View.ShowTitleInPreview = ShowTitleInPreview;
            settings.Editor.RenumberFootnotesOnSave = RenumberFootnotesOnSave;
            settings.Appearance.LibraryResourceSource = LibraryResourceSource;
            settings.Appearance.Theme = Theme;
            settings.Logging.MinimumLevel = MinimumLogLevel;
            settings.Logging.EnableFileLogging = EnableFileLogging;

            // 追加フロントマタープロパティの保存 (順序維持)
            settings.Editor.AdditionalFrontMatter = DefaultFrontMatterProperties
                .Where(p => !string.IsNullOrWhiteSpace(p.Key))
                .Select(p => new FrontMatterAdditionalProperty { Key = p.Key, Value = p.Value?.ToString() ?? "" })
                .ToList();

            _settingsService.SaveSettings(settings);
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
