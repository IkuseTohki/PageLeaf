using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageLeaf.Utilities;
using System;

namespace PageLeaf.Tests.Utilities
{
    [TestClass]
    public class UnitConversionHelperTests
    {
        private const double BasePx = 16.0;

        [TestMethod]
        public void PxToEm_ShouldConvertCorrectly()
        {
            // 16px -> 1em
            Assert.AreEqual(1.0, UnitConversionHelper.PxToEm(16.0), 0.001);
            // 20px -> 1.25em
            Assert.AreEqual(1.25, UnitConversionHelper.PxToEm(20.0), 0.001);
            // 0px -> 0em
            Assert.AreEqual(0.0, UnitConversionHelper.PxToEm(0.0), 0.001);
        }

        [TestMethod]
        public void EmToPx_ShouldConvertCorrectly()
        {
            // 1em -> 16px
            Assert.AreEqual(16.0, UnitConversionHelper.EmToPx(1.0), 0.001);
            // 1.5em -> 24px
            Assert.AreEqual(24.0, UnitConversionHelper.EmToPx(1.5), 0.001);
        }

        [TestMethod]
        public void PxToPercent_ShouldConvertCorrectly()
        {
            // 16px -> 100%
            Assert.AreEqual(100.0, UnitConversionHelper.PxToPercent(16.0), 0.001);
            // 8px -> 50%
            Assert.AreEqual(50.0, UnitConversionHelper.PxToPercent(8.0), 0.001);
        }

        [TestMethod]
        public void PercentToPx_ShouldConvertCorrectly()
        {
            // 100% -> 16px
            Assert.AreEqual(16.0, UnitConversionHelper.PercentToPx(100.0), 0.001);
            // 125% -> 20px
            Assert.AreEqual(20.0, UnitConversionHelper.PercentToPx(125.0), 0.001);
        }

        [TestMethod]
        public void Round_ShouldFormatToReasonablePrecision()
        {
            // 小数点第3位以下を四捨五入して扱いやすくする
            Assert.AreEqual(1.23, UnitConversionHelper.Round(1.2345), 0.001);
            Assert.AreEqual(1.24, UnitConversionHelper.Round(1.2351), 0.001);
        }

        [TestMethod]
        public void ParseAndConvert_ShouldHandleVariousUnits()
        {
            // テスト観点: CSSの単位付き文字列を解析し、指定した単位に正しく変換されることを確認する。

            // "24px" -> "1.5" (em)
            Assert.AreEqual("1.5", UnitConversionHelper.ParseAndConvert("24px", "em"));
            // "1.25em" -> "20" (px)
            Assert.AreEqual("20", UnitConversionHelper.ParseAndConvert("1.25em", "px"));
            // "150%" -> "1.5" (em)
            Assert.AreEqual("1.5", UnitConversionHelper.ParseAndConvert("150%", "em"));
            // "16" (単位なし) -> "16" (px指定時)
            Assert.AreEqual("16", UnitConversionHelper.ParseAndConvert("16", "px"));
        }

        [TestMethod]
        public void ParseAndConvert_ShouldReturnDefault_WhenInputIsInvalid()
        {
            // テスト観点: 不正な入力や空文字の場合、指定されたデフォルト値を返すことを確認する。
            Assert.AreEqual("16", UnitConversionHelper.ParseAndConvert("", "px", "16"));
            Assert.AreEqual("1", UnitConversionHelper.ParseAndConvert(null, "em", "1"));
            // "auto" は数値解析不能なため、defaultValue ("16") が返るのが現在の仕様
            Assert.AreEqual("16", UnitConversionHelper.ParseAndConvert("auto", "px", "16"));
        }
    }
}
