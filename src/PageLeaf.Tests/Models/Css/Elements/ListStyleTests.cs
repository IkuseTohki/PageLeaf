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
            // テスト観点: ul, ol, li の各種スタイル（新設の行間含む）が正しく抽出されること
            // ListIndent は ul (または ol) の padding-left から取得する
            var css = "ul { list-style-type: square; padding-left: 20px; } ol { list-style-type: none; counter-reset: item; } li { font-size: 1.2em; line-height: 1.8; }";
            var sheet = CreateSheet(css);
            var style = new ListStyle();

            style.UpdateFrom(sheet);

            Assert.AreEqual("square", style.UnorderedListMarkerType);
            Assert.AreEqual("decimal-nested", style.OrderedListMarkerType);
            Assert.AreEqual(20.0, style.ListIndent?.Value);
            Assert.AreEqual(1.2, style.MarkerSize?.Value);
            Assert.AreEqual("1.8", style.LineHeight);
        }

        [TestMethod]
        public void UpdateFrom_ShouldParsePeriodVisibility()
        {
            // テスト観点: li::before の content からピリオドの有無が正しく抽出されること
            // ピリオドなし
            var css1 = "ol { list-style-type: none; } ol > li::before { content: counter(list-item) \" \"; }";
            var style1 = new ListStyle();
            style1.UpdateFrom(CreateSheet(css1));
            Assert.IsFalse(style1.HasOrderedListPeriod);

            // ピリオドあり（明示的設定）
            var css2 = "ol { list-style-type: none; } ol > li::before { content: counter(list-item) \". \"; }";
            var style2 = new ListStyle();
            style2.UpdateFrom(CreateSheet(css2));
            Assert.IsTrue(style2.HasOrderedListPeriod);

            // 未設定
            var style3 = new ListStyle();
            style3.UpdateFrom(CreateSheet(""));
            Assert.IsNull(style3.HasOrderedListPeriod);
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
                MarkerSize = new CssSize(14, CssUnit.Px),
                LineHeight = "1.6"
            };
            var sheet = CreateSheet("");

            style.ApplyTo(sheet);

            var ulRule = sheet.Rules.OfType<ICssStyleRule>().First(r => r.SelectorText == "ul");
            var olRule = sheet.Rules.OfType<ICssStyleRule>().First(r => r.SelectorText == "ol");
            var liRule = sheet.Rules.OfType<ICssStyleRule>().First(r => r.SelectorText == "li");

            Assert.AreEqual("circle", ulRule.Style.GetPropertyValue("list-style-type"));
            Assert.AreEqual("2em", ulRule.Style.GetPropertyValue("padding-left"));

            Assert.AreEqual("decimal", olRule.Style.GetPropertyValue("list-style-type"));
            Assert.AreEqual("2em", olRule.Style.GetPropertyValue("padding-left"));

            Assert.AreEqual("14px", liRule.Style.GetPropertyValue("font-size"));
            Assert.AreEqual("1.6", liRule.Style.GetPropertyValue("line-height"));
        }

        [TestMethod]
        public void ApplyTo_ShouldSetPeriodVisibility()
        {
            // テスト観点: HasOrderedListPeriod の値が設定されている場合でも、
            //            ListStyle.ApplyTo 自体は疑似要素ルールを生成しないこと。
            //            (二重定義防止のため、ルールの生成・制御は CssEditorService に一任された)

            // false の場合
            var style1 = new ListStyle { HasOrderedListPeriod = false, OrderedListMarkerType = "decimal" };
            var sheet1 = CreateSheet("");
            style1.ApplyTo(sheet1);

            var markerRule1 = sheet1.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText.Replace(" ", "").Contains("li::before"));
            Assert.IsNull(markerRule1, "ListStyle.ApplyTo は疑似要素ルールを生成してはならない");

            // true の場合
            var style2 = new ListStyle { HasOrderedListPeriod = true, OrderedListMarkerType = "decimal" };
            var sheet2 = CreateSheet("");
            style2.ApplyTo(sheet2);

            var markerRule2 = sheet2.Rules.OfType<ICssStyleRule>().FirstOrDefault(r => r.SelectorText.Replace(" ", "").Contains("li::before"));
            Assert.IsNull(markerRule2, "ListStyle.ApplyTo は疑似要素ルールを生成してはならない");
        }
    }
}
