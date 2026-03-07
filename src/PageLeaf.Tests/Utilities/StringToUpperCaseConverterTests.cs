using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageLeaf.Utilities;
using System.Globalization;

namespace PageLeaf.Tests.Utilities
{
    /// <summary>
    /// StringToUpperCaseConverter のユニットテストです。
    /// </summary>
    [TestClass]
    public class StringToUpperCaseConverterTests
    {
        private StringToUpperCaseConverter _converter = null!;

        [TestInitialize]
        public void Setup()
        {
            _converter = new StringToUpperCaseConverter();
        }

        [TestMethod]
        public void Convert_ShouldReturnUpperCaseString_WhenInputIsString()
        {
            // テスト観点: 入力が文字列の場合、すべて大文字に変換されて返されることを確認する。
            var input = "TestString";
            var result = _converter.Convert(input, typeof(string), null!, CultureInfo.InvariantCulture);

            Assert.AreEqual("TESTSTRING", result);
        }

        [TestMethod]
        public void Convert_ShouldReturnOriginalValue_WhenInputIsNotString()
        {
            // テスト観点: 入力が文字列でない場合（例: 数値）、変換を行わずに元の値をそのまま返すことを確認する。
            int input = 123;
            var result = _converter.Convert(input, typeof(string), null!, CultureInfo.InvariantCulture);

            Assert.AreEqual(input, result);
        }

        [TestMethod]
        public void Convert_ShouldReturnNull_WhenInputIsNull()
        {
            // テスト観点: 入力が null の場合、null が返されることを確認する。
            object? input = null;
            var result = _converter.Convert(input, typeof(string), null!, CultureInfo.InvariantCulture);

            Assert.IsNull(result);
        }
    }
}
