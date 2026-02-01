using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageLeaf.Models.Css.Values;
using System;

namespace PageLeaf.Tests.Models.Css.Values
{
    [TestClass]
    public class CssColorTests
    {
        [TestMethod]
        public void Parse_Hex_ShouldReturnCorrectColor()
        {
            // テスト観点: "#FF0000" から赤色が正しくパースされること
            var color = CssColor.Parse("#FF0000");
            Assert.AreEqual("#FF0000", color.HexCode.ToUpper());
        }

        [TestMethod]
        public void Parse_Rgb_ShouldReturnCorrectHex()
        {
            // テスト観点: "rgb(255, 0, 0)" から HEX "#FF0000" に変換されること
            var color = CssColor.Parse("rgb(255, 0, 0)");
            Assert.AreEqual("#FF0000", color.HexCode.ToUpper());
        }

        [TestMethod]
        public void Parse_Rgba_ShouldReturnCorrectHex()
        {
            // テスト観点: "rgba(255, 0, 0, 1.0)" から HEX "#FF0000" に変換されること
            var color = CssColor.Parse("rgba(255, 0, 0, 1.0)");
            Assert.AreEqual("#FF0000", color.HexCode.ToUpper());
        }

        [TestMethod]
        public void Parse_Transparent_ShouldReturnTransparentKeyword()
        {
            // テスト観点: "transparent" キーワードが保持されること
            var color = CssColor.Parse("transparent");
            Assert.AreEqual("transparent", color.HexCode.ToLower());
        }

        [TestMethod]
        public void ToString_ShouldReturnHex()
        {
            var color = new CssColor("#00FF00");
            Assert.AreEqual("#00FF00", color.ToString().ToUpper());
        }
    }
}
