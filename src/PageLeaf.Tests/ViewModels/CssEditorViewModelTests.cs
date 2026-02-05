using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PageLeaf.Models;
using PageLeaf.Models.Css;
using PageLeaf.Models.Settings;
using PageLeaf.Services;
using PageLeaf.ViewModels;
using PageLeaf.UseCases;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace PageLeaf.Tests.ViewModels
{
    [TestClass]
    public class CssEditorViewModelTests
    {
        private Mock<ICssManagementService> _mockCssManagementService = null!;
        private Mock<ILoadCssUseCase> _mockLoadCssUseCase = null!;
        private Mock<ISaveCssUseCase> _mockSaveCssUseCase = null!;
        private Mock<IDialogService> _mockDialogService = null!;
        private Mock<ISettingsService> _mockSettingsService = null!;
        private CssEditorViewModel _viewModel = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockCssManagementService = new Mock<ICssManagementService>();
            _mockLoadCssUseCase = new Mock<ILoadCssUseCase>();
            _mockSaveCssUseCase = new Mock<ISaveCssUseCase>();
            _mockDialogService = new Mock<IDialogService>();
            _mockSettingsService = new Mock<ISettingsService>();

            _mockSettingsService.Setup(s => s.CurrentSettings).Returns(new ApplicationSettings());

            _mockCssManagementService.Setup(s => s.GetCssContent(It.IsAny<string>())).Returns("");
            _mockCssManagementService.Setup(s => s.GenerateCss(It.IsAny<string>(), It.IsAny<CssStyleInfo>())).Returns("");
            _mockLoadCssUseCase.Setup(u => u.Execute(It.IsAny<string>())).Returns(("", new CssStyleInfo()));

            _viewModel = new CssEditorViewModel(
                _mockCssManagementService.Object,
                _mockLoadCssUseCase.Object,
                _mockSaveCssUseCase.Object,
                _mockDialogService.Object,
                _mockSettingsService.Object);
        }

        [TestMethod]
        public void CodeStyleProperties_ShouldBeAccessible()
        {
            _viewModel.InlineCodeTextColor = "#111111";
            Assert.AreEqual("#111111", _viewModel.InlineCodeTextColor);
        }

        [TestMethod]
        public void IsTitleTabVisible_ShouldReflectSettings()
        {
            // Arrange (Setting = true)
            var settings = new ApplicationSettings();
            settings.View.ShowTitleInPreview = true;
            _mockSettingsService.Setup(s => s.CurrentSettings).Returns(() => settings);
            _viewModel.NotifySettingsChanged();

            // Assert
            Assert.IsTrue(_viewModel.IsTitleTabVisible);

            // Arrange (Setting = false)
            settings.View.ShowTitleInPreview = false;
            _viewModel.NotifySettingsChanged();

            // Assert
            Assert.IsFalse(_viewModel.IsTitleTabVisible);
        }

        [TestMethod]
        public void SelectedTab_ShouldSwitch_WhenTitleTabBecomesHidden()
        {
            // Arrange
            var settings = new ApplicationSettings();
            settings.View.ShowTitleInPreview = true;
            _mockSettingsService.Setup(s => s.CurrentSettings).Returns(() => settings);
            _viewModel.NotifySettingsChanged();
            _viewModel.SelectedTab = CssEditorTab.Title;

            // Act: 非表示にする
            settings.View.ShowTitleInPreview = false;
            _viewModel.NotifySettingsChanged();

            // Assert
            Assert.AreNotEqual(CssEditorTab.Title, _viewModel.SelectedTab, "SelectedTab should have switched away from Title.");
            Assert.AreEqual(CssEditorTab.General, _viewModel.SelectedTab, "SelectedTab should default to General.");
        }
    }
}
