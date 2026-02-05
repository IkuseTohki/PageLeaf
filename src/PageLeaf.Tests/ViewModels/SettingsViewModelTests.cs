using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PageLeaf.Models;
using PageLeaf.Models.Settings;
using PageLeaf.Services;
using PageLeaf.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;

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
            _settings = new ApplicationSettings();
            // テスト用に初期値を設定
            _settings.Editor.AdditionalFrontMatter = new List<FrontMatterAdditionalProperty>();

            _settingsServiceMock.Setup(x => x.CurrentSettings).Returns(() => _settings);
            _settingsServiceMock.Setup(x => x.SaveSettings(It.IsAny<ApplicationSettings>()))
                .Callback<ApplicationSettings>(s => _settings = s);
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
            Assert.AreEqual(1, _settings.Editor.AdditionalFrontMatter.Count);
            Assert.AreEqual("test_key", _settings.Editor.AdditionalFrontMatter[0].Key);
            Assert.AreEqual("test_value", _settings.Editor.AdditionalFrontMatter[0].Value);
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
            Assert.AreEqual(2, _settings.Editor.AdditionalFrontMatter.Count);
            Assert.AreEqual("first", _settings.Editor.AdditionalFrontMatter[0].Key);
            Assert.AreEqual("second", _settings.Editor.AdditionalFrontMatter[1].Key);
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
            Assert.AreEqual(AppTheme.Dark, _settings.Appearance.Theme);
            _settingsServiceMock.Verify(x => x.SaveSettings(It.IsAny<ApplicationSettings>()), Times.Once);
        }

        [TestMethod]
        public void Theme_ShouldInitializeFromSettings()
        {
            // テスト観点: ビューモデル初期化時に設定からテーマが正しく読み込まれることを確認する。

            // Arrange
            _settings.Appearance.Theme = AppTheme.Light;

            // Act
            var viewModel = new SettingsViewModel(_settingsServiceMock.Object);

            // Assert
            Assert.AreEqual(AppTheme.Light, viewModel.Theme);
        }

        [TestMethod]
        public void IsCdnEnabled_ShouldSyncWithLibraryResourceSource()
        {
            // テスト観点: IsCdnEnabled プロパティ（UI用）が LibraryResourceSource と連動することを確認する。

            // Arrange
            var viewModel = new SettingsViewModel(_settingsServiceMock.Object);

            // Act: UIからCDNを有効にする
            viewModel.IsCdnEnabled = true;

            // Assert: 内部的な ResourceSource が Cdn になっていること
            Assert.AreEqual(ResourceSource.Cdn, viewModel.LibraryResourceSource);

            // Act: UIからCDNを無効にする
            viewModel.IsCdnEnabled = false;

            // Assert: 内部的な ResourceSource が Local になっていること
            Assert.AreEqual(ResourceSource.Local, viewModel.LibraryResourceSource);
        }

        [TestMethod]
        public void Save_ShouldApplyAllProperties()
        {
            // テスト観点: 全ての設定項目が ViewModel から ApplicationSettings に正しく保存されることを確認する。

            // Arrange
            var viewModel = new SettingsViewModel(_settingsServiceMock.Object);
            viewModel.IndentSize = 2;
            viewModel.EditorFontSize = 18;
            viewModel.ShowTitleInPreview = false;
            viewModel.UseSpacesForIndent = false;
            viewModel.AutoInsertFrontMatter = false;

            // Act
            viewModel.SaveCommand.Execute(null);

            // Assert
            Assert.AreEqual(2, _settings.Editor.IndentSize);
            Assert.AreEqual(18, _settings.Editor.EditorFontSize);
            Assert.IsFalse(_settings.View.ShowTitleInPreview);
            Assert.IsFalse(_settings.Editor.UseSpacesForIndent);
            Assert.IsFalse(_settings.Editor.AutoInsertFrontMatter);
        }
    }
}
