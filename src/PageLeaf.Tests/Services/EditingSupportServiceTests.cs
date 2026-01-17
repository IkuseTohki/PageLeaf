using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageLeaf.Models;
using PageLeaf.Services;
using System;

namespace PageLeaf.Tests.Services
{
    [TestClass]
    public class EditingSupportServiceTests
    {
        private EditingSupportService _service = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _service = new EditingSupportService();
        }

        [TestMethod]
        public void GetAutoIndent_ShouldReturnEmpty_WhenLineHasNoIndent()
        {
            // テスト観点: インデントがない行で改行したとき、空文字列を返す。
            var line = "Hello world";
            var result = _service.GetAutoIndent(line);
            Assert.AreEqual("", result);
        }

        [TestMethod]
        public void GetAutoIndent_ShouldReturnSpaces_WhenLineStartsWithSpaces()
        {
            // テスト観点: スペースで始まる行で改行したとき、同じスペースを返す。
            var line = "    Indented line";
            var result = _service.GetAutoIndent(line);
            Assert.AreEqual("    ", result);
        }

        [TestMethod]
        public void GetAutoIndent_ShouldReturnTabs_WhenLineStartsWithTabs()
        {
            // テスト観点: タブで始まる行で改行したとき、同じタブを返す。
            var line = "		Tabbed line";
            var result = _service.GetAutoIndent(line);
            Assert.AreEqual("		", result);
        }

        [TestMethod]
        public void GetAutoIndent_ShouldReturnMixedIndent_WhenLineHasMixedIndent()
        {
            // テスト観点: スペースとタブが混在している場合も、先頭のインデント部分をそのまま返す。
            var line = "	  Mixed line";
            var result = _service.GetAutoIndent(line);
            Assert.AreEqual("	  ", result);
        }

        [TestMethod]
        public void GetAutoListMarker_ShouldReturnNull_WhenLineIsNotList()
        {
            // テスト観点: リストでない行では null を返す。
            Assert.IsNull(_service.GetAutoListMarker("Normal text"));
        }

        [TestMethod]
        public void GetAutoListMarker_ShouldReturnSameMarker_WhenUnorderedList()
        {
            // テスト観点: 順序なしリストの場合、同じ記号を返す。
            Assert.AreEqual("* ", _service.GetAutoListMarker("* Item"));
            Assert.AreEqual("- ", _service.GetAutoListMarker("- Item"));
            Assert.AreEqual("+ ", _service.GetAutoListMarker("+ Item"));
            Assert.AreEqual("  * ", _service.GetAutoListMarker("  * Item")); // インデント付き
        }

        [TestMethod]
        public void GetAutoListMarker_ShouldReturnNextNumber_WhenOrderedList()
        {
            // テスト観点: 順序付きリストの場合、次の番号を返す。
            Assert.AreEqual("2. ", _service.GetAutoListMarker("1. Item"));
            Assert.AreEqual("10. ", _service.GetAutoListMarker("9. Item"));
            Assert.AreEqual("  2. ", _service.GetAutoListMarker("  1. Item")); // インデント付き
        }

        [TestMethod]
        public void GetAutoListMarker_ShouldReturnEmpty_WhenMarkerIsOnlyContent()
        {
            // テスト観点: 記号のみの行の場合、リスト終了を示す空文字列を返す。
            Assert.AreEqual(string.Empty, _service.GetAutoListMarker("* "));
            Assert.AreEqual(string.Empty, _service.GetAutoListMarker("- "));
            Assert.AreEqual(string.Empty, _service.GetAutoListMarker("1. "));
            Assert.AreEqual(string.Empty, _service.GetAutoListMarker("  * "));
        }

        [TestMethod]
        public void GetAutoListMarker_ShouldHandleTaskList()
        {
            // テスト観点: タスクリスト形式を維持して改行されることを確認する。
            Assert.AreEqual("- [ ] ", _service.GetAutoListMarker("- [ ] Task"));
            Assert.AreEqual("* [ ] ", _service.GetAutoListMarker("* [x] Done Task")); // 完了済みも未完了として新規作成
        }

        [TestMethod]
        public void GetAutoListMarker_ShouldReturnNull_WhenNoSpaceAfterMarker()
        {
            // テスト観点: 記号の直後にスペースがない場合はリストとみなさない（Markdown仕様）。
            Assert.IsNull(_service.GetAutoListMarker("*NotAList"));
            Assert.IsNull(_service.GetAutoListMarker("1.NotAList"));
        }

        [TestMethod]
        public void GetPairCharacter_ShouldReturnCorrectPair()
        {
            // テスト観点: 入力された文字に対して、対となる文字を返す。
            Assert.AreEqual(')', _service.GetPairCharacter('('));
            Assert.AreEqual(']', _service.GetPairCharacter('['));
            Assert.AreEqual('}', _service.GetPairCharacter('{'));
            Assert.AreEqual('"', _service.GetPairCharacter('"'));
            Assert.AreEqual('\'', _service.GetPairCharacter('\''));
        }

        [TestMethod]
        public void GetPairCharacter_ShouldReturnNull_WhenNoPairExists()
        {
            // テスト観点: 対となる文字がない場合は null を返す。
            Assert.IsNull(_service.GetPairCharacter('a'));
            Assert.IsNull(_service.GetPairCharacter(' '));
        }

        [TestMethod]
        public void IsCodeBlockStart_ShouldReturnTrue_WhenLineStartsWithBackticks()
        {
            // テスト観点: バックティック3つで始まる行をコードブロック開始と判定する。
            Assert.IsTrue(_service.IsCodeBlockStart("```"));
            Assert.IsTrue(_service.IsCodeBlockStart("```csharp"));
            Assert.IsTrue(_service.IsCodeBlockStart("  ```js")); // インデント付き
        }

        [TestMethod]
        public void IsCodeBlockStart_ShouldReturnFalse_WhenNormalText()
        {
            // テスト観点: 通常のテキストやバックティック2つ以下は判定しない。
            Assert.IsFalse(_service.IsCodeBlockStart("Normal text"));
            Assert.IsFalse(_service.IsCodeBlockStart("``"));
            Assert.IsFalse(_service.IsCodeBlockStart("  `inline code`"));
        }

        [TestMethod]
        public void IsCodeBlockStart_ShouldHandleTildes()
        {
            // テスト観点: チルダ3つによるコードブロック開始も判定できること。
            Assert.IsTrue(_service.IsCodeBlockStart("~~~"));
            Assert.IsTrue(_service.IsCodeBlockStart("~~~csharp"));
            Assert.IsTrue(_service.IsCodeBlockStart("  ~~~js"));
        }

        [TestMethod]
        public void GetIndentString_ShouldReturnCorrectString()
        {
            // テスト観点: 設定に応じて正しいインデント文字列を返す。

            // スペース4つ
            var settings = new ApplicationSettings { IndentSize = 4, UseSpacesForIndent = true };
            Assert.AreEqual("    ", _service.GetIndentString(settings));

            // スペース2つ
            settings = new ApplicationSettings { IndentSize = 2, UseSpacesForIndent = true };
            Assert.AreEqual("  ", _service.GetIndentString(settings));

            // タブ
            settings = new ApplicationSettings { UseSpacesForIndent = false };
            Assert.AreEqual("\t", _service.GetIndentString(settings));
        }

        [TestMethod]
        public void DecreaseIndent_ShouldRemoveLeadingSpacesOrTab()
        {
            // テスト観点: 行頭のインデントを1レベル分削除する。
            var settings = new ApplicationSettings { IndentSize = 4, UseSpacesForIndent = true };

            Assert.AreEqual("No indent", _service.DecreaseIndent("    No indent", settings));
            Assert.AreEqual("  Partially removed", _service.DecreaseIndent("      Partially removed", settings));
            Assert.AreEqual("No indent", _service.DecreaseIndent("\tNo indent", settings)); // タブも削除対象
            Assert.AreEqual("Normal", _service.DecreaseIndent("Normal", settings)); // インデントなし
        }

        [TestMethod]
        public void IncreaseIndent_ShouldAddLeadingSpacesOrTab()
        {
            // テスト観点: 行頭に1レベル分のインデントを追加する。
            var settings = new ApplicationSettings { IndentSize = 4, UseSpacesForIndent = true };

            Assert.AreEqual("    * Item", _service.IncreaseIndent("* Item", settings));
            Assert.AreEqual("        * Item", _service.IncreaseIndent("    * Item", settings));

            settings = new ApplicationSettings { UseSpacesForIndent = false };
            Assert.AreEqual("\t* Item", _service.IncreaseIndent("* Item", settings));
        }

        [TestMethod]
        public void ToggleHeading_ShouldSetCorrectLevel()
        {
            // テスト観点: 行を指定された見出しレベルに変換する。
            Assert.AreEqual("# Hello", _service.ToggleHeading("Hello", 1));
            Assert.AreEqual("### Hello", _service.ToggleHeading("Hello", 3));

            // 既存の見出しを上書き
            Assert.AreEqual("## Hello", _service.ToggleHeading("# Hello", 2));
            Assert.AreEqual("# Hello", _service.ToggleHeading("### Hello", 1));
        }

        [TestMethod]
        public void ToggleHeading_ShouldRemoveHeading_WhenSameLevelIsSpecified()
        {
            // テスト観点: すでに同じレベルの見出しである場合、見出しを解除する。
            Assert.AreEqual("Hello", _service.ToggleHeading("# Hello", 1));
            Assert.AreEqual("Hello", _service.ToggleHeading("### Hello", 3));
        }

        [TestMethod]
        public void ToggleHeading_ShouldHandleEmptyLine()
        {
            // テスト観点: 空行に見出しを適用した場合、記号のみが挿入されること。
            Assert.AreEqual("# ", _service.ToggleHeading("", 1));
            Assert.AreEqual("## ", _service.ToggleHeading("  ", 2));
        }

        [TestMethod]
        public void ToggleHeading_ShouldValidateLevel()
        {
            // テスト観点: 1-6以外のレベルが指定された場合は何もしない（元の行を返す）。
            var line = "Hello";
            Assert.AreEqual(line, _service.ToggleHeading(line, 0));
            Assert.AreEqual(line, _service.ToggleHeading(line, 7));
        }

        [TestMethod]
        public void ToggleHeading_ShouldTrimContent()
        {
            // テスト観点: 前後に空白がある場合も適切に処理されること。
            Assert.AreEqual("# Hello", _service.ToggleHeading("  Hello  ", 1));
        }

        [TestMethod]
        public void ConvertToMarkdownTable_ShouldConvertTsvCorrectly()
        {
            // テスト観点: TSV形式のテキストをMarkdownテーブルに変換する。
            var tsv = "H1\tH2\r\nD1\tD2";
            var expected = "| H1 | H2 |" + Environment.NewLine +
                           "| --- | --- |" + Environment.NewLine +
                           "| D1 | D2 |";

            var result = _service.ConvertToMarkdownTable(tsv);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ConvertToMarkdownTable_ShouldConvertCsvCorrectly()
        {
            // テスト観点: CSV形式のテキストをMarkdownテーブルに変換する。
            var csv = "H1,H2\r\nD1,D2";
            var expected = "| H1 | H2 |" + Environment.NewLine +
                           "| --- | --- |" + Environment.NewLine +
                           "| D1 | D2 |";

            var result = _service.ConvertToMarkdownTable(csv);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void ConvertToMarkdownTable_ShouldHandleVaryingColumnCounts()
        {
            // テスト観点: 行によって列数が異なる場合、最大列数に合わせて不足分を補う。
            var input = "A\tB\r\nC";
            var expected = "| A | B |" + Environment.NewLine +
                           "| --- | --- |" + Environment.NewLine +
                           "| C |  |";

            var result = _service.ConvertToMarkdownTable(input);
            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void GetPageBreakString_ShouldReturnCorrectHtml()
        {
            // テスト観点: 改ページ用のHTMLタグを正しく返す。
            var expected = "<div style=\"page-break-after: always;\"></div>";
            Assert.AreEqual(expected, _service.GetPageBreakString());
        }
    }
}
