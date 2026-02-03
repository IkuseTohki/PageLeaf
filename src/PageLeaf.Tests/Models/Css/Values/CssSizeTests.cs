using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageLeaf.Models.Css.Values;
using System;

namespace PageLeaf.Tests.Models.Css.Values
{
    [TestClass]
    public class CssSizeTests
    {
        [TestMethod]
        public void Parse_ValidPx_ShouldReturnCorrectSize()
        {
            // テスト観点: "16px" という文字列から数値 16 と単位 Px が正しくパースされること
            var size = CssSize.Parse("16px");
            Assert.IsNotNull(size);
            Assert.AreEqual(16.0, size.Value);
            Assert.AreEqual(CssUnit.Px, size.Unit);
        }

        [TestMethod]
        public void Parse_ValidEm_ShouldReturnCorrectSize()
        {
            // テスト観点: "1.5em" という文字列から数値 1.5 と単位 Em が正しくパースされること
            var size = CssSize.Parse("1.5em");
            Assert.IsNotNull(size);
            Assert.AreEqual(1.5, size.Value);
            Assert.AreEqual(CssUnit.Em, size.Unit);
        }

        [TestMethod]
        public void Parse_InvalidFormat_ShouldThrowException()
        {
            Assert.ThrowsException<FormatException>(() => CssSize.Parse("invalid"));
            Assert.ThrowsException<FormatException>(() => CssSize.Parse("10.5.2px"));
        }

        [TestMethod]
        public void Parse_ZeroValue_ShouldReturnZeroSize()
        {
            // テスト観点: 0px は null ではなく、値 0 のインスタンスとしてパースされること。
            var size = CssSize.Parse("0px");
            Assert.IsNotNull(size);
            Assert.AreEqual(0.0, size.Value);
            Assert.AreEqual(CssUnit.Px, size.Unit);
        }

        [TestMethod]
        public void Parse_UnitlessValue_ShouldReturnNoneUnit()
        {
            // テスト観点: 単位がない数値（line-height等）が正しくパースされること。
            var size = CssSize.Parse("1.5");
            Assert.IsNotNull(size);
            Assert.AreEqual(1.5, size.Value);
            Assert.AreEqual(CssUnit.None, size.Unit);
        }

        [TestMethod]
        public void Parse_EmptyOrNull_ShouldReturnNull()
        {
            // テスト観点: 空文字や null はインスタンスを生成せず null を返すこと。
            Assert.IsNull(CssSize.Parse(""));
            Assert.IsNull(CssSize.Parse(null));
            Assert.IsNull(CssSize.Parse("   "));
        }

        [TestMethod]
        public void ToString_ZeroWithUnit_ShouldIncludeUnit()
        {
            var size = new CssSize(0, CssUnit.Px);
            Assert.AreEqual("0px", size.ToString());
        }

        [TestMethod]
        public void ToString_Unitless_ShouldNotIncludeUnit()
        {
            var size = new CssSize(1.6, CssUnit.None);
            Assert.AreEqual("1.6", size.ToString());
        }

        [TestMethod]
        public void ToString_ShouldReturnCssString()
        {
            // テスト観点: 自身の状態を CSS 形式の文字列 ("16px") に変換できること
            var size = new CssSize(16.0, CssUnit.Px);
            Assert.AreEqual("16px", size.ToString());
        }

        [TestMethod]
        public void ToPx_FromEm_ShouldConvertCorrectly()
        {
            // テスト観点: 基準フォントサイズ 16px のとき、1.5em が 24px に変換されること
            var size = new CssSize(1.5, CssUnit.Em);
            var pxSize = size.ToPx(16.0);
            Assert.AreEqual(24.0, pxSize.Value);
            Assert.AreEqual(CssUnit.Px, pxSize.Unit);
        }
    }
}
