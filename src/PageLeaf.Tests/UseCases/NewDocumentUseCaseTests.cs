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
            // テスト観点: ドキュメントに変更がない場合、保存確認を行わずに新規作成処理が実行されることを確認する。
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
            // テスト観点: 保存確認でキャンセルが選択された場合、新規作成処理が中止されることを確認する。
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
            // テスト観点: 保存確認で「保存」が選択された場合、保存処理の後に新規作成処理が実行されることを確認する。
            // Arrange
            _editorServiceMock.Setup(x => x.PromptForSaveIfDirty()).Returns(SaveConfirmationResult.Save);
            _saveDocumentUseCaseMock.Setup(x => x.Execute()).Returns(true);
            _editorServiceMock.Setup(x => x.CurrentDocument).Returns(new MarkdownDocument());

            // Act
            _useCase.Execute();

            // Assert
            _saveDocumentUseCaseMock.Verify(x => x.Execute(), Times.Once);
            _editorServiceMock.Verify(x => x.NewDocument(), Times.Once);
        }

        [TestMethod]
        public void Execute_ShouldAbort_WhenSaveFails()
        {
            // テスト観点: 保存確認で「保存」を選択したが、保存処理自体が失敗（またはキャンセル）した場合、
            //            新規作成処理が中断されることを確認する。
            // Arrange
            _editorServiceMock.Setup(x => x.PromptForSaveIfDirty()).Returns(SaveConfirmationResult.Save);
            _saveDocumentUseCaseMock.Setup(x => x.Execute()).Returns(false); // 保存失敗

            // Act
            _useCase.Execute();

            // Assert
            _saveDocumentUseCaseMock.Verify(x => x.Execute(), Times.Once);
            _editorServiceMock.Verify(x => x.NewDocument(), Times.Never); // 中断されるべき
        }

        [TestMethod]
        public void Execute_ShouldNewDocumentWithoutSave_WhenUserSelectsDiscard()
        {
            // テスト観点: 保存確認で「破棄」が選択された場合、保存処理を行わずに新規作成処理が実行されることを確認する。
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
            // テスト観点: 新規作成時に、テンプレート（フロントマターや初期コンテンツ）が適用されることを確認する。
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
