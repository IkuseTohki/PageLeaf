using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PageLeaf.Models;
using PageLeaf.Services;
using PageLeaf.UseCases;
using System;

namespace PageLeaf.Tests.UseCases
{
    [TestClass]
    public class OpenDocumentUseCaseTests
    {
        private Mock<IEditorService> _editorServiceMock = null!;
        private Mock<IFileService> _fileServiceMock = null!;
        private Mock<IDialogService> _dialogServiceMock = null!;
        private Mock<ISaveDocumentUseCase> _saveDocumentUseCaseMock = null!;
        private Mock<IMarkdownService> _markdownServiceMock = null!;
        private OpenDocumentUseCase _useCase = null!;

        [TestInitialize]
        public void Setup()
        {
            _editorServiceMock = new Mock<IEditorService>();
            _fileServiceMock = new Mock<IFileService>();
            _dialogServiceMock = new Mock<IDialogService>();
            _saveDocumentUseCaseMock = new Mock<ISaveDocumentUseCase>();
            _markdownServiceMock = new Mock<IMarkdownService>();

            // Splitのデフォルト動作を設定
            _markdownServiceMock.Setup(x => x.Split(It.IsAny<string>()))
                .Returns((new System.Collections.Generic.Dictionary<string, object>(), "body"));

            _useCase = new OpenDocumentUseCase(
                _editorServiceMock.Object,
                _fileServiceMock.Object,
                _dialogServiceMock.Object,
                _saveDocumentUseCaseMock.Object,
                _markdownServiceMock.Object);
        }

        [TestMethod]
        public void Execute_ShouldCallOpenDialog_WhenNotDirty()
        {
            // Arrange
            _editorServiceMock.Setup(x => x.PromptForSaveIfDirty()).Returns(SaveConfirmationResult.NoAction);
            _dialogServiceMock.Setup(x => x.ShowOpenFileDialog(It.IsAny<string>(), It.IsAny<string>())).Returns("test.md");
            var doc = new MarkdownDocument();
            _fileServiceMock.Setup(x => x.Open("test.md")).Returns(doc);

            // Act
            _useCase.Execute();

            // Assert
            _dialogServiceMock.Verify(x => x.ShowOpenFileDialog(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
            _editorServiceMock.Verify(x => x.LoadDocument(doc), Times.Once);
        }

        [TestMethod]
        public void Execute_ShouldCancel_WhenUserCancelsSavePrompt()
        {
            // Arrange
            _editorServiceMock.Setup(x => x.PromptForSaveIfDirty()).Returns(SaveConfirmationResult.Cancel);

            // Act
            _useCase.Execute();

            // Assert
            _dialogServiceMock.Verify(x => x.ShowOpenFileDialog(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public void Execute_ShouldSaveAndOpen_WhenUserSelectsSave()
        {
            // Arrange
            _editorServiceMock.Setup(x => x.PromptForSaveIfDirty()).Returns(SaveConfirmationResult.Save);
            _dialogServiceMock.Setup(x => x.ShowOpenFileDialog(It.IsAny<string>(), It.IsAny<string>())).Returns("test.md");
            var doc = new MarkdownDocument();
            _fileServiceMock.Setup(x => x.Open("test.md")).Returns(doc);

            // Act
            _useCase.Execute();

            // Assert
            _saveDocumentUseCaseMock.Verify(x => x.Execute(), Times.Once);
            _editorServiceMock.Verify(x => x.LoadDocument(doc), Times.Once);
        }

        [TestMethod]
        public void Execute_ShouldOpenWithoutSave_WhenUserSelectsDiscard()
        {
            // Arrange
            _editorServiceMock.Setup(x => x.PromptForSaveIfDirty()).Returns(SaveConfirmationResult.Discard);
            _dialogServiceMock.Setup(x => x.ShowOpenFileDialog(It.IsAny<string>(), It.IsAny<string>())).Returns("test.md");
            var doc = new MarkdownDocument();
            _fileServiceMock.Setup(x => x.Open("test.md")).Returns(doc);

            // Act
            _useCase.Execute();

            // Assert
            _saveDocumentUseCaseMock.Verify(x => x.Execute(), Times.Never);
            _editorServiceMock.Verify(x => x.LoadDocument(doc), Times.Once);
        }

        [TestMethod]
        public void Execute_ShouldNotLoad_WhenOpenDialogIsCancelled()
        {
            // Arrange
            _editorServiceMock.Setup(x => x.PromptForSaveIfDirty()).Returns(SaveConfirmationResult.NoAction);
            _dialogServiceMock.Setup(x => x.ShowOpenFileDialog(It.IsAny<string>(), It.IsAny<string>())).Returns((string?)null);

            // Act
            _useCase.Execute();

            // Assert
            _editorServiceMock.Verify(x => x.LoadDocument(It.IsAny<MarkdownDocument>()), Times.Never);
        }
    }
}
