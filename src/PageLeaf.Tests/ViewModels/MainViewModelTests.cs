using Moq;
using PageLeaf.Models;
using PageLeaf.Services;
using PageLeaf.ViewModels;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace PageLeaf.Tests.ViewModels
{
    [TestClass]
    public class MainViewModelTests
    {
        private readonly Mock<IFileService> _mockFileService;
        private readonly Mock<ILogger<MainViewModel>> _mockLogger;
        private readonly Mock<IDialogService> _mockDialogService;
        private readonly Mock<IMarkdownService> _mockMarkdownService; // 追加
        private readonly MainViewModel _viewModel;

        public MainViewModelTests()
        {
            _mockFileService = new Mock<IFileService>();
            _mockLogger = new Mock<ILogger<MainViewModel>>();
            _mockDialogService = new Mock<IDialogService>();
            _mockMarkdownService = new Mock<IMarkdownService>(); // 追加
            _viewModel = new MainViewModel(_mockFileService.Object, _mockLogger.Object, _mockDialogService.Object, _mockMarkdownService.Object); // 引数に追加
        }

        [TestMethod]
        public void OpenFileCommand_ShouldUpdateCurrentDocumentAndNotify_WhenFileSelectedAndOpenedSuccessfully()
        {
            // テスト観点: OpenFileCommand が正常なファイルパスを受け取った際に、CurrentDocument が正しく更新され、PropertyChanged イベントが発行されることを確認する。
            // Arrange
            string testFilePath = @"C:\test\test.md";
            string fileContent = "# Test Markdown";
            var markdownDocument = new MarkdownDocument { FilePath = testFilePath, Content = fileContent };

            _mockDialogService.Setup(s => s.ShowOpenFileDialog(It.IsAny<string>(), It.IsAny<string>()))
                              .Returns(testFilePath);
            _mockFileService.Setup(s => s.Open(testFilePath))
                            .Returns(markdownDocument);

            var receivedEvents = new List<string>();
            _viewModel.PropertyChanged += (sender, e) => { receivedEvents.Add(e.PropertyName!); };

            // Act
            _viewModel.OpenFileCommand.Execute(null);

            // Assert
            Assert.IsNotNull(_viewModel.CurrentDocument);
            Assert.AreEqual(testFilePath, _viewModel.CurrentDocument.FilePath);
            Assert.AreEqual(fileContent, _viewModel.CurrentDocument.Content);
            _mockFileService.Verify(s => s.Open(testFilePath), Times.Once);
            CollectionAssert.Contains(receivedEvents, nameof(MainViewModel.CurrentDocument));
        }
        [TestMethod]
        public void OpenFileCommand_ShouldNotChangeCurrentDocument_WhenDialogIsCancelled()
        {
            // テスト観点: OpenFileCommand がファイル選択ダイアログでキャンセルされた場合、CurrentDocument が変更されないことを確認する。
            // Arrange
            var initialDocument = _viewModel.CurrentDocument;
            _mockDialogService.Setup(s => s.ShowOpenFileDialog(It.IsAny<string>(), It.IsAny<string>()))
                              .Returns((string?)null);

            // Act
            _viewModel.OpenFileCommand.Execute(null);

            // Assert
            Assert.AreSame(initialDocument, _viewModel.CurrentDocument);
        }

        [TestMethod]
        public void OpenFileCommand_ShouldLogExceptionAndNotChangeCurrentDocument_WhenFileServiceThrowsException()
        {
            // テスト観点: OpenFileCommand がファイル読み込みに失敗した場合、エラーが適切にログに記録され、CurrentDocument が変更されないことを確認する。
            // Arrange
            string testFilePath = "C:\test\test.md";
            var initialDocument = _viewModel.CurrentDocument;
            var exception = new System.IO.FileNotFoundException("File not found");

            _mockDialogService.Setup(s => s.ShowOpenFileDialog(It.IsAny<string>(), It.IsAny<string>()))
                              .Returns(testFilePath);
            _mockFileService.Setup(s => s.Open(testFilePath))
                            .Throws(exception);

            // Act
            _viewModel.OpenFileCommand.Execute(null);

            // Assert
            Assert.AreSame(initialDocument, _viewModel.CurrentDocument);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to open file")), // ログメッセージの検証
                    exception,
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);
        }

        [TestMethod]
        public void Constructor_ShouldThrowArgumentNullException_WhenFileServiceIsNull()
        {
            // テスト観点: IFileService が null の場合に ArgumentNullException がスローされることを確認する。
            // Arrange, Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new MainViewModel(null!, _mockLogger.Object, _mockDialogService.Object, _mockMarkdownService.Object));
        }

        [TestMethod]
        public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
        {
            // テスト観点: ILogger が null の場合に ArgumentNullException がスローされることを確認する。
            // Arrange, Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new MainViewModel(_mockFileService.Object, null!, _mockDialogService.Object, _mockMarkdownService.Object));
        }

        [TestMethod]
        public void Constructor_ShouldThrowArgumentNullException_WhenDialogServiceIsNull()
        {
            // テスト観点: IDialogService が null の場合に ArgumentNullException がスローされることを確認する。
            // Arrange, Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new MainViewModel(_mockFileService.Object, _mockLogger.Object, null!, _mockMarkdownService.Object));
        }

        [TestMethod]
        public void Constructor_ShouldThrowArgumentNullException_WhenMarkdownServiceIsNull()
        {
            // テスト観点: IMarkdownService が null の場合に ArgumentNullException がスローされることを確認する。
            // Arrange, Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new MainViewModel(_mockFileService.Object, _mockLogger.Object, _mockDialogService.Object, null!));
        }

        [TestMethod]
        public void SaveFileCommand_ShouldCallFileServiceSave_WhenDocumentHasFilePath()
        {
            // テスト観点: SaveFileCommand が、ファイルパスを持つ CurrentDocument の内容を IFileService.Save を介して上書き保存することを確認する。
            // Arrange
            string testFilePath = @"C:\test\existing.md";
            string fileContent = "# Existing Content";
            var documentToSave = new MarkdownDocument { FilePath = testFilePath, Content = fileContent };
            _viewModel.CurrentDocument = documentToSave;

            // Act
            _viewModel.SaveFileCommand.Execute(null);

            // Assert
            _mockFileService.Verify(s => s.Save(It.Is<MarkdownDocument>(doc => doc.FilePath == testFilePath && doc.Content == fileContent)), Times.Once);
        }

        [TestMethod]
        public void SaveAsFileCommand_ShouldCallFileServiceSave_WhenUserSelectsFilePath()
        {
            // テスト観点: SaveAsFileCommand が、IDialogService.ShowSaveFileDialog を呼び出し、
            //             ユーザーがファイルパスを選択した場合に IFileService.Save が
            //             正しい内容と新しいファイルパスで呼び出されることを確認する。
            // Arrange
            string initialFilePath = @"C:\test\initial.md";
            string initialContent = "# Initial Content";
            var initialDocument = new MarkdownDocument { FilePath = initialFilePath, Content = initialContent };
            _viewModel.CurrentDocument = initialDocument;

            string newFilePath = @"C:\new\path\to\file.md";
            _mockDialogService.Setup(s => s.ShowSaveFileDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()))
                              .Returns(newFilePath);

            // Act
            _viewModel.SaveAsFileCommand.Execute(null);

            // Assert
            _mockDialogService.Verify(s => s.ShowSaveFileDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()), Times.Once);
            _mockFileService.Verify(s => s.Save(It.Is<MarkdownDocument>(doc =>
                doc.FilePath == newFilePath && doc.Content == initialContent)), Times.Once);
            Assert.AreEqual(newFilePath, _viewModel.CurrentDocument.FilePath, "CurrentDocument.FilePath should be updated to the new path.");
        }

        [TestMethod]
        public void SelectedMode_Changes_And_Notifies()
        {
            // テスト観点: SelectedMode プロパティが変更されたときに、PropertyChanged イベントが正しく発行されることを確認する。
            // Arrange
            var receivedEvents = new List<string>();
            _viewModel.PropertyChanged += (sender, e) => { receivedEvents.Add(e.PropertyName!); };

            // Act
            // 初期値がViewerである可能性があるので、異なる値に設定してからViewerに戻す
            _viewModel.SelectedMode = DisplayMode.Markdown; // まず異なる値に設定
            _viewModel.SelectedMode = DisplayMode.Viewer;   // その後、テストしたい値に設定

            // Assert
            CollectionAssert.Contains(receivedEvents, nameof(MainViewModel.SelectedMode));
        }

        [TestMethod]
        public void IsMarkdownEditorVisible_ShouldBeTrue_WhenSelectedModeIsMarkdown()
        {
            // テスト観点: SelectedMode が DisplayMode.Markdown のとき、IsMarkdownEditorVisible が true になることを確認する。
            // Arrange
            _viewModel.SelectedMode = DisplayMode.Viewer; // 初期状態をViewerにしておく

            // Act
            _viewModel.SelectedMode = DisplayMode.Markdown;

            // Assert
            Assert.IsTrue(_viewModel.IsMarkdownEditorVisible);
            Assert.IsFalse(_viewModel.IsViewerVisible);
        }

        [TestMethod]
        public void IsViewerVisible_ShouldBeTrue_WhenSelectedModeIsViewer()
        {
            // テスト観点: SelectedMode が DisplayMode.Viewer のとき、IsViewerVisible が true になることを確認する。
            // Arrange
            _viewModel.SelectedMode = DisplayMode.Markdown; // 初期状態をMarkdownにしておく

            // Act
            _viewModel.SelectedMode = DisplayMode.Viewer;

            // Assert
            Assert.IsTrue(_viewModel.IsViewerVisible);
            Assert.IsFalse(_viewModel.IsMarkdownEditorVisible);
        }
        [TestMethod]
        public void HtmlContent_ShouldBeUpdatedAndNotify_WhenCurrentDocumentContentChangesAndModeIsViewer()
        {
            // テスト観点: CurrentDocument の Content が変更され、かつモードが Viewer のときに、
            //             IMarkdownService を介して HTML 変換が行われ、HtmlContent が更新され、
            //             PropertyChanged イベントが発行されることを確認する。
            // Arrange
            var mockMarkdownService = new Mock<IMarkdownService>();
            var viewModel = new MainViewModel(_mockFileService.Object, _mockLogger.Object, _mockDialogService.Object, mockMarkdownService.Object); // IMarkdownService を追加
            viewModel.SelectedMode = DisplayMode.Viewer; // Viewerモードに設定

            string markdownContent = "# Test Markdown";
            string expectedHtml = "<h1>Test Markdown</h1>";
            mockMarkdownService.Setup(s => s.ConvertToHtml(markdownContent)).Returns(expectedHtml);

            var receivedEvents = new List<string>();
            viewModel.PropertyChanged += (sender, e) => { receivedEvents.Add(e.PropertyName!); };

            // Act
            viewModel.CurrentDocument = new MarkdownDocument { Content = markdownContent };

            // Assert
            mockMarkdownService.Verify(s => s.ConvertToHtml(markdownContent), Times.Once);
            Assert.AreEqual(expectedHtml, viewModel.HtmlContent);
            CollectionAssert.Contains(receivedEvents, nameof(MainViewModel.HtmlContent));
        }

        [TestMethod]
        public void HtmlContent_ShouldBeUpdatedAndNotify_WhenSelectedModeChangesToViewer()
        {
            // テスト観点: SelectedMode が Viewer に変更されたときに、
            //             IMarkdownService を介して HTML 変換が行われ、HtmlContent が更新され、
            //             PropertyChanged イベントが発行されることを確認する。
            // Arrange
            var mockMarkdownService = new Mock<IMarkdownService>();
            var viewModel = new MainViewModel(_mockFileService.Object, _mockLogger.Object, _mockDialogService.Object, mockMarkdownService.Object); // IMarkdownService を追加
            viewModel.CurrentDocument = new MarkdownDocument { Content = "# Initial Markdown" };
            viewModel.SelectedMode = DisplayMode.Markdown; // 初期モードをMarkdownに設定

            string markdownContent = "# Initial Markdown";
            string expectedHtml = "<h1>Initial Markdown</h1>";
            mockMarkdownService.Setup(s => s.ConvertToHtml(markdownContent)).Returns(expectedHtml);

            var receivedEvents = new List<string>();
            viewModel.PropertyChanged += (sender, e) => { receivedEvents.Add(e.PropertyName!); };

            // Act
            viewModel.SelectedMode = DisplayMode.Viewer; // Viewerモードに切り替え

            // Assert
            mockMarkdownService.Verify(s => s.ConvertToHtml(markdownContent), Times.AtLeastOnce()); // 少なくとも1回呼び出されることを検証
            Assert.AreEqual(expectedHtml, viewModel.HtmlContent);
            CollectionAssert.Contains(receivedEvents, nameof(MainViewModel.HtmlContent));
        }
    }
}
