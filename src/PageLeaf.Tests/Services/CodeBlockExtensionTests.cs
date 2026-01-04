using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PageLeaf.Models;
using PageLeaf.Services;
using System;

namespace PageLeaf.Tests.Services
{
    [TestClass]
    public class CodeBlockExtensionTests
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
        public void ConvertToHtml_ShouldIncludeFileNameAndCopyButton_WhenCodeBlockHasInfo()
        {
            // テスト観点: コードブロックに言語名とファイル名が指定された場合、HTMLにファイル名とコピーボタンが含まれることを確認する。
            // Arrange
            var markdown = "```csharp:test.cs" + Environment.NewLine + "test" + Environment.NewLine + "```";

            // Act
            string html = _service.ConvertToHtml(markdown, null);

            // Assert
            StringAssert.Contains(html, "class=\"code-block-header\"");
            StringAssert.Contains(html, "test.cs");
            StringAssert.Contains(html, "class=\"code-block-copy-button\"");
        }
    }
}
