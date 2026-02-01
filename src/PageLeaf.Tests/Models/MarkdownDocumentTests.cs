using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageLeaf.Models;
using System;

namespace PageLeaf.Tests.Models
{
    [TestClass]
    public class MarkdownDocumentTests
    {
        [TestMethod]
        public void Load_ShouldSplitFrontMatterAndContent()
        {
            // テスト観点: 生のテキストから、フロントマターと本文が正しく分離されることを確認する。
            // Arrange
            var document = new MarkdownDocument();
            var rawText = "---\ntitle: test\n---\n# Body Content";

            // Act
            document.Load(rawText);

            // Assert
            Assert.AreEqual("# Body Content", document.Content);
            Assert.AreEqual("test", document.FrontMatter["title"]);
            Assert.IsFalse(document.IsDirty, "Load直後はIsDirtyはFalseであるべき");
        }

        [TestMethod]
        public void Load_ShouldHandleNoFrontMatter()
        {
            // テスト観点: フロントマターがないテキストでも、本文が正しく読み込まれることを確認する。
            // Arrange
            var document = new MarkdownDocument();
            var rawText = "# Only Content";

            // Act
            document.Load(rawText);

            // Assert
            Assert.AreEqual(rawText, document.Content);
            Assert.AreEqual(0, document.FrontMatter.Count);
        }

        [TestMethod]
        public void ToFullString_ShouldCombineFrontMatterAndContent()
        {
            // テスト観点: フロントマターと本文が、正しい形式（---で囲まれる）で結合されることを確認する。
            // Arrange
            var document = new MarkdownDocument();
            document.FrontMatter["title"] = "test";
            document.Content = "# Content";

            // Act
            var result = document.ToFullString();

            // Assert
            var nl = Environment.NewLine;
            StringAssert.Contains(result, "---" + nl);
            StringAssert.Contains(result, "title: test" + nl);
            StringAssert.Contains(result, "---" + nl + "# Content");
        }

        [TestMethod]
        public void RenumberFootnotes_ShouldReorderFootnotes()
        {
            // テスト観点: 脚注番号がバラバラなドキュメントに対し、出現順に番号が振り直されることを確認する。
            // Arrange
            var document = new MarkdownDocument();
            document.Content = "本文[^10] と [^2]\n\n[^10]: 10番の注釈\n[^2]: 2番の注釈";

            // Act
            document.RenumberFootnotes();

            // Assert
            // 番号が 1, 2 に振り直されていることを期待
            StringAssert.Contains(document.Content, "[^1]");
            StringAssert.Contains(document.Content, "[^2]");
            StringAssert.Contains(document.Content, "[^1]: 10番の注釈");
            StringAssert.Contains(document.Content, "[^2]: 2番の注釈");
        }

        [TestMethod]
        public void UpdateTimestamp_ShouldUpdateUpdatedProperty_WhenFrontMatterIsNotEmpty()
        {
            // テスト観点: フロントマターが存在する場合、UpdateTimestampを実行すると 'updated' プロパティが現在日時で更新されることを確認する。
            // Arrange
            var document = new MarkdownDocument();
            document.FrontMatter["updated"] = "2020-01-01 00:00:00";

            // Act
            document.UpdateTimestamp();

            // Assert
            Assert.AreNotEqual("2020-01-01 00:00:00", document.FrontMatter["updated"]);
            // yyyy-MM-dd 形式が含まれていることを確認
            StringAssert.Matches(document.FrontMatter["updated"].ToString(), new System.Text.RegularExpressions.Regex(@"\d{4}-\d{2}-\d{2}"));
        }
    }
}
