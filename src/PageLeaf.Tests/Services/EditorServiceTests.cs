using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PageLeaf.Models;
using PageLeaf.Services;

namespace PageLeaf.Tests.Services
{
    [TestClass]
    public class EditorServiceTests
    {
        [TestMethod]
        public void SelectedMode_ShouldUpdateVisibilityProperties()
        {
            // テスト観点: SelectedMode を変更すると、IsMarkdownEditorVisible と IsViewerVisible が正しく更新されることを確認する。
            // Arrange
            var mockMarkdownService = new Mock<IMarkdownService>();
            var editorService = new EditorService(mockMarkdownService.Object);

            // Act
            editorService.SelectedMode = DisplayMode.Markdown;

            // Assert
            Assert.IsTrue(editorService.IsMarkdownEditorVisible);
            Assert.IsFalse(editorService.IsViewerVisible);

            // Act
            editorService.SelectedMode = DisplayMode.Viewer;

            // Assert
            Assert.IsFalse(editorService.IsMarkdownEditorVisible);
            Assert.IsTrue(editorService.IsViewerVisible);
        }

        [TestMethod]
        public void LoadDocument_ShouldUpdateEditorTextAndHtmlContent()
        {
            // テスト観点: LoadDocument を実行すると、EditorText が更新され、
            //             SelectedMode が Viewer であれば HtmlContent も更新されることを確認する。

            // Arrange
            var mockMarkdownService = new Mock<IMarkdownService>();
            var newDocument = new MarkdownDocument { Content = "## New Content" };
            var expectedHtml = "<h2>New Content</h2>";
            mockMarkdownService.Setup(m => m.ConvertToHtml("## New Content")).Returns(expectedHtml);

            var editorService = new EditorService(mockMarkdownService.Object);

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
    }
}
