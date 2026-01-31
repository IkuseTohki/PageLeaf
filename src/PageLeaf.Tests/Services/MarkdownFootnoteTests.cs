using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageLeaf.Services;
using Moq;
using PageLeaf.Models;
using System.Text.RegularExpressions;

namespace PageLeaf.Tests.Services
{
    [TestClass]
    public class MarkdownFootnoteTests
    {
        private Mock<ISettingsService> _settingsServiceMock = null!;
        private MarkdownService _service = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _settingsServiceMock = new Mock<ISettingsService>();
            _settingsServiceMock.Setup(s => s.CurrentSettings).Returns(new ApplicationSettings());
            _service = new MarkdownService(_settingsServiceMock.Object);
        }

        [TestMethod]
        public void ConvertToHtml_ShouldGenerateFootnoteLinksAndIds()
        {
            // テスト観点: 脚注記法が、正しいリンク(href="#fn:1")と、
            //             ジャンプ先(id="fn:1")を持つHTMLに変換されることを確認する。
            // Arrange
            string markdown = "本文[^1]\n\n[^1]: 脚注の内容";

            // Act
            string html = _service.ConvertToHtml(markdown, null, null);

            // Assert
            // 1. 本文中のリンクがフラグメントを指しているか
            // Note: 実際のHTMLを確認するため一旦簡易的なContainsでチェック
            Assert.IsTrue(html.Contains("href=\"#fn:1\"") || html.Contains("href=\"#fnref:1\""), "Footnote reference link not found.");

            // 2. 脚注エリアにIDが存在するか
            Assert.IsTrue(html.Contains("id=\"fn:1\"") || html.Contains("id=\"fnref:1\""), "Footnote definition id not found.");
        }

        [TestMethod]
        public void ConvertToHtml_ShouldIncludePreviewExtensionScriptLink()
        {
            // テスト観点: 生成されるHTMLに、外部JSファイル(js/preview-extensions.js)へのリンクが含まれていることを確認する。
            // Arrange
            string markdown = "test";

            // Act
            string html = _service.ConvertToHtml(markdown, null, null);

            // Assert
            StringAssert.Contains(html, "js/preview-extensions.js", "Preview extension script link not found in HTML.");
            Assert.IsFalse(html.Contains("link.getAttribute('href')"), "Inline script should have been removed.");
        }
    }
}
