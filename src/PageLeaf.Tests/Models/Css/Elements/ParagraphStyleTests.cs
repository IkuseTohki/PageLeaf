using Microsoft.VisualStudio.TestTools.UnitTesting;
using AngleSharp.Css.Dom;
using PageLeaf.Models.Css.Elements;
using PageLeaf.Models.Css.Values;
using AngleSharp.Css.Parser;
using System;

namespace PageLeaf.Tests.Models.Css.Elements
{
    [TestClass]
    public class ParagraphStyleTests
    {
        private ICssStyleRule CreateRule(string css)
        {
            var parser = new CssParser();
            var stylesheet = parser.ParseStyleSheet($"p {{ {css} }}");
            return (ICssStyleRule)stylesheet.Rules[0];
        }

        [TestMethod]
        public void UpdateFrom_ShouldParseParagraphProperties()
        {
            // テスト観点: CSSルールから段落の各種プロパティが正しく抽出されること
            var rule = CreateRule("line-height: 1.6; margin-bottom: 1em; text-indent: 20px;");
            var style = new ParagraphStyle();

            style.UpdateFrom(rule);

            Assert.AreEqual("1.6", style.LineHeight);
            Assert.AreEqual(1.0, style.MarginBottom?.Value);
            Assert.AreEqual(CssUnit.Em, style.MarginBottom?.Unit);
            Assert.AreEqual(20.0, style.TextIndent?.Value);
            Assert.AreEqual(CssUnit.Px, style.TextIndent?.Unit);
        }

        [TestMethod]
        public void ApplyTo_ShouldSetPropertiesToRule()
        {
            // テスト観点: モデルの状態がCSSルールへ正しく書き出されること
            var style = new ParagraphStyle
            {
                LineHeight = "1.8",
                MarginBottom = new CssSize(1.5, CssUnit.Em),
                TextIndent = new CssSize(2, CssUnit.Em)
            };
            var rule = CreateRule("");

            style.ApplyTo(rule);

            var styleDecl = rule.Style;
            Assert.AreEqual("1.8", styleDecl.GetPropertyValue("line-height"));
            Assert.AreEqual("1.5em", styleDecl.GetPropertyValue("margin-bottom"));
            Assert.AreEqual("2em", styleDecl.GetPropertyValue("text-indent"));
        }
    }
}
