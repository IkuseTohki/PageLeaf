using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using AngleSharp.Css.Dom;
using PageLeaf.Models.Css.Elements;
using PageLeaf.Models.Css.Values;
using AngleSharp.Css.Parser;
using System;

namespace PageLeaf.Tests.Models.Css.Elements
{
    [TestClass]
    public class BodyStyleTests
    {
        private ICssStyleRule CreateRule(string css)
        {
            var parser = new CssParser();
            var stylesheet = parser.ParseStyleSheet($"body {{ {css} }}");
            return (ICssStyleRule)stylesheet.Rules[0];
        }

        [TestMethod]
        public void UpdateFrom_ShouldParseBasicProperties()
        {
            // テスト観点: CSSルールから文字色、背景色、フォントサイズ、フォントファミリーが正しく抽出されること
            var rule = CreateRule("color: #ff0000; background-color: rgb(255, 255, 255); font-size: 16px; font-family: 'Meiryo', sans-serif;");
            var style = new BodyStyle();

            style.UpdateFrom(rule);

            Assert.AreEqual("#FF0000", style.TextColor?.HexCode.ToUpper());
            Assert.AreEqual("#FFFFFF", style.BackgroundColor?.HexCode.ToUpper());
            Assert.AreEqual(16.0, style.FontSize?.Value);
            Assert.AreEqual(CssUnit.Px, style.FontSize?.Unit);
            // フォントファミリーは先頭のものが抽出され、引用符が除去されていること
            Assert.AreEqual("Meiryo, sans-serif", style.FontFamily);
        }

        [TestMethod]
        public void ApplyTo_ShouldSetPropertiesToRule()
        {
            // テスト観点: モデルの状態がCSSルールへ正しく書き出されること
            var style = new BodyStyle
            {
                TextColor = new CssColor("#0000FF"),
                BackgroundColor = new CssColor("#EEEEEE"),
                FontSize = new CssSize(1.2, CssUnit.Em),
                FontFamily = "MS Gothic"
            };
            var rule = CreateRule(""); // 空のルール

            style.ApplyTo(rule);

            var actualColor = rule.Style.GetPropertyValue("color");
            Assert.IsTrue(actualColor.Contains("0, 0, 255") || actualColor.Equals("#0000FF", StringComparison.OrdinalIgnoreCase));

            var actualBgColor = rule.Style.GetPropertyValue("background-color");
            Assert.IsTrue(actualBgColor.Contains("238, 238, 238") || actualBgColor.Equals("#EEEEEE", StringComparison.OrdinalIgnoreCase));

            Assert.AreEqual("1.2em", rule.Style.GetPropertyValue("font-size"));
            // 空白を含むフォント名が引用符で囲まれていること
            Assert.AreEqual("\"MS Gothic\"", rule.Style.GetPropertyValue("font-family"));
        }
    }
}
