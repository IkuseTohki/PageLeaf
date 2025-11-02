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
            _mockFileService.Setup(s => s.FileExists(testFilePath)).Returns(true); // ファイルが存在するとモック

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

        [TestMethod]
        public void ExecuteSaveFile_WhenFileDoesNotExist_ShouldCallExecuteSaveAsFile()
        {
            // テスト観点: Editor.CurrentDocument.FilePath が存在しない場合、ExecuteSaveFile が ExecuteSaveAsFile を呼び出すことを検証する。
            // Arrange
            string nonExistentFilePath = @"C:\nonexistent\file.md";
            var documentToSave = new MarkdownDocument { FilePath = nonExistentFilePath, Content = "# New Content" };
            _mockEditorService.Setup(e => e.CurrentDocument).Returns(documentToSave);
            _mockFileService.Setup(s => s.FileExists(nonExistentFilePath)).Returns(false); // ファイルが存在しないとモック

            // ExecuteSaveAsFile が呼び出されたことを検証するためのモック
            // SaveAsFileCommand は DelegateCommand なので、直接 Verify できない。
            // そのため、SaveAsFileCommand が内部で呼び出す ShowSaveFileDialog をモックし、それが呼び出されることを検証する。
            _mockDialogService.Setup(s => s.ShowSaveFileDialog(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string?>()
            )).Returns((string?)null); // ダイアログはキャンセルされたと仮定

            // Act
            _viewModel.SaveFileCommand.Execute(null);

            // Assert
            // ExecuteSaveAsFile が呼び出されたことを間接的に検証
            _mockDialogService.Verify(s => s.ShowSaveFileDialog(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string?>()
            ), Times.Once);
            // ファイルが存在しないため、_fileService.Save は呼び出されないことを確認
            _mockFileService.Verify(s => s.Save(It.IsAny<MarkdownDocument>()), Times.Never);
        }

        [TestMethod]
        public void ExecuteSaveFile_WhenFileDoesNotExistAndSaveAsIsCanceled_ShouldNotSave()
        {
            // テスト観点: Editor.CurrentDocument.FilePath が存在せず、ExecuteSaveAsFile 内の ShowSaveFileDialog がキャンセルされた場合、_fileService.Save が呼び出されないことを検証する。
            // Arrange
            string nonExistentFilePath = @"C:\nonexistent\file.md";
            var documentToSave = new MarkdownDocument { FilePath = nonExistentFilePath, Content = "# New Content" };
            _mockEditorService.Setup(e => e.CurrentDocument).Returns(documentToSave);
            _mockFileService.Setup(s => s.FileExists(nonExistentFilePath)).Returns(false); // ファイルが存在しないとモック

            _mockDialogService.Setup(s => s.ShowSaveFileDialog(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string?>()
            )).Returns((string?)null); // ダイアログはキャンセルされたと仮定

            // Act
            _viewModel.SaveFileCommand.Execute(null);

            // Assert
            _mockFileService.Verify(s => s.Save(It.IsAny<MarkdownDocument>()), Times.Never);
        }

        [TestMethod]
        public void ExecuteSaveFile_WhenFileDoesNotExistAndSaveAsIsSuccessful_ShouldSaveWithNewPath()
        {
            // テスト観点: Editor.CurrentDocument.FilePath が存在せず、ExecuteSaveAsFile 内の ShowSaveFileDialog で新しいパスが選択された場合、Editor.CurrentDocument.FilePath が更新され、_fileService.Save が新しいパスで呼び出されることを検証する。
            // Arrange
            string nonExistentFilePath = @"C:\nonexistent\file.md";
            string newFilePath = @"C:\new\path\to\saved_file.md";
            string fileContent = "# New Content";
            var documentToSave = new MarkdownDocument { FilePath = nonExistentFilePath, Content = fileContent };
            _mockEditorService.Setup(e => e.CurrentDocument).Returns(documentToSave);
            _mockFileService.Setup(s => s.FileExists(nonExistentFilePath)).Returns(false); // ファイルが存在しないとモック

            _mockDialogService.Setup(s => s.ShowSaveFileDialog(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string?>()
            )).Returns(newFilePath); // 新しいパスが選択されたと仮定

            // Act
            _viewModel.SaveFileCommand.Execute(null);

            // Assert
            // Editor.CurrentDocument.FilePath が新しいパスに更新されていることを確認
            Assert.AreEqual(newFilePath, _mockEditorService.Object.CurrentDocument.FilePath);
            // _fileService.Save が新しいパスで呼び出されたことを確認
            _mockFileService.Verify(s => s.Save(It.Is<MarkdownDocument>(doc =>
                doc.FilePath == newFilePath && doc.Content == fileContent)), Times.Once);
        }

    }
}
