using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PageLeaf.Models;
using PageLeaf.Services;
using PageLeaf.UseCases;
using System;

namespace PageLeaf.Tests.UseCases
{
    [TestClass]
    public class SaveAsDocumentUseCaseTests
    {
        private Mock<IEditorService> _editorServiceMock = null!;
        private Mock<IFileService> _fileServiceMock = null!;
        private Mock<IDialogService> _dialogServiceMock = null!;
        private SaveAsDocumentUseCase _useCase = null!;

        [TestInitialize]
        public void Setup()
        {
            _editorServiceMock = new Mock<IEditorService>();
            _fileServiceMock = new Mock<IFileService>();
            _dialogServiceMock = new Mock<IDialogService>();
            _useCase = new SaveAsDocumentUseCase(_editorServiceMock.Object, _fileServiceMock.Object, _dialogServiceMock.Object);
        }

        [TestMethod]
        public void Execute_ShouldReturnFalse_WhenCurrentDocumentIsNull()
        {
            // Arrange
            _editorServiceMock.Setup(x => x.CurrentDocument).Returns((MarkdownDocument)null!);

            // Act
            var result = _useCase.Execute();

            // Assert
            Assert.IsFalse(result);
            _dialogServiceMock.Verify(x => x.ShowSaveFileDialog(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public void Execute_ShouldNotSave_WhenDialogIsCancelled()
        {
            // Arrange
            var doc = new MarkdownDocument { FilePath = "old.md" };
            _editorServiceMock.Setup(x => x.CurrentDocument).Returns(doc);
            _dialogServiceMock.Setup(x => x.ShowSaveFileDialog(It.IsAny<string>(), It.IsAny<string>(), "old.md")).Returns((string?)null);

            // Act
            var result = _useCase.Execute();

            // Assert
            Assert.IsFalse(result);
            _fileServiceMock.Verify(x => x.Save(It.IsAny<MarkdownDocument>()), Times.Never);
        }

        [TestMethod]
        public void Execute_ShouldSaveToNewPath_WhenPathIsProvided()
        {
            // Arrange
            var doc = new MarkdownDocument { FilePath = "old.md", IsDirty = true };
            _editorServiceMock.Setup(x => x.CurrentDocument).Returns(doc);
            _dialogServiceMock.Setup(x => x.ShowSaveFileDialog(It.IsAny<string>(), It.IsAny<string>(), "old.md")).Returns("new.md");

            // Act
            var result = _useCase.Execute();

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual("new.md", doc.FilePath);
            Assert.IsFalse(doc.IsDirty, "IsDirty should be false after successful Save As.");
            _fileServiceMock.Verify(x => x.Save(doc), Times.Once);
        }

        [TestMethod]
        public void Execute_ShouldReturnFalse_WhenSaveThrowsException()
        {
            // Arrange
            var doc = new MarkdownDocument { FilePath = "old.md" };
            _editorServiceMock.Setup(x => x.CurrentDocument).Returns(doc);
            _dialogServiceMock.Setup(x => x.ShowSaveFileDialog(It.IsAny<string>(), It.IsAny<string>(), "old.md")).Returns("new.md");
            _fileServiceMock.Setup(x => x.Save(doc)).Throws(new Exception("Save failed"));

            // Act
            var result = _useCase.Execute();

            // Assert
            Assert.IsFalse(result);
        }
    }
}
