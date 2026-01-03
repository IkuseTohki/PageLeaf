using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PageLeaf.Models;
using PageLeaf.Services;
using PageLeaf.UseCases;
using System;

namespace PageLeaf.Tests.UseCases
{
    [TestClass]
    public class SaveDocumentUseCaseTests
    {
        private Mock<IEditorService> _editorServiceMock = null!;
        private Mock<IFileService> _fileServiceMock = null!;
        private Mock<ISaveAsDocumentUseCase> _saveAsDocumentUseCaseMock = null!;
        private SaveDocumentUseCase _useCase = null!;

        [TestInitialize]
        public void Setup()
        {
            _editorServiceMock = new Mock<IEditorService>();
            _fileServiceMock = new Mock<IFileService>();
            _saveAsDocumentUseCaseMock = new Mock<ISaveAsDocumentUseCase>();
            _useCase = new SaveDocumentUseCase(_editorServiceMock.Object, _fileServiceMock.Object, _saveAsDocumentUseCaseMock.Object);
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
            _fileServiceMock.Verify(x => x.Save(It.IsAny<MarkdownDocument>()), Times.Never);
        }

        [TestMethod]
        public void Execute_ShouldCallSaveAs_WhenFilePathIsEmpty()
        {
            // Arrange
            var doc = new MarkdownDocument { FilePath = string.Empty };
            _editorServiceMock.Setup(x => x.CurrentDocument).Returns(doc);
            _saveAsDocumentUseCaseMock.Setup(x => x.Execute()).Returns(true);

            // Act
            var result = _useCase.Execute();

            // Assert
            Assert.IsTrue(result);
            _saveAsDocumentUseCaseMock.Verify(x => x.Execute(), Times.Once);
            _fileServiceMock.Verify(x => x.Save(It.IsAny<MarkdownDocument>()), Times.Never);
        }

        [TestMethod]
        public void Execute_ShouldCallSaveAs_WhenFileDoesNotExist()
        {
            // Arrange
            var doc = new MarkdownDocument { FilePath = "not_exist.md" };
            _editorServiceMock.Setup(x => x.CurrentDocument).Returns(doc);
            _fileServiceMock.Setup(x => x.FileExists("not_exist.md")).Returns(false);
            _saveAsDocumentUseCaseMock.Setup(x => x.Execute()).Returns(true);

            // Act
            var result = _useCase.Execute();

            // Assert
            Assert.IsTrue(result);
            _saveAsDocumentUseCaseMock.Verify(x => x.Execute(), Times.Once);
            _fileServiceMock.Verify(x => x.Save(It.IsAny<MarkdownDocument>()), Times.Never);
        }

        [TestMethod]
        public void Execute_ShouldSaveFile_WhenFileExists()
        {
            // Arrange
            var doc = new MarkdownDocument { FilePath = "exist.md", IsDirty = true };
            _editorServiceMock.Setup(x => x.CurrentDocument).Returns(doc);
            _fileServiceMock.Setup(x => x.FileExists("exist.md")).Returns(true);

            // Act
            var result = _useCase.Execute();

            // Assert
            Assert.IsTrue(result);
            Assert.IsFalse(doc.IsDirty, "IsDirty should be false after successful save.");
            _fileServiceMock.Verify(x => x.Save(doc), Times.Once);
            _saveAsDocumentUseCaseMock.Verify(x => x.Execute(), Times.Never);
        }

        [TestMethod]
        public void Execute_ShouldReturnFalse_WhenSaveThrowsException()
        {
            // Arrange
            var doc = new MarkdownDocument { FilePath = "exist.md" };
            _editorServiceMock.Setup(x => x.CurrentDocument).Returns(doc);
            _fileServiceMock.Setup(x => x.FileExists("exist.md")).Returns(true);
            _fileServiceMock.Setup(x => x.Save(doc)).Throws(new Exception("Save failed"));

            // Act
            var result = _useCase.Execute();

            // Assert
            Assert.IsFalse(result);
            _fileServiceMock.Verify(x => x.Save(doc), Times.Once);
        }
    }
}
