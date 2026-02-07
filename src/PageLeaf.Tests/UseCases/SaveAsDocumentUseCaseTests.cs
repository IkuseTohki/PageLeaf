using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PageLeaf.Models;
using PageLeaf.Models.Markdown;
using PageLeaf.Models.Css;
using PageLeaf.Models.Css.Elements;
using PageLeaf.Models.Settings;
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
        private Mock<IEditingSupportService> _editingSupportServiceMock = null!;
        private SaveAsDocumentUseCase _useCase = null!;

        [TestInitialize]
        public void Setup()
        {
            _editorServiceMock = new Mock<IEditorService>();
            _fileServiceMock = new Mock<IFileService>();
            _dialogServiceMock = new Mock<IDialogService>();
            _editingSupportServiceMock = new Mock<IEditingSupportService>();

            // デフォルトではそのまま返す
            _editingSupportServiceMock.Setup(x => x.EnforceEmptyLineAtEnd(It.IsAny<string>())).Returns<string>(s => s);

            _useCase = new SaveAsDocumentUseCase(
                _editorServiceMock.Object,
                _fileServiceMock.Object,
                _dialogServiceMock.Object,
                _editingSupportServiceMock.Object);
        }

        [TestMethod]
        public void Execute_ShouldEnforceEmptyLineAtEnd()
        {
            // テスト観点: 名前を付けて保存時に IEditingSupportService.EnforceEmptyLineAtEnd が呼び出されることを確認する。
            // Arrange
            var doc = new MarkdownDocument { FilePath = "old.md", Content = "No newline", IsDirty = true };
            _editorServiceMock.Setup(x => x.CurrentDocument).Returns(doc);
            _dialogServiceMock.Setup(x => x.ShowSaveFileDialog(It.IsAny<string>(), It.IsAny<string>(), "old.md")).Returns("new.md");
            _editingSupportServiceMock.Setup(x => x.EnforceEmptyLineAtEnd(It.IsAny<string>())).Returns("No newline\n");

            // Act
            _useCase.Execute();

            // Assert
            _editingSupportServiceMock.Verify(x => x.EnforceEmptyLineAtEnd(It.IsAny<string>()), Times.Once);
            _fileServiceMock.Verify(x => x.Save(It.Is<MarkdownDocument>(d => d.Content == "No newline\n")), Times.Once);
        }

        [TestMethod]
        public void Execute_ShouldReturnFalse_WhenCurrentDocumentIsNull()
        {
            // テスト観点: 現在のドキュメントがnullの場合、処理が失敗しfalseが返されることを確認する。
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
            // テスト観点: ファイル保存ダイアログでキャンセルされた場合、保存処理が行われずfalseが返されることを確認する。
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
            // テスト観点: ファイル保存ダイアログで新しいパスが指定された場合、そのパスに保存され、ドキュメントの状態が更新されることを確認する。
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
            _fileServiceMock.Verify(x => x.Save(It.Is<MarkdownDocument>(d => d.FilePath == "new.md")), Times.Once);
        }

        [TestMethod]
        public void Execute_ShouldReturnFalse_WhenSaveThrowsException()
        {
            // テスト観点: 保存処理中に例外が発生した場合、例外が捕捉されfalseが返されることを確認する。
            // Arrange
            var doc = new MarkdownDocument { FilePath = "old.md" };
            _editorServiceMock.Setup(x => x.CurrentDocument).Returns(doc);
            _dialogServiceMock.Setup(x => x.ShowSaveFileDialog(It.IsAny<string>(), It.IsAny<string>(), "old.md")).Returns("new.md");
            _fileServiceMock.Setup(x => x.Save(It.IsAny<MarkdownDocument>())).Throws(new Exception("Save failed"));

            // Act
            var result = _useCase.Execute();

            // Assert
            Assert.IsFalse(result);
        }
    }
}
