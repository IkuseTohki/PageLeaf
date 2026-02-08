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
        private Mock<IEditorService> _mockEditorService = null!;
        private CssEditorViewModel _viewModel = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockCssManagementService = new Mock<ICssManagementService>();
            _mockLoadCssUseCase = new Mock<ILoadCssUseCase>();
            _mockSaveCssUseCase = new Mock<ISaveCssUseCase>();
            _mockDialogService = new Mock<IDialogService>();
            _mockSettingsService = new Mock<ISettingsService>();
            _mockEditorService = new Mock<IEditorService>();

            _mockSettingsService.Setup(s => s.CurrentSettings).Returns(new ApplicationSettings());
            _mockEditorService.Setup(s => s.CurrentDocument).Returns(new PageLeaf.Models.Markdown.MarkdownDocument());

            _mockCssManagementService.Setup(s => s.GetCssContent(It.IsAny<string>())).Returns("");
            _mockCssManagementService.Setup(s => s.GenerateCss(It.IsAny<string>(), It.IsAny<CssStyleInfo>())).Returns("");
            _mockLoadCssUseCase.Setup(u => u.Execute(It.IsAny<string>())).Returns(("", new CssStyleInfo()));

            _viewModel = new CssEditorViewModel(
                _mockCssManagementService.Object,
                _mockLoadCssUseCase.Object,
                _mockSaveCssUseCase.Object,
                _mockDialogService.Object,
                _mockSettingsService.Object,
                _mockEditorService.Object);
        }

        [TestMethod]
        public void NewProperties_ShouldBeAccessibleAndSetDirty()
        {
            _viewModel.Load("test.css");
            Assert.IsFalse(_viewModel.IsDirty);

            _viewModel.BodyFontFamily = "Arial";
            Assert.AreEqual("Arial", _viewModel.BodyFontFamily);
            Assert.IsTrue(_viewModel.IsDirty);
            Assert.IsTrue(_viewModel.IsGeneralTabDirty);

            _viewModel.IsDirty = false;
            _viewModel.HeadingMarginTop = "10px";
            Assert.AreEqual("10px", _viewModel.HeadingMarginTop);
            Assert.IsTrue(_viewModel.IsDirty);
            Assert.IsTrue(_viewModel.IsHeadingsTabDirty);

            _viewModel.IsDirty = false;
            _viewModel.ListLineHeight = "1.8";
            Assert.AreEqual("1.8", _viewModel.ListLineHeight);
            Assert.IsTrue(_viewModel.IsDirty);
            Assert.IsTrue(_viewModel.IsListTabDirty);

            _viewModel.IsDirty = false;
            _viewModel.TableWidth = "100%";
            Assert.AreEqual("100%", _viewModel.TableWidth);
            Assert.IsTrue(_viewModel.IsDirty);
            Assert.IsTrue(_viewModel.IsTableTabDirty);

            _viewModel.IsDirty = false;
            _viewModel.QuoteIsItalic = true;
            Assert.IsTrue(_viewModel.QuoteIsItalic);
            Assert.IsTrue(_viewModel.IsDirty);
            Assert.IsTrue(_viewModel.IsQuoteTabDirty);

            // リストのピリオド設定のテスト
            _viewModel.IsDirty = false;
            _viewModel.NumberedListHasPeriod = false;
            Assert.IsFalse(_viewModel.NumberedListHasPeriod);
            Assert.IsTrue(_viewModel.IsDirty, "フラグ変更時に IsDirty が true になること");
            Assert.IsTrue(_viewModel.IsListTabDirty, "フラグ変更時に IsListTabDirty が true になること");
        }

        [TestMethod]
        public void IsDirty_ShouldBeCentralizedInMarkTabDirty()
        {
            // テスト観点: 各カテゴリのフラグや値を変更した際、
            //            集約された MarkTabDirty を経由して IsDirty が正しくセットされることを網羅的に確認する。

            // 1. Title
            _viewModel.IsDirty = false;
            _viewModel.IsTitleBold = true;
            Assert.IsTrue(_viewModel.IsDirty);
            Assert.IsTrue(_viewModel.IsTitleTabDirty);

            // 2. Footnote
            _viewModel.IsDirty = false;
            _viewModel.IsFootnoteMarkerBold = true;
            Assert.IsTrue(_viewModel.IsDirty);
            Assert.IsTrue(_viewModel.IsFootnoteTabDirty);

            // 3. Headings
            _viewModel.IsDirty = false;
            _viewModel.IsHeadingBold = true;
            Assert.IsTrue(_viewModel.IsDirty);
            Assert.IsTrue(_viewModel.IsHeadingsTabDirty);
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
