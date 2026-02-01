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
            // テスト観点: CSSルールから文字色、背景色、フォントサイズが正しく抽出されること
            var rule = CreateRule("color: #ff0000; background-color: rgb(255, 255, 255); font-size: 16px;");
            var style = new BodyStyle();

            style.UpdateFrom(rule);

            Assert.AreEqual("#FF0000", style.TextColor?.HexCode.ToUpper());
            Assert.AreEqual("#FFFFFF", style.BackgroundColor?.HexCode.ToUpper());
            Assert.AreEqual(16.0, style.FontSize?.Value);
            Assert.AreEqual(CssUnit.Px, style.FontSize?.Unit);
        }

        [TestMethod]
        public void ApplyTo_ShouldSetPropertiesToRule()
        {
            // テスト観点: モデルの状態がCSSルールへ正しく書き出されること
            var style = new BodyStyle
            {
                TextColor = new CssColor("#0000FF"),
                BackgroundColor = new CssColor("#EEEEEE"),
                FontSize = new CssSize(1.2, CssUnit.Em)
            };
            var rule = CreateRule(""); // 空のルール

            style.ApplyTo(rule);

            var actualColor = rule.Style.GetPropertyValue("color");
            // AngleSharpは正規化した値を返す可能性があるため、パースし直して比較するか、
            // 文字列の含有を確認する。ここではHEXへの再変換を想定
            Assert.IsTrue(actualColor.Contains("0, 0, 255") || actualColor.Equals("#0000FF", StringComparison.OrdinalIgnoreCase));

            var actualBgColor = rule.Style.GetPropertyValue("background-color");
            Assert.IsTrue(actualBgColor.Contains("238, 238, 238") || actualBgColor.Equals("#EEEEEE", StringComparison.OrdinalIgnoreCase));

            Assert.AreEqual("1.2em", rule.Style.GetPropertyValue("font-size"));
        }
    }
}
