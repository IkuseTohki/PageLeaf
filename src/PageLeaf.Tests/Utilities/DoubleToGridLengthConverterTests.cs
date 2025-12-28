using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageLeaf.Utilities;
using System.Globalization;
using System.Windows;

namespace PageLeaf.Tests.Utilities
{
    [TestClass]
    public class DoubleToGridLengthConverterTests
    {
        private DoubleToGridLengthConverter _converter = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _converter = new DoubleToGridLengthConverter();
        }

        [TestMethod]
        public void Convert_ShouldReturnGridLength_ForValidDouble()
        {
            // テスト観点: 正のdouble値が、同じ値を持つ絶対指定(Pixel)のGridLengthに変換されることを確認する。

            var value = 150.5;
            var result = _converter.Convert(value, typeof(GridLength), null!, CultureInfo.CurrentCulture);

            Assert.IsInstanceOfType(result, typeof(GridLength));
            var gridLength = (GridLength)result;
            Assert.IsTrue(gridLength.IsAbsolute);
            Assert.AreEqual(value, gridLength.Value);
        }

        [TestMethod]
        public void Convert_ShouldReturnZeroGridLength_ForInvalidDouble()
        {
            // テスト観点: 負の値やNaN、または不正な型が渡された場合、幅0のGridLengthを返すことを確認する。

            Assert.AreEqual(new GridLength(0), _converter.Convert(-10.0, typeof(GridLength), null!, CultureInfo.CurrentCulture));
            Assert.AreEqual(new GridLength(0), _converter.Convert(double.NaN, typeof(GridLength), null!, CultureInfo.CurrentCulture));
            Assert.AreEqual(new GridLength(0), _converter.Convert("not a double", typeof(GridLength), null!, CultureInfo.CurrentCulture));
        }

        [TestMethod]
        public void ConvertBack_ShouldReturnDouble_FromGridLength()
        {
            // テスト観点: GridLength(絶対指定)からdouble値へ逆変換できることを確認する。

            var gridLength = new GridLength(200.0);
            var result = _converter.ConvertBack(gridLength, typeof(double), null!, CultureInfo.CurrentCulture);

            Assert.AreEqual(200.0, result);
        }
    }
}
