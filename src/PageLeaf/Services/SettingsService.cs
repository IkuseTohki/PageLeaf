using Microsoft.Extensions.Logging;
using PageLeaf.Models;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

namespace PageLeaf.Services
{
    /// <summary>
    /// アプリケーションの設定をファイルに保存およびロードするサービスです。
    /// </summary>
    public class SettingsService : ISettingsService
    {
        private readonly ILogger<SettingsService> _logger;
        private readonly string _settingsFilePath;
        private ApplicationSettings _currentSettings;

        /// <summary>
        /// SettingsService クラスの新しいインスタンスを初期化します。
        /// </summary>
        /// <param name="logger">ロガー。</param>
        /// <param name="appDataPath">設定ファイルを保存するアプリケーションデータフォルダのパス。</param>
        public SettingsService(ILogger<SettingsService> logger, string? appDataPath = null)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // アプリケーションデータフォルダのパスを決定
            var baseAppDataPath = appDataPath ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "PageLeaf");
            if (!Directory.Exists(baseAppDataPath))
            {
                Directory.CreateDirectory(baseAppDataPath);
            }
            _settingsFilePath = Path.Combine(baseAppDataPath, "settings.json");
            _logger.LogInformation("Settings file path: {SettingsFilePath}", _settingsFilePath);

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
                var options = new JsonSerializerOptions();
                options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                var jsonString = File.ReadAllText(_settingsFilePath);
                var settings = JsonSerializer.Deserialize<ApplicationSettings>(jsonString, options);
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
                var options = new JsonSerializerOptions { WriteIndented = true };
                options.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
                var jsonString = JsonSerializer.Serialize(settings, options);
                File.WriteAllText(_settingsFilePath, jsonString);
                _logger.LogInformation("Settings saved successfully to {SettingsFilePath}.", _settingsFilePath);
                _currentSettings = settings; // Update current settings after saving
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving settings to {SettingsFilePath}.", _settingsFilePath);
            }
        }
    }
}
