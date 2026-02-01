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
        private OpenDocumentUseCase _useCase = null!;

        [TestInitialize]
        public void Setup()
        {
            _editorServiceMock = new Mock<IEditorService>();
            _fileServiceMock = new Mock<IFileService>();
            _dialogServiceMock = new Mock<IDialogService>();
            _saveDocumentUseCaseMock = new Mock<ISaveDocumentUseCase>();

            _useCase = new OpenDocumentUseCase(
                _editorServiceMock.Object,
                _fileServiceMock.Object,
                _dialogServiceMock.Object,
                _saveDocumentUseCaseMock.Object);
        }

        [TestMethod]
        public void Execute_ShouldCallOpenDialog_WhenNotDirty()
        {
            // テスト観点: ドキュメントに変更がない場合、保存確認を行わずにファイルを開くダイアログが表示されることを確認する。
            // Arrange
            _editorServiceMock.Setup(x => x.PromptForSaveIfDirty()).Returns(SaveConfirmationResult.NoAction);
            _dialogServiceMock.Setup(x => x.ShowOpenFileDialog(It.IsAny<string>(), It.IsAny<string>())).Returns("test.md");
            var doc = new MarkdownDocument { Content = "# Content" };
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
            // テスト観点: 保存確認でキャンセルが選択された場合、ファイルを開く処理が中止されることを確認する。
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
            // テスト観点: 保存確認で「保存」が選択された場合、保存処理の後にファイルを開くダイアログが表示され、ドキュメントが読み込まれることを確認する。
            // Arrange
            _editorServiceMock.Setup(x => x.PromptForSaveIfDirty()).Returns(SaveConfirmationResult.Save);
            _saveDocumentUseCaseMock.Setup(x => x.Execute()).Returns(true);
            _dialogServiceMock.Setup(x => x.ShowOpenFileDialog(It.IsAny<string>(), It.IsAny<string>())).Returns("test.md");
            var doc = new MarkdownDocument { Content = "# Content" };
            _fileServiceMock.Setup(x => x.Open("test.md")).Returns(doc);

            // Act
            _useCase.Execute();

            // Assert
            _saveDocumentUseCaseMock.Verify(x => x.Execute(), Times.Once);
            _editorServiceMock.Verify(x => x.LoadDocument(doc), Times.Once);
        }

        [TestMethod]
        public void Execute_ShouldAbort_WhenSaveFails()
        {
            // テスト観点: 保存確認で「保存」を選択したが、保存処理自体が失敗（またはキャンセル）した場合、
            //            ファイルを開く処理が中断されることを確認する。
            // Arrange
            _editorServiceMock.Setup(x => x.PromptForSaveIfDirty()).Returns(SaveConfirmationResult.Save);
            _saveDocumentUseCaseMock.Setup(x => x.Execute()).Returns(false); // 保存失敗

            // Act
            _useCase.Execute();

            // Assert
            _saveDocumentUseCaseMock.Verify(x => x.Execute(), Times.Once);
            _dialogServiceMock.Verify(x => x.ShowOpenFileDialog(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public void Execute_ShouldOpenWithoutSave_WhenUserSelectsDiscard()
        {
            // テスト観点: 保存確認で「破棄」が選択された場合、保存処理を行わずにファイルを開くダイアログが表示され、ドキュメントが読み込まれることを確認する。
            // Arrange
            _editorServiceMock.Setup(x => x.PromptForSaveIfDirty()).Returns(SaveConfirmationResult.Discard);
            _dialogServiceMock.Setup(x => x.ShowOpenFileDialog(It.IsAny<string>(), It.IsAny<string>())).Returns("test.md");
            var doc = new MarkdownDocument { Content = "# Content" };
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
            // テスト観点: ファイルを開くダイアログでキャンセルされた場合、ドキュメントの読み込みが行われないことを確認する。
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

            // Act
            _useCase.OpenPath("test.md");

            // Assert
            _editorServiceMock.Verify(x => x.LoadDocument(It.Is<MarkdownDocument>(d => d.FrontMatter["css"].ToString() == "report.css")), Times.Once);
        }
    }
}
