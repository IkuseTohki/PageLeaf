using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Windows.Media;
using PageLeaf.Utilities;
using System.Windows;

namespace PageLeaf.Tests.Utilities
{
    [TestClass]
    public class StringToBrushConverterTests
    {
        /// <summary>
        /// テスト観点: 正常な16進数カラーコードが正しいSolidColorBrushに変換されることを確認する。
        /// </summary>
        [TestMethod]
        public void Convert_ValidHexColor_ReturnsSolidColorBrush()
        {
            // Arrange
            var converter = new StringToBrushConverter();
            string hexColor = "#FF0000"; // Red

            // Act
            var result = converter.Convert(hexColor, typeof(Brush), null!, System.Globalization.CultureInfo.CurrentCulture);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsInstanceOfType(result, typeof(SolidColorBrush));
            var brush = (SolidColorBrush)result;
            Assert.AreEqual(Colors.Red, brush.Color);
        }

        /// <summary>
        /// テスト観点: 不正な形式の文字列がDependencyProperty.UnsetValueに変換されることを確認する。
        /// </summary>
        [TestMethod]
        public void Convert_InvalidString_ReturnsUnsetValue()
        {
            // Arrange
            var converter = new StringToBrushConverter();
            string invalidString = "notAColor";

            // Act
            var result = converter.Convert(invalidString, typeof(Brush), null!, System.Globalization.CultureInfo.CurrentCulture);

            // Assert
            Assert.AreEqual(DependencyProperty.UnsetValue, result);
        }

        /// <summary>
        /// テスト観点: 空の文字列がDependencyProperty.UnsetValueに変換されることを確認する。
        /// </summary>
        [TestMethod]
        public void Convert_EmptyString_ReturnsUnsetValue()
        {
            // Arrange
            var converter = new StringToBrushConverter();
            string emptyString = "";

            // Act
            var result = converter.Convert(emptyString, typeof(Brush), null!, System.Globalization.CultureInfo.CurrentCulture);

            // Assert
            Assert.AreEqual(DependencyProperty.UnsetValue, result);
        }

        /// <summary>
        /// テスト観点: nullがDependencyProperty.UnsetValueに変換されることを確認する。
        /// </summary>
        [TestMethod]
        public void Convert_Null_ReturnsUnsetValue()
        {
            // Arrange
            var converter = new StringToBrushConverter();
            string? nullString = null;

            // Act
            var result = converter.Convert(nullString!, typeof(Brush), null!, System.Globalization.CultureInfo.CurrentCulture);

            // Assert
            Assert.AreEqual(DependencyProperty.UnsetValue, result);
        }

        /// <summary>
        /// テスト観点: ConvertBackメソッドがNotSupportedExceptionをスローすることを確認する。
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(NotSupportedException))]
        public void ConvertBack_ThrowsNotSupportedException()
        {
            // Arrange
            var converter = new StringToBrushConverter();
            SolidColorBrush brush = new SolidColorBrush(Colors.Blue);

            // Act
            converter.ConvertBack(brush, typeof(string), null!, System.Globalization.CultureInfo.CurrentCulture);

            // Assert (ExpectedException handles this)
        }
    }
}
