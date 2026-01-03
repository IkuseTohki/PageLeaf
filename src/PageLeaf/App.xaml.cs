using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PageLeaf.Services;
using PageLeaf.UseCases;
using PageLeaf.ViewModels;
using PageLeaf.Views;
using Serilog;
using System;
using System.IO;
using System.Windows;
using AngleSharp.Css.Values;

namespace PageLeaf
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// DIコンテナやロギングなどのアプリケーションサービスをホストします。
        /// </summary>
        public static IHost? AppHost { get; private set; }

        /// <summary>
        /// アプリケーションの起動時に呼び出されます。
        /// </summary>
        /// <param name="e">スタートアップイベントのデータ。</param>
        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // AngleSharpが色をHEX形式で出力するように設定
            Color.UseHex = true;

            // DIコンテナとロギングを設定
            AppHost = Host.CreateDefaultBuilder()
                .UseSerilog((hostContext, services, configuration) =>
                {
                    // ログファイルのパスを設定
                    var logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logs", "PageLeaf-.txt");

                    configuration
                        .MinimumLevel.Debug()
                        .Enrich.FromLogContext()
                        .WriteTo.File(
                            logPath,
                            rollingInterval: RollingInterval.Day,
                            retainedFileCountLimit: 7,
                            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                        );
                })
                .ConfigureServices((hostContext, services) =>
                {
                    // Services をDIコンテナに登録
                    // いくつかの Service は複数の依存関係を持つが、コンストラクタが一つであり、
                    // 全ての依存関係がDIコンテナに登録されているため、自動解決が可能。
                    // 可読性と一貫性のため、シンプルな登録方法を採用している。
                    services.AddSingleton<IFileService, FileService>();
                    services.AddSingleton<ICssService, CssService>();
                    services.AddSingleton<ISettingsService, SettingsService>();
                    services.AddSingleton<IDialogService, DialogService>();
                    services.AddSingleton<IMarkdownService, MarkdownService>();
                    services.AddSingleton<IEditorService, EditorService>(); // EditorService を登録
                    services.AddSingleton<ICssEditorService, CssEditorService>();
                    services.AddSingleton<ICssManagementService, CssManagementService>();

                    // UseCases
                    services.AddTransient<ISaveAsDocumentUseCase, SaveAsDocumentUseCase>();
                    services.AddTransient<ISaveDocumentUseCase, SaveDocumentUseCase>();
                    services.AddTransient<INewDocumentUseCase, NewDocumentUseCase>();
                    services.AddTransient<IOpenDocumentUseCase, OpenDocumentUseCase>();
                    services.AddTransient<ILoadCssUseCase, LoadCssUseCase>();
                    services.AddTransient<ISaveCssUseCase, SaveCssUseCase>();

                    // ViewModels と Views をDIコンテナに登録
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

                var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
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
            if (AppHost != null)
            {
                await AppHost.StopAsync();
                AppHost.Dispose();
            }

            base.OnExit(e);
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
                // アプリケーションを終了させるため、ここでは処理済みとはせず、
                // HandleFatalException 内で終了処理を促すか、e.Handled を false のままにする。
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
