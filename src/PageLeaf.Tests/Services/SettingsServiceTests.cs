using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageLeaf.Models;
using PageLeaf.Services;
using System;
using System.IO;
using System.Text.Json;
using Moq;
using Microsoft.Extensions.Logging;

namespace PageLeaf.Tests.Services
{
    [TestClass]
    public class SettingsServiceTests
    {
        private string _testSettingsFilePath = null!;
        private string _testAppDataPath = null!;
        private Mock<ILogger<SettingsService>> _mockLogger = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<SettingsService>>();
            // テスト用のアプリケーションデータパスを設定
            _testAppDataPath = Path.Combine(Path.GetTempPath(), "PageLeafTestAppData", Guid.NewGuid().ToString());
            Directory.CreateDirectory(_testAppDataPath);
            _testSettingsFilePath = Path.Combine(_testAppDataPath, "settings.json");

            // 環境変数をモックして、SettingsServiceがテスト用のパスを使用するようにする
            // これは直接モックできないため、SettingsServiceのコンストラクタでパスを受け取るように変更するか、
            // テスト中にEnvironment.GetFolderPathの動作を一時的に変更する必要がある。
            // 今回はSettingsServiceのコンストラクタでパスを受け取るように実装することを前提とする。
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_testAppDataPath))
            {
                Directory.Delete(_testAppDataPath, true);
            }
        }

        [TestMethod]
        public void test_LoadSettings_ShouldReturnDefaultSettings_WhenFileDoesNotExist()
        {
            // テスト観点: 設定ファイルが存在しない場合、LoadSettingsがデフォルト設定を返すことを確認する。
            // Arrange
            var service = new SettingsService(_mockLogger.Object, _testAppDataPath);

            // Act
            var settings = service.LoadSettings();

            // Assert
            Assert.IsNotNull(settings);
            Assert.AreEqual(FolderTreePosition.Left, settings.FolderTreePosition);
            Assert.AreEqual("", settings.SelectedCss);
            Assert.AreEqual("", settings.LastOpenedFolder);
        }

        [TestMethod]
        public void test_LoadSettings_ShouldLoadSettingsFromFile_WhenFileExists()
        {
            // テスト観点: 設定ファイルが存在する場合、LoadSettingsがファイルから設定を正しくロードすることを確認する。
            // Arrange
            var expectedSettings = new ApplicationSettings
            {
                FolderTreePosition = FolderTreePosition.Right,
                SelectedCss = "solarized-dark.css",
                LastOpenedFolder = "C:\\Users\\Test\\Documents"
            };
            var serializeOptions = new JsonSerializerOptions();
            serializeOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            File.WriteAllText(_testSettingsFilePath, JsonSerializer.Serialize(expectedSettings, serializeOptions));
            var service = new SettingsService(_mockLogger.Object, _testAppDataPath);

            // Act
            var actualSettings = service.LoadSettings();

            // Assert
            Assert.IsNotNull(actualSettings);
            Assert.AreEqual(expectedSettings.FolderTreePosition, actualSettings.FolderTreePosition);
            Assert.AreEqual(expectedSettings.SelectedCss, actualSettings.SelectedCss);
            Assert.AreEqual(expectedSettings.LastOpenedFolder, actualSettings.LastOpenedFolder);
        }

        [TestMethod]
        public void test_SaveSettings_ShouldSaveSettingsToFile()
        {
            // テスト観点: SaveSettingsが設定をファイルに正しく保存することを確認する。
            // Arrange
            var settingsToSave = new ApplicationSettings
            {
                FolderTreePosition = FolderTreePosition.Right,
                SelectedCss = "github.css",
                LastOpenedFolder = "D:\\Projects\\MyProject"
            };

            var service = new SettingsService(_mockLogger.Object, _testAppDataPath);

            // Act
            service.SaveSettings(settingsToSave);

            // Assert
            Assert.IsTrue(File.Exists(_testSettingsFilePath));
            var savedContent = File.ReadAllText(_testSettingsFilePath);
            var deserializeOptions = new JsonSerializerOptions();
            deserializeOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
            var actualSettings = JsonSerializer.Deserialize<ApplicationSettings>(savedContent, deserializeOptions);

            Assert.IsNotNull(actualSettings);
            Assert.AreEqual(settingsToSave.FolderTreePosition, actualSettings.FolderTreePosition);
            Assert.AreEqual(settingsToSave.SelectedCss, actualSettings.SelectedCss);
            Assert.AreEqual(settingsToSave.LastOpenedFolder, actualSettings.LastOpenedFolder);
        }

        [TestMethod]
        public void test_SaveSettings_ShouldCreateDirectoryIfNotExist()
        {
            // テスト観点: SaveSettingsが設定ファイルを保存する際に、ディレクトリが存在しない場合は作成することを確認する。
            // Arrange
            var nonExistentAppDataPath = Path.Combine(Path.GetTempPath(), "NonExistentApp", Guid.NewGuid().ToString());
            var service = new SettingsService(_mockLogger.Object, nonExistentAppDataPath);
            var settingsToSave = new ApplicationSettings();

            // Act
            service.SaveSettings(settingsToSave);

            // Assert
            Assert.IsTrue(Directory.Exists(nonExistentAppDataPath));
            Assert.IsTrue(File.Exists(Path.Combine(nonExistentAppDataPath, "settings.json")));

            // Cleanup
            if (Directory.Exists(nonExistentAppDataPath))
            {
                Directory.Delete(nonExistentAppDataPath, true);
            }
        }

        [TestMethod]
        public void test_LoadSettings_ShouldHandleCorruptedFileGracefully()
        {
            // テスト観点: 設定ファイルが破損している場合、LoadSettingsが例外をスローせずにデフォルト設定を返すことを確認する。
            // Arrange
            File.WriteAllText(_testSettingsFilePath, "{ \"corrupted\": \"json");
            var service = new SettingsService(_mockLogger.Object, _testAppDataPath);

            // Act
            var settings = service.LoadSettings();

            // Assert
            Assert.IsNotNull(settings);
            Assert.AreEqual(FolderTreePosition.Left, settings.FolderTreePosition);
            Assert.AreEqual("", settings.SelectedCss);
            Assert.AreEqual("", settings.LastOpenedFolder);
        }
    }
}
