using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageLeaf.Utilities;
using LeafKit.UI.Converters;
using System.Globalization;

namespace PageLeaf.Tests.Utilities
{
    [TestClass]
    public class ConverterTests
    {
        // ==========================================
        // StringToUpperCaseConverter Tests
        // ==========================================

        [TestMethod]
        public void StringToUpperCaseConverter_Convert_ShouldReturnUpperCaseString()
        {
            /*
            テスト観点:
            入力が文字列の場合、すべて大文字に変換されて返されることを確認する。
            */
            // Arrange
            var converter = new StringToUpperCaseConverter();
            var input = "TestString";

            // Act
            var result = converter.Convert(input, typeof(string), null!, CultureInfo.InvariantCulture);

            // Assert
            Assert.AreEqual("TESTSTRING", result);
        }

        [TestMethod]
        public void StringToUpperCaseConverter_Convert_ShouldReturnOriginalValue_WhenNotString()
        {
            /*
            テスト観点:
            入力が文字列でない場合（例: 数値、nullなど）、変換を行わずに元の値をそのまま返すことを確認する。
            */
            // Arrange
            var converter = new StringToUpperCaseConverter();
            int input = 123;

            // Act
            var result = converter.Convert(input, typeof(string), null!, CultureInfo.InvariantCulture);

            // Assert
            Assert.AreEqual(input, result);
        }

        [TestMethod]
        public void StringToUpperCaseConverter_Convert_ShouldReturnNull_WhenInputIsNull()
        {
            /*
            テスト観点:
            入力がnullの場合、nullが返されることを確認する。
            */
            // Arrange
            var converter = new StringToUpperCaseConverter();
            object? input = null;

            // Act
            var result = converter.Convert(input, typeof(string), null!, CultureInfo.InvariantCulture);

            // Assert
            Assert.IsNull(result);
        }
    }
}
