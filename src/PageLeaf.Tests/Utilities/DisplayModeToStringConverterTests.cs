using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageLeaf.Models;
using PageLeaf.Models.Markdown;
using PageLeaf.Models.Css;
using PageLeaf.Models.Css.Elements;
using PageLeaf.Models.Settings;
using PageLeaf.Utilities;
using System.Globalization;

namespace PageLeaf.Tests.Utilities
{
    [TestClass]
    public class DisplayModeToStringConverterTests
    {
        private DisplayModeToStringConverter _converter = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _converter = new DisplayModeToStringConverter();
        }

        [TestMethod]
        public void Convert_ShouldReturnCorrectString_ForEachMode()
        {
            // テスト観点: 各表示モードが、UIに表示するための適切な日本語文字列に変換されることを確認する。

            Assert.AreEqual("ビューアーモード", _converter.Convert(DisplayMode.Viewer, typeof(string), null!, CultureInfo.CurrentCulture));
            Assert.AreEqual("Markdown 編集モード", _converter.Convert(DisplayMode.Markdown, typeof(string), null!, CultureInfo.CurrentCulture));
        }

        [TestMethod]
        public void Convert_ShouldReturnEmpty_ForInvalidValue()
        {
            // テスト観点: DisplayMode型以外の値が渡された場合、空文字列を返すことを確認する。

            Assert.AreEqual(string.Empty, _converter.Convert("invalid", typeof(string), null!, CultureInfo.CurrentCulture));
            Assert.AreEqual(string.Empty, _converter.Convert(null!, typeof(string), null!, CultureInfo.CurrentCulture));
        }
    }
}
