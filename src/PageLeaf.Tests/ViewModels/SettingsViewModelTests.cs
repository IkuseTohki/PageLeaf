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
    }
}
