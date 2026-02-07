using Microsoft.VisualStudio.TestTools.UnitTesting;
using PageLeaf.Services;
using System;

namespace PageLeaf.Tests.Services
{
    [TestClass]
    public class EditingSupportServiceEnhancedTests
    {
        private EditingSupportService _service = null!;

        [TestInitialize]
        public void TestInitialize()
        {
            _service = new EditingSupportService();
        }

        #region Blockquote Auto-Continue

        [TestMethod]
        public void GetAutoListMarker_ShouldContinueBlockquote()
        {
            // テスト観点: 引用行で改行したとき、引用記号を継続する。
            Assert.AreEqual("> ", _service.GetAutoListMarker("> Quote"));
            Assert.AreEqual("> ", _service.GetAutoListMarker(">Quote")); // スペースなしでも継続
        }

        [TestMethod]
        public void GetAutoListMarker_ShouldContinueNestedBlockquote()
        {
            // テスト観点: ネストされた引用も継続する。
            Assert.AreEqual(">> ", _service.GetAutoListMarker(">> Nested Quote"));
        }

        [TestMethod]
        public void GetAutoListMarker_ShouldStopBlockquote_WhenEmpty()
        {
            // テスト観点: 引用記号のみの行で改行したとき、引用を終了する（空文字列を返す）。
            Assert.AreEqual(string.Empty, _service.GetAutoListMarker("> "));
            Assert.AreEqual(string.Empty, _service.GetAutoListMarker(">> "));
        }

        #endregion

        #region Task List Toggle

        [TestMethod]
        public void ToggleTaskList_ShouldCheckTask_WhenUnchecked()
        {
            // テスト観点: 未完了タスクを完了にする。
            Assert.AreEqual("- [x] Task", _service.ToggleTaskList("- [ ] Task"));
            Assert.AreEqual("* [x] Task", _service.ToggleTaskList("* [ ] Task"));
        }

        [TestMethod]
        public void ToggleTaskList_ShouldUncheckTask_WhenChecked()
        {
            // テスト観点: 完了タスクを未完了にする。
            // [x] and [X] should be supported
            Assert.AreEqual("- [ ] Task", _service.ToggleTaskList("- [x] Task"));
            Assert.AreEqual("- [ ] Task", _service.ToggleTaskList("- [X] Task"));
        }

        [TestMethod]
        public void ToggleTaskList_ShouldConvertToTaskList_WhenNormalList()
        {
            // テスト観点: 通常のリストをタスクリストに変換する。
            Assert.AreEqual("- [ ] Item", _service.ToggleTaskList("- Item"));
            Assert.AreEqual("* [ ] Item", _service.ToggleTaskList("* Item"));
        }

        [TestMethod]
        public void ToggleTaskList_ShouldConvertToTaskList_WhenNormalText()
        {
            // テスト観点: 通常のテキストをタスクリストに変換する。
            Assert.AreEqual("- [ ] Text", _service.ToggleTaskList("Text"));
        }

        [TestMethod]
        public void ToggleTaskList_ShouldRemoveTaskList_WhenEmptyTask()
        {
            // テスト観点: 空のタスクリスト（記号のみ）の場合、通常のリストに戻すか、テキストに戻すか...
            // VSCodeの挙動: "- [ ] " -> "- " (タスク解除)
            Assert.AreEqual("- ", _service.ToggleTaskList("- [ ] "));
        }

        #endregion

        #region Table Formatting

        [TestMethod]
        public void FormatTableLine_ShouldFormatPipeSpacing()
        {
            // テスト観点: パイプの前後にスペースを入れる。
            Assert.AreEqual("| a | b |", _service.FormatTableLine("|a|b|"));
        }

        [TestMethod]
        public void FormatTableLine_ShouldNotDuplicateSpaces()
        {
            // テスト観点: すでにスペースがある場合は追加しない。
            Assert.AreEqual("| a | b |", _service.FormatTableLine("| a | b |"));
        }

        [TestMethod]
        public void FormatTableLine_ShouldHandleEscapedPipes()
        {
            // テスト観点: エスケープされたパイプ `\|` は区切りとして扱わない（スペースを入れない、または適切に扱う）。
            // 実装簡略化のため、まずはエスケープ無視でも良いが、理想は `| a \| b |` -> `| a\|b |` (Cell content is "a|b")
            Assert.AreEqual(@"| a\|b |", _service.FormatTableLine(@"|a\|b|"));
        }

        #endregion

        #region Enter at Line Start

        [TestMethod]
        public void ShouldAutoContinueList_Perspectives()
        {
            var line = "    * item"; // Indent: 4, Marker: "* ", Total Prefix: 6

            // テスト観点: 絶対行頭 (offset 0)
            // 理由: 行全体を下に押し下げたい（上に空行を入れたい）ため、リスト継続は不要。
            Assert.IsFalse(_service.ShouldAutoContinueList(line, 0), "絶対行頭では継続しないこと");

            // テスト観点: インデントエリア内 (0 < offset < 4)
            // 理由: リスト構造の編集ではなく、レイアウトの調整（空行挿入）を意図しているため、継続しない。
            Assert.IsFalse(_service.ShouldAutoContinueList(line, 2), "インデント内では継続しないこと");

            // テスト観点: マーカー直前 (offset 4)
            // 理由: マーカーを含めて押し下げたい。ここで継続すると "* * item" のように重複するため、継続しない。
            Assert.IsFalse(_service.ShouldAutoContinueList(line, 4), "マーカー直前では継続しないこと");

            // テスト観点: マーカー内 (offset 5)
            // 理由: マーカーを破壊する位置での改行。通常は継続せず、行の分離として扱う。
            Assert.IsFalse(_service.ShouldAutoContinueList(line, 5), "マーカー内では継続しないこと");

            // テスト観点: 本文開始位置 (offset 6)
            // 理由: 項目に対して内容を書き始める位置。ここでの改行は「次の項目へ行く」意図が明確なため、継続する。
            Assert.IsTrue(_service.ShouldAutoContinueList(line, 6), "本文開始位置では継続すること");

            // テスト観点: 本文途中・末尾 (offset > 6)
            // 理由: 通常の改行による次項目作成。
            Assert.IsTrue(_service.ShouldAutoContinueList(line, 10), "本文末尾では継続すること");
        }

        [TestMethod]
        public void ShouldAutoIndent_Perspectives()
        {
            var line = "    * item";

            // テスト観点: 絶対行頭 (offset 0)
            // 理由: 前に「完全な空行（インデントなし）」を挿入したいため、オートインデントは行わない。
            Assert.IsFalse(_service.ShouldAutoIndent(line, 0), "絶対行頭ではインデントを引き継がないこと");

            // テスト観点: インデントエリア内 (offset 2)
            // 理由: 上に「インデント付きの空行」を作って階層を維持したいため、インデントを引き継ぐ。
            Assert.IsTrue(_service.ShouldAutoIndent(line, 2), "インデント内ではインデントを引き継ぐこと");

            // テスト観点: 本文開始位置 (offset 6)
            // 理由: 通常のオートインデント動作。
            Assert.IsTrue(_service.ShouldAutoIndent(line, 6), "本文開始位置ではインデントを引き継ぐこと");
        }

        #endregion

        #region String Construction

        [TestMethod]
        public void GetCodeBlockCompletion_ShouldReturnCorrectFormat()
        {
            // テスト観点: コードブロック補完用の文字列が正しい形式（改行、インデント、閉じ記号）で生成されること。
            string indent = "    ";
            string expected = "\r\n    \r\n    ```";
            Assert.AreEqual(expected, _service.GetCodeBlockCompletion(indent));
        }

        [TestMethod]
        public void GetCodeBlockCompletion_ShouldHandleEmptyIndent()
        {
            // テスト観点: インデントがない場合でも正しく補完されること。
            string indent = "";
            string expected = "\r\n\r\n```";
            Assert.AreEqual(expected, _service.GetCodeBlockCompletion(indent));
        }

        [TestMethod]
        public void GetShiftEnterInsertion_ShouldReturnPageBreakAndNewline()
        {
            // テスト観点: 改ページタグの後に改行が含まれていること。
            string expected = "<div style=\"page-break-after: always;\"></div>\r\n";
            Assert.AreEqual(expected, _service.GetShiftEnterInsertion());
        }

        #endregion

        #region Table Navigation

        [TestMethod]
        public void GetNextCellOffset_ShouldReturnStartOfNextCell()
        {
            // | a | b |
            // 012345678
            string line = "| a | b |";
            // | の直後（空白の前）に移動するのが標準的
            Assert.AreEqual(2, _service.GetNextCellOffset(line, 0)); // | から a
            Assert.AreEqual(6, _service.GetNextCellOffset(line, 2)); // a から b
            Assert.AreEqual(9, _service.GetNextCellOffset(line, 6)); // b から末尾
        }

        [TestMethod]
        public void GetNextCellOffset_ShouldHandleEscapedPipes()
        {
            // | a \| b | c |
            // 01234567890123
            string line = @"| a \| b | c |";
            // | (0) -> Next is 'a' (2)
            Assert.AreEqual(2, _service.GetNextCellOffset(line, 0));
            // 'a' (2) -> Next is 'c' (11) because \| is ignored
            Assert.AreEqual(11, _service.GetNextCellOffset(line, 2));
        }

        [TestMethod]
        public void GetNextCellOffset_ShouldReturnEnd_WhenNoMoreCells()
        {
            string line = "| a |";
            Assert.AreEqual(5, _service.GetNextCellOffset(line, 2));
        }

        #endregion

        #region Auto-Pairing Skip

        [TestMethod]
        public void ShouldSkipClosingCharacter_ShouldReturnTrue_WhenMatchesNextChar()
        {
            // テスト観点: カーソル直後の文字と入力文字が一致する場合、かつそれが閉じ記号対象である場合、スキップを許可する。
            Assert.IsTrue(_service.ShouldSkipClosingCharacter(']', "[]", 1));
            Assert.IsTrue(_service.ShouldSkipClosingCharacter(')', "()", 1));
            Assert.IsTrue(_service.ShouldSkipClosingCharacter('}', "{}", 1));
            Assert.IsTrue(_service.ShouldSkipClosingCharacter('"', "\"\"", 1));
            Assert.IsTrue(_service.ShouldSkipClosingCharacter('\'', "''", 1));
            Assert.IsTrue(_service.ShouldSkipClosingCharacter('`', "``", 1));
        }

        [TestMethod]
        public void ShouldSkipClosingCharacter_ShouldReturnFalse_WhenNotMatchesNextChar()
        {
            // テスト観点: カーソル直後の文字と一致しない場合はスキップしない。
            Assert.IsFalse(_service.ShouldSkipClosingCharacter(']', "[}", 1));
            Assert.IsFalse(_service.ShouldSkipClosingCharacter(')', "(]", 1));
        }

        [TestMethod]
        public void ShouldSkipClosingCharacter_ShouldReturnFalse_WhenAtEnd()
        {
            // テスト観点: カーソルが末尾にある（次の文字がない）場合はスキップしない。
            Assert.IsFalse(_service.ShouldSkipClosingCharacter(']', "[]", 2));
        }

        [TestMethod]
        public void ShouldSkipClosingCharacter_ShouldReturnFalse_WhenNotClosingTarget()
        {
            // テスト観点: a などの普通の文字は、一致していてもスキップ（上書き）しない。
            Assert.IsFalse(_service.ShouldSkipClosingCharacter('a', "aa", 1));
        }

        #endregion

        [TestMethod]
        public void EnforceEmptyLineAtEnd_ShouldAddNewline_WhenMissing()
        {
            // テスト観点: 末尾に改行がない場合、新しく改行が追加されること。
            var service = new EditingSupportService();
            var input = "hoge";
            var expected = "hoge" + Environment.NewLine;

            var result = service.EnforceEmptyLineAtEnd(input);

            Assert.AreEqual(expected, result);
        }

        [TestMethod]
        public void EnforceEmptyLineAtEnd_ShouldNotAddNewline_WhenAlreadyPresent()
        {
            // テスト観点: すでに末尾が改行で終わっている場合、何も追加されないこと。
            var service = new EditingSupportService();
            var input = "hoge" + Environment.NewLine;

            var result = service.EnforceEmptyLineAtEnd(input);

            Assert.AreEqual(input, result);
        }

        [TestMethod]
        public void EnforceEmptyLineAtEnd_ShouldHandleNull()
        {
            // テスト観点: 入力が null の場合、改行のみが返されること。
            var service = new EditingSupportService();
            string input = null!;

            var result = service.EnforceEmptyLineAtEnd(input);

            Assert.AreEqual(Environment.NewLine, result);
        }
    }
}
