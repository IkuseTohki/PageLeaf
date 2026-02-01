using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PageLeaf.Models;
using PageLeaf.Models.Markdown;
using PageLeaf.Models.Css;
using PageLeaf.Models.Css.Elements;
using PageLeaf.Models.Settings;
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
        private Mock<ISettingsService> _settingsServiceMock = null!;
        private NewDocumentUseCase _useCase = null!;

        [TestInitialize]
        public void Setup()
        {
            _editorServiceMock = new Mock<IEditorService>();
            _saveDocumentUseCaseMock = new Mock<ISaveDocumentUseCase>();
            _markdownServiceMock = new Mock<IMarkdownService>();
            _settingsServiceMock = new Mock<ISettingsService>();

            // デフォルト設定
            _settingsServiceMock.Setup(x => x.CurrentSettings).Returns(new ApplicationSettings());

            _useCase = new NewDocumentUseCase(
                _editorServiceMock.Object,
                _saveDocumentUseCaseMock.Object,
                _markdownServiceMock.Object,
                _settingsServiceMock.Object);
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
            // テスト観点: 新規作成時に、テンプレートの自動挿入設定が有効であれば、
            //            標準プロパティ（title, created, updated等）が適用されることを確認する。
            // Arrange
            var settings = new ApplicationSettings { AutoInsertFrontMatter = true };
            _settingsServiceMock.Setup(x => x.CurrentSettings).Returns(settings);

            var doc = new MarkdownDocument();
            _editorServiceMock.Setup(x => x.PromptForSaveIfDirty()).Returns(SaveConfirmationResult.NoAction);
            _editorServiceMock.Setup(x => x.CurrentDocument).Returns(doc);

            // Act
            _useCase.Execute();

            // Assert
            Assert.AreEqual("", doc.FrontMatter["title"]);
            Assert.IsTrue(doc.FrontMatter.ContainsKey("created"));
            Assert.IsTrue(doc.FrontMatter.ContainsKey("updated"));
            Assert.IsTrue(doc.FrontMatter.ContainsKey("css"));
            Assert.IsTrue(doc.FrontMatter.ContainsKey("syntax_highlight"));
            StringAssert.StartsWith(doc.Content, "# Untitled");
        }

        [TestMethod]
        public void Execute_ShouldNotApplyTemplate_WhenAutoInsertIsDisabled()
        {
            // テスト観点: 設定でフロントマターの自動挿入がオフになっている場合、
            //            新規作成時にフロントマターや初期コンテンツが挿入されないことを確認する。
            // Arrange
            var settings = new ApplicationSettings { AutoInsertFrontMatter = false };
            _settingsServiceMock.Setup(x => x.CurrentSettings).Returns(settings);

            var doc = new MarkdownDocument();
            _editorServiceMock.Setup(x => x.PromptForSaveIfDirty()).Returns(SaveConfirmationResult.NoAction);
            _editorServiceMock.Setup(x => x.CurrentDocument).Returns(doc);

            // Act
            _useCase.Execute();

            // Assert
            Assert.AreEqual(0, doc.FrontMatter.Count);
            Assert.AreEqual(string.Empty, doc.Content);
        }

        [TestMethod]
        public void Execute_ShouldApplyCustomFrontMatter_FromSettings()
        {
            // テスト観点: アプリ管理の標準プロパティに加えて、設定で定義された追加プロパティが
            //            新規作成時に適用されることを確認する。
            // Arrange
            var additionalFrontMatter = new System.Collections.Generic.List<FrontMatterAdditionalProperty>
            {
                new FrontMatterAdditionalProperty { Key = "author", Value = "Test Author" },
                new FrontMatterAdditionalProperty { Key = "tags", Value = "test" }
            };
            var settings = new ApplicationSettings
            {
                AutoInsertFrontMatter = true,
                AdditionalFrontMatter = additionalFrontMatter
            };
            _settingsServiceMock.Setup(x => x.CurrentSettings).Returns(settings);

            var doc = new MarkdownDocument();
            _editorServiceMock.Setup(x => x.PromptForSaveIfDirty()).Returns(SaveConfirmationResult.NoAction);
            _editorServiceMock.Setup(x => x.CurrentDocument).Returns(doc);

            // Act
            _useCase.Execute();

            // Assert
            // 標準プロパティ
            Assert.AreEqual("", doc.FrontMatter["title"]);
            // 追加プロパティ
            Assert.AreEqual("Test Author", doc.FrontMatter["author"]);
            Assert.AreEqual("test", doc.FrontMatter["tags"]);
        }
    }
}
