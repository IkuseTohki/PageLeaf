using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageLeaf.Services;

namespace PageLeaf.Tests.Services
{
    [TestClass]
    public class MarkdownServiceTests
    {
        [TestMethod]
        public void ConvertToHtml_ShouldConvertMarkdownToHtml_ForSimpleHeader()
        {
            // テスト観点: 単純なMarkdownヘッダーが正しくHTMLに変換されることを確認する。
            // Arrange
            var service = new MarkdownService();
            string markdown = "# Hello";
            string expectedHtml = "<!DOCTYPE html>\n<html>\n<head>\n<meta charset=\"UTF-8\">\n</head>\n<body>\n<h1>Hello</h1>\n</body>\n</html>\n";

            // Act
            string actualHtml = service.ConvertToHtml(markdown);

            // Assert
            Assert.AreEqual(expectedHtml.Replace("\r\n", "\n"), actualHtml.Replace("\r\n", "\n"));
        }
    }
}