using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageLeaf.Services;

namespace PageLeaf.Tests.Services
{
    [TestClass]
    public class MarkdownServiceTests
    {
        [TestMethod]
        public void ConvertToHtml_ShouldConvertMarkdownToHtml_WhenCssPathIsNull()
        {
            // テスト観点: CSSパスがnullの場合、単純なMarkdownがCSSリンクなしの完全なHTMLに変換されることを確認する。
            // Arrange
            var service = new MarkdownService();
            string markdown = "# Hello";
            string expectedHtml = "<!DOCTYPE html>\n<html>\n<head>\n<meta charset=\"UTF-8\">\n</head>\n<body>\n<h1>Hello</h1>\n</body>\n</html>\n";

            // Act
            string actualHtml = service.ConvertToHtml(markdown, null);

            // Assert
            Assert.AreEqual(expectedHtml.Replace("\r\n", "\n"), actualHtml.Replace("\r\n", "\n"));
        }

        [TestMethod]
        public void ConvertToHtml_ShouldIncludeCssLink_WhenCssPathIsProvided()
        {
            // テスト観点: CSSパスが提供された場合、生成されるHTMLの<head>内に<link>タグが正しく挿入され、
            //             キャッシュバスティングのためのタイムスタンプが付与されることを確認する。
            // Arrange
            var service = new MarkdownService();
            string markdown = "# Hello";
            string cssPath = "C:\\styles\\github.css";

            // Act
            string actualHtml = service.ConvertToHtml(markdown, cssPath);

            // Assert
            // <link rel="stylesheet" href="C:\styles\github.css?v={timestamp}"> の形式を正規表現で検証
            StringAssert.Matches(actualHtml, new System.Text.RegularExpressions.Regex($@"<link rel=""stylesheet"" href=""{System.Text.RegularExpressions.Regex.Escape(cssPath)}\?v=\d+"">"));
        }
    }
}