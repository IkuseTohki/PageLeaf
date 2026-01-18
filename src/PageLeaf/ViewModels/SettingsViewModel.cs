using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using PageLeaf.Models;
using PageLeaf.Services;
using PageLeaf.Utilities;

namespace PageLeaf.ViewModels
{
    /// <summary>
    /// 設定のカテゴリを定義します。
    /// </summary>
    public enum SettingsCategory
    {
        Editor,
        Image,
        Code
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
        private ResourceSource _libraryResourceSource = ResourceSource.Local;
        private SettingsCategory _currentCategory = SettingsCategory.Editor;

        /// <summary>
        /// 現在選択されているカテゴリ。
        /// </summary>
        public SettingsCategory CurrentCategory
        {
            get => _currentCategory;
            set { if (_currentCategory != value) { _currentCategory = value; OnPropertyChanged(); } }
        }

        /// <summary>
        /// エディタのフォントサイズ。
        /// </summary>
        public double EditorFontSize
        {
            get => _editorFontSize;
            set { if (_editorFontSize != value) { _editorFontSize = value; OnPropertyChanged(); } }
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
            set { if (_indentSize != value) { _indentSize = value; OnPropertyChanged(); } }
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
            _selectedCodeBlockTheme = settings.CodeBlockTheme;
            _useCustomCodeBlockStyle = settings.UseCustomCodeBlockStyle;
            _imageSaveDirectory = settings.ImageSaveDirectory;
            _imageFileNameTemplate = settings.ImageFileNameTemplate;
            _indentSize = settings.IndentSize;
            _useSpacesForIndent = settings.UseSpacesForIndent;
            _editorFontSize = settings.EditorFontSize;
            _autoInsertFrontMatter = settings.AutoInsertFrontMatter;
            _libraryResourceSource = settings.LibraryResourceSource;

            // 追加フロントマタープロパティのロード
            DefaultFrontMatterProperties.Clear();
            if (settings.AdditionalFrontMatter != null)
            {
                foreach (var prop in settings.AdditionalFrontMatter)
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
                // 実行ディレクトリからの相対パスでテーマを探す
                // 開発環境と実行環境の両方を考慮
                var baseDir = App.BaseDirectory;
                var themesDir = Path.Combine(baseDir, "highlight", "styles");

                if (!Directory.Exists(themesDir))
                {
                    // 開発環境用のフォールバック（src/PageLeaf/Resources/...）
                    // 実際にはビルド時にコピーされるため、通常は上のパスで見つかるはず
                    _selectedCodeBlockTheme = "github.css";
                    AvailableThemes.Add("github.css");
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

                // 現在の選択がリストにない場合は追加（またはデフォルトに）
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
            settings.CodeBlockTheme = SelectedCodeBlockTheme;
            settings.UseCustomCodeBlockStyle = UseCustomCodeBlockStyle;
            settings.ImageSaveDirectory = ImageSaveDirectory;
            settings.ImageFileNameTemplate = ImageFileNameTemplate;
            settings.IndentSize = IndentSize;
            settings.UseSpacesForIndent = UseSpacesForIndent;
            settings.EditorFontSize = EditorFontSize;
            settings.AutoInsertFrontMatter = AutoInsertFrontMatter;
            settings.LibraryResourceSource = LibraryResourceSource;

            // 追加フロントマタープロパティの保存 (順序維持)
            settings.AdditionalFrontMatter = DefaultFrontMatterProperties
                .Where(p => !string.IsNullOrWhiteSpace(p.Key))
                .Select(p => new FrontMatterAdditionalProperty { Key = p.Key, Value = p.Value?.ToString() ?? "" })
                .ToList();

            _settingsService.SaveSettings(settings);
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
