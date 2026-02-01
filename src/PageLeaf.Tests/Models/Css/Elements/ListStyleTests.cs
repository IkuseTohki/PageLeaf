using Microsoft.VisualStudio.TestTools.UnitTesting;
using AngleSharp.Css.Dom;
using PageLeaf.Models.Css.Elements;
using PageLeaf.Models.Css.Values;
using AngleSharp.Css.Parser;
using System;
using System.Linq;

using ListStyle = PageLeaf.Models.Css.Elements.ListStyle;

namespace PageLeaf.Tests.Models.Css.Elements
{
    [TestClass]
    public class ListStyleTests
    {
        private ICssStyleSheet CreateSheet(string css)
        {
            var parser = new CssParser();
            return parser.ParseStyleSheet(css);
        }

        [TestMethod]
        public void UpdateFrom_ShouldParseListProperties()
        {
            // テスト観点: ul, ol, li の各種スタイルが正しく抽出されること
            // ListIndent は ul (または ol) の padding-left から取得する
            var css = "ul { list-style-type: square; padding-left: 20px; } ol { list-style-type: none; counter-reset: item; } li { font-size: 1.2em; }";
            var sheet = CreateSheet(css);
            var style = new ListStyle();

            style.UpdateFrom(sheet);

            Assert.AreEqual("square", style.UnorderedListMarkerType);
            Assert.AreEqual("decimal-nested", style.OrderedListMarkerType); // counter-reset があれば nested とみなす既存仕様
            Assert.AreEqual(20.0, style.ListIndent?.Value);
            Assert.AreEqual(1.2, style.MarkerSize?.Value);
        }

        [TestMethod]
        public void ApplyTo_ShouldSetPropertiesToSheet()
        {
            // テスト観点: モデルの状態がCSSシートへ正しく書き出されること
            var style = new ListStyle
            {
                UnorderedListMarkerType = "circle",
                OrderedListMarkerType = "decimal",
                ListIndent = new CssSize(2, CssUnit.Em),
                MarkerSize = new CssSize(14, CssUnit.Px)
            };
            var sheet = CreateSheet("");

            style.ApplyTo(sheet);

            var ulRule = sheet.Rules.OfType<ICssStyleRule>().First(r => r.SelectorText == "ul");
            var olRule = sheet.Rules.OfType<ICssStyleRule>().First(r => r.SelectorText == "ol");
            var liRule = sheet.Rules.OfType<ICssStyleRule>().First(r => r.SelectorText == "li");

            Assert.AreEqual("circle", ulRule.Style.GetPropertyValue("list-style-type"));
            Assert.AreEqual("2em", ulRule.Style.GetPropertyValue("padding-left")); // ul にインデントが設定されること

            Assert.AreEqual("decimal", olRule.Style.GetPropertyValue("list-style-type"));
            Assert.AreEqual("2em", olRule.Style.GetPropertyValue("padding-left")); // ol にもインデントが設定されること

            Assert.AreEqual("14px", liRule.Style.GetPropertyValue("font-size"));
        }
    }
}
