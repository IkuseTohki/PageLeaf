using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Drawing;
using PageLeaf.Utilities;

namespace PageLeaf.Tests.Utilities
{
    [TestClass]
    public class ColorConverterHelperTests
    {
        /// <summary>
        /// テスト観点: Colorオブジェクトが#RRGGBB形式の文字列に正しく変換されることを確認する。
        /// </summary>
        [TestMethod]
        public void ToRgbString_WithOpaqueRed_ShouldReturnRgbFormat()
        {
            // Arrange
            var color = Color.FromArgb(255, 255, 0, 0); // Opaque Red
            var expected = "#FF0000";

            // Act
            var result = ColorConverterHelper.ToRgbString(color);

            // Assert
            Assert.AreEqual(expected, result);
        }
    }
}
