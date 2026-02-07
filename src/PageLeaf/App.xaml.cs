using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PageLeaf.Infrastructure.Logging;
using PageLeaf.Models.Settings;
using PageLeaf.Services;
using PageLeaf.UseCases;
using PageLeaf.ViewModels;
using PageLeaf.Views;
using Serilog;
using System;
using System.Diagnostics;
using System.IO;
using System.Windows;
using AngleSharp.Css.Values;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using Microsoft.Win32;

namespace PageLeaf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ISettingsService? _settingsService;
        private LoggingBootstrapper? _loggingBootstrapper;

        [DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

        private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

        /// <summary>
        /// アプリケーションのベースディレクトリを取得します。
        /// SingleFile発行時は実行ファイルの場所、それ以外は AppDomain.CurrentDomain.BaseDirectory を返します。
        /// </summary>
        public static string BaseDirectory
        {
            get
            {
                // .NET Core 3.1 互換のため、Environment.ProcessPath の代わりに Process.GetCurrentProcess().MainModule.FileName を使用
                string? processPath = null;
                try
                {
                    using (var process = Process.GetCurrentProcess())
                    {
                        processPath = process.MainModule?.FileName;
                    }
                }
                catch
                {
                    // アクセス拒否などの例外時はフォールバック
                }

                if (!string.IsNullOrEmpty(processPath))
                {
                    return Path.GetDirectoryName(processPath)!;
                }
                return AppDomain.CurrentDomain.BaseDirectory;
            }
        }

        /// <summary>
        /// アプリケーション専用の一時ディレクトリパスを取得します。
        /// </summary>
        public static string AppInternalTempDirectory => Path.Combine(Path.GetTempPath(), "PageLeaf", "v1.1.23");

        /// <summary>
        /// DIコンテナやロギングなどのアプリケーションサービスをホストします。
        /// </summary>
        public static IHost? AppHost { get; private set; }

        /// <summary>
        /// 埋め込みリソースを物理ファイルとして展開します。
        /// </summary>
        private void InitializeResources()
        {
            var extractionService = new ResourceExtractionService(typeof(App).Assembly);
            extractionService.ExtractAll(BaseDirectory, AppInternalTempDirectory);
        }

        /// <summary>
        /// アプリケーションの起動時に呼び出されます。
        /// </summary>
        /// <param name="e">スタートアップイベントのデータ。</param>
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // リソースの初期化（SingleFile対応）
            InitializeResources();

            // ロギング設定の先行読み込み（循環依存回避のため Bootstrapper に委譲）
            _loggingBootstrapper = new LoggingBootstrapper(BaseDirectory);
            _loggingBootstrapper.LoadInitialSettings();

            // AngleSharpが色をHEX形式で出力するように設定
            Color.UseHex = true;

            // DIコンテナとロギングを設定
            AppHost = Host.CreateDefaultBuilder()
                .UseSerilog((hostContext, services, configuration) =>
                {
                    // ログファイルのパスを設定
                    var logPath = Path.Combine(BaseDirectory, "logs", "PageLeaf-.txt");

                    configuration
                        .MinimumLevel.ControlledBy(_loggingBootstrapper.LevelSwitch)
                        .Enrich.FromLogContext()
                        .WriteTo.Conditional(
                            evt => _loggingBootstrapper.EnableFileLogging,
                            wt => wt.File(
                                logPath,
                                rollingInterval: RollingInterval.Day,
                                retainedFileCountLimit: 7,
                                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                            )
                        );
                })
                .ConfigureServices((hostContext, services) =>
                {
                    // Services をDIコンテナに登録
                    services.AddSingleton<IResourceExtractionService>(sp => new ResourceExtractionService(typeof(App).Assembly));
                    services.AddSingleton<IFileService, FileService>();
                    services.AddSingleton<ICssService, CssService>();
                    services.AddSingleton<ISettingsService>(sp => new SettingsService(sp.GetRequiredService<ILogger<SettingsService>>(), App.BaseDirectory));
                    services.AddSingleton<ISystemThemeProvider, SystemThemeProvider>();
                    services.AddSingleton<IThemeService, ThemeService>();
                    services.AddSingleton<IDialogService, DialogService>();
                    services.AddSingleton<IMarkdownService, MarkdownService>();
                    services.AddSingleton<IEditorService, EditorService>();
                    services.AddSingleton<ICssEditorService, CssEditorService>();
                    services.AddSingleton<ICssManagementService, CssManagementService>();
                    services.AddSingleton<IImagePasteService, ImagePasteService>();
                    services.AddSingleton<IEditingSupportService, EditingSupportService>();
                    services.AddSingleton<IWindowService, WindowService>();

                    // UseCases
                    services.AddTransient<ISaveAsDocumentUseCase, SaveAsDocumentUseCase>();
                    services.AddTransient<ISaveDocumentUseCase, SaveDocumentUseCase>();
                    services.AddTransient<INewDocumentUseCase, NewDocumentUseCase>();
                    services.AddTransient<IOpenDocumentUseCase, OpenDocumentUseCase>();
                    services.AddTransient<ILoadCssUseCase, LoadCssUseCase>();
                    services.AddTransient<ISaveCssUseCase, SaveCssUseCase>();
                    services.AddTransient<IPasteImageUseCase, PasteImageUseCase>();

                    // ViewModels と Views をDIコンテナに登録
                    services.AddTransient<SettingsViewModel>();
                    services.AddTransient<CheatSheetViewModel>();
                    services.AddTransient<AboutViewModel>();
                    services.AddSingleton<CssEditorViewModel>();
                    services.AddSingleton<MainViewModel>();
                    services.AddSingleton<MainWindow>();
                })
                .Build();

            // グローバル例外ハンドリングを設定
            SetupGlobalExceptionHandling();

            try
            {
                await AppHost.StartAsync();

                var settingsService = AppHost.Services.GetRequiredService<ISettingsService>();
                var themeService = AppHost.Services.GetRequiredService<IThemeService>();

                _settingsService = settingsService;

                // OSのテーマ変更（ライト/ダーク）を監視
                SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;

                // 先に MainWindow を生成（DI経由）
                var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();

                // 設定の初期適用
                ApplyTheme(settingsService.CurrentSettings.Appearance.Theme);
                _loggingBootstrapper.UpdateFromSettings(settingsService.CurrentSettings.Logging);

                // 設定変更時の適用
                settingsService.SettingsChanged += (s, settings) =>
                {
                    ApplyTheme(settings.Appearance.Theme);
                    _loggingBootstrapper.UpdateFromSettings(settings.Logging);
                };

                mainWindow.Show();
            }
            catch (Exception ex)
            {
                // 起動時の例外を捕捉して通知
                HandleFatalException(ex);
            }
        }

        /// <summary>
        /// アプリケーションの終了時に呼び出されます。
        /// </summary>
        /// <param name="e">終了イベントのデータ。</param>
        protected override async void OnExit(ExitEventArgs e)
        {
            SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;

            if (AppHost != null)
            {
                await AppHost.StopAsync();
                AppHost.Dispose();
            }

            base.OnExit(e);
        }

        /// <summary>
        /// Windowsの設定（テーマなど）が変更された際に呼び出されます。
        /// </summary>
        private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
        {
            if (e.Category == UserPreferenceCategory.General || e.Category == UserPreferenceCategory.VisualStyle)
            {
                if (_settingsService?.CurrentSettings.Appearance.Theme == Models.AppTheme.System)
                {
                    // UIスレッドで実行
                    Dispatcher.Invoke(() => ApplyTheme(Models.AppTheme.System));
                }
            }
        }

        /// <summary>
        /// 指定されたテーマをアプリケーションに適用します。
        /// </summary>
        /// <param name="theme">適用するテーマ。</param>
        private void ApplyTheme(Models.AppTheme theme)
        {
            var themeService = AppHost!.Services.GetRequiredService<IThemeService>();
            var actualTheme = themeService.GetActualTheme();

            var themeUri = actualTheme == Models.AppTheme.Dark
                ? new Uri("Resources/DarkColors.xaml", UriKind.Relative)
                : new Uri("Resources/LightColors.xaml", UriKind.Relative);

            try
            {
                // MergedDictionariesの最初のリソース（ThemeColors）を差し替える
                var dictionaries = Application.Current.Resources.MergedDictionaries;
                if (dictionaries.Count > 0)
                {
                    dictionaries[0] = new ResourceDictionary { Source = themeUri };
                }

                // タイトルバーのダークモード対応 (Windows 11)
                UpdateTitleBarTheme(actualTheme == Models.AppTheme.Dark);
            }
            catch (Exception ex)
            {
                var logger = AppHost?.Services.GetService<ILogger<App>>();
                logger?.LogError(ex, "Failed to apply theme: {ThemeUri}", themeUri);
            }
        }

        /// <summary>
        /// ウィンドウのタイトルバーのテーマを更新します。
        /// </summary>
        private void UpdateTitleBarTheme(bool isDark)
        {
            if (MainWindow == null) return;

            var helper = new WindowInteropHelper(MainWindow);
            var hwnd = helper.Handle;

            if (hwnd != IntPtr.Zero)
            {
                int useImmersiveDarkMode = isDark ? 1 : 0;
                DwmSetWindowAttribute(hwnd, DWMWA_USE_IMMERSIVE_DARK_MODE, ref useImmersiveDarkMode, sizeof(int));
            }
            else
            {
                // ウィンドウハンドルが未作成の場合は、作成完了後に一度だけ適用するイベントを登録
                // += を重ねないよう、一度削除してから追加するか、フラグで制御する
                EventHandler handler = null!;
                handler = (s, e) =>
                {
                    MainWindow.SourceInitialized -= handler;
                    var h = new WindowInteropHelper(MainWindow).Handle;
                    int val = isDark ? 1 : 0;
                    DwmSetWindowAttribute(h, DWMWA_USE_IMMERSIVE_DARK_MODE, ref val, sizeof(int));
                };
                MainWindow.SourceInitialized += handler;
            }
        }

        /// <summary>
        /// アプリケーション全体でハンドルされなかった例外を補足し、ログに記録します。
        /// </summary>
        private void SetupGlobalExceptionHandling()
        {
            // UIスレッド以外の未処理例外
            AppDomain.CurrentDomain.UnhandledException += (sender, e) =>
            {
                if (e.ExceptionObject is Exception ex)
                {
                    HandleFatalException(ex);
                }
            };

            // UIスレッドの未処理例外
            DispatcherUnhandledException += (sender, e) =>
            {
                HandleFatalException(e.Exception);
                e.Handled = true;
            };
        }

        /// <summary>
        /// 致命的な例外を処理し、ユーザーに通知した後にアプリケーションを終了します。
        /// </summary>
        /// <param name="ex">発生した例外。</param>
        private void HandleFatalException(Exception ex)
        {
            // ロギング
            var logger = AppHost?.Services.GetService<ILogger<App>>();
            logger?.LogCritical(ex, "Fatal exception occurred. Application will shut down.");

            // ユーザー通知
            // AppHostが構築されていない、またはサービスが取得できない場合に備えて
            // 直接ErrorWindowを出すフォールバックも考慮する
            var dialogService = AppHost?.Services.GetService<IDialogService>();
            if (dialogService != null)
            {
                dialogService.ShowExceptionDialog("致命的なエラーが発生したため、アプリケーションを終了します。", ex);
            }
            else
            {
                var viewModel = new ViewModels.ErrorViewModel("致命的なエラーが発生したため、アプリケーションを終了します。", ex);
                var errorWindow = new Views.ErrorWindow(viewModel);
                errorWindow.ShowDialog();
            }

            // 安全な終了
            if (Application.Current != null)
            {
                Application.Current.Shutdown();
            }
            else
            {
                Environment.Exit(1);
            }
        }
    }
}
