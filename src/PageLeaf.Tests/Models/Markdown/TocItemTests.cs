using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageLeaf.Models.Markdown;
using System;

namespace PageLeaf.Tests.Models.Markdown
{
    [TestClass]
    public class TocItemTests
    {
        /// <summary>
        /// テスト観点: コンストラクタでプロパティが正しく設定されることを確認する。
        /// </summary>
        [TestMethod]
        public void Constructor_ShouldSetPropertiesCorrectly()
        {
            // Arrange
            var level = 2;
            var text = "Heading Text";
            var id = "heading-text";
            var lineNumber = 10;

            // Act
            var item = new TocItem(level, text, id, lineNumber);

            // Assert
            Assert.AreEqual(level, item.Level);
            Assert.AreEqual(text, item.Text);
            Assert.AreEqual(id, item.Id);
            Assert.AreEqual(lineNumber, item.LineNumber);
        }

        /// <summary>
        /// テスト観点: 不正なレベルを指定した場合、例外が送出されることを確認する。
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_WithInvalidLevel_ShouldThrowException()
        {
            new TocItem(0, "text", "id", 0);
        }

        /// <summary>
        /// テスト観点: テキストがnullの場合、例外が送出されることを確認する。
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullText_ShouldThrowException()
        {
            new TocItem(1, null!, "id", 0);
        }

        /// <summary>
        /// テスト観点: 不正な行番号を指定した場合、例外が送出されることを確認する。
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void Constructor_WithInvalidLineNumber_ShouldThrowException()
        {
            new TocItem(1, "text", "id", -1);
        }

        /// <summary>
        /// テスト観点: アンカーリンクが ID に基づいて正しく生成されることを確認する。
        /// </summary>
        [TestMethod]
        [DataRow("heading-1", "#heading-1")]
        [DataRow("", "")]
        [DataRow(null, "")]
        public void GetAnchorLink_ShouldReturnCorrectFormat(string id, string expected)
        {
            // Arrange
            var item = new TocItem(1, "text", id, 0);

            // Act
            var actual = item.GetAnchorLink();

            // Assert
            Assert.AreEqual(expected, actual);
        }
    }
}
