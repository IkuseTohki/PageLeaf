using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageLeaf.Models.Markdown;
using PageLeaf.Models.Settings;
using PageLeaf.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PageLeaf.Tests.Services
{
    [TestClass]
    public class EditingSupportServiceTests
    {
        private EditingSupportService _service = null!;

        [TestInitialize]
        public void Setup()
        {
            _service = new EditingSupportService();
        }

        [TestMethod]
        public void GetAutoIndent_ShouldReturnCorrectIndent()
        {
            Assert.AreEqual("    ", _service.GetAutoIndent("    text"));
            Assert.AreEqual("\t", _service.GetAutoIndent("\ttext"));
            Assert.AreEqual(string.Empty, _service.GetAutoIndent("text"));
        }

        [TestMethod]
        public void GetIndentString_ShouldReturnCorrectString()
        {
            // テスト観点: 設定に応じて正しいインデント文字列を返す。

            // スペース4つ
            var settings = new ApplicationSettings { Editor = new EditorSettings { IndentSize = 4, UseSpacesForIndent = true } };
            Assert.AreEqual("    ", settings.Editor.GetIndentString());

            // スペース2つ
            settings = new ApplicationSettings { Editor = new EditorSettings { IndentSize = 2, UseSpacesForIndent = true } };
            Assert.AreEqual("  ", settings.Editor.GetIndentString());

            // タブ
            settings = new ApplicationSettings { Editor = new EditorSettings { UseSpacesForIndent = false } };
            Assert.AreEqual("\t", settings.Editor.GetIndentString());
        }

        [TestMethod]
        public void DecreaseIndent_ShouldRemoveLeadingSpacesOrTab()
        {
            // テスト観点: 行頭のインデントを1レベル分削除する。
            var settings = new ApplicationSettings { Editor = new EditorSettings { IndentSize = 4, UseSpacesForIndent = true } };

            Assert.AreEqual("No indent", settings.Editor.DecreaseIndent("    No indent"));
            Assert.AreEqual("  Partially removed", settings.Editor.DecreaseIndent("      Partially removed"));
            Assert.AreEqual("No indent", settings.Editor.DecreaseIndent("\tNo indent")); // タブも削除対象
            Assert.AreEqual("Normal", settings.Editor.DecreaseIndent("Normal")); // インデントなし
        }

        [TestMethod]
        public void IncreaseIndent_ShouldAddLeadingSpacesOrTab()
        {
            // テスト観点: 行頭に1レベル分のインデントを追加する。
            var settings = new ApplicationSettings { Editor = new EditorSettings { IndentSize = 4, UseSpacesForIndent = true } };

            Assert.AreEqual("    * Item", settings.Editor.IncreaseIndent("* Item"));
            Assert.AreEqual("        * Item", settings.Editor.IncreaseIndent("    * Item"));

            settings = new ApplicationSettings { Editor = new EditorSettings { UseSpacesForIndent = false } };
            Assert.AreEqual("\t* Item", settings.Editor.IncreaseIndent("* Item"));
        }

        [TestMethod]
        public void ToggleHeading_ShouldSetCorrectLevel()
        {
            Assert.AreEqual("# Hello", _service.ToggleHeading("Hello", 1));
            Assert.AreEqual("### Hello", _service.ToggleHeading("Hello", 3));
            Assert.AreEqual("## Hello", _service.ToggleHeading("# Hello", 2));
            Assert.AreEqual("# Hello", _service.ToggleHeading("### Hello", 1));
        }
    }
}
