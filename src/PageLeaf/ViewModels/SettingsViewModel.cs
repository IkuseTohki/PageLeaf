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
        /// 保存コマンド。
        /// </summary>
        public ICommand SaveCommand { get; }

        /// <summary>
        /// キャンセルコマンド。
        /// </summary>
        public ICommand CancelCommand { get; }

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

            // テーマ一覧の取得
            LoadAvailableThemes();

            SaveCommand = new DelegateCommand((o) => Save());
            CancelCommand = new DelegateCommand((o) => RequestClose?.Invoke(this, EventArgs.Empty));
        }

        private void LoadAvailableThemes()
        {
            try
            {
                // 実行ディレクトリからの相対パスでテーマを探す
                // 開発環境と実行環境の両方を考慮
                var baseDir = AppDomain.CurrentDomain.BaseDirectory;
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

            _settingsService.SaveSettings(settings);
            RequestClose?.Invoke(this, EventArgs.Empty);
        }
    }
}
