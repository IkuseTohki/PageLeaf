using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageLeaf.Services;

namespace PageLeaf.Tests.Services
{
    [TestClass]
    public class CssEditorServiceTests
    {
        [TestMethod]
        public void ParseBodyStyles_ShouldExtractCorrectColor()
        {
            // テスト観点: CSS文字列からbodyセレクタのcolorプロパティが正しく抽出されることを確認する。
            // Arrange
            var service = new CssEditorService();
            var cssContent = "body { color: #333333; font-size: 16px; }";

            // Act
            var styles = service.ParseCss(cssContent);

            // Assert
            Assert.AreEqual("rgba(51, 51, 51, 1)", styles.BodyTextColor);
        }

        [TestMethod]
        public void ParseBodyStyles_ShouldExtractCorrectBackgroundColor()
        {
            // テスト観点: CSS文字列からbodyセレクタのbackground-colorプロパティが正しく抽出されることを確認する。
            // Arrange
            var service = new CssEditorService();
            var cssContent = "body { background-color: #f0f0f0; }";

            // Act
            var styles = service.ParseCss(cssContent);

            // Assert
            Assert.AreEqual("rgba(240, 240, 240, 1)", styles.BodyBackgroundColor);
        }
    }
}
