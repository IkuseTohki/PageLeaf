using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageLeaf.Utilities;

namespace PageLeaf.Tests.Utilities
{
    [TestClass]
    public class UnitConversionHelperTests
    {
        [TestMethod]
        [DataRow("16px", 16.0, "px")]
        [DataRow("1.2em", 1.2, "em")]
        [DataRow("150%", 150.0, "%")]
        [DataRow("24", 24.0, "")] // 単位なしは空
        [DataRow(null, 0.0, "")]   // null は空
        [DataRow("", 0.0, "")]     // 空文字は空
        public void Split_ShouldSeparateValueAndUnit(string? input, double expectedValue, string expectedUnit)
        {
            // テスト観点: CSS文字列を数値と単位に正しく分解できることを確認する。
            var (value, unit) = UnitConversionHelper.Split(input);
            Assert.AreEqual(expectedValue, value);
            Assert.AreEqual(expectedUnit, unit);
        }

        [TestMethod]
        [DataRow(16.0, "px", "em", 1.0)]
        [DataRow(1.0, "em", "px", 16.0)]
        [DataRow(16.0, "px", "%", 100.0)]
        [DataRow(100.0, "%", "px", 16.0)]
        [DataRow(1.0, "em", "%", 100.0)]
        public void Convert_ShouldConvertBetweenUnits(double input, string from, string to, double expected)
        {
            // テスト観点: サポートされている単位（px, em, %）間で正しく数値変換が行われることを確認する。
            var result = UnitConversionHelper.Convert(input, from, to);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        [DataRow(14.0, "px", "14px")]
        [DataRow(1.25, "em", "1.25em")]
        [DataRow(100.0, "%", "100%")]
        public void Format_ShouldReturnCssString(double value, string unit, string expected)
        {
            // テスト観点: 数値と単位を結合して、正しいCSS形式の文字列を生成できることを確認する。
            var result = UnitConversionHelper.Format(value, unit);
            Assert.AreEqual(expected, result);
        }
    }
}
