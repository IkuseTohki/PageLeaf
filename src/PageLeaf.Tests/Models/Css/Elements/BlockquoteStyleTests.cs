using Microsoft.VisualStudio.TestTools.UnitTesting;
using AngleSharp.Css.Dom;
using PageLeaf.Models.Css.Elements;
using PageLeaf.Models.Css.Values;
using AngleSharp.Css.Parser;
using System;
using System.Linq;

namespace PageLeaf.Tests.Models.Css.Elements
{
    [TestClass]
    public class BlockquoteStyleTests
    {
        private ICssStyleSheet CreateSheet(string css)
        {
            var parser = new CssParser();
            return parser.ParseStyleSheet(css);
        }

        private ICssStyleRule CreateRule(string css)
        {
            var stylesheet = CreateSheet($"blockquote {{ {css} }}");
            return (ICssStyleRule)stylesheet.Rules[0];
        }

        [TestMethod]
        public void UpdateFrom_ShouldParseBlockquoteProperties()
        {
            // テスト観点: CSSルールから引用の各種プロパティ（新設項目含む）が正しく抽出されること
            var css = "blockquote { color: #111111; background-color: #eeeeee; border: 5px solid #ff0000; font-style: italic; padding: 10px; border-radius: 4px; } ";
            var sheet = CreateSheet(css);
            var style = new BlockquoteStyle();

            style.UpdateFrom(sheet);

            Assert.AreEqual("#111111", style.TextColor?.HexCode.ToUpper());
            Assert.AreEqual("#EEEEEE", style.BackgroundColor?.HexCode.ToUpper());
            Assert.AreEqual("#FF0000", style.BorderColor?.ToUpper());
            Assert.IsTrue(style.IsItalic);
            Assert.AreEqual("10px", style.Padding);
            Assert.AreEqual("4px", style.BorderRadius);
        }

        [TestMethod]
        public void ApplyTo_ShouldSetPropertiesToRule()
        {
            // テスト観点: モデルの状態がCSSシートへ正しく書き出されること
            var style = new BlockquoteStyle
            {
                TextColor = new CssColor("#222222"),
                BackgroundColor = new CssColor("#F0F0F0"),
                BorderColor = "#0000FF",
                IsItalic = true,
                Padding = "15px",
                BorderRadius = "8px"
            };
            var sheet = CreateSheet("");

            style.ApplyTo(sheet);

            var rule = sheet.Rules.OfType<ICssStyleRule>().First(r => r.SelectorText == "blockquote");
            var styleDecl = rule.Style;
            Assert.IsTrue(styleDecl.GetPropertyValue("color").Contains("34, 34, 34") || styleDecl.GetPropertyValue("color").Equals("#222222", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(styleDecl.GetPropertyValue("border-color").Contains("0, 0, 255") || styleDecl.GetPropertyValue("border-color").Equals("#0000FF", StringComparison.OrdinalIgnoreCase));
            Assert.AreEqual("italic", styleDecl.GetPropertyValue("font-style"));
            Assert.AreEqual("15px", styleDecl.GetPropertyValue("padding"));
            Assert.AreEqual("8px", styleDecl.GetPropertyValue("border-radius"));
        }
    }
}
