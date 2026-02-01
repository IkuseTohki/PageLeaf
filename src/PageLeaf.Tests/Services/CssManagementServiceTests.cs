using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using PageLeaf.Models;
using PageLeaf.Models.Markdown;
using PageLeaf.Models.Css;
using PageLeaf.Models.Css.Elements;
using PageLeaf.Models.Settings;
using PageLeaf.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PageLeaf.Tests.Services
{
    [TestClass]
    public class CssManagementServiceTests
    {
        private Mock<ICssService> _cssServiceMock = null!;
        private Mock<ICssEditorService> _cssEditorServiceMock = null!;
        private Mock<IFileService> _fileServiceMock = null!;
        private Mock<ILogger<CssManagementService>> _loggerMock = null!;
        private CssManagementService _service = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _cssServiceMock = new Mock<ICssService>();
            _cssEditorServiceMock = new Mock<ICssEditorService>();
            _fileServiceMock = new Mock<IFileService>();
            _loggerMock = new Mock<ILogger<CssManagementService>>();

            _service = new CssManagementService(
                _cssServiceMock.Object,
                _cssEditorServiceMock.Object,
                _fileServiceMock.Object,
                _loggerMock.Object);
        }

        [TestMethod]
        public void GetAvailableCssFileNames_ShouldDelegateToCssService()
        {
            // テスト観点: GetAvailableCssFileNamesがICssServiceに正しく委譲されていることを確認する。

            // Arrange
            var expectedFiles = new List<string> { "style1.css", "style2.css" };
            _cssServiceMock.Setup(s => s.GetAvailableCssFileNames()).Returns(expectedFiles);

            // Act
            var result = _service.GetAvailableCssFileNames();

            // Assert
            CollectionAssert.AreEqual(expectedFiles, result.ToList());
            _cssServiceMock.Verify(s => s.GetAvailableCssFileNames(), Times.Once);
        }

        [TestMethod]
        public void LoadStyle_WithValidFile_ShouldReturnParsedStyle()
        {
            // テスト観点: 存在するCSSファイルを読み込んだ際、その内容が正しくパースされて返されることを確認する。

            // Arrange
            string fileName = "test.css";
            string filePath = @"C:\\styles\\test.css";
            string fileContent = "body { color: red; }";
            var expectedStyle = new CssStyleInfo { BodyTextColor = "red" };

            _cssServiceMock.Setup(s => s.GetCssPath(fileName)).Returns(filePath);
            _fileServiceMock.Setup(s => s.FileExists(filePath)).Returns(true);
            _fileServiceMock.Setup(s => s.ReadAllText(filePath)).Returns(fileContent);
            _cssEditorServiceMock.Setup(s => s.ParseCss(fileContent)).Returns(expectedStyle);

            // Act
            var result = _service.LoadStyle(fileName);

            // Assert
            Assert.AreEqual(expectedStyle, result);
            _fileServiceMock.Verify(s => s.ReadAllText(filePath), Times.Once);
            _cssEditorServiceMock.Verify(s => s.ParseCss(fileContent), Times.Once);
        }

        [TestMethod]
        public void LoadStyle_WhenFileNotFound_ShouldReturnEmptyStyle()
        {
            // テスト観点: 指定されたCSSファイルが存在しない場合、空のスタイル情報を返し、警告がログ出力されることを確認する。

            // Arrange
            string fileName = "missing.css";
            string filePath = @"C:\\styles\\missing.css";

            _cssServiceMock.Setup(s => s.GetCssPath(fileName)).Returns(filePath);
            _fileServiceMock.Setup(s => s.FileExists(filePath)).Returns(false);

            // Act
            var result = _service.LoadStyle(fileName);

            // Assert
            Assert.IsNotNull(result);
            _fileServiceMock.Verify(s => s.ReadAllText(It.IsAny<string>()), Times.Never);
        }

        [TestMethod]
        public void SaveStyle_ShouldUpdateAndWriteFile()
        {
            // テスト観点: スタイルの保存時、既存のファイル内容が読み込まれ、更新された内容がファイルに書き込まれることを確認する。

            // Arrange
            string fileName = "save.css";
            string filePath = @"C:\\styles\\save.css";
            string existingContent = "/* original */";
            string updatedContent = "/* updated */";
            var styleInfo = new CssStyleInfo { BodyTextColor = "blue" };

            _cssServiceMock.Setup(s => s.GetCssPath(fileName)).Returns(filePath);
            _fileServiceMock.Setup(s => s.FileExists(filePath)).Returns(true);
            _fileServiceMock.Setup(s => s.ReadAllText(filePath)).Returns(existingContent);
            _cssEditorServiceMock.Setup(s => s.UpdateCssContent(existingContent, styleInfo)).Returns(updatedContent);

            // Act
            _service.SaveStyle(fileName, styleInfo);

            // Assert
            _fileServiceMock.Verify(s => s.WriteAllText(filePath, updatedContent), Times.Once);
        }
    }
}
