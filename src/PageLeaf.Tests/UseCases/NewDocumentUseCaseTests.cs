using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PageLeaf.Models;
using PageLeaf.Services;
using PageLeaf.UseCases;

namespace PageLeaf.Tests.UseCases
{
    [TestClass]
    public class NewDocumentUseCaseTests
    {
        private Mock<IEditorService> _editorServiceMock = null!;
        private Mock<ISaveDocumentUseCase> _saveDocumentUseCaseMock = null!;
        private Mock<IMarkdownService> _markdownServiceMock = null!;
        private NewDocumentUseCase _useCase = null!;

        [TestInitialize]
        public void Setup()
        {
            _editorServiceMock = new Mock<IEditorService>();
            _saveDocumentUseCaseMock = new Mock<ISaveDocumentUseCase>();
            _markdownServiceMock = new Mock<IMarkdownService>();

            _useCase = new NewDocumentUseCase(_editorServiceMock.Object, _saveDocumentUseCaseMock.Object, _markdownServiceMock.Object);
        }

        [TestMethod]
        public void Execute_ShouldCallNewDocument_WhenNotDirty()
        {
            // Arrange
            _editorServiceMock.Setup(x => x.PromptForSaveIfDirty()).Returns(SaveConfirmationResult.NoAction);
            _editorServiceMock.Setup(x => x.CurrentDocument).Returns(new MarkdownDocument());

            // Act
            _useCase.Execute();

            // Assert
            _editorServiceMock.Verify(x => x.NewDocument(), Times.Once);
            _saveDocumentUseCaseMock.Verify(x => x.Execute(), Times.Never);
        }

        [TestMethod]
        public void Execute_ShouldCancel_WhenUserCancels()
        {
            // Arrange
            _editorServiceMock.Setup(x => x.PromptForSaveIfDirty()).Returns(SaveConfirmationResult.Cancel);

            // Act
            _useCase.Execute();

            // Assert
            _editorServiceMock.Verify(x => x.NewDocument(), Times.Never);
            _saveDocumentUseCaseMock.Verify(x => x.Execute(), Times.Never);
        }

        [TestMethod]
        public void Execute_ShouldSaveAndNewDocument_WhenUserSelectsSave()
        {
            // Arrange
            _editorServiceMock.Setup(x => x.PromptForSaveIfDirty()).Returns(SaveConfirmationResult.Save);
            _editorServiceMock.Setup(x => x.CurrentDocument).Returns(new MarkdownDocument());

            // Act
            _useCase.Execute();

            // Assert
            _saveDocumentUseCaseMock.Verify(x => x.Execute(), Times.Once);
            _editorServiceMock.Verify(x => x.NewDocument(), Times.Once);
        }

        [TestMethod]
        public void Execute_ShouldNewDocumentWithoutSave_WhenUserSelectsDiscard()
        {
            // Arrange
            _editorServiceMock.Setup(x => x.PromptForSaveIfDirty()).Returns(SaveConfirmationResult.Discard);
            _editorServiceMock.Setup(x => x.CurrentDocument).Returns(new MarkdownDocument());

            // Act
            _useCase.Execute();

            // Assert
            _saveDocumentUseCaseMock.Verify(x => x.Execute(), Times.Never);
            _editorServiceMock.Verify(x => x.NewDocument(), Times.Once);
        }

        [TestMethod]
        public void Execute_ShouldApplyTemplate_WhenNewDocumentIsCreated()
        {
            // Arrange
            var doc = new MarkdownDocument();
            _editorServiceMock.Setup(x => x.PromptForSaveIfDirty()).Returns(SaveConfirmationResult.NoAction);
            _editorServiceMock.Setup(x => x.CurrentDocument).Returns(doc);

            // Act
            _useCase.Execute();

            // Assert
            Assert.AreEqual("Untitled", doc.FrontMatter["title"]);
            Assert.IsTrue(doc.FrontMatter.ContainsKey("created"));
            Assert.IsTrue(doc.FrontMatter.ContainsKey("updated"));
            StringAssert.StartsWith(doc.Content, "# Untitled");
        }
    }
}
