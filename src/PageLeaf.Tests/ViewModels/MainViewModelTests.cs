using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PageLeaf.Models;
using PageLeaf.Services;
using PageLeaf.UseCases;
using PageLeaf.ViewModels;
using System.Collections.Generic;

namespace PageLeaf.Tests.ViewModels
{
    [TestClass]
    public class MainViewModelTests
    {
        private Mock<IEditorService> _editorServiceMock = null!;
        private Mock<IFileService> _fileServiceMock = null!;
        private Mock<IDialogService> _dialogServiceMock = null!;
        private Mock<ISettingsService> _settingsServiceMock = null!;
        private Mock<ICssManagementService> _cssManagementServiceMock = null!;
        private Mock<ILogger<MainViewModel>> _loggerMock = null!;
        private Mock<INewDocumentUseCase> _newDocumentUseCaseMock = null!;
        private Mock<IOpenDocumentUseCase> _openDocumentUseCaseMock = null!;
        private Mock<ISaveDocumentUseCase> _saveDocumentUseCaseMock = null!;
        private Mock<ISaveAsDocumentUseCase> _saveAsDocumentUseCaseMock = null!;
        private Mock<IPasteImageUseCase> _pasteImageUseCaseMock = null!;
        private Mock<IMarkdownService> _markdownServiceMock = null!;
        private MainViewModel _viewModel = null!;
        private CssEditorViewModel _cssEditorViewModel = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _editorServiceMock = new Mock<IEditorService>();
            _fileServiceMock = new Mock<IFileService>();
            _dialogServiceMock = new Mock<IDialogService>();
            _settingsServiceMock = new Mock<ISettingsService>();
            _cssManagementServiceMock = new Mock<ICssManagementService>();
            _loggerMock = new Mock<ILogger<MainViewModel>>();
            _newDocumentUseCaseMock = new Mock<INewDocumentUseCase>();
            _openDocumentUseCaseMock = new Mock<IOpenDocumentUseCase>();
            _saveDocumentUseCaseMock = new Mock<ISaveDocumentUseCase>();
            _saveAsDocumentUseCaseMock = new Mock<ISaveAsDocumentUseCase>();
            _pasteImageUseCaseMock = new Mock<IPasteImageUseCase>();
            _markdownServiceMock = new Mock<IMarkdownService>();

            _cssEditorViewModel = new CssEditorViewModel(
                _cssManagementServiceMock.Object,
                new Mock<ILoadCssUseCase>().Object,
                new Mock<ISaveCssUseCase>().Object,
                _dialogServiceMock.Object,
                _settingsServiceMock.Object);

            _cssManagementServiceMock.Setup(s => s.GetAvailableCssFileNames()).Returns(new List<string> { "default.css" });
            _settingsServiceMock.Setup(s => s.CurrentSettings).Returns(new ApplicationSettings { SelectedCss = "default.css" });

            _viewModel = new MainViewModel(
                _fileServiceMock.Object,
                _loggerMock.Object,
                _dialogServiceMock.Object,
                _editorServiceMock.Object,
                _settingsServiceMock.Object,
                _cssManagementServiceMock.Object,
                _cssEditorViewModel,
                _newDocumentUseCaseMock.Object,
                _openDocumentUseCaseMock.Object,
                _saveDocumentUseCaseMock.Object,
                _saveAsDocumentUseCaseMock.Object,
                _pasteImageUseCaseMock.Object,
                _markdownServiceMock.Object);
        }

        [TestMethod]
        public void ToggleDisplayMode_ShouldSwitchMode()
        {
            // テスト観点: 表示モードがトグルされることを確認する。
            // Arrange
            _editorServiceMock.SetupProperty(e => e.SelectedMode, DisplayMode.Viewer);

            // Act: Viewer -> Markdown
            _viewModel.ToggleDisplayModeCommand.Execute(null);

            // Assert
            Assert.AreEqual(DisplayMode.Markdown, _editorServiceMock.Object.SelectedMode);

            // Act: Markdown -> Viewer
            _viewModel.ToggleDisplayModeCommand.Execute(null);

            // Assert
            Assert.AreEqual(DisplayMode.Viewer, _editorServiceMock.Object.SelectedMode);
        }

        [TestMethod]
        public void ToggleDisplayMode_ShouldRequestFocus()
        {
            // テスト観点: モード切替時にフォーカス要求イベントが発行されることを確認する。
            // Arrange
            _editorServiceMock.SetupProperty(e => e.SelectedMode, DisplayMode.Viewer);
            DisplayMode? requestedMode = null;
            _viewModel.RequestFocus += (s, mode) => requestedMode = mode;

            // Act: Viewer -> Markdown
            _viewModel.ToggleDisplayModeCommand.Execute(null);

            // Assert
            Assert.AreEqual(DisplayMode.Markdown, requestedMode);

            // Act: Markdown -> Viewer
            _viewModel.ToggleDisplayModeCommand.Execute(null);

            // Assert
            Assert.AreEqual(DisplayMode.Viewer, requestedMode);
        }

        [TestMethod]
        public void ToggleToc_ShouldLoadHeaders_WhenOpening()
        {
            // テスト観点: TOCを開くときにMarkdownServiceからヘッダーをロードすることを確認する。
            // Arrange
            _viewModel.IsTocOpen = false;
            var headers = new List<TocItem> { new TocItem { Text = "H1", Level = 1, Id = "h1" } };
            _editorServiceMock.Setup(e => e.EditorText).Returns("# H1");
            _markdownServiceMock.Setup(m => m.ExtractHeaders("# H1")).Returns(headers);

            // Act
            _viewModel.ToggleTocCommand.Execute(null);

            // Assert
            Assert.IsTrue(_viewModel.IsTocOpen);
            Assert.AreEqual(1, _viewModel.TocItems.Count);
            Assert.AreEqual("H1", _viewModel.TocItems[0].Text);
            _markdownServiceMock.Verify(m => m.ExtractHeaders("# H1"), Times.Once);
        }

        [TestMethod]
        public void ToggleToc_ShouldNotLoadHeaders_WhenClosing()
        {
            // テスト観点: TOCを閉じるときにはヘッダーの再ロードは不要であることを確認する。
            // Arrange
            _editorServiceMock.Setup(e => e.EditorText).Returns("");
            _markdownServiceMock.Setup(m => m.ExtractHeaders(It.IsAny<string>())).Returns(new List<TocItem>());

            // 一旦開く（このときはロードされる）
            _viewModel.IsTocOpen = true;
            _markdownServiceMock.Invocations.Clear(); // 呼び出し履歴クリア

            // Act
            _viewModel.ToggleTocCommand.Execute(null);

            // Assert
            Assert.IsFalse(_viewModel.IsTocOpen);
            _markdownServiceMock.Verify(m => m.ExtractHeaders(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public void NavigateToHeader_ShouldCloseTocAndRequestScroll()
        {
            // テスト観点: ヘッダーへナビゲートすると、TOCが閉じられ、スクロールリクエストイベントが発行されることを確認する。
            // Arrange
            _editorServiceMock.Setup(e => e.EditorText).Returns("");
            _markdownServiceMock.Setup(m => m.ExtractHeaders(It.IsAny<string>())).Returns(new List<TocItem>());

            _viewModel.IsTocOpen = true;
            string? scrolledId = null;
            _viewModel.RequestScrollToHeader += (s, item) => scrolledId = item.Id;
            var item = new TocItem { Id = "target-id" };

            // Act
            _viewModel.NavigateToHeaderCommand.Execute(item);

            // Assert
            Assert.IsFalse(_viewModel.IsTocOpen);
            Assert.AreEqual("target-id", scrolledId);
        }

        [TestMethod]
        public void ToggleCssEditor_ShouldNotify()
        {
            // テスト観点: IsCssEditorVisibleプロパティの変更に応じて、プロパティ変更通知が発行されることを確認する。
            //            幅の制御はView側で行うため、ViewModelは可視状態のみを管理する。

            // Arrange
            var notifiedProperties = new List<string>();
            _viewModel.PropertyChanged += (sender, e) => notifiedProperties.Add(e.PropertyName!);
            _viewModel.IsCssEditorVisible = false;

            notifiedProperties.Clear();

            // Act: 表示をtrueにする
            _viewModel.IsCssEditorVisible = true;

            // Assert: 表示状態
            Assert.IsTrue(_viewModel.IsCssEditorVisible);
            Assert.IsTrue(notifiedProperties.Contains(nameof(MainViewModel.IsCssEditorVisible)));

            notifiedProperties.Clear();

            // Act: 表示をfalseにする
            _viewModel.IsCssEditorVisible = false;

            // Assert: 非表示状態
            Assert.IsFalse(_viewModel.IsCssEditorVisible);
            Assert.IsTrue(notifiedProperties.Contains(nameof(MainViewModel.IsCssEditorVisible)));
        }

        [TestMethod]
        public void CssEditorColumnWidth_ShouldUpdateAndNotify()
        {
            // テスト観点: CssEditorColumnWidthプロパティがdouble型として正しく更新され、通知されることを確認する。

            // Arrange
            var notifiedProperties = new List<string>();
            _viewModel.PropertyChanged += (sender, e) => notifiedProperties.Add(e.PropertyName!);
            var initialWidth = 300.0;
            var newWidth = 450.0;
            _viewModel.CssEditorColumnWidth = initialWidth;

            notifiedProperties.Clear();

            // Act
            _viewModel.CssEditorColumnWidth = newWidth;

            // Assert
            Assert.AreEqual(newWidth, _viewModel.CssEditorColumnWidth);
            Assert.IsTrue(notifiedProperties.Contains(nameof(MainViewModel.CssEditorColumnWidth)));
        }
    }
}
