using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PageLeaf.Models;
using PageLeaf.Services;

namespace PageLeaf.Tests.Services
{
    [TestClass]
    public class EditorServiceTests
    {
        private Mock<IMarkdownService> _mockMarkdownService = null!;
        private Mock<ICssService> _mockCssService = null!;
        private Mock<IDialogService> _mockDialogService = null!; // IDialogService のモックを追加
        private EditorService _editorService = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockMarkdownService = new Mock<IMarkdownService>();
            _mockCssService = new Mock<ICssService>();
            _mockDialogService = new Mock<IDialogService>(); // モックを初期化
            _editorService = new EditorService(_mockMarkdownService.Object, _mockCssService.Object, _mockDialogService.Object);
        }

        [TestMethod]
        public void SelectedMode_ShouldUpdateVisibilityProperties()
        {
            // テスト観点: SelectedMode を変更すると、IsMarkdownEditorVisible と IsViewerVisible が正しく更新されることを確認する。
            // Arrange

            // Act
            _editorService.SelectedMode = DisplayMode.Markdown;

            // Assert
            Assert.IsTrue(_editorService.IsMarkdownEditorVisible);
            Assert.IsFalse(_editorService.IsViewerVisible);

            // Act
            _editorService.SelectedMode = DisplayMode.Viewer;

            // Assert
            Assert.IsFalse(_editorService.IsMarkdownEditorVisible);
            Assert.IsTrue(_editorService.IsViewerVisible);
        }

        [TestMethod]
        public void LoadDocument_ShouldUpdateEditorTextAndHtmlContent()
        {
            // テスト観点: LoadDocument を実行すると、EditorText が更新され、
            //             SelectedMode が Viewer であれば HtmlContent も更新されることを確認する。

            // Arrange
            var newDocument = new MarkdownDocument { Content = "## New Content" };
            var expectedHtml = "<h2>New Content</h2>";
            _mockMarkdownService.Setup(m => m.ConvertToHtml("## New Content", It.IsAny<string?>())).Returns(expectedHtml);

            var editorService = new EditorService(_mockMarkdownService.Object, _mockCssService.Object, _mockDialogService.Object);

            // Act (Mode: Markdown)
            editorService.SelectedMode = DisplayMode.Markdown;
            editorService.LoadDocument(newDocument);

            // Assert (Mode: Markdown)
            Assert.AreEqual(newDocument.Content, editorService.EditorText);
            Assert.AreEqual(string.Empty, editorService.HtmlContent, "MarkdownモードではHtmlContentは空のはずです");

            // Act (Mode: Viewer)
            // LoadDocument を呼んだ後で Mode を変えた場合でも追従して HtmlContent が更新されることを確認
            editorService.SelectedMode = DisplayMode.Viewer;

            // Assert (Mode: Viewer)
            Assert.AreEqual(newDocument.Content, editorService.EditorText);
            Assert.AreEqual(expectedHtml, editorService.HtmlContent, "ViewerモードではHtmlContentが更新されるはずです");
        }

        [TestMethod]
        public void ApplyCss_ShouldUpdateHtmlContent_WithNewCss()
        {
            // テスト観点: ApplyCss を呼び出すと、新しいCSSパスでHTMLが再生成されることを確認する。
            // Arrange
            var document = new MarkdownDocument { Content = "# Title" };
            _editorService.LoadDocument(document);
            _editorService.SelectedMode = DisplayMode.Viewer; // Viewerモードにしておく

            var cssFileName = "github.css";
            var cssPath = "C:\\css\\github.css";
            var expectedHtml = "<h1>Title with CSS</h1>";

            _mockCssService.Setup(s => s.GetCssPath(cssFileName)).Returns(cssPath);
            _mockMarkdownService.Setup(m => m.ConvertToHtml("# Title", cssPath)).Returns(expectedHtml);

            // Act
            _editorService.ApplyCss(cssFileName);

            // Assert
            _mockCssService.Verify(s => s.GetCssPath(cssFileName), Times.Once);
            _mockMarkdownService.Verify(m => m.ConvertToHtml("# Title", cssPath), Times.Once);
            Assert.AreEqual(expectedHtml, _editorService.HtmlContent);
        }

        [TestMethod]
        public void EditorText_WhenChanged_ShouldReconvertHtmlWithCurrentCss()
        {
            // テスト観点: EditorTextが変更された際、現在適用されているCSSでHTMLが再生成されることを確認する。
            // Arrange
            var document = new MarkdownDocument { Content = "# Title" };
            _editorService.LoadDocument(document);
            _editorService.SelectedMode = DisplayMode.Viewer;

            // 最初にCSSを適用しておく
            var cssFileName = "github.css";
            var cssPath = "C:\\css\\github.css";
            _mockCssService.Setup(s => s.GetCssPath(cssFileName)).Returns(cssPath);
            _editorService.ApplyCss(cssFileName);

            // EditorText変更後のHTMLモックを設定
            var newMarkdown = "## Subtitle";
            var expectedHtml = "<h2>Subtitle with CSS</h2>";
            _mockMarkdownService.Setup(m => m.ConvertToHtml(newMarkdown, cssPath)).Returns(expectedHtml);

            // Act
            _editorService.EditorText = newMarkdown;

            // Assert
            _mockMarkdownService.Verify(m => m.ConvertToHtml(newMarkdown, cssPath), Times.Once);
            Assert.AreEqual(expectedHtml, _editorService.HtmlContent);
        }
        [TestMethod]
        public void NewDocument_ShouldResetDocumentProperties()
        {
            // テスト観点: NewDocument メソッドを呼び出すと、CurrentDocument の Content が空文字列に、FilePath が null にリセットされることを確認する。
            // Arrange
            var initialDocument = new MarkdownDocument { Content = "Existing Content", FilePath = "C:\\path\\to\\file.md" };
            _editorService.LoadDocument(initialDocument);
            _editorService.SelectedMode = DisplayMode.Viewer; // Viewerモードに設定
            _mockMarkdownService.Invocations.Clear(); // LoadDocumentによるConvertToHtmlの呼び出し履歴をクリア

            // Act
            _editorService.NewDocument();

            // Assert
            Assert.AreEqual(string.Empty, _editorService.CurrentDocument.Content);
            Assert.IsNull(_editorService.CurrentDocument.FilePath);
            _mockMarkdownService.Verify(m => m.ConvertToHtml(string.Empty, It.IsAny<string?>()), Times.Once); // HTMLコンテンツも更新されることを確認
        }

        [TestMethod]
        public void EditorText_WhenChanged_ShouldSetIsDirtyToTrue()
        {
            // テスト観点: EditorText が変更されたときに EditorService.IsDirty が true になることを確認する。
            // Arrange
            _editorService.CurrentDocument.IsDirty = false; // 初期状態をfalseに設定

            // Act
            _editorService.EditorText = "Some new content";

            // Assert
            Assert.IsTrue(_editorService.IsDirty);
        }

        [TestMethod]
        public void LoadDocument_ShouldResetIsDirtyToFalse()
        {
            // テスト観点: LoadDocument 後に EditorService.IsDirty が false にリセットされることを確認する。
            // Arrange
            var initialDocument = new MarkdownDocument { Content = "Existing Content", IsDirty = true };
            _editorService.LoadDocument(initialDocument);

            // Act
            _editorService.LoadDocument(new MarkdownDocument { Content = "New Content" });

            // Assert
            Assert.IsFalse(_editorService.IsDirty);
        }

        [TestMethod]
        public void NewDocument_ShouldResetIsDirtyToFalse()
        {
            // テスト観点: NewDocument 後に EditorService.IsDirty が false にリセットされることを確認する。
            // Arrange
            _editorService.CurrentDocument.IsDirty = true; // 初期状態をtrueに設定

            // Act
            _editorService.NewDocument();

            // Assert
            Assert.IsFalse(_editorService.IsDirty);
        }

        [TestMethod]
        public void PromptForSaveIfDirty_WhenDirty_ShouldCallDialogService()
        {
            // テスト観点: IsDirty が true の場合、PromptForSaveIfDirty が IDialogService.ShowSaveConfirmationDialog を呼び出すことを確認する。
            // Arrange
            _editorService.CurrentDocument.IsDirty = true; // 変更ありの状態にする
            _mockDialogService.Setup(d => d.ShowSaveConfirmationDialog()).Returns(SaveConfirmationResult.Cancel); // モックの振る舞いを設定

            // Act
            _editorService.PromptForSaveIfDirty();

            // Assert
            _mockDialogService.Verify(d => d.ShowSaveConfirmationDialog(), Times.Once);
        }

        [TestMethod]
        public void PromptForSaveIfDirty_WhenNotDirty_ShouldNotCallDialogService()
        {
            // テスト観点: IsDirty が false の場合、PromptForSaveIfDirty が IDialogService.ShowSaveConfirmationDialog を呼び出さないことを確認する。
            // Arrange
            _editorService.CurrentDocument.IsDirty = false; // 変更なしの状態にする

            // Act
            _editorService.PromptForSaveIfDirty();

            // Assert
            _mockDialogService.Verify(d => d.ShowSaveConfirmationDialog(), Times.Never);
        }
    }
}
