using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageLeaf.Models;
using PageLeaf.Models.Markdown;
using PageLeaf.Models.Css;
using PageLeaf.Models.Css.Elements;
using PageLeaf.Models.Settings;
using PageLeaf.Services;
using System;
using System.IO;
using Moq;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace PageLeaf.Tests.Services
{
    [TestClass]
    public class SettingsServiceTests
    {
        private string _testSettingsFilePath = null!;
        private string _testAppDataPath = null!;
        private Mock<ILogger<SettingsService>> _mockLogger = null!;
        private ISerializer _serializer = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockLogger = new Mock<ILogger<SettingsService>>();
            _testAppDataPath = Path.Combine(Path.GetTempPath(), "PageLeafTestAppData", Guid.NewGuid().ToString());
            if (!Directory.Exists(_testAppDataPath))
            {
                Directory.CreateDirectory(_testAppDataPath);
            }
            _testSettingsFilePath = Path.Combine(_testAppDataPath, "settings.yaml");

            _serializer = new SerializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .Build();
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
            Assert.AreEqual("", settings.View.SelectedCss);
        }

        [TestMethod]
        public void test_LoadSettings_ShouldLoadSettingsFromFile_WhenFileExists()
        {
            // テスト観点: 設定ファイルが存在する場合、LoadSettingsがファイルから設定を正しくロードすることを確認する。
            // Arrange
            var expectedSettings = new ApplicationSettings();
            expectedSettings.View.SelectedCss = "solarized-dark.css";

            var yaml = _serializer.Serialize(expectedSettings);
            File.WriteAllText(_testSettingsFilePath, yaml);

            var service = new SettingsService(_mockLogger.Object, _testAppDataPath);

            // Act
            var actualSettings = service.LoadSettings();

            // Assert
            Assert.IsNotNull(actualSettings);
            Assert.AreEqual(expectedSettings.View.SelectedCss, actualSettings.View.SelectedCss);
        }

        [TestMethod]
        public void test_SaveAndLoad_LibraryResourceSource()
        {
            // テスト観点: LibraryResourceSource プロパティが正しく保存および読み込みされることを確認する。
            // Arrange
            var settings = new ApplicationSettings();
            settings.Appearance.LibraryResourceSource = ResourceSource.Cdn;
            var service = new SettingsService(_mockLogger.Object, _testAppDataPath);

            // Act
            service.SaveSettings(settings);
            var loadedSettings = service.LoadSettings();

            // Assert
            Assert.AreEqual(ResourceSource.Cdn, loadedSettings.Appearance.LibraryResourceSource);
        }

        [TestMethod]
        public void test_SaveSettings_ShouldSaveSettingsToFile()
        {
            // テスト観点: SaveSettingsが設定をファイルに正しく保存することを確認する。
            // Arrange
            var settingsToSave = new ApplicationSettings();
            settingsToSave.View.SelectedCss = "github.css";

            var service = new SettingsService(_mockLogger.Object, _testAppDataPath);

            // Act
            service.SaveSettings(settingsToSave);

            // Assert
            Assert.IsTrue(File.Exists(_testSettingsFilePath));
            var savedContent = File.ReadAllText(_testSettingsFilePath);
            // YAML構造がネストされていることを確認
            Assert.IsTrue(savedContent.Contains("View:"));
            Assert.IsTrue(savedContent.Contains("SelectedCss: github.css"));
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
            Assert.IsTrue(File.Exists(Path.Combine(nonExistentAppDataPath, "settings.yaml")));

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
            Assert.AreEqual("", settings.View.SelectedCss);
        }

        [TestMethod]
        public void test_SaveAndLoad_EditorFontSize()
        {
            // テスト観点: EditorFontSize プロパティが正しく保存および読み込みされることを確認する。
            // Arrange
            var settings = new ApplicationSettings();
            settings.Editor.EditorFontSize = 18;
            var service = new SettingsService(_mockLogger.Object, _testAppDataPath);

            // Act
            service.SaveSettings(settings);
            var loadedSettings = service.LoadSettings();

            // Assert
            Assert.AreEqual(18, loadedSettings.Editor.EditorFontSize);
        }

        [TestMethod]
        public void test_SaveAndLoad_ShowTitleInPreview()
        {
            // テスト観点: ShowTitleInPreview プロパティが正しく保存および読み込みされることを確認する。
            // Arrange
            var settings = new ApplicationSettings();
            settings.View.ShowTitleInPreview = true;
            var service = new SettingsService(_mockLogger.Object, _testAppDataPath);

            // Act
            service.SaveSettings(settings);
            var loadedSettings = service.LoadSettings();

            // Assert
            Assert.IsTrue(loadedSettings.View.ShowTitleInPreview);
        }

        [TestMethod]
        public void test_SaveAndLoad_Theme()
        {
            // テスト観点: Theme プロパティが正しく保存および読み込みされることを確認する。
            // Arrange
            var settings = new ApplicationSettings();
            settings.Appearance.Theme = AppTheme.Dark;
            var service = new SettingsService(_mockLogger.Object, _testAppDataPath);

            // Act
            service.SaveSettings(settings);
            var loadedSettings = service.LoadSettings();

            // Assert
            Assert.AreEqual(AppTheme.Dark, loadedSettings.Appearance.Theme);
        }

        [TestMethod]
        public void test_DefaultSettingsFilePath_ShouldBeInBaseDirectory()
        {
            // テスト観点: appDataPath が null の場合、設定ファイルが実行ディレクトリ配下に設定されることを確認する。
            // Arrange
            var service = new SettingsService(_mockLogger.Object, null);

            // Act
            var expectedPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "settings.yaml");

            // テスト実行前にファイルが存在しないことを確認
            if (File.Exists(expectedPath)) File.Delete(expectedPath);

            try
            {
                // Act
                service.SaveSettings(new ApplicationSettings());

                // Assert
                Assert.IsTrue(File.Exists(expectedPath), $"Settings file should be created at {expectedPath}");
            }
            finally
            {
                // Cleanup
                if (File.Exists(expectedPath)) File.Delete(expectedPath);
            }
        }
    }
}
