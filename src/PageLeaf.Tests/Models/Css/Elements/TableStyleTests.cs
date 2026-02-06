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
    public class TableStyleTests
    {
        private ICssStyleSheet CreateSheet(string css)
        {
            var parser = new CssParser();
            return parser.ParseStyleSheet(css);
        }

        [TestMethod]
        public void UpdateFrom_ShouldParseTableProperties()
        {
            // テスト観点: table, th, td および th のスタイルが正しく抽出されること
            var css = "table { width: 100%; } th, td { border: 2px solid #ff0000; padding: 10px; } th { background-color: #cccccc; color: #000000; font-size: 1.2em; text-align: center; }";
            var sheet = CreateSheet(css);
            var style = new TableStyle();

            style.UpdateFrom(sheet);

            Assert.AreEqual("100%", style.Width);
            Assert.AreEqual("#FF0000", style.BorderColor?.HexCode.ToUpper());
            Assert.AreEqual("2px", style.BorderWidth);
            Assert.AreEqual("10px", style.CellPadding);
            Assert.AreEqual("#CCCCCC", style.HeaderBackgroundColor?.HexCode.ToUpper());
            Assert.AreEqual("#000000", style.HeaderTextColor?.HexCode.ToUpper());
            Assert.AreEqual("1.2em", style.HeaderFontSize);
            Assert.AreEqual("center", style.HeaderAlignment);
        }

        [TestMethod]
        public void ApplyTo_ShouldSetPropertiesToSheet()
        {
            // テスト観点: モデルの状態がCSSシートへ正しく書き出されること
            var style = new TableStyle
            {
                Width = "auto",
                BorderWidth = "1px",
                BorderStyle = "solid",
                BorderColor = new CssColor("#DDDDDD"),
                CellPadding = "8px",
                HeaderBackgroundColor = new CssColor("#F2F2F2")
            };
            var sheet = CreateSheet("");

            style.ApplyTo(sheet);

            var tableRule = sheet.Rules.OfType<ICssStyleRule>().First(r => r.SelectorText == "table");
            var thTdRule = sheet.Rules.OfType<ICssStyleRule>().First(r => r.SelectorText == "th, td");
            var thRule = sheet.Rules.OfType<ICssStyleRule>().First(r => r.SelectorText == "th");

            Assert.AreEqual("auto", tableRule.Style.GetPropertyValue("width"));
            Assert.IsTrue(thTdRule.Style.GetPropertyValue("border").Contains("1px solid") || thTdRule.Style.GetPropertyValue("border").Contains("#DDDDDD"));
            Assert.AreEqual("8px", thTdRule.Style.GetPropertyValue("padding"));
            Assert.AreEqual("rgba(242, 242, 242, 1)", thRule.Style.GetPropertyValue("background-color"));
        }
    }
}
