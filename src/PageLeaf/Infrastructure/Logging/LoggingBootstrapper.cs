using PageLeaf.Models.Settings;
using Serilog;
using Serilog.Core;
using System;
using System.IO;

namespace PageLeaf.Infrastructure.Logging
{
    /// <summary>
    /// アプリケーション起動時のロギング構成を担当するブートストラッパーです。
    /// DIコンテナ構築前の「先行読み込み」を行い、循環依存を回避します。
    /// </summary>
    public class LoggingBootstrapper
    {
        private readonly string _baseDirectory;
        private readonly string _settingsFilePath;

        public LoggingLevelSwitch LevelSwitch { get; } = new LoggingLevelSwitch();
        public bool EnableFileLogging { get; private set; } = false; // デフォルトはfalse

        public LoggingBootstrapper(string baseDirectory)
        {
            _baseDirectory = baseDirectory;
            _settingsFilePath = Path.Combine(baseDirectory, "settings.yaml");
        }

        /// <summary>
        /// 設定ファイルからログ設定を先行読み込みし、状態を初期化します。
        /// </summary>
        public void LoadInitialSettings()
        {
            try
            {
                if (File.Exists(_settingsFilePath))
                {
                    var yaml = File.ReadAllText(_settingsFilePath);
                    ParseSettings(yaml);
                }
                else
                {
                    SetDefaultSettings();
                }
            }
            catch
            {
                SetDefaultSettings();
            }
        }

        private void ParseSettings(string yaml)
        {
            // nameof を使用してプロパティ名と同期させる
            // YAML上では "Logging: \n  EnableFileLogging: ..." のような構造になるが、
            // 簡易検索として "EnableFileLogging: value" を探す。

            var enableKey = nameof(LoggingSettings.EnableFileLogging);
            var levelKey = nameof(LoggingSettings.MinimumLevel);

            // ファイル出力設定
            if (yaml.Contains($"{enableKey}: false", StringComparison.OrdinalIgnoreCase))
            {
                EnableFileLogging = false;
            }
            else if (yaml.Contains($"{enableKey}: true", StringComparison.OrdinalIgnoreCase))
            {
                EnableFileLogging = true;
            }
            else
            {
                EnableFileLogging = true; // キーが見つからない場合のデフォルト
            }

            // ログレベル設定
            // Enumの文字列表現も nameof で同期させる
            if (yaml.Contains($"{levelKey}: {nameof(LogOutputLevel.Development)}", StringComparison.OrdinalIgnoreCase))
            {
                LevelSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Debug;
            }
            else if (yaml.Contains($"{levelKey}: {nameof(LogOutputLevel.Standard)}", StringComparison.OrdinalIgnoreCase))
            {
                LevelSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Information;
            }
            else
            {
                LevelSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Information; // デフォルト
            }
        }

        private void SetDefaultSettings()
        {
            EnableFileLogging = true;
            LevelSwitch.MinimumLevel = Serilog.Events.LogEventLevel.Information;
        }

        /// <summary>
        /// アプリケーション設定オブジェクトから動的に状態を更新します。
        /// </summary>
        public void UpdateFromSettings(LoggingSettings settings)
        {
            EnableFileLogging = settings.EnableFileLogging;
            LevelSwitch.MinimumLevel = MapLogLevel(settings.MinimumLevel);
        }

        private static Serilog.Events.LogEventLevel MapLogLevel(LogOutputLevel level)
        {
            return level switch
            {
                LogOutputLevel.Standard => Serilog.Events.LogEventLevel.Information,
                LogOutputLevel.Development => Serilog.Events.LogEventLevel.Debug,
                _ => Serilog.Events.LogEventLevel.Information
            };
        }
    }
}
