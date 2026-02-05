using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageLeaf.Models.Markdown;
using PageLeaf.Utilities;
using System.Windows;

namespace PageLeaf.Tests.Utilities
{
    [TestClass]
    public class LevelToMarginConverterTests
    {
        private LevelToMarginConverter _converter = null!;

        [TestInitialize]
        public void SetUp()
        {
            _converter = new LevelToMarginConverter();
        }

        [TestMethod]
        [DataRow(1, 0.0)]
        [DataRow(2, 15.0)]
        [DataRow(3, 30.0)]
        public void Convert_WithTocItem_ShouldReturnCorrectThickness(int level, double expectedLeft)
        {
            // Arrange
            var item = new TocItem(level, "text", "id", 0);

            // Act
            var result = _converter.Convert(item, typeof(Thickness), null!, null!);

            // Assert
            Assert.IsInstanceOfType(result, typeof(Thickness));
            var thickness = (Thickness)result;
            Assert.AreEqual(expectedLeft, thickness.Left);
            Assert.AreEqual(0, thickness.Top);
            Assert.AreEqual(0, thickness.Right);
            Assert.AreEqual(0, thickness.Bottom);
        }

        [TestMethod]
        [DataRow(1, 0.0)]
        [DataRow(2, 15.0)]
        [DataRow(3, 30.0)]
        public void Convert_WithInt_ShouldReturnCorrectThickness(int level, double expectedLeft)
        {
            // Arrange

            // Act
            var result = _converter.Convert(level, typeof(Thickness), null!, null!);

            // Assert
            Assert.IsInstanceOfType(result, typeof(Thickness));
            var thickness = (Thickness)result;
            Assert.AreEqual(expectedLeft, thickness.Left);
        }

        [TestMethod]
        public void Convert_WithInvalidType_ShouldReturnZeroThickness()
        {
            // Arrange
            var invalidInput = "invalid";

            // Act
            var result = _converter.Convert(invalidInput, typeof(Thickness), null!, null!);

            // Assert
            Assert.IsInstanceOfType(result, typeof(Thickness));
            var thickness = (Thickness)result;
            Assert.AreEqual(0, thickness.Left);
        }
    }
}
