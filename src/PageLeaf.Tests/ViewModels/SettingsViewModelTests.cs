using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PageLeaf.Models;
using PageLeaf.Services;
using PageLeaf.ViewModels;
using System.Linq;
using System.Collections.Generic;

namespace PageLeaf.Tests.ViewModels
{
    [TestClass]
    public class SettingsViewModelTests
    {
        private Mock<ISettingsService> _settingsServiceMock = null!;
        private ApplicationSettings _settings = null!;

        [TestInitialize]
        public void Setup()
        {
            _settingsServiceMock = new Mock<ISettingsService>();
            _settings = new ApplicationSettings
            {
                AdditionalFrontMatter = new List<FrontMatterAdditionalProperty>()
            };
            _settingsServiceMock.Setup(x => x.CurrentSettings).Returns(_settings);
        }

        [TestMethod]
        public void Save_ShouldPreserveItemsInCollection()
        {
            // テスト観点: 追加したプロパティが Save 実行後に AdditionalFrontMatter に正しく反映されることを確認する。

            // Arrange
            var viewModel = new SettingsViewModel(_settingsServiceMock.Object);
            viewModel.AddFrontMatterPropertyCommand.Execute(null);
            var item = viewModel.DefaultFrontMatterProperties.Last();
            item.Key = "test_key";
            item.Value = "test_value";

            // Act
            viewModel.SaveCommand.Execute(null);

            // Assert
            Assert.AreEqual(1, _settings.AdditionalFrontMatter.Count);
            Assert.AreEqual("test_key", _settings.AdditionalFrontMatter[0].Key);
            Assert.AreEqual("test_value", _settings.AdditionalFrontMatter[0].Value);
        }

        [TestMethod]
        public void Save_ShouldPreserveOrder()
        {
            // テスト観点: プロパティの並び順が設定に正しく保存されることを確認する。

            // Arrange
            var viewModel = new SettingsViewModel(_settingsServiceMock.Object);

            viewModel.AddFrontMatterPropertyCommand.Execute(null);
            viewModel.DefaultFrontMatterProperties[0].Key = "first";

            viewModel.AddFrontMatterPropertyCommand.Execute(null);
            viewModel.DefaultFrontMatterProperties[1].Key = "second";

            // Act
            viewModel.SaveCommand.Execute(null);

            // Assert
            Assert.AreEqual(2, _settings.AdditionalFrontMatter.Count);
            Assert.AreEqual("first", _settings.AdditionalFrontMatter[0].Key);
            Assert.AreEqual("second", _settings.AdditionalFrontMatter[1].Key);
        }

        [TestMethod]
        public void Save_ShouldSaveTheme()
        {
            // テスト観点: テーマ設定が正しく保存されることを確認する。

            // Arrange
            var viewModel = new SettingsViewModel(_settingsServiceMock.Object);
            viewModel.Theme = AppTheme.Dark;

            // Act
            viewModel.SaveCommand.Execute(null);

            // Assert
            Assert.AreEqual(AppTheme.Dark, _settings.Theme);
            _settingsServiceMock.Verify(x => x.SaveSettings(It.IsAny<ApplicationSettings>()), Times.Once);
        }

        [TestMethod]
        public void Theme_ShouldInitializeFromSettings()
        {
            // テスト観点: ビューモデル初期化時に設定からテーマが正しく読み込まれることを確認する。

            // Arrange
            _settings.Theme = AppTheme.Light;

            // Act
            var viewModel = new SettingsViewModel(_settingsServiceMock.Object);

            // Assert
            Assert.AreEqual(AppTheme.Light, viewModel.Theme);
        }

        [TestMethod]
        public void IsCdnEnabled_ShouldSyncWithLibraryResourceSource()
        {
            // テスト観点: IsCdnEnabled と LibraryResourceSource が互いに連動することを確認する。

            // Arrange
            var viewModel = new SettingsViewModel(_settingsServiceMock.Object);

            // Act & Assert (Local -> Cdn via IsCdnEnabled)
            viewModel.IsCdnEnabled = true;
            Assert.AreEqual(ResourceSource.Cdn, viewModel.LibraryResourceSource);

            // Act & Assert (Cdn -> Local via LibraryResourceSource)
            viewModel.LibraryResourceSource = ResourceSource.Local;
            Assert.IsFalse(viewModel.IsCdnEnabled);
        }

        [TestMethod]
        public void Save_ShouldSaveBasicProperties()
        {
            // テスト観点: 基本的な数値や真偽値の設定が正しく保存されることを確認する。

            // Arrange
            var viewModel = new SettingsViewModel(_settingsServiceMock.Object);
            viewModel.EditorFontSize = 20.5;
            viewModel.ShowTitleInPreview = false;
            viewModel.IndentSize = 2;
            viewModel.UseSpacesForIndent = false;
            viewModel.AutoInsertFrontMatter = false;

            // Act
            viewModel.SaveCommand.Execute(null);

            // Assert
            Assert.AreEqual(20.5, _settings.EditorFontSize);
            Assert.IsFalse(_settings.ShowTitleInPreview);
            Assert.AreEqual(2, _settings.IndentSize);
            Assert.IsFalse(_settings.UseSpacesForIndent);
            Assert.IsFalse(_settings.AutoInsertFrontMatter);
        }

        [TestMethod]
        public void CurrentCategory_ShouldChange()
        {
            // テスト観点: カテゴリの切り替えができることを確認する。

            // Arrange
            var viewModel = new SettingsViewModel(_settingsServiceMock.Object);

            // Act
            viewModel.CurrentCategory = SettingsCategory.Editor;

            // Assert
            Assert.AreEqual(SettingsCategory.Editor, viewModel.CurrentCategory);
        }
    }
}
