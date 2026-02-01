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
            Assert.AreEqual(16.0, size.Value);
            Assert.AreEqual(CssUnit.Px, size.Unit);
        }

        [TestMethod]
        public void Parse_ValidEm_ShouldReturnCorrectSize()
        {
            // テスト観点: "1.5em" という文字列から数値 1.5 と単位 Em が正しくパースされること
            var size = CssSize.Parse("1.5em");
            Assert.AreEqual(1.5, size.Value);
            Assert.AreEqual(CssUnit.Em, size.Unit);
        }

        [TestMethod]
        public void Parse_InvalidFormat_ShouldThrowException()
        {
            // テスト観点: 不正な形式の文字列に対して例外が投げられること
            Assert.ThrowsException<FormatException>(() => CssSize.Parse("invalid"));
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
