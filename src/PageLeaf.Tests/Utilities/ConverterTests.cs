using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageLeaf.Utilities;
using System.Globalization;

namespace PageLeaf.Tests.Utilities
{
    [TestClass]
    public class ConverterTests
    {
        // ==========================================
        // MultiValueConverter Tests
        // ==========================================

        [TestMethod]
        public void MultiValueConverter_Convert_ShouldReturnClonedArray()
        {
            /*
            テスト観点:
            MultiValueConverterが、入力されたオブジェクト配列の複製（クローン）を正しく返すか確認する。
            */
            // Arrange
            var converter = new MultiValueConverter();
            var values = new object[] { "Test", 123, true };

            // Act
            var result = converter.Convert(values, typeof(object), null!, CultureInfo.InvariantCulture);

            // Assert
            Assert.IsInstanceOfType(result, typeof(object[]));
            var resultArray = (object[])result!;

            // Reference checks
            Assert.AreNotSame(values, resultArray, "Result should be a clone, not the same reference.");

            // Content checks
            Assert.AreEqual(values.Length, resultArray.Length);
            Assert.AreEqual(values[0], resultArray[0]);
            Assert.AreEqual(values[1], resultArray[1]);
            Assert.AreEqual(values[2], resultArray[2]);
        }

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
