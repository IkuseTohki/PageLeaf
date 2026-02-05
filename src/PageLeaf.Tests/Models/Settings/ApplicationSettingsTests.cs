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
            // テスト観点: デフォルトコンストラクタで各カテゴリの期待されるデフォルト値が設定されていることを確認する。
            // Arrange & Act
            var settings = new ApplicationSettings();

            // Assert
            Assert.AreEqual("github.css", settings.View.CodeBlockTheme);
            Assert.AreEqual("images", settings.Image.SaveDirectory);
            Assert.AreEqual("image_{Date}_{Time}", settings.Image.FileNameTemplate);
            Assert.AreEqual(4, settings.Editor.IndentSize);
            Assert.IsTrue(settings.Editor.UseSpacesForIndent);
            Assert.AreEqual(14.0, settings.Editor.EditorFontSize);
            Assert.IsTrue(settings.View.ShowTitleInPreview);
            Assert.IsTrue(settings.Editor.AutoInsertFrontMatter);
            Assert.AreEqual(ResourceSource.Local, settings.Appearance.LibraryResourceSource);
            Assert.AreEqual(AppTheme.System, settings.Appearance.Theme);
            Assert.IsTrue(settings.Editor.RenumberFootnotesOnSave);
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
            settings.Editor.EditorFontSize = fontSize;

            // Assert
            Assert.AreEqual(fontSize, settings.Editor.EditorFontSize);
        }

        [TestMethod]
        [DataRow(0.0)]
        [DataRow(-1.0)]
        [DataRow(101.0)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void EditorFontSize_InvalidValue_ShouldThrowException(double fontSize)
        {
            // テスト観点: 範囲外のフォントサイズを設定しようとした場合に例外が送出されることを確認する。
            var settings = new ApplicationSettings();
            settings.Editor.EditorFontSize = fontSize;
        }

        [TestMethod]
        [DataRow(1)]
        [DataRow(4)]
        [DataRow(32)]
        public void IndentSize_ValidValue_ShouldBeAccepted(int indentSize)
        {
            // テスト観点: 正常なインデントサイズが受け入れられることを確認する。
            // Arrange
            var settings = new ApplicationSettings();

            // Act
            settings.Editor.IndentSize = indentSize;

            // Assert
            Assert.AreEqual(indentSize, settings.Editor.IndentSize);
        }

        [TestMethod]
        [DataRow(0)]
        [DataRow(-1)]
        [DataRow(33)]
        [ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void IndentSize_InvalidValue_ShouldThrowException(int indentSize)
        {
            var settings = new ApplicationSettings();
            settings.Editor.IndentSize = indentSize;
        }

        [TestMethod]
        public void GetIndentString_ShouldReturnCorrectString()
        {
            // テスト観点: UseSpacesForIndent が true の場合、IndentSize 分のスペースが返されることを確認する。
            // Arrange
            var settings = new ApplicationSettings();
            settings.Editor.UseSpacesForIndent = true;
            settings.Editor.IndentSize = 2;

            // Act & Assert
            Assert.AreEqual("  ", settings.Editor.GetIndentString());

            settings.Editor.UseSpacesForIndent = false;
            Assert.AreEqual("\t", settings.Editor.GetIndentString());
        }

        [TestMethod]
        public void IncreaseIndent_ShouldAddIndentString()
        {
            // テスト観点: IncreaseIndent が設定に基づいたインデントを文字列の先頭に追加することを確認する。
            // Arrange
            var settings = new ApplicationSettings();
            settings.Editor.UseSpacesForIndent = true;
            settings.Editor.IndentSize = 4;
            var line = "text";

            // Act
            var result = settings.Editor.IncreaseIndent(line);

            // Assert
            Assert.AreEqual("    text", result);
        }

        [TestMethod]
        public void DecreaseIndent_ShouldRemoveLeadingIndent()
        {
            // Arrange
            var settings = new ApplicationSettings();
            settings.Editor.UseSpacesForIndent = true;
            settings.Editor.IndentSize = 4;

            // Act & Assert
            Assert.AreEqual("text", settings.Editor.DecreaseIndent("    text"));
            Assert.AreEqual("  text", settings.Editor.DecreaseIndent("      text"));

            settings.Editor.UseSpacesForIndent = false;
            Assert.AreEqual("text", settings.Editor.DecreaseIndent("\ttext"));
        }
    }
}
