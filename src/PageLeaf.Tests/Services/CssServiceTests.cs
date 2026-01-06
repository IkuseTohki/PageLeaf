using Moq;
using PageLeaf.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.IO;

namespace PageLeaf.Tests.Services
{
    [TestClass]
    public class CssServiceTests
    {
        private Mock<IFileService> _mockFileService = null!;
        private Mock<ILogger<CssService>> _mockLogger = null!;
        private CssService _cssService = null!; // この行は CssService が存在しないため、コンパイルエラーになります。

        [TestInitialize]
        public void Setup()
        {
            _mockFileService = new Mock<IFileService>();
            _mockLogger = new Mock<ILogger<CssService>>();
        }

        [TestMethod]
        public void test_GetAvailableCssFileNames_ShouldReturnCorrectNames()
        {
            // テスト観点: CssService が IFileService を利用して、指定されたディレクトリからCSSファイルのファイル名を正しく取得できることを確認する。
            // Arrange
            var cssFiles = new List<string>
            {
                "C:\\app\\css\\github.css",
                "C:\\app\\css\\solarized-light.css",
                "C:\\app\\css\\another.css"
            };
            _mockFileService.Setup(fs => fs.GetFiles(It.IsAny<string>(), "*.css"))
                            .Returns(cssFiles);

            _cssService = new CssService(_mockFileService.Object, _mockLogger.Object);

            // Act
            var result = _cssService.GetAvailableCssFileNames().ToList();

            // Assert
            CollectionAssert.AreEquivalent(new[] { "github.css", "solarized-light.css", "another.css" }, result);
            _mockFileService.Verify(fs => fs.GetFiles(It.IsAny<string>(), "*.css"), Times.Once);
        }

        [TestMethod]
        public void test_GetAvailableCssFileNames_ShouldReturnEmptyList_WhenNoCssFilesFound()
        {
            // テスト観点: CSSファイルが見つからない場合に、空のリストが返されることを確認する。
            // Arrange
            _mockFileService.Setup(fs => fs.GetFiles(It.IsAny<string>(), "*.css"))
                            .Returns(new List<string>());

            _cssService = new CssService(_mockFileService.Object, _mockLogger.Object);

            // Act
            var result = _cssService.GetAvailableCssFileNames().ToList();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
            _mockFileService.Verify(fs => fs.GetFiles(It.IsAny<string>(), "*.css"), Times.Once);
        }

        [TestMethod]
        public void test_GetAvailableCssFileNames_ShouldReturnOnlyFileNames()
        {
            // テスト観点: 取得したパスからファイル名のみが抽出されて返されることを確認する。
            // Arrange
            var cssFilesWithPaths = new List<string>
            {
                "C:\\path\\to\\css\\file1.css",
                "D:\\another\\location\\file2.css"
            };
            _mockFileService.Setup(fs => fs.GetFiles(It.IsAny<string>(), "*.css"))
                            .Returns(cssFilesWithPaths);

            _cssService = new CssService(_mockFileService.Object, _mockLogger.Object);

            // Act
            var result = _cssService.GetAvailableCssFileNames().ToList();

            // Assert
            CollectionAssert.AreEquivalent(new[] { "file1.css", "file2.css" }, result);
            _mockFileService.Verify(fs => fs.GetFiles(It.IsAny<string>(), "*.css"), Times.Once);
        }

        [TestMethod]
        public void test_GetAvailableCssFileNames_ShouldExcludeExtensionsCss()
        {
            // テスト観点: extensions.css が利用可能なCSSファイルリストから除外されることを確認する。
            // Arrange
            var cssFiles = new List<string>
            {
                "C:\\app\\css\\github.css",
                "C:\\app\\css\\extensions.css",
                "C:\\app\\css\\another.css"
            };
            _mockFileService.Setup(fs => fs.GetFiles(It.IsAny<string>(), "*.css"))
                            .Returns(cssFiles);

            _cssService = new CssService(_mockFileService.Object, _mockLogger.Object);

            // Act
            var result = _cssService.GetAvailableCssFileNames().ToList();

            // Assert
            CollectionAssert.AreEquivalent(new[] { "github.css", "another.css" }, result);
            Assert.IsFalse(result.Contains("extensions.css"));
        }

        [TestMethod]
        public void test_GetAvailableCssFileNames_ShouldHandleFileServiceExceptionGracefully()
        {
            // テスト観点: IFileService.GetFiles が例外をスローした場合でも、CssService が適切に処理し、空のリストを返すことを確認する。
            // Arrange
            _mockFileService.Setup(fs => fs.GetFiles(It.IsAny<string>(), "*.css"))
                            .Throws(new System.Exception("Simulated file service error"));

            _cssService = new CssService(_mockFileService.Object, _mockLogger.Object);

            // Act
            var result = _cssService.GetAvailableCssFileNames().ToList();

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
            _mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Error getting available CSS file names")),
                    It.IsAny<System.Exception>(),
                    It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
                Times.Once);
        }

        [TestMethod]
        public void test_GetCssPath_ShouldReturnCorrectAbsolutePath()
        {
            // テスト観点: GetCssPath が、指定されたファイル名に対して正しい絶対パスを返すことを確認する。
            // Arrange
            var cssFileName = "github.css";
            _cssService = new CssService(_mockFileService.Object, _mockLogger.Object);
            var expectedPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "css", cssFileName);

            // Act
            var result = _cssService.GetCssPath(cssFileName);

            // Assert
            Assert.AreEqual(expectedPath, result);
        }

        [TestMethod]
        public void test_GetCssPath_ShouldReturnEmptyString_WhenFileNameIsEmpty()
        {
            // テスト観点: ファイル名が空の場合、GetCssPath が空文字列を返すことを確認する。
            // Arrange
            _cssService = new CssService(_mockFileService.Object, _mockLogger.Object);

            // Act
            var result = _cssService.GetCssPath("");

            // Assert
            Assert.AreEqual(string.Empty, result);
        }
    }
}
