using Microsoft.Extensions.Logging;
using PageLeaf.Models;
using System;
using System.IO;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PageLeaf.Services
{
    /// <summary>
    /// アプリケーションの設定をYAMLファイルに保存およびロードするサービスです。
    /// </summary>
    public class SettingsService : ISettingsService
    {
        private readonly ILogger<SettingsService> _logger;
        private readonly string _settingsFilePath;
        private ApplicationSettings _currentSettings;
        private readonly ISerializer _serializer;
        private readonly IDeserializer _deserializer;

        /// <summary>
        /// 設定が変更されたときに発生するイベント。
        /// </summary>
        public event EventHandler<ApplicationSettings>? SettingsChanged;

        /// <summary>
        /// SettingsService クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="logger">ロガー。</param>
        /// <param name="basePath">設定ファイルを保存するディレクトリのパス。nullの場合は実行ディレクトリが使用されます。</param>
        public SettingsService(ILogger<SettingsService> logger, string? basePath = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // 設定ファイルの保存ディレクトリを決定（デフォルトは実行ファイルと同階層）
            var finalPath = basePath ?? AppDomain.CurrentDomain.BaseDirectory;
            if (!Directory.Exists(finalPath))
            {
                Directory.CreateDirectory(finalPath);
            }
            _settingsFilePath = Path.Combine(finalPath, "settings.yaml");
            _logger.LogInformation("Settings file path: {SettingsFilePath}", _settingsFilePath);

            _serializer = new SerializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .Build();

            _deserializer = new DeserializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            _currentSettings = LoadSettingsInternal();
        }

        /// <summary>
        /// 現在のアプリケーション設定を取得します。
        /// </summary>
        public ApplicationSettings CurrentSettings => _currentSettings;

        /// <summary>
        /// 設定ファイルをロードし、ApplicationSettings オブジェクトを返します。
        /// ファイルが存在しない場合や読み込みに失敗した場合は、デフォルト設定を返します。
        /// </summary>
        /// <returns>ロードされた、またはデフォルトの ApplicationSettings オブジェクト。</returns>
        public ApplicationSettings LoadSettings()
        {
            return LoadSettingsInternal();
        }

        private ApplicationSettings LoadSettingsInternal()
        {
            if (!File.Exists(_settingsFilePath))
            {
                _logger.LogInformation("Settings file not found. Returning default settings.");
                return new ApplicationSettings();
            }

            try
            {
                var yamlString = File.ReadAllText(_settingsFilePath);
                var settings = _deserializer.Deserialize<ApplicationSettings>(yamlString);
                if (settings == null)
                {
                    _logger.LogWarning("Deserialized settings were null. Returning default settings.");
                    return new ApplicationSettings();
                }
                _logger.LogInformation("Settings loaded successfully from {SettingsFilePath}.", _settingsFilePath);
                return settings;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading settings from {SettingsFilePath}. Returning default settings.", _settingsFilePath);
                return new ApplicationSettings();
            }
        }

        /// <summary>
        /// 指定された ApplicationSettings オブジェクトをファイルに保存します。
        /// </summary>
        /// <param name="settings">保存する ApplicationSettings オブジェクト。</param>
        public void SaveSettings(ApplicationSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            try
            {
                var yamlString = _serializer.Serialize(settings);
                File.WriteAllText(_settingsFilePath, yamlString);
                _logger.LogInformation("Settings saved successfully to {SettingsFilePath}.", _settingsFilePath);
                _currentSettings = settings; // Update current settings after saving
                SettingsChanged?.Invoke(this, settings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving settings to {SettingsFilePath}.", _settingsFilePath);
            }
        }
    }
}
