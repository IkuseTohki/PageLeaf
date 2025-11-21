using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageLeaf.Services;
using System.Text.RegularExpressions;

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
            string expectedHtml = "<!DOCTYPE html>\n" +
                                  "<html>\n" +
                                  "<head>\n" +
                                  "<meta charset=\"UTF-8\">\n" +
                                  "</head>\n" +
                                  "<body>\n" +
                                  "<h1>Hello</h1>\n" +
                                  "</body>\n" +
                                  "</html>\n";

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
            StringAssert.Matches(actualHtml, new Regex($"<link rel=\"stylesheet\" href=\"{Regex.Escape(cssPath)}\\?v=\\d+\">"));
        }

        [TestMethod]
        public void ConvertToHtml_ShouldConvertPipeTableToHtmlTable()
        {
            // テスト観点: Markdownのパイプテーブルが、意図したHTMLのテーブル構造に正しく変換されることを確認する。
            // Arrange
            var service = new MarkdownService();
            var markdown = "|赤身|白身|軍艦|\n" +
                           "|:---|:---:|---:|\n" +
                           "|マグロ|ヒラメ|ウニ|\n" +
                           "|カツオ|タイ|イクラ|\n" +
                           "|トロ|カンパチ|ネギトロ|";

            // Act
            string html = service.ConvertToHtml(markdown, null);

            // Assert
            StringAssert.Contains(html, "<table>");
            StringAssert.Contains(html, "<thead>");
            StringAssert.Contains(html, "<tbody>");
            StringAssert.Matches(html, new Regex(@"<th.*>赤身</th>"));
            StringAssert.Matches(html, new Regex(@"<td.*>マグロ</td>"));

            // ヘッダー1行 + データ3行 = 4つの<tr>タグがあるはず
            int trCount = System.Text.RegularExpressions.Regex.Matches(html, "<tr>").Count;
            Assert.AreEqual(4, trCount, "Expected 4 <tr> tags for header and 3 data rows.");
        }
    }
}