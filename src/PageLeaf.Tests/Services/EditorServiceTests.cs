using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PageLeaf.Models;
using PageLeaf.Models.Markdown;
using PageLeaf.Models.Css;
using PageLeaf.Models.Css.Elements;
using PageLeaf.Models.Settings;
using PageLeaf.Services;
using System.IO; // Added for Path.GetTempPath and File.Delete

namespace PageLeaf.Tests.Services
{
    [TestClass]
    public class EditorServiceTests
    {
        private Mock<IMarkdownService> _mockMarkdownService = null!;
        private Mock<ICssService> _mockCssService = null!;
        private Mock<IDialogService> _mockDialogService = null!;
        private EditorService _editorService = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockMarkdownService = new Mock<IMarkdownService>();
            _mockCssService = new Mock<ICssService>();
            _mockDialogService = new Mock<IDialogService>(); // モックを初期化

            _editorService = new EditorService(_mockMarkdownService.Object, _mockCssService.Object, _mockDialogService.Object);
        }

        [TestCleanup]
        public void Cleanup()
        {
            // テスト後に生成された一時ファイルを削除
            if (!string.IsNullOrEmpty(_editorService.HtmlFilePath) && File.Exists(_editorService.HtmlFilePath))
            {
                File.Delete(_editorService.HtmlFilePath);
            }
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
        public void LoadDocument_ShouldUpdateEditorTextAndHtmlFilePath()
        {
            // テスト観点: LoadDocument を実行すると、EditorText が更新され、
            //             SelectedMode が Viewer であれば HtmlFilePath も更新されることを確認する。

            // Arrange
            var newDocument = new MarkdownDocument { Content = "## New Content" };
            var expectedHtml = "<h2>New Content</h2>";
            _mockMarkdownService.Setup(m => m.ConvertToHtml("## New Content", It.IsAny<string?>(), It.IsAny<string?>())).Returns(expectedHtml);

            var editorService = new EditorService(_mockMarkdownService.Object, _mockCssService.Object, _mockDialogService.Object);

            // Act (Mode: Markdown)
            editorService.SelectedMode = DisplayMode.Markdown;
            editorService.LoadDocument(newDocument);

            // Assert (Mode: Markdown)
            Assert.AreEqual(newDocument.Content, editorService.EditorText);
            Assert.AreEqual(string.Empty, editorService.HtmlFilePath, "MarkdownモードではHtmlFilePathは空のはずです");

            // Act (Mode: Viewer)
            // LoadDocument を呼んだ後で Mode を変えた場合でも追従して HtmlFilePath が更新されることを確認
            editorService.SelectedMode = DisplayMode.Viewer;

            // Assert (Mode: Viewer)
            Assert.AreEqual(newDocument.Content, editorService.EditorText);
            Assert.IsFalse(string.IsNullOrEmpty(editorService.HtmlFilePath), "ViewerモードではHtmlFilePathが更新されるはずです");
            Assert.IsTrue(File.Exists(editorService.HtmlFilePath));
            Assert.AreEqual(expectedHtml, File.ReadAllText(editorService.HtmlFilePath));
        }

        [TestMethod]
        public void ApplyCss_ShouldUpdateHtmlContent_WhenNotVisible()
        {
            // テスト観点: 非表示（Markdownモード）中に ApplyCss を呼び出すと、
            //             JS同期ではなくフルリロード（UpdateHtmlContent）が要求されることを確認する。
            // Arrange
            _editorService.SelectedMode = DisplayMode.Markdown;
            Assert.AreEqual(string.Empty, _editorService.HtmlFilePath);

            var cssFileName = "github.css";
            var cssPath = "C:\\css\\github.css";
            _mockCssService.Setup(s => s.GetCssPath(cssFileName)).Returns(cssPath);

            // Act
            _editorService.ApplyCss(cssFileName);

            // Assert
            // Markdownモードなので ConvertToHtml 自体は呼ばれないが、
            // UpdateHtmlContent メソッドが呼ばれ、その中の SelectedMode チェックで終了することを確認
            _mockMarkdownService.Verify(m => m.ConvertToHtml(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>()), Times.Never);
            Assert.AreEqual(string.Empty, _editorService.HtmlFilePath);
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
            _mockMarkdownService.Setup(m => m.ConvertToHtml(newMarkdown, cssPath, It.IsAny<string?>())).Returns(expectedHtml);

            // Act
            _editorService.EditorText = newMarkdown;

            // Assert
            _mockMarkdownService.Verify(m => m.ConvertToHtml(newMarkdown, cssPath, It.IsAny<string?>()), Times.Once);
            Assert.IsFalse(string.IsNullOrEmpty(_editorService.HtmlFilePath));
            Assert.IsTrue(File.Exists(_editorService.HtmlFilePath));
            Assert.AreEqual(expectedHtml, File.ReadAllText(_editorService.HtmlFilePath));
        }

        [TestMethod]
        public void UpdateHtmlContent_ShouldIncludeFrontMatter_WhenCallingMarkdownService()
        {
            // テスト観点: HTML変換時に、本文(Content)だけでなくフロントマターも結合された状態で
            //             MarkdownService.ConvertToHtml が呼び出されていることを確認する。
            // Arrange
            var frontMatter = new System.Collections.Generic.Dictionary<string, object> { { "title", "monokai" } };
            var document = new MarkdownDocument { Content = "# Body", FrontMatter = frontMatter };
            _editorService.LoadDocument(document);
            _editorService.SelectedMode = DisplayMode.Viewer;

            // Act
            _editorService.UpdatePreview();

            // Assert
            // ToFullStringの結果（---で囲まれた形式）が ConvertToHtml の第1引数として渡されていることを検証
            _mockMarkdownService.Verify(m => m.ConvertToHtml(It.Is<string>(s => s.Contains("title: monokai")), It.IsAny<string?>(), It.IsAny<string?>()), Times.AtLeastOnce());
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
            Assert.AreEqual(string.Empty, _editorService.HtmlFilePath); // HtmlFilePathもリセットされることを確認
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

        [TestMethod]
        public void RequestInsertText_ShouldRaiseTextInsertionRequestedEvent()
        {
            // テスト観点: RequestInsertText を呼び出すと、TextInsertionRequested イベントが発生することを確認する。

            // Arrange
            string insertedText = "Inserted Text";
            string? receivedText = null;
            _editorService.TextInsertionRequested += (s, text) => receivedText = text;

            // Act
            _editorService.RequestInsertText(insertedText);

            // Assert
            Assert.AreEqual(insertedText, receivedText);
        }

        [TestMethod]
        public void ApplyCss_ShouldRaiseUserCssChangedEvent()
        {
            // テスト観点: ApplyCss を呼び出すと、UserCssChanged イベントが発生することを確認する。
            // Arrange
            _editorService.SelectedMode = DisplayMode.Viewer;
            _editorService.EditorText = "# Title"; // HTMLを生成させる
            Assert.IsFalse(string.IsNullOrEmpty(_editorService.HtmlFilePath), "前提条件: HtmlFilePath が設定されていること");

            var cssFileName = "custom.css";
            var cssPath = "C:\\path\\to\\custom.css";
            _mockCssService.Setup(s => s.GetCssPath(cssFileName)).Returns(cssPath);

            string? receivedPath = null;
            _editorService.UserCssChanged += (s, path) => receivedPath = path;

            // Act
            _editorService.ApplyCss(cssFileName);

            // Assert
            Assert.AreEqual(cssPath, receivedPath);
        }

        [TestMethod]
        public void SyncQuoteSettings_ShouldRaiseSyncQuoteSettingsRequestedEvent()
        {
            // テスト観点: SyncQuoteSettings を呼び出すと、SyncQuoteSettingsRequested イベントが発生することを確認する。
            // Arrange
            var eventRaised = false;
            _editorService.SyncQuoteSettingsRequested += (s, e) => eventRaised = true;

            // Act
            _editorService.SyncQuoteSettings();

            // Assert
            Assert.IsTrue(eventRaised);
        }

        [TestMethod]
        public void CurrentDocument_ContentChangedExternally_ShouldRaiseEditorTextPropertyChanged()
        {
            // テスト観点: CurrentDocument の Content が直接変更された場合、EditorService が EditorText の変更通知を発行することを確認する。
            // Arrange
            var document = new MarkdownDocument { Content = "Initial" };
            _editorService.LoadDocument(document);

            var propertyChangedRaised = false;
            _editorService.PropertyChanged += (s, e) =>
            {
                if (e.PropertyName == nameof(EditorService.EditorText))
                {
                    propertyChangedRaised = true;
                }
            };

            // Act
            document.Content = "Updated";

            // Assert
            Assert.IsTrue(propertyChangedRaised, "EditorTextの変更通知が発生すべきです");
            Assert.AreEqual("Updated", _editorService.EditorText);
        }

        [TestMethod]
        public void CurrentDocument_FrontMatterChangedExternally_ShouldUpdatePreview()
        {
            // テスト観点: CurrentDocument の FrontMatter が直接変更された場合、EditorService が HTML を再生成することを確認する。
            // Arrange
            var document = new MarkdownDocument { Content = "# Body" };
            _editorService.LoadDocument(document);
            _editorService.SelectedMode = DisplayMode.Viewer;

            var newFrontMatter = new System.Collections.Generic.Dictionary<string, object> { { "title", "New Title" } };
            _mockMarkdownService.Setup(m => m.ConvertToHtml(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<string?>()))
                .Returns("<h1>New Title</h1>");

            // Act
            document.FrontMatter = newFrontMatter;

            // Assert
            _mockMarkdownService.Verify(m => m.ConvertToHtml(It.Is<string>(s => s.Contains("title: New Title")), It.IsAny<string?>(), It.IsAny<string?>()), Times.AtLeastOnce());
            Assert.AreEqual("<h1>New Title</h1>", File.ReadAllText(_editorService.HtmlFilePath));
        }
    }
}

