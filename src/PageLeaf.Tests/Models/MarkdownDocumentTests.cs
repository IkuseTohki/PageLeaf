using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageLeaf.Models;
using System.Collections.Generic;

namespace PageLeaf.Tests.Models
{
    [TestClass]
    public class MarkdownDocumentTests
    {
        [TestMethod]
        public void SuggestedCss_ShouldReturnCssPropertyFromFrontMatter()
        {
            /*
            テスト観点:
            フロントマターに "css" キーがある場合、SuggestedCss プロパティがその値を返すことを確認する。
            */
            // Arrange
            var doc = new MarkdownDocument();
            doc.FrontMatter = new Dictionary<string, object> { { "css", "report.css" } };

            // Act & Assert
            Assert.AreEqual("report.css", doc.SuggestedCss);
        }

        [TestMethod]
        public void PreferredSyntaxHighlight_ShouldReturnSyntaxHighlightPropertyFromFrontMatter()
        {
            /*
            テスト観点:
            フロントマターに "syntax_highlight" キーがある場合、PreferredSyntaxHighlight プロパティがその値を返すことを確認する。
            */
            // Arrange
            var doc = new MarkdownDocument();
            doc.FrontMatter = new Dictionary<string, object> { { "syntax_highlight", "monokai" } };

            // Act & Assert
            Assert.AreEqual("monokai", doc.PreferredSyntaxHighlight);
        }

        [TestMethod]
        public void HelperProperties_ShouldReturnNull_WhenKeyDoesNotExist()
        {
            /*
            テスト観点:
            該当するキーがフロントマターに存在しない場合、null を返すことを確認する。
            */
            // Arrange
            var doc = new MarkdownDocument();
            doc.FrontMatter = new Dictionary<string, object>();

            // Act & Assert
            Assert.IsNull(doc.SuggestedCss);
            Assert.IsNull(doc.PreferredSyntaxHighlight);
        }
    }
}
