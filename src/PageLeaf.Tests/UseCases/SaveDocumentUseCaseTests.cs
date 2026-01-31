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
        private Mock<IMarkdownService> _markdownServiceMock = null!;
        private Mock<ISettingsService> _settingsServiceMock = null!;
        private SaveDocumentUseCase _useCase = null!;

        [TestInitialize]
        public void Setup()
        {
            _editorServiceMock = new Mock<IEditorService>();
            _fileServiceMock = new Mock<IFileService>();
            _saveAsDocumentUseCaseMock = new Mock<ISaveAsDocumentUseCase>();
            _markdownServiceMock = new Mock<IMarkdownService>();
            _settingsServiceMock = new Mock<ISettingsService>();

            // デフォルトの振る舞い
            _markdownServiceMock.Setup(m => m.ParseFrontMatter(It.IsAny<string>()))
                .Returns(new System.Collections.Generic.Dictionary<string, object>());
            _settingsServiceMock.Setup(s => s.CurrentSettings).Returns(new ApplicationSettings());

            _useCase = new SaveDocumentUseCase(
                _editorServiceMock.Object,
                _fileServiceMock.Object,
                _saveAsDocumentUseCaseMock.Object,
                _markdownServiceMock.Object,
                _settingsServiceMock.Object);
        }

        [TestMethod]
        public void Execute_ShouldRenumberFootnotesIfEnabled()
        {
            // テスト観点: 設定が有効な場合、保存時に脚注番号が振り直されることを確認する。
            // Arrange
            var settings = new ApplicationSettings { RenumberFootnotesOnSave = true };
            _settingsServiceMock.Setup(s => s.CurrentSettings).Returns(settings);

            var doc = new MarkdownDocument
            {
                FilePath = "test.md",
                Content = "Text[^5]\n\n[^5]: Note",
                IsDirty = true
            };
            _editorServiceMock.Setup(x => x.CurrentDocument).Returns(doc);
            _fileServiceMock.Setup(x => x.FileExists("test.md")).Returns(true);
            _markdownServiceMock.Setup(m => m.Join(It.IsAny<System.Collections.Generic.Dictionary<string, object>>(), It.IsAny<string>()))
                .Returns((System.Collections.Generic.Dictionary<string, object> fm, string content) => content);

            // Act
            var result = _useCase.Execute();

            // Assert
            Assert.IsTrue(result);
            // 本文がリナンバリングされていること ( [^5] -> [^1] )
            // MarkdownFootnoteHelper.Renumber は末尾に定義を追加し、改行を付与する
            var expectedContent = "Text[^1]" + Environment.NewLine + Environment.NewLine + "[^1]: Note" + Environment.NewLine;
            Assert.AreEqual(expectedContent, doc.Content);
            _fileServiceMock.Verify(x => x.Save(It.Is<MarkdownDocument>(d => d.Content == expectedContent)), Times.Once);
        }

        [TestMethod]
        public void Execute_ShouldNotRenumberFootnotesIfDisabled()
        {
            // テスト観点: 設定が無効な場合、保存時に脚注番号が振り直されないことを確認する。
            // Arrange
            var settings = new ApplicationSettings { RenumberFootnotesOnSave = false };
            _settingsServiceMock.Setup(s => s.CurrentSettings).Returns(settings);

            var doc = new MarkdownDocument
            {
                FilePath = "test.md",
                Content = "Text[^5]\n\n[^5]: Note",
                IsDirty = true
            };
            _editorServiceMock.Setup(x => x.CurrentDocument).Returns(doc);
            _fileServiceMock.Setup(x => x.FileExists("test.md")).Returns(true);
            _markdownServiceMock.Setup(m => m.Join(It.IsAny<System.Collections.Generic.Dictionary<string, object>>(), It.IsAny<string>()))
                .Returns((System.Collections.Generic.Dictionary<string, object> fm, string content) => content);

            // Act
            var result = _useCase.Execute();

            // Assert
            Assert.IsTrue(result);
            // リナンバリングされていないこと
            Assert.AreEqual("Text[^5]\n\n[^5]: Note", doc.Content);
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
            _fileServiceMock.Verify(x => x.Save(It.IsAny<MarkdownDocument>()), Times.Never);
        }

        [TestMethod]
        public void Execute_ShouldCallSaveAs_WhenFilePathIsEmpty()
        {
            // テスト観点: ファイルパスが設定されていない（新規作成時など）場合、「名前を付けて保存」処理が呼び出されることを確認する。
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
            // テスト観点: 指定されたファイルパスにファイルが存在しない場合、「名前を付けて保存」処理が呼び出されることを確認する。
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
            // テスト観点: ファイルが存在する場合、上書き保存処理が実行され、IsDirtyフラグがクリアされることを確認する。
            // Arrange
            var doc = new MarkdownDocument { FilePath = "exist.md", IsDirty = true };
            _editorServiceMock.Setup(x => x.CurrentDocument).Returns(doc);
            _fileServiceMock.Setup(x => x.FileExists("exist.md")).Returns(true);

            // Act
            var result = _useCase.Execute();

            // Assert
            Assert.IsTrue(result);
            Assert.IsFalse(doc.IsDirty, "IsDirty should be false after successful save.");
            // 保存用には別インスタンスが作られるため、プロパティで検証
            _fileServiceMock.Verify(x => x.Save(It.Is<MarkdownDocument>(d => d.FilePath == "exist.md")), Times.Once);
            _saveAsDocumentUseCaseMock.Verify(x => x.Execute(), Times.Never);
        }

        [TestMethod]
        public void Execute_ShouldReturnFalse_WhenSaveThrowsException()
        {
            // テスト観点: 保存処理中に例外が発生した場合、例外が捕捉されfalseが返されることを確認する。
            // Arrange
            var doc = new MarkdownDocument { FilePath = "exist.md" };
            _editorServiceMock.Setup(x => x.CurrentDocument).Returns(doc);
            _fileServiceMock.Setup(x => x.FileExists("exist.md")).Returns(true);
            _fileServiceMock.Setup(x => x.Save(It.IsAny<MarkdownDocument>())).Throws(new Exception("Save failed"));

            // Act
            var result = _useCase.Execute();

            // Assert
            Assert.IsFalse(result);
            _fileServiceMock.Verify(x => x.Save(It.IsAny<MarkdownDocument>()), Times.Once);
        }

        [TestMethod]
        public void Execute_ShouldUpdateFrontMatter_WhenExists()
        {
            // テスト観点: ドキュメントにフロントマターが存在する場合、updatedフィールドが更新された状態で保存されることを確認する。
            // Arrange
            var doc = new MarkdownDocument
            {
                FilePath = "exist.md",
                Content = "# Body",
                FrontMatter = new System.Collections.Generic.Dictionary<string, object> { { "title", "test" } }
            };
            _editorServiceMock.Setup(x => x.CurrentDocument).Returns(doc);
            _fileServiceMock.Setup(x => x.FileExists("exist.md")).Returns(true);

            _markdownServiceMock.Setup(m => m.Join(It.IsAny<System.Collections.Generic.Dictionary<string, object>>(), doc.Content))
                .Returns("---\ntitle: test\nupdated: now\n---\n# Body");

            // Act
            var result = _useCase.Execute();

            // Assert
            _markdownServiceMock.Verify(m => m.Join(It.Is<System.Collections.Generic.Dictionary<string, object>>(d => d.ContainsKey("updated")), doc.Content), Times.Once);
            _fileServiceMock.Verify(x => x.Save(It.Is<MarkdownDocument>(d => d.Content == "---\ntitle: test\nupdated: now\n---\n# Body")), Times.Once);
        }

        [TestMethod]
        public void Execute_ShouldAddUpdatedField_WhenFrontMatterExistsButUpdatedDoesNot()
        {
            // テスト観点: フロントマターは存在するが updated フィールドがない場合、updated が追加されることを確認する。
            // Arrange
            var doc = new MarkdownDocument
            {
                FilePath = "exist.md",
                Content = "# Body",
                FrontMatter = new System.Collections.Generic.Dictionary<string, object> { { "title", "test" } }
            };
            _editorServiceMock.Setup(x => x.CurrentDocument).Returns(doc);
            _fileServiceMock.Setup(x => x.FileExists("exist.md")).Returns(true);

            _markdownServiceMock.Setup(m => m.Join(It.IsAny<System.Collections.Generic.Dictionary<string, object>>(), doc.Content))
                .Returns("---\ntitle: test\nupdated: now\n---\n# Body");

            // Act
            var result = _useCase.Execute();

            // Assert
            Assert.IsTrue(doc.FrontMatter.ContainsKey("updated"), "Updated field should be added to dictionary");
            _markdownServiceMock.Verify(m => m.Join(doc.FrontMatter, doc.Content), Times.Once);
        }
    }
}
