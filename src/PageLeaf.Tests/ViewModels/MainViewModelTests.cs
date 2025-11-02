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
        private Mock<IFileService> _mockFileService;
        private Mock<ILogger<MainViewModel>> _mockLogger;
        private Mock<IDialogService> _mockDialogService;
        private Mock<IEditorService> _mockEditorService;
        private MainViewModel _viewModel;

        [TestInitialize]
        public void Setup()
        {
            _mockFileService = new Mock<IFileService>();
            _mockLogger = new Mock<ILogger<MainViewModel>>();
            _mockDialogService = new Mock<IDialogService>();
            _mockEditorService = new Mock<IEditorService>();
            _viewModel = new MainViewModel(
                _mockFileService.Object,
                _mockLogger.Object,
                _mockDialogService.Object,
                _mockEditorService.Object);
        }

        [TestMethod]
        public void OpenFileCommand_ShouldCallEditorServiceLoadDocument_WhenFileSelected()
        {
            // テスト観点: OpenFileCommand が実行され、ファイルが選択された際に、
            //             IEditorService の LoadDocument メソッドが正しいドキュメントで呼び出されることを確認する。
            // Arrange
            string testFilePath = @"C:	est	est.md";
            var markdownDocument = new MarkdownDocument { FilePath = testFilePath, Content = "# Test" };

            _mockDialogService.Setup(s => s.ShowOpenFileDialog(It.IsAny<string>(), It.IsAny<string>()))
                              .Returns(testFilePath);
            _mockFileService.Setup(s => s.Open(testFilePath))
                            .Returns(markdownDocument);

            // Act
            _viewModel.OpenFileCommand.Execute(null);

            // Assert
            _mockEditorService.Verify(s => s.LoadDocument(markdownDocument), Times.Once);
        }

        [TestMethod]
        public void OpenFileCommand_ShouldDoNothing_WhenDialogIsCancelled()
        {
            // テスト観点: ファイル選択ダイアログがキャンセルされた場合、IEditorService のメソッドが何も呼ばれないことを確認する。
            // Arrange
            _mockDialogService.Setup(s => s.ShowOpenFileDialog(It.IsAny<string>(), It.IsAny<string>()))
                              .Returns((string?)null);

            // Act
            _viewModel.OpenFileCommand.Execute(null);

            // Assert
            _mockEditorService.Verify(s => s.LoadDocument(It.IsAny<MarkdownDocument>()), Times.Never);
        }

        [TestMethod]
        public void OpenFileCommand_ShouldLogExceptionAndDoNothing_WhenFileServiceThrowsException()
        {
            // テスト観点: ファイル読み込みに失敗した場合、エラーがログ記録され、IEditorService のメソッドが呼ばれないことを確認する。
            // Arrange
            string testFilePath = "C:\test\test.md";
            var exception = new System.IO.FileNotFoundException("File not found");

            _mockDialogService.Setup(s => s.ShowOpenFileDialog(It.IsAny<string>(), It.IsAny<string>()))
                              .Returns(testFilePath);
            _mockFileService.Setup(s => s.Open(testFilePath))
                            .Throws(exception);

            // Act
            _viewModel.OpenFileCommand.Execute(null);

            // Assert
            _mockEditorService.Verify(s => s.LoadDocument(It.IsAny<MarkdownDocument>()), Times.Never);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Failed to open file")),
                    exception,
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);
        }

        [TestMethod]
        public void Constructor_ShouldThrowArgumentNullException_WhenFileServiceIsNull()
        {
            // テスト観点: IFileService が null の場合に ArgumentNullException がスローされることを確認する。
            Assert.ThrowsException<ArgumentNullException>(() => new MainViewModel(null!, _mockLogger.Object, _mockDialogService.Object, _mockEditorService.Object));
        }

        [TestMethod]
        public void Constructor_ShouldThrowArgumentNullException_WhenLoggerIsNull()
        {
            // テスト観点: ILogger が null の場合に ArgumentNullException がスローされることを確認する。
            Assert.ThrowsException<ArgumentNullException>(() => new MainViewModel(_mockFileService.Object, null!, _mockDialogService.Object, _mockEditorService.Object));
        }

        [TestMethod]
        public void Constructor_ShouldThrowArgumentNullException_WhenDialogServiceIsNull()
        {
            // テスト観点: IDialogService が null の場合に ArgumentNullException がスローされることを確認する。
            Assert.ThrowsException<ArgumentNullException>(() => new MainViewModel(_mockFileService.Object, _mockLogger.Object, null!, _mockEditorService.Object));
        }

        [TestMethod]
        public void Constructor_ShouldThrowArgumentNullException_WhenEditorServiceIsNull()
        {
            // テスト観点: IEditorService が null の場合に ArgumentNullException がスローされることを確認する。
            Assert.ThrowsException<ArgumentNullException>(() => new MainViewModel(_mockFileService.Object, _mockLogger.Object, _mockDialogService.Object, null!));
        }

        [TestMethod]
        public void SaveFileCommand_ShouldCallFileServiceSave_WhenDocumentHasFilePath()
        {
            // テスト観点: SaveFileCommand が、Editor.CurrentDocument の内容を IFileService.Save を介して上書き保存することを確認する。
            // Arrange
            string testFilePath = @"C:\test\existing.md";
            string fileContent = "# Existing Content";
            var documentToSave = new MarkdownDocument { FilePath = testFilePath, Content = fileContent };
            _mockEditorService.Setup(e => e.CurrentDocument).Returns(documentToSave);

            // Act
            _viewModel.SaveFileCommand.Execute(null);

            // Assert
            _mockFileService.Verify(s => s.Save(It.Is<MarkdownDocument>(doc => doc.FilePath == testFilePath && doc.Content == fileContent)), Times.Once);
        }

        [TestMethod]
        public void SaveAsFileCommand_ShouldCallFileServiceSave_WhenUserSelectsFilePath()
        {
            // テスト観点: SaveAsFileCommand が、正しい内容と新しいファイルパスで IFileService.Save を呼び出すことを確認する。
            // Arrange
            string initialContent = "# Initial Content";
            var initialDocument = new MarkdownDocument { Content = initialContent };
            _mockEditorService.Setup(e => e.CurrentDocument).Returns(initialDocument);

            string newFilePath = @"C:\new\path\to\file.md";
            _mockDialogService.Setup(s => s.ShowSaveFileDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>()))
                              .Returns(newFilePath);

            // Act
            _viewModel.SaveAsFileCommand.Execute(null);

            // Assert
            _mockFileService.Verify(s => s.Save(It.Is<MarkdownDocument>(doc =>
                doc.FilePath == newFilePath && doc.Content == initialContent)), Times.Once);
        }

    }
}
