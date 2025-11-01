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
        private readonly MainViewModel _viewModel;

        public MainViewModelTests()
        {
            _mockFileService = new Mock<IFileService>();
            _mockLogger = new Mock<ILogger<MainViewModel>>();
            _mockDialogService = new Mock<IDialogService>();
            _viewModel = new MainViewModel(_mockFileService.Object, _mockLogger.Object, _mockDialogService.Object);
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
            Assert.ThrowsException<ArgumentNullException>(() => new MainViewModel(null!, _mockLogger.Object, _mockDialogService.Object));
        }

        [TestMethod]
        public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
        {
            // テスト観点: ILogger が null の場合に ArgumentNullException がスローされることを確認する。
            // Arrange, Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new MainViewModel(_mockFileService.Object, null!, _mockDialogService.Object));
        }

        [TestMethod]
        public void Constructor_ShouldThrowArgumentNullException_WhenDialogServiceIsNull()
        {
            // テスト観点: IDialogService が null の場合に ArgumentNullException がスローされることを確認する。
            // Arrange, Act & Assert
            Assert.ThrowsException<ArgumentNullException>(() => new MainViewModel(_mockFileService.Object, _mockLogger.Object, null!));
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
    }
}
