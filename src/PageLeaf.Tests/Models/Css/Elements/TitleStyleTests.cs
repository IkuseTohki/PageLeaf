using Microsoft.VisualStudio.TestTools.UnitTesting;
using AngleSharp.Css.Dom;
using PageLeaf.Models.Css.Elements;
using PageLeaf.Models.Css.Values;
using AngleSharp.Css.Parser;
using System;

namespace PageLeaf.Tests.Models.Css.Elements
{
    [TestClass]
    public class TitleStyleTests
    {
        private ICssStyleRule CreateRule(string css)
        {
            var parser = new CssParser();
            var stylesheet = parser.ParseStyleSheet($"#page-title {{ {css} }}");
            return (ICssStyleRule)stylesheet.Rules[0];
        }

        [TestMethod]
        public void UpdateFrom_ShouldParseTitleProperties()
        {
            // テスト観点: CSSルールからタイトルの各種プロパティが正しく抽出されること
            var rule = CreateRule("color: #123456; font-size: 24px; font-family: Arial; text-align: center; margin-bottom: 20px; font-weight: bold; font-style: italic; text-decoration: underline;");
            var style = new TitleStyle();

            style.UpdateFrom(rule);

            Assert.AreEqual("#123456", style.TextColor?.HexCode.ToUpper());
            Assert.AreEqual(24.0, style.FontSize?.Value);
            Assert.AreEqual("Arial", style.FontFamily);
            Assert.AreEqual(CssAlignment.Center, style.TextAlignment);
            Assert.AreEqual(20.0, style.MarginBottom?.Value);
            Assert.IsTrue(style.IsBold);
            Assert.IsTrue(style.TextStyle.IsItalic);
            Assert.IsTrue(style.TextStyle.IsUnderline);
        }

        [TestMethod]
        public void ApplyTo_ShouldSetPropertiesToRule()
        {
            // テスト観点: モデルの状態がCSSルールへ正しく書き出されること
            var style = new TitleStyle
            {
                TextColor = new CssColor("#654321"),
                FontSize = new CssSize(2.0, CssUnit.Em),
                FontFamily = "Segoe UI",
                TextAlignment = CssAlignment.Left,
                MarginBottom = new CssSize(1.5, CssUnit.Em),
                IsBold = true
            };
            style.TextStyle.IsItalic = true;
            style.TextStyle.IsUnderline = true;
            var rule = CreateRule("");

            style.ApplyTo(rule);

            var styleDecl = rule.Style;
            Assert.IsTrue(styleDecl.GetPropertyValue("color").Contains("101, 67, 33") || styleDecl.GetPropertyValue("color").Equals("#654321", StringComparison.OrdinalIgnoreCase));
            Assert.AreEqual("2em", styleDecl.GetPropertyValue("font-size"));
            Assert.AreEqual("Segoe UI", styleDecl.GetPropertyValue("font-family").Trim(' ', '\'', '"'));
            Assert.AreEqual("left", styleDecl.GetPropertyValue("text-align"));
            Assert.AreEqual("1.5em", styleDecl.GetPropertyValue("margin-bottom"));
            Assert.AreEqual("bold", styleDecl.GetPropertyValue("font-weight"));
            Assert.AreEqual("italic", styleDecl.GetPropertyValue("font-style"));
            Assert.IsTrue(styleDecl.GetPropertyValue("text-decoration").Contains("underline"));
        }
    }
}
