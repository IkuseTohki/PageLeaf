
using PageLeaf.Utilities;

namespace PageLeaf.Tests.Utilities
{
    [TestClass]
    public class RgbaToHexConverterTests
    {
        private RgbaToHexConverter _converter;

        [TestInitialize]
        public void Setup()
        {
            _converter = new RgbaToHexConverter();
        }

        /// <summary>
        /// テスト観点: Convertメソッドが、有効なRGBA文字列を正しいHEX文字列に変換することを確認する。
        /// </summary>
        [TestMethod]
        [DataRow("rgba(255, 0, 0, 1)", "#ff0000")]
        [DataRow("rgba(0, 255, 0, 0.5)", "#00ff00")]
        [DataRow("rgba(0, 0, 255, 1)", "#0000ff")]
        [DataRow("rgba(255, 255, 255, 1)", "#ffffff")]
        [DataRow("rgba(0, 0, 0, 1)", "#000000")]
        public void Convert_WithValidRgba_ShouldReturnCorrectHex(string rgba, string expectedHex)
        {
            // Act
            var result = _converter.Convert(rgba, typeof(string), null, System.Globalization.CultureInfo.CurrentCulture);

            // Assert
            Assert.AreEqual(expectedHex, result);
        }

        /// <summary>
        /// テスト観点: ConvertBackメソッドが、有効なHEX文字列を正しいRGBA文字列に変換することを確認する。
        /// </summary>
        [TestMethod]
        [DataRow("#ff0000", "rgba(255, 0, 0, 1)")]
        [DataRow("#00ff00", "rgba(0, 255, 0, 1)")]
        [DataRow("#0000ff", "rgba(0, 0, 255, 1)")]
        [DataRow("#ffffff", "rgba(255, 255, 255, 1)")]
        [DataRow("#000000", "rgba(0, 0, 0, 1)")]
        public void ConvertBack_WithValidHex_ShouldReturnCorrectRgba(string hex, string expectedRgba)
        {
            // Act
            var result = _converter.ConvertBack(hex, typeof(string), null, System.Globalization.CultureInfo.CurrentCulture);

            // Assert
            Assert.AreEqual(expectedRgba, result);
        }

        /// <summary>
        /// テスト観点: Convertメソッドが無効な入力に対して、元の値をそのまま返すことを確認する。
        /// </summary>
        [TestMethod]
        [DataRow("rgb(255, 0, 0)")] // 'a'がない
        [DataRow("rgba(255, 0, 0)")]  // 要素が足りない
        [DataRow("invalid-string")]
        [DataRow(null)]
        public void Convert_WithInvalidInput_ShouldReturnOriginalValue(object invalidInput)
        {
            // Act
            var result = _converter.Convert(invalidInput, typeof(string), null, System.Globalization.CultureInfo.CurrentCulture);

            // Assert
            Assert.AreEqual(invalidInput, result);
        }

        /// <summary>
        /// テスト観点: ConvertBackメソッドが無効な入力に対して、元の値をそのまま返すことを確認する。
        /// </summary>
        [TestMethod]
        [DataRow("#ff00f")] // 短い
        [DataRow("invalid-hex")]
        [DataRow(null)]
        public void ConvertBack_WithInvalidInput_ShouldReturnOriginalValue(object invalidInput)
        {
            // Act
            var result = _converter.ConvertBack(invalidInput, typeof(string), null, System.Globalization.CultureInfo.CurrentCulture);

            // Assert
            Assert.AreEqual(invalidInput, result);
        }
    }
}
