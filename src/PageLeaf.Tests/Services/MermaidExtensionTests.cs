using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PageLeaf.Models;
using PageLeaf.Models.Markdown;
using PageLeaf.Models.Css;
using PageLeaf.Models.Css.Elements;
using PageLeaf.Models.Settings;
using PageLeaf.Services;
using System;

namespace PageLeaf.Tests.Services
{
    [TestClass]
    public class MermaidExtensionTests
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
        public void ConvertToHtml_ShouldRenderMermaidBlock()
        {
            // テスト観点: mermaid 記法のブロックが、Mermaid.js が認識可能な形式 (div.mermaid) で出力されていることを確認する。
            // Arrange
            var markdown = "```mermaid" + Environment.NewLine + "graph TD; A-->B;" + Environment.NewLine + "```";

            // Act
            string html = _service.ConvertToHtml(markdown, null, null);

            // Assert
            // タグが div になっていること、およびクラスが付与されていることを確認
            StringAssert.Contains(html, "<div class=\"mermaid\">");
            // スクリプトの読み込みが含まれているか確認
            StringAssert.Contains(html, "mermaid.min.js");
            // エスケープされずに記号が含まれているか確認 (--> がそのまま出ているか)
            StringAssert.Contains(html, "A-->B;");
        }
    }
}
