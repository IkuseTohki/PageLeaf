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
    public class FootnoteStyleTests
    {
        private ICssStyleSheet CreateSheet(string css)
        {
            var parser = new CssParser();
            return parser.ParseStyleSheet(css);
        }

        [TestMethod]
        public void UpdateFrom_ShouldParseFootnoteProperties()
        {
            // テスト観点: 脚注マーカー、エリア、HR等のスタイルが正しく抽出されること
            var css = @"
                .footnote-ref { color: #ff00ff; font-weight: bold; }
                .footnote-ref::before { content: '['; }
                .footnotes { font-size: 14px; color: #333333; margin-top: 2em; }
                .footnotes hr { border-top: 2px dashed #999999; }
                .footnotes li { line-height: 1.5; }
                .footnote-back-ref { display: none; }";

            var sheet = CreateSheet(css);
            var style = new FootnoteStyle();

            style.UpdateFrom(sheet);

            Assert.AreEqual("#FF00FF", style.MarkerTextColor?.HexCode.ToUpper());
            Assert.IsTrue(style.IsMarkerBold);
            Assert.IsTrue(style.HasMarkerBrackets);
            Assert.AreEqual(14.0, style.AreaFontSize?.Value);
            Assert.AreEqual("#333333", style.AreaTextColor?.HexCode.ToUpper());
            Assert.AreEqual(2.0, style.AreaMarginTop?.Value);
            Assert.AreEqual("2px", style.AreaBorderTopWidth);
            Assert.AreEqual("#999999", style.AreaBorderTopColor?.HexCode.ToUpper());
            Assert.AreEqual("dashed", style.AreaBorderTopStyle);
            Assert.AreEqual("1.5", style.ListItemLineHeight);
            Assert.IsFalse(style.IsBackLinkVisible);
        }

        [TestMethod]
        public void ApplyTo_ShouldSetPropertiesToSheet()
        {
            // テスト観点: モデルの状態がCSSシートへ正しく書き出されること
            var style = new FootnoteStyle
            {
                MarkerTextColor = new CssColor("#FF0000"),
                IsMarkerBold = true,
                HasMarkerBrackets = true,
                AreaFontSize = new CssSize(12, CssUnit.Px),
                IsBackLinkVisible = true
            };
            var sheet = CreateSheet("");

            style.ApplyTo(sheet);

            var markerRule = sheet.Rules.OfType<ICssStyleRule>().First(r => r.SelectorText == ".footnote-ref");
            var beforeRule = sheet.Rules.OfType<ICssStyleRule>().First(r => r.SelectorText == ".footnote-ref::before");
            var areaRule = sheet.Rules.OfType<ICssStyleRule>().First(r => r.SelectorText == ".footnotes");
            var backLinkRule = sheet.Rules.OfType<ICssStyleRule>().First(r => r.SelectorText == ".footnote-back-ref");

            Assert.AreEqual("rgba(255, 0, 0, 1)", markerRule.Style.GetPropertyValue("color"));
            Assert.AreEqual("bold", markerRule.Style.GetPropertyValue("font-weight"));
            Assert.AreEqual("\"[\"", beforeRule.Style.GetPropertyValue("content"));
            Assert.AreEqual("12px", areaRule.Style.GetPropertyValue("font-size"));
            Assert.AreEqual(string.Empty, backLinkRule.Style.GetPropertyValue("display")); // Visible なので display プロパティはなし
        }
    }
}
