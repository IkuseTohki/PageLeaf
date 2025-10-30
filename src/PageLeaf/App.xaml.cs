using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using PageLeaf.Services;
using PageLeaf.ViewModels;
using PageLeaf.Views; // NEW
using Serilog;
using System;
using System.IO;
using System.Windows;

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

            // DIコンテナとロギングを設定
            AppHost = Host.CreateDefaultBuilder()
                .UseSerilog((hostContext, services, configuration) => {
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
                    services.AddSingleton<IFileService, FileService>();
                    services.AddSingleton<IDialogService, DialogService>(); // NEW

                    // ViewModels と Views をDIコンテナに登録
                    services.AddSingleton<MainViewModel>();
                    services.AddSingleton<MainWindow>();
                })
                .Build();

            // グローバル例外ハンドリングを設定
            SetupGlobalExceptionHandling();

            await AppHost.StartAsync();

            var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();
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
                var logger = AppHost?.Services.GetService<ILogger<App>>();
                logger?.LogError(e.ExceptionObject as Exception, "Unhandled exception occurred.");
            };

            // UIスレッドの未処理例外
            DispatcherUnhandledException += (sender, e) =>
            {
                var logger = AppHost?.Services.GetService<ILogger<App>>();
                logger?.LogError(e.Exception, "Dispatcher unhandled exception occurred.");
                // アプリケーションのクラッシュを防ぐために、例外を処理済みにマーク
                e.Handled = true;
            };
        }
    }
}
