using Microsoft.VisualStudio.TestTools.UnitTesting;
using AngleSharp.Css.Dom;
using PageLeaf.Models.Css.Elements;
using PageLeaf.Models.Css.Values;
using AngleSharp.Css.Parser;
using System;

namespace PageLeaf.Tests.Models.Css.Elements
{
    [TestClass]
    public class HeadingStyleTests
    {
        private ICssStyleRule CreateRule(string tag, string css)
        {
            var parser = new CssParser();
            var stylesheet = parser.ParseStyleSheet($"{tag} {{ {css} }}");
            return (ICssStyleRule)stylesheet.Rules[0];
        }

        [TestMethod]
        public void UpdateFrom_ShouldParseHeadingProperties()
        {
            // テスト観点: CSSルールから見出しの各種プロパティ（新設のマージン含む）が正しく抽出されること
            var rule = CreateRule("h1", "color: #ff00ff; font-size: 2em; font-family: 'Times New Roman'; text-align: right; font-weight: bold; font-style: italic; text-decoration: underline; margin-top: 10px; margin-bottom: 20px;");
            var style = new HeadingStyle();

            style.UpdateFrom(rule);

            Assert.AreEqual("#FF00FF", style.TextColor?.HexCode.ToUpper());
            Assert.AreEqual(2.0, style.FontSize?.Value);
            Assert.AreEqual(CssUnit.Em, style.FontSize?.Unit);
            // フォントファミリーは引用符なしで保持されること
            Assert.AreEqual("Times New Roman", style.FontFamily);
            Assert.AreEqual(CssAlignment.Right, style.TextAlignment);
            Assert.IsTrue(style.IsBold);
            Assert.IsTrue(style.IsItalic);
            Assert.IsTrue(style.IsUnderline);
            Assert.AreEqual(10.0, style.MarginTop?.Value);
            Assert.AreEqual(20.0, style.MarginBottom?.Value);
        }

        [TestMethod]
        public void ApplyTo_ShouldSetPropertiesToRule()
        {
            // テスト観点: モデルの状態がCSSルールへ正しく書き出されること
            var style = new HeadingStyle
            {
                TextColor = new CssColor("#112233"),
                FontSize = new CssSize(1.5, CssUnit.Em),
                FontFamily = "Consolas",
                TextAlignment = CssAlignment.Center,
                MarginTop = new CssSize(1.0, CssUnit.Em),
                MarginBottom = new CssSize(0, CssUnit.Px),
                IsBold = false,
                IsItalic = true,
                IsUnderline = true
            };
            var rule = CreateRule("h2", "");

            style.ApplyTo(rule);

            var styleDecl = rule.Style;
            Assert.IsTrue(styleDecl.GetPropertyValue("color").Contains("17, 34, 51") || styleDecl.GetPropertyValue("color").Equals("#112233", StringComparison.OrdinalIgnoreCase));
            Assert.AreEqual("1.5em", styleDecl.GetPropertyValue("font-size"));
            Assert.AreEqual("Consolas", styleDecl.GetPropertyValue("font-family").Trim(' ', '"'));
            Assert.AreEqual("center", styleDecl.GetPropertyValue("text-align"));
            Assert.AreEqual("italic", styleDecl.GetPropertyValue("font-style"));
            Assert.IsTrue(styleDecl.GetPropertyValue("text-decoration").Contains("underline"));
            Assert.AreEqual("1em", styleDecl.GetPropertyValue("margin-top"));
            var actualMarginBottom = styleDecl.GetPropertyValue("margin-bottom");
            Assert.IsTrue(actualMarginBottom == "0px" || actualMarginBottom == "0");
        }
    }
}
