using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageLeaf.Services;
using PageLeaf.Models;
using System.Linq;
using System;

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
            Assert.AreEqual("#333333", styles.BodyTextColor);
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
            Assert.AreEqual("#F0F0F0", styles.BodyBackgroundColor);
        }
        /// <summary>
        /// テスト観点: UpdateCssContentメソッドが、既存のCSS文字列のbodyセレクタ内の指定プロパティを正しく更新することを確認する。
        /// </summary>
        [TestMethod]
        public void UpdateCssContent_ShouldUpdateExistingProperties()
        {
            // Arrange
            var service = new CssEditorService();
            var existingCss = "body { font-family: Arial; color: #000000; background-color: #ffffff; }";
            var styleInfo = new CssStyleInfo
            {
                BodyTextColor = "#ff0000", // 赤に変更
                BodyBackgroundColor = "#00ff00" // 緑に変更
            };
            // Act
            var updatedCss = service.UpdateCssContent(existingCss, styleInfo);

            // Assert
            // 生成されたCSS文字列を再度パースして検証する
            var parsedUpdatedStyles = service.ParseCss(updatedCss);
            Assert.AreEqual("#FF0000", parsedUpdatedStyles.BodyTextColor);
            Assert.AreEqual("#00FF00", parsedUpdatedStyles.BodyBackgroundColor);
        }

        /// <summary>
        /// テスト観点: UpdateCssContentメソッドが、bodyセレクタが存在しない場合に新しく追加することを確認する。
        /// </summary>
        [TestMethod]
        public void UpdateCssContent_ShouldAddNewBodySelectorIfMissing()
        {
            // Arrange
            var service = new CssEditorService();
            var existingCss = "h1 { font-size: 24px; }"; // bodyセレクタがない
            var styleInfo = new CssStyleInfo
            {
                BodyTextColor = "#000000",
                BodyBackgroundColor = "#ffffff"
            };

            // Act
            var updatedCss = service.UpdateCssContent(existingCss, styleInfo);

            // Assert
            // 文字列完全一致ではなく、パースして内容を検証する
            var parsedStyles = service.ParseCss(updatedCss);
            Assert.AreEqual("#000000", parsedStyles.BodyTextColor);
            Assert.AreEqual("#FFFFFF", parsedStyles.BodyBackgroundColor);

            // h1のスタイルが消えていないことも確認
            var parser = new AngleSharp.Css.Parser.CssParser();
            var stylesheet = parser.ParseStyleSheet(updatedCss);
            var h1Rule = stylesheet.Rules.OfType<AngleSharp.Css.Dom.ICssStyleRule>().FirstOrDefault(r => r.SelectorText == "h1");
            Assert.IsNotNull(h1Rule);
            Assert.AreEqual("24px", h1Rule.Style.GetPropertyValue("font-size"));
        }

        /// <summary>
        /// テスト観点: UpdateCssContentメソッドが、bodyセレクタ内にプロパティが存在しない場合に新しく追加することを確認する。
        /// </summary>
        [TestMethod]
        public void UpdateCssContent_ShouldAddNewPropertiesIfMissingInBodySelector()
        {
            // Arrange
            var service = new CssEditorService();
            var existingCss = "body { font-family: Arial; }"; // colorとbackground-colorがない
            var styleInfo = new CssStyleInfo
            {
                BodyTextColor = "#000000",
                BodyBackgroundColor = "#ffffff"
            };

            // Act
            var updatedCss = service.UpdateCssContent(existingCss, styleInfo);

            // Assert
            var parsedStyles = service.ParseCss(updatedCss);
            Assert.AreEqual("#000000", parsedStyles.BodyTextColor);
            Assert.AreEqual("#FFFFFF", parsedStyles.BodyBackgroundColor);

            // font-familyが消えていないことも確認
            var parser = new AngleSharp.Css.Parser.CssParser();
            var stylesheet = parser.ParseStyleSheet(updatedCss);
            var bodyRule = stylesheet.Rules.OfType<AngleSharp.Css.Dom.ICssStyleRule>().FirstOrDefault(r => r.SelectorText == "body");
            Assert.IsNotNull(bodyRule);
            Assert.AreEqual("Arial", bodyRule.Style.GetPropertyValue("font-family"));
        }

        /// <summary>
        /// テスト観点: UpdateCssContentが、入力のフォーマットに関わらず、PrettyStyleFormatterで整形されたCSSを返すことを確認する。
        /// </summary>
        [TestMethod]
        public void UpdateCssContent_ShouldApplyPrettyFormatting()
        {
            // Arrange
            var service = new CssEditorService();
            var unformattedCss = "h1{color:red}body{font-size:12px;}";
            var styleInfo = new CssStyleInfo
            {
                BodyTextColor = "#000000",
                BodyBackgroundColor = "#ffffff"
            };

            // Act
            var updatedCss = service.UpdateCssContent(unformattedCss, styleInfo);

            // Assert
            var expectedCss = string.Join(Environment.NewLine,
                "h1 {",
                "  color: rgba(255, 0, 0, 1);",
                "}",
                "",
                "body {",
                "  font-size: 12px;",
                "  color: rgba(0, 0, 0, 1);",
                "  background-color: rgba(255, 255, 255, 1);",
                "}"
            );

            Assert.AreEqual(expectedCss, updatedCss);
        }
    }
}
