using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageLeaf.Models.Css.Values;
using PageLeaf.Services;
using AngleSharp.Css.Dom;
using AngleSharp.Css.Parser;
using System.Linq;

namespace PageLeaf.Tests.Models.Css.Values
{
    [TestClass]
    public class CssBorderTests
    {
        [TestMethod]
        public void ToString_ShouldReturnCorrectFormat()
        {
            var border = new CssBorder(new CssSize(2, CssUnit.Px), "dashed", new CssColor("#FF0000"));
            Assert.AreEqual("2px dashed #FF0000", border.ToString());
        }

        [TestMethod]
        public void Parse_ShouldCreateFromIndividualValues()
        {
            var border = CssBorder.Parse("3px", "dotted", "#0000FF");
            Assert.IsNotNull(border);
            Assert.IsNotNull(border.Width);
            Assert.IsNotNull(border.Color);
            Assert.AreEqual(3.0, border.Width.Value);
            Assert.AreEqual("dotted", border.Style);
            Assert.AreEqual("#0000FF", border.Color.HexCode.ToUpper());
        }

        [TestMethod]
        public void ApplyTo_ShouldSetPropertyInRule()
        {
            var parser = new CssParser();
            var sheet = parser.ParseStyleSheet("div {}");
            var rule = (ICssStyleRule)sheet.Rules[0];
            var border = new CssBorder(new CssSize(1, CssUnit.Px), "solid", new CssColor("#000000"));

            border.ApplyTo(rule.Style, "left");

            Assert.IsTrue(rule.Style.GetPropertyValue("border-left").Contains("1px solid"));
        }

        [TestMethod]
        public void Parse_MixedOrder_ShouldWork()
        {
            // テスト観点: CSSの仕様に基づき、順序が入れ替わっていても正しく抽出できること。
            // (FootnoteStyle.UpdateFrom 等で使用されるロジックのベースを確認)
            var service = new CssEditorService();
            var css = ".footnotes hr { border-top: solid #FF0000 2px; }";
            var profile = service.ParseToProfile(css);

            Assert.AreEqual("2px", profile.Footnote.AreaBorder?.Width?.ToString());
            Assert.AreEqual("solid", profile.Footnote.AreaBorder?.Style);
            Assert.AreEqual("#FF0000", profile.Footnote.AreaBorder?.Color?.HexCode);
        }

        [TestMethod]
        public void Parse_MissingParts_ShouldHandleGracefully()
        {
            // テスト観点: 色がない場合、スタイルがない場合でも、存在する部分だけ抽出されること。
            var service = new CssEditorService();

            var css1 = ".footnotes hr { border-top: 5px dashed; }"; // 色なし
            var profile1 = service.ParseToProfile(css1);
            Assert.AreEqual("5px", profile1.Footnote.AreaBorder?.Width?.ToString());
            Assert.AreEqual("dashed", profile1.Footnote.AreaBorder?.Style);
            Assert.IsNull(profile1.Footnote.AreaBorder?.Color);

            var css2 = ".footnotes hr { border-top: #00FF00; }"; // 色のみ
            var profile2 = service.ParseToProfile(css2);
            Assert.AreEqual("#00FF00", profile2.Footnote.AreaBorder?.Color?.HexCode);
            Assert.IsNull(profile2.Footnote.AreaBorder?.Width);
        }
    }
}
