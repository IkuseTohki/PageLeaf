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

        [TestMethod]
        public void Execute_ShouldLoadDocument_WhenFrontMatterHasCssProperty()
        {
            // テスト観点: フロントマターにcssプロパティがある場合でも、正常にドキュメントがロードされることを確認する。
            // Arrange
            _editorServiceMock.Setup(x => x.PromptForSaveIfDirty()).Returns(SaveConfirmationResult.NoAction);
            _dialogServiceMock.Setup(x => x.ShowOpenFileDialog(It.IsAny<string>(), It.IsAny<string>())).Returns("test.md");

            var docContent = "---\ncss: report.css\n---\n# Body";
            var doc = new MarkdownDocument { Content = docContent };
            _fileServiceMock.Setup(x => x.Open("test.md")).Returns(doc);

            var frontMatter = new System.Collections.Generic.Dictionary<string, object> { { "css", "report.css" } };
            _markdownServiceMock.Setup(x => x.Split(docContent)).Returns((frontMatter, "# Body"));

            // Act
            _useCase.Execute();

            // Assert
            _editorServiceMock.Verify(x => x.LoadDocument(It.Is<MarkdownDocument>(d => d.FrontMatter["css"].ToString() == "report.css")), Times.Once);
        }

        [TestMethod]
        public void OpenPath_ShouldLoadDocument_WhenFrontMatterHasCssProperty()
        {
            // テスト観点: OpenPath実行時、フロントマターにcssプロパティがある場合でも、正常にドキュメントがロードされることを確認する。
            // Arrange
            _editorServiceMock.Setup(x => x.PromptForSaveIfDirty()).Returns(SaveConfirmationResult.NoAction);

            var docContent = "---\ncss: report.css\n---\n# Body";
            var doc = new MarkdownDocument { Content = docContent };
            _fileServiceMock.Setup(x => x.Open("test.md")).Returns(doc);

            var frontMatter = new System.Collections.Generic.Dictionary<string, object> { { "css", "report.css" } };
            _markdownServiceMock.Setup(x => x.Split(docContent)).Returns((frontMatter, "# Body"));

            // Act
            _useCase.OpenPath("test.md");

            // Assert
            _editorServiceMock.Verify(x => x.LoadDocument(It.Is<MarkdownDocument>(d => d.FrontMatter["css"].ToString() == "report.css")), Times.Once);
        }
    }
}
