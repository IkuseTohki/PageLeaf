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
    public class CodeStyleTests
    {
        private ICssStyleSheet CreateSheet(string css)
        {
            var parser = new CssParser();
            return parser.ParseStyleSheet(css);
        }

        [TestMethod]
        public void UpdateFrom_ShouldParseCodeProperties()
        {
            // テスト観点: インラインコードとブロックコードのスタイルが正しく抽出されること
            var css = "code { color: #ff0000; font-family: Consolas; } pre code { color: #ffffff; background-color: #000000; }";
            var sheet = CreateSheet(css);
            var style = new CodeStyle();

            style.UpdateFrom(sheet);

            Assert.AreEqual("#FF0000", style.TextColor?.HexCode.ToUpper());
            Assert.AreEqual("Consolas", style.FontFamily);
            Assert.AreEqual("#FFFFFF", style.BlockTextColor?.HexCode.ToUpper());
            Assert.AreEqual("#000000", style.BlockBackgroundColor?.HexCode.ToUpper());
            Assert.IsTrue(style.IsBlockOverrideEnabled);
        }

        [TestMethod]
        public void ApplyTo_ShouldSetPropertiesToSheet()
        {
            // テスト観点: モデルの状態がCSSシートへ正しく書き出されること
            var style = new CodeStyle
            {
                TextColor = new CssColor("#123456"),
                IsBlockOverrideEnabled = true,
                BlockTextColor = new CssColor("#FFFFFF")
            };
            var sheet = CreateSheet("");

            style.ApplyTo(sheet);

            var codeRule = sheet.Rules.OfType<ICssStyleRule>().First(r => r.SelectorText == "code");
            var preCodeRule = sheet.Rules.OfType<ICssStyleRule>().First(r => r.SelectorText == "pre code");

            Assert.IsTrue(codeRule.Style.GetPropertyValue("color").Contains("18, 52, 86") || codeRule.Style.GetPropertyValue("color").Equals("#123456", StringComparison.OrdinalIgnoreCase));
            Assert.AreEqual("rgba(255, 255, 255, 1)", preCodeRule.Style.GetPropertyValue("color"));
            Assert.AreEqual("important", preCodeRule.Style.GetPropertyPriority("color"));
        }
    }
}
