using Microsoft.VisualStudio.TestTools.UnitTesting;
using AngleSharp.Css.Dom;
using PageLeaf.Models.Css.Elements;
using PageLeaf.Models.Css.Values;
using AngleSharp.Css.Parser;
using System;

namespace PageLeaf.Tests.Models.Css.Elements
{
    [TestClass]
    public class BlockquoteStyleTests
    {
        private ICssStyleRule CreateRule(string css)
        {
            var parser = new CssParser();
            var stylesheet = parser.ParseStyleSheet($"blockquote {{ {css} }}");
            return (ICssStyleRule)stylesheet.Rules[0];
        }

        [TestMethod]
        public void UpdateFrom_ShouldParseBlockquoteProperties()
        {
            // テスト観点: CSSルールから引用の各種プロパティが正しく抽出されること
            var rule = CreateRule("color: #111111; background-color: #eeeeee; border-left: 5px solid #ff0000;");
            var style = new BlockquoteStyle();

            style.UpdateFrom(rule);

            Assert.AreEqual("#111111", style.TextColor?.HexCode.ToUpper());
            Assert.AreEqual("#EEEEEE", style.BackgroundColor?.HexCode.ToUpper());
            Assert.AreEqual("#FF0000", style.BorderColor?.HexCode.ToUpper());
            Assert.AreEqual("5px", style.BorderWidth);
            Assert.AreEqual("solid", style.BorderStyle);
        }

        [TestMethod]
        public void ApplyTo_ShouldSetPropertiesToRule()
        {
            // テスト観点: モデルの状態がCSSルールへ正しく書き出されること
            var style = new BlockquoteStyle
            {
                TextColor = new CssColor("#222222"),
                BackgroundColor = new CssColor("#F0F0F0"),
                BorderColor = new CssColor("#0000FF"),
                BorderWidth = "2px",
                BorderStyle = "dashed"
            };
            var rule = CreateRule("");

            style.ApplyTo(rule);

            var styleDecl = rule.Style;
            Assert.IsTrue(styleDecl.GetPropertyValue("color").Contains("34, 34, 34") || styleDecl.GetPropertyValue("color").Equals("#222222", StringComparison.OrdinalIgnoreCase));
            // border-left は一括指定で設定される
            Assert.IsTrue(styleDecl.GetPropertyValue("border-left").Contains("2px dashed") || styleDecl.GetPropertyValue("border-left").Contains("#0000FF"));
        }
    }
}
