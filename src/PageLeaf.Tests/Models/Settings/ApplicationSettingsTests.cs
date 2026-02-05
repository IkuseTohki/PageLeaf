using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageLeaf.Models;
using PageLeaf.Models.Settings;
using System;

namespace PageLeaf.Tests.Models.Settings
{
    [TestClass]
    public class ApplicationSettingsTests
    {
        [TestMethod]
        public void Constructor_ShouldSetDefaultValues()
        {
            // テスト観点: デフォルトコンストラクタで期待されるデフォルト値が設定されていることを確認する。
            // Arrange & Act
            var settings = new ApplicationSettings();

            // Assert
            Assert.AreEqual("github.css", settings.CodeBlockTheme);
            Assert.AreEqual("images", settings.ImageSaveDirectory);
            Assert.AreEqual("image_{Date}_{Time}", settings.ImageFileNameTemplate);
            Assert.AreEqual(4, settings.IndentSize);
            Assert.IsTrue(settings.UseSpacesForIndent);
            Assert.AreEqual(14.0, settings.EditorFontSize);
            Assert.IsTrue(settings.ShowTitleInPreview);
            Assert.IsTrue(settings.AutoInsertFrontMatter);
            Assert.AreEqual(ResourceSource.Local, settings.LibraryResourceSource);
            Assert.AreEqual(AppTheme.System, settings.Theme);
            Assert.IsTrue(settings.RenumberFootnotesOnSave);
        }

        [TestMethod]
        [DataRow(8.0)]
        [DataRow(14.0)]
        [DataRow(72.0)]
        public void EditorFontSize_ValidValue_ShouldBeAccepted(double fontSize)
        {
            // テスト観点: 正常なフォントサイズが受け入れられることを確認する。
            // Arrange
            var settings = new ApplicationSettings();

            // Act
            settings.EditorFontSize = fontSize;

            // Assert
            Assert.AreEqual(fontSize, settings.EditorFontSize);
        }

        [TestMethod]
        [DataRow(0.0)]
        [DataRow(-1.0)]
        [DataRow(500.0)]
        public void EditorFontSize_InvalidValue_ShouldThrowException(double fontSize)
        {
            // テスト観点: 範囲外のフォントサイズを設定しようとした場合に例外が送出されることを確認する。
            var settings = new ApplicationSettings();

            Assert.ThrowsException<ArgumentOutOfRangeException>(() => settings.EditorFontSize = fontSize);
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(4)]
        [DataRow(8)]
        public void IndentSize_ValidValue_ShouldBeAccepted(int indentSize)
        {
            // テスト観点: 正常なインデントサイズが受け入れられることを確認する。
            // Arrange
            var settings = new ApplicationSettings();

            // Act
            settings.IndentSize = indentSize;

            // Assert
            Assert.AreEqual(indentSize, settings.IndentSize);
        }

        [TestMethod]
        public void GetIndentString_ShouldReturnSpaces_WhenUseSpacesIsTrue()
        {
            // テスト観点: UseSpacesForIndent が true の場合、IndentSize 分のスペースが返されることを確認する。
            // Arrange
            var settings = new ApplicationSettings { UseSpacesForIndent = true, IndentSize = 2 };

            // Act
            var indent = settings.GetIndentString();

            // Assert
            Assert.AreEqual("  ", indent);
        }

        [TestMethod]
        public void IncreaseIndent_ShouldAddIndentString()
        {
            // テスト観点: IncreaseIndent が設定に基づいたインデントを文字列の先頭に追加することを確認する。
            // Arrange
            var settings = new ApplicationSettings { UseSpacesForIndent = true, IndentSize = 4 };
            var line = "text";

            // Act
            var result = settings.IncreaseIndent(line);

            // Assert
            Assert.AreEqual("    text", result);
        }

        [TestMethod]
        public void DecreaseIndent_WithSpaces_ShouldRemoveSpaces()
        {
            // テスト観点: DecreaseIndent が設定されたスペース数分のインデントを削除することを確認する。
            // Arrange
            var settings = new ApplicationSettings { UseSpacesForIndent = true, IndentSize = 4 };

            // ちょうど4つ
            Assert.AreEqual("text", settings.DecreaseIndent("    text"));
            // 4つ以上
            Assert.AreEqual("  text", settings.DecreaseIndent("      text"));
            // 4つ未満
            Assert.AreEqual("text", settings.DecreaseIndent("  text"));
        }

        [TestMethod]
        public void DecreaseIndent_WithTab_ShouldRemoveTab()
        {
            // テスト観点: DecreaseIndent がタブ文字を1つ削除することを確認する。
            // Arrange
            var settings = new ApplicationSettings(); // デフォルト設定

            // Act & Assert
            Assert.AreEqual("text", settings.DecreaseIndent("\ttext"));
            Assert.AreEqual("\ttext", settings.DecreaseIndent("\t\ttext"));
        }
    }
}
